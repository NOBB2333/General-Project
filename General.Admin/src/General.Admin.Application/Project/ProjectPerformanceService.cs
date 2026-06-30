using General.Admin.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace General.Admin.Project;

public class ProjectPerformanceService : ITransientDependency
{
    private const decimal DefaultWorklogHourlyRate = 100m;

    private readonly IRepository<ProjectBaseline, Guid> _projectBaselineRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<ProjectPerformanceSnapshot, Guid> _projectPerformanceSnapshotRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly ISettingProvider _settingProvider;

    public ProjectPerformanceService(
        IRepository<ProjectBaseline, Guid> projectBaselineRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<ProjectPerformanceSnapshot, Guid> projectPerformanceSnapshotRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository,
        IRepository<ProjectWorklog, Guid> projectWorklogRepository,
        ISettingProvider settingProvider)
    {
        _projectBaselineRepository = projectBaselineRepository;
        _projectRepository = projectRepository;
        _projectPerformanceSnapshotRepository = projectPerformanceSnapshotRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _settingProvider = settingProvider;
    }

    public async Task<Guid> CreateSnapshotAsync(Guid projectId, ProjectPerformanceSnapshotSaveInput input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var baselines = await _projectBaselineRepository.GetListAsync(item => item.ProjectId == projectId);
        var baseline = input.ProjectBaselineId.HasValue
            ? baselines.FirstOrDefault(item => item.Id == input.ProjectBaselineId.Value)
            : baselines.FirstOrDefault(item => item.IsActive);
        if (input.ProjectBaselineId.HasValue && baseline == null)
        {
            throw new BusinessException("绩效快照关联的基线必须属于当前项目。");
        }

        var snapshotDate = input.SnapshotDate?.Date ?? DateTime.Today;
        if (await _projectPerformanceSnapshotRepository.AnyAsync(item => item.ProjectId == projectId && item.SnapshotDate == snapshotDate))
        {
            throw new BusinessException("当前统计日期已存在绩效快照。");
        }

        var calculated = await CalculatePerformanceValuesAsync(project, baseline, snapshotDate);
        var snapshot = new ProjectPerformanceSnapshot(
            Guid.NewGuid(),
            projectId,
            snapshotDate,
            string.IsNullOrWhiteSpace(input.SnapshotPeriod) ? "Weekly" : input.SnapshotPeriod.Trim(),
            input.PlannedValue ?? calculated.PlannedValue,
            input.EarnedValue ?? calculated.EarnedValue,
            input.ActualCost ?? calculated.ActualCost,
            baseline?.Id);
        await _projectPerformanceSnapshotRepository.InsertAsync(snapshot, autoSave: true);
        return snapshot.Id;
    }

    public async Task<int> CreateMissingWeeklySnapshotsAsync(DateTime snapshotDate)
    {
        var date = snapshotDate.Date;
        var projects = (await _projectRepository.GetListAsync())
            .Where(item => !ProjectStatuses.IsClosed(item.Status))
            .ToList();
        var existingProjectIds = (await _projectPerformanceSnapshotRepository.GetListAsync(item => item.SnapshotDate == date))
            .Select(item => item.ProjectId)
            .ToHashSet();
        var createdCount = 0;

        foreach (var project in projects.Where(item => !existingProjectIds.Contains(item.Id)))
        {
            var baselines = await _projectBaselineRepository.GetListAsync(item => item.ProjectId == project.Id);
            var baseline = baselines.FirstOrDefault(item => item.IsActive);
            var calculated = await CalculatePerformanceValuesAsync(project, baseline, date);
            var snapshot = new ProjectPerformanceSnapshot(
                Guid.NewGuid(),
                project.Id,
                date,
                "Weekly",
                calculated.PlannedValue,
                calculated.EarnedValue,
                calculated.ActualCost,
                baseline?.Id);

            await _projectPerformanceSnapshotRepository.InsertAsync(snapshot, autoSave: true);
            createdCount++;
        }

        return createdCount;
    }

    private async Task<(decimal PlannedValue, decimal EarnedValue, decimal ActualCost)> CalculatePerformanceValuesAsync(
        Project project,
        ProjectBaseline? baseline,
        DateTime snapshotDate)
    {
        var tasks = await _projectTaskRepository.GetListAsync(item => item.ProjectId == project.Id);
        var worklogs = await _projectWorklogRepository.GetListAsync(item => item.ProjectId == project.Id);
        var budgetAtCompletion = baseline?.BudgetAtCompletion
            ?? project.BudgetTotalAmount
            ?? project.ContractTotalAmount
            ?? 0m;
        if (budgetAtCompletion <= 0 || tasks.Count == 0)
        {
            return (0m, 0m, 0m);
        }

        var taskBudget = budgetAtCompletion / tasks.Count;
        var plannedValue = tasks.Sum(task => CalculatePlannedTaskRatio(task, snapshotDate) * taskBudget);
        var earnedValue = tasks.Sum(task => ResolveTaskCompletionRatio(task) * taskBudget);
        var hourlyRate = await ResolveWorklogHourlyRateAsync();
        var worklogCost = (decimal)worklogs
            .Where(item => item.WorkDate.Date <= snapshotDate.Date)
            .Sum(item => item.Hours) * hourlyRate;
        var taskActualCost = tasks
            .Where(item => item.ActualWorkHours.HasValue)
            .Sum(item => (decimal)item.ActualWorkHours!.Value * hourlyRate);
        var actualCost = worklogCost > 0 ? worklogCost : taskActualCost;

        return (
            Math.Round(plannedValue, 2),
            Math.Round(earnedValue, 2),
            Math.Round(actualCost, 2));
    }

    private static decimal CalculatePlannedTaskRatio(ProjectTask task, DateTime snapshotDate)
    {
        var start = task.BaselineStartTime ?? task.PlannedStartTime;
        var end = task.BaselineEndTime ?? task.PlannedEndTime;
        if (!start.HasValue && !end.HasValue)
        {
            return ProjectStatuses.IsClosed(task.Status) ? 1m : 0m;
        }

        if (!start.HasValue)
        {
            return snapshotDate.Date >= end!.Value.Date ? 1m : 0m;
        }

        if (!end.HasValue)
        {
            return snapshotDate.Date >= start.Value.Date ? 1m : 0m;
        }

        if (snapshotDate.Date < start.Value.Date)
        {
            return 0m;
        }

        if (snapshotDate.Date >= end.Value.Date)
        {
            return 1m;
        }

        var totalDays = Math.Max(1, (end.Value.Date - start.Value.Date).Days + 1);
        var elapsedDays = Math.Max(0, (snapshotDate.Date - start.Value.Date).Days + 1);
        return Math.Clamp((decimal)elapsedDays / totalDays, 0m, 1m);
    }

    private static decimal ResolveTaskCompletionRatio(ProjectTask task)
    {
        if (ProjectStatuses.IsClosed(task.Status))
        {
            return 1m;
        }

        return task.Status switch
        {
            ProjectStatuses.InProgress => 0.5m,
            ProjectStatuses.Testing => 0.75m,
            ProjectStatuses.Accepting => 0.9m,
            _ => 0m
        };
    }

    private async Task<decimal> ResolveWorklogHourlyRateAsync()
    {
        var value = await _settingProvider.GetOrNullAsync(AdminSettings.ProjectEvmWorklogHourlyRate);
        return decimal.TryParse(value, out var rate) && rate > 0
            ? rate
            : DefaultWorklogHourlyRate;
    }
}
