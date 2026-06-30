using Volo.Abp.DependencyInjection;
using static General.Admin.Project.ProjectProjectionMapper;

namespace General.Admin.Project;

public class ProjectDashboardService : ITransientDependency
{
    private readonly BusinessService _businessService;
    private readonly ProjectContextLoader _contextLoader;

    public ProjectDashboardService(
        BusinessService businessService,
        ProjectContextLoader contextLoader)
    {
        _businessService = businessService;
        _contextLoader = contextLoader;
    }

    public async Task<ProjectMyExecutionDashboardDto> GetMyRelatedDashboardAsync()
    {
        var context = await _contextLoader.LoadAsync();
        if (!context.CurrentUserId.HasValue)
        {
            return new ProjectMyExecutionDashboardDto();
        }

        var currentUserId = context.CurrentUserId.Value;
        var weekStartDate = GetWeekStart(DateTime.Today);
        var myProjects = context.Projects
            .Where(project => IsMyRelatedProject(project, context))
            .OrderByDescending(project => project.IsKeyProject)
            .ThenBy(project => project.PlannedEndDate ?? DateTime.MaxValue)
            .Select(project => BuildMyRelatedProject(project, currentUserId, weekStartDate, context))
            .ToList();
        var myTasks = context.Tasks
            .Where(task => IsCurrentUserTask(task, currentUserId, context))
            .Where(task => !IsClosed(task.Status))
            .OrderBy(task => task.PlannedEndTime ?? DateTime.MaxValue)
            .Take(12)
            .Select(task => MapTask(task, context))
            .ToList();
        var myIssues = context.Issues
            .Where(issue => issue.OwnerUserId == currentUserId)
            .Where(issue => !IsClosed(issue.Status))
            .OrderBy(issue => issue.DueDate ?? DateTime.MaxValue)
            .Take(10)
            .Select(issue => MapIssue(issue, context))
            .ToList();
        var recentWorklogs = context.Worklogs
            .Where(worklog => worklog.UserId == currentUserId)
            .OrderByDescending(worklog => worklog.WorkDate)
            .ThenByDescending(worklog => worklog.CreationTime)
            .Take(10)
            .Select(worklog => MapWorklog(worklog, context))
            .ToList();
        var actionItems = BuildActionItems(
            context.Projects.Where(project => IsMyRelatedProject(project, context)),
            context,
            task => IsCurrentUserTask(task, currentUserId, context),
            issue => issue.OwnerUserId == currentUserId,
            12);

        return new ProjectMyExecutionDashboardDto
        {
            ActionItems = actionItems,
            Metrics =
            [
                Metric("相关项目", myProjects.Count),
                Metric("我的待办", myTasks.Count),
                Metric("我的问题", myIssues.Count),
                Metric("本周工时", (decimal)recentWorklogs.Where(item => item.WorkDate.Date >= weekStartDate.Date).Sum(item => item.Hours), "小时")
            ],
            MyIssues = myIssues,
            MyProjects = myProjects,
            MyTasks = myTasks,
            RecentWorklogs = recentWorklogs
        };
    }

    public async Task<ProjectPmDashboardDto> GetPmDashboardAsync()
    {
        var context = await _contextLoader.LoadAsync();
        var businessMap = await LoadBusinessSummaryMapAsync();
        var projects = context.CurrentUserId.HasValue
            ? context.Projects.Where(project => project.ManagerUserId == context.CurrentUserId.Value).ToList()
            : [];

        return new ProjectPmDashboardDto
        {
            ActionItems = BuildActionItems(projects, context, null, null, 50),
            BusinessRisks = BuildBusinessRisks(projects, businessMap, 8),
            EarnedValueSummary = BuildEarnedValueSummary(projects, context),
            HealthBuckets = BuildHealthBuckets(projects, context),
            Metrics = BuildPortfolioMetrics(projects, context),
            PerformancePoints = BuildPerformancePoints(projects, context),
            Projects = BuildProjectCards(projects, context, businessMap)
        };
    }

    public async Task<ProjectPmoDashboardDto> GetPmoDashboardAsync()
    {
        var context = await _contextLoader.LoadAsync();
        var businessMap = await LoadBusinessSummaryMapAsync();
        var projects = context.Projects;
        var projectCards = BuildProjectCards(projects, context, businessMap);

        return new ProjectPmoDashboardDto
        {
            ActionItems = BuildActionItems(projects, context, null, null, 100),
            BusinessRisks = BuildBusinessRisks(projects, businessMap, 10),
            DepartmentPressures = BuildDepartmentPressures(projectCards),
            EarnedValueSummary = BuildEarnedValueSummary(projects, context),
            ExceptionProjects = BuildExceptionProjects(projectCards, 8),
            HealthBuckets = BuildHealthBuckets(projects, context),
            Metrics = BuildPortfolioMetrics(projects, context),
            PerformancePoints = BuildPerformancePoints(projects, context),
            Projects = projectCards
        };
    }

    private static ProjectOverviewItemDto BuildMyRelatedProject(
        Project project,
        Guid currentUserId,
        DateTime weekStartDate,
        ProjectContext context)
    {
        var tasks = (context.TasksByProject.GetValueOrDefault(project.Id) ?? [])
            .Where(task => IsCurrentUserTask(task, currentUserId, context) && !IsClosed(task.Status))
            .ToList();
        var issues = (context.IssuesByProject.GetValueOrDefault(project.Id) ?? [])
            .Where(issue => issue.OwnerUserId == currentUserId && !IsClosed(issue.Status))
            .ToList();
        var weekHours = (context.WorklogsByProject.GetValueOrDefault(project.Id) ?? [])
            .Where(worklog => worklog.UserId == currentUserId && worklog.WeekStartDate.Date == weekStartDate.Date)
            .Sum(worklog => worklog.Hours);
        var nextSprint = (context.SprintsByProject.GetValueOrDefault(project.Id) ?? [])
            .Where(sprint => !IsClosed(sprint.Status))
            .OrderBy(sprint => sprint.StartDate ?? DateTime.MaxValue)
            .FirstOrDefault();

        return new ProjectOverviewItemDto
        {
            ActiveIssueCount = issues.Count,
            DueSoonTaskCount = tasks.Count(task => IsDueSoon(task.PlannedEndTime)),
            Id = project.Id,
            MyRelation = ResolveMyRelation(project, context),
            Name = project.Name,
            NextSprintName = nextSprint?.Name ?? "-",
            ProjectCode = project.ProjectCode,
            Status = project.Status,
            WeekHours = Math.Round(weekHours, 1)
        };
    }

    private static List<ProjectDashboardProjectCardDto> BuildProjectCards(
        IReadOnlyCollection<Project> projects,
        ProjectContext context,
        IReadOnlyDictionary<Guid, BusinessProjectSummaryDto> businessMap)
    {
        return projects
            .Select(project =>
            {
                var tasks = context.TasksByProject.GetValueOrDefault(project.Id) ?? [];
                var milestones = context.MilestonesByProject.GetValueOrDefault(project.Id) ?? [];
                var raidItems = context.RaidItemsByProject.GetValueOrDefault(project.Id) ?? [];
                var issues = context.IssuesByProject.GetValueOrDefault(project.Id) ?? [];
                var snapshot = LatestSnapshot(project.Id, context);
                var completedTaskCount = tasks.Count(task => IsClosed(task.Status));
                var progress = tasks.Count == 0
                    ? 0
                    : (int)Math.Round(completedTaskCount * 100d / tasks.Count, MidpointRounding.AwayFromZero);
                var healthScore = CalculateHealthRiskScore(tasks, issues, raidItems, progress);
                businessMap.TryGetValue(project.Id, out var business);

                return new ProjectDashboardProjectCardDto
                {
                    BlockedTaskCount = tasks.Count(task => task.IsBlocked && !IsClosed(task.Status)),
                    CompletedTaskCount = completedTaskCount,
                    ContractAmount = business?.ContractAmount ?? project.ContractTotalAmount,
                    CostAmount = business?.CostAmount,
                    CostPerformanceIndex = snapshot?.CostPerformanceIndex,
                    EarnedValue = snapshot?.EarnedValue,
                    HealthRiskScore = healthScore,
                    HealthStatus = ResolveHealthStatus(healthScore),
                    HighRiskCount = raidItems.Count(item => IsHighRisk(item)),
                    Id = project.Id,
                    IsKeyProject = project.IsKeyProject,
                    ManagerName = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
                    Name = project.Name,
                    OpenIssueCount = issues.Count(issue => !IsClosed(issue.Status)),
                    OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(project.OrganizationUnitId) ?? "-",
                    OverdueMilestoneCount = milestones.Count(milestone => milestone.PlannedCompletionDate.Date < DateTime.Today && !IsClosed(milestone.Status)),
                    OverdueTaskCount = tasks.Count(task => IsOverdue(task.PlannedEndTime, task.Status)),
                    PlannedEndDate = project.PlannedEndDate,
                    PlannedStartDate = project.PlannedStartDate,
                    PlannedValue = snapshot?.PlannedValue,
                    Priority = project.Priority,
                    Progress = progress,
                    ProjectCode = project.ProjectCode,
                    ReceivedAmount = business?.ReceivedAmount ?? project.ReceivedAmount,
                    SchedulePerformanceIndex = snapshot?.SchedulePerformanceIndex,
                    Status = project.Status,
                    TaskTotalCount = tasks.Count
                };
            })
            .OrderByDescending(item => item.HealthRiskScore)
            .ThenByDescending(item => item.IsKeyProject)
            .ThenBy(item => item.PlannedEndDate ?? DateTime.MaxValue)
            .ToList();
    }

    private static List<ProjectDashboardMetricDto> BuildPortfolioMetrics(
        IReadOnlyCollection<Project> projects,
        ProjectContext context)
    {
        var projectIds = projects.Select(project => project.Id).ToHashSet();
        var tasks = context.Tasks.Where(task => projectIds.Contains(task.ProjectId)).ToList();
        var milestones = context.Milestones.Where(milestone => projectIds.Contains(milestone.ProjectId)).ToList();
        var raidItems = context.RaidItems.Where(raid => projectIds.Contains(raid.ProjectId)).ToList();
        var issues = context.Issues.Where(issue => projectIds.Contains(issue.ProjectId)).ToList();
        var worklogs = context.Worklogs.Where(worklog => projectIds.Contains(worklog.ProjectId)).ToList();

        return
        [
            Metric("项目总数", projects.Count),
            Metric("执行中", projects.Count(project => !IsClosed(project.Status))),
            Metric("逾期任务", tasks.Count(task => IsOverdue(task.PlannedEndTime, task.Status))),
            Metric("阻塞任务", tasks.Count(task => task.IsBlocked && !IsClosed(task.Status))),
            Metric("P0风险", raidItems.Count(IsHighRisk)),
            Metric("开放问题", issues.Count(issue => !IsClosed(issue.Status))),
            Metric("逾期里程碑", milestones.Count(milestone => milestone.PlannedCompletionDate.Date < DateTime.Today && !IsClosed(milestone.Status))),
            Metric("累计工时", (decimal)Math.Round(worklogs.Sum(worklog => worklog.Hours), 1), "小时")
        ];
    }

    private static List<ProjectDashboardHealthBucketDto> BuildHealthBuckets(
        IReadOnlyCollection<Project> projects,
        ProjectContext context)
    {
        return projects
            .Select(project =>
            {
                var tasks = context.TasksByProject.GetValueOrDefault(project.Id) ?? [];
                var issues = context.IssuesByProject.GetValueOrDefault(project.Id) ?? [];
                var raidItems = context.RaidItemsByProject.GetValueOrDefault(project.Id) ?? [];
                var completedTaskCount = tasks.Count(task => IsClosed(task.Status));
                var progress = tasks.Count == 0
                    ? 0
                    : (int)Math.Round(completedTaskCount * 100d / tasks.Count, MidpointRounding.AwayFromZero);

                return ResolveHealthStatus(CalculateHealthRiskScore(tasks, issues, raidItems, progress));
            })
            .GroupBy(status => status)
            .Select(group => new ProjectDashboardHealthBucketDto
            {
                Count = group.Count(),
                HealthStatus = group.Key
            })
            .OrderBy(item => item.HealthStatus)
            .ToList();
    }

    private static List<ProjectDashboardPerformancePointDto> BuildPerformancePoints(
        IReadOnlyCollection<Project> projects,
        ProjectContext context)
    {
        var projectIds = projects.Select(project => project.Id).ToHashSet();

        return context.PerformanceSnapshots
            .Where(snapshot => projectIds.Contains(snapshot.ProjectId))
            .GroupBy(snapshot => snapshot.ProjectId)
            .Select(group => group.OrderByDescending(snapshot => snapshot.SnapshotDate).First())
            .OrderBy(snapshot => snapshot.SchedulePerformanceIndex == 0 ? decimal.MaxValue : snapshot.SchedulePerformanceIndex)
            .ThenBy(snapshot => snapshot.CostPerformanceIndex == 0 ? decimal.MaxValue : snapshot.CostPerformanceIndex)
            .Select(snapshot => new ProjectDashboardPerformancePointDto
            {
                ActualCost = snapshot.ActualCost,
                CostPerformanceIndex = snapshot.CostPerformanceIndex,
                EarnedValue = snapshot.EarnedValue,
                PlannedValue = snapshot.PlannedValue,
                ProjectId = snapshot.ProjectId,
                ProjectName = context.ProjectNameMap.GetValueOrDefault(snapshot.ProjectId) ?? "-",
                SchedulePerformanceIndex = snapshot.SchedulePerformanceIndex,
                SnapshotDate = snapshot.SnapshotDate
            })
            .ToList();
    }

    private static ProjectDashboardEarnedValueSummaryDto BuildEarnedValueSummary(
        IReadOnlyCollection<Project> projects,
        ProjectContext context)
    {
        var projectIds = projects.Select(project => project.Id).ToHashSet();
        var latestSnapshots = context.PerformanceSnapshots
            .Where(snapshot => projectIds.Contains(snapshot.ProjectId))
            .GroupBy(snapshot => snapshot.ProjectId)
            .Select(group => group.OrderByDescending(snapshot => snapshot.SnapshotDate).First())
            .ToList();
        var plannedValue = latestSnapshots.Sum(snapshot => snapshot.PlannedValue);
        var earnedValue = latestSnapshots.Sum(snapshot => snapshot.EarnedValue);
        var actualCost = latestSnapshots.Sum(snapshot => snapshot.ActualCost);

        return new ProjectDashboardEarnedValueSummaryDto
        {
            ActualCost = actualCost,
            CostPerformanceIndex = actualCost <= 0 ? 0 : Math.Round(earnedValue / actualCost, 2),
            EarnedValue = earnedValue,
            PlannedValue = plannedValue,
            SchedulePerformanceIndex = plannedValue <= 0 ? 0 : Math.Round(earnedValue / plannedValue, 2),
            SnapshotDate = latestSnapshots
                .OrderByDescending(snapshot => snapshot.SnapshotDate)
                .Select(snapshot => (DateTime?)snapshot.SnapshotDate)
                .FirstOrDefault()
        };
    }

    private static List<ProjectDashboardActionItemDto> BuildActionItems(
        IEnumerable<Project> projects,
        ProjectContext context,
        Func<ProjectTask, bool>? taskPredicate,
        Func<ProjectIssue, bool>? issuePredicate,
        int takeCount)
    {
        var projectIds = projects.Select(project => project.Id).ToHashSet();
        var taskItems = context.Tasks
            .Where(task => projectIds.Contains(task.ProjectId))
            .Where(task => taskPredicate?.Invoke(task) ?? true)
            .Where(task => (task.IsBlocked || IsOverdue(task.PlannedEndTime, task.Status) || IsDueSoon(task.PlannedEndTime)) && !IsClosed(task.Status))
            .Select(task => ActionItem(
                task.ProjectId,
                context.ProjectNameMap.GetValueOrDefault(task.ProjectId) ?? "-",
                task.Id,
                "任务",
                task.Title,
                task.Status,
                task.IsBlocked ? "P0" : task.Priority,
                task.PlannedEndTime,
                task.IsBlocked ? "任务阻塞" : IsOverdue(task.PlannedEndTime, task.Status) ? "任务逾期" : "7天内到期",
                "task"));
        var issueItems = context.Issues
            .Where(issue => projectIds.Contains(issue.ProjectId))
            .Where(issue => issuePredicate?.Invoke(issue) ?? true)
            .Where(issue => !IsClosed(issue.Status))
            .Where(issue => IsIssueOverdue(issue) || issue.Level == ProjectPriorities.P0 || (issue.DueDate.HasValue && issue.DueDate.Value.Date <= DateTime.Today.AddDays(7)))
            .Select(issue => ActionItem(
                issue.ProjectId,
                context.ProjectNameMap.GetValueOrDefault(issue.ProjectId) ?? "-",
                issue.Id,
                "问题",
                issue.Title,
                issue.Status,
                issue.Level,
                issue.DueDate,
                IsIssueOverdue(issue) ? "问题逾期" : issue.Level == ProjectPriorities.P0 ? "P0问题" : "7天内到期",
                "issue"));
        var raidItems = context.RaidItems
            .Where(raid => projectIds.Contains(raid.ProjectId))
            .Where(raid => !IsClosed(raid.Status))
            .Where(raid => IsHighRisk(raid) || (raid.PlannedResolveDate.HasValue && raid.PlannedResolveDate.Value.Date < DateTime.Today))
            .Select(raid => ActionItem(
                raid.ProjectId,
                context.ProjectNameMap.GetValueOrDefault(raid.ProjectId) ?? "-",
                raid.Id,
                raid.Type,
                raid.Title,
                raid.Status,
                raid.Level,
                raid.PlannedResolveDate,
                IsHighRisk(raid) ? "高风险RAID" : "RAID逾期",
                "raid"));
        var milestoneItems = context.Milestones
            .Where(milestone => projectIds.Contains(milestone.ProjectId))
            .Where(milestone => milestone.PlannedCompletionDate.Date < DateTime.Today && !IsClosed(milestone.Status))
            .Select(milestone => ActionItem(
                milestone.ProjectId,
                context.ProjectNameMap.GetValueOrDefault(milestone.ProjectId) ?? "-",
                milestone.Id,
                "里程碑",
                milestone.Name,
                milestone.Status,
                ProjectPriorities.P1,
                milestone.PlannedCompletionDate,
                "里程碑逾期",
                "milestone"));

        return taskItems
            .Concat(issueItems)
            .Concat(raidItems)
            .Concat(milestoneItems)
            .OrderByDescending(item => ProjectPriorities.ResolveWeight(item.Level))
            .ThenBy(item => item.DueDate ?? DateTime.MaxValue)
            .Take(takeCount)
            .ToList();
    }

    private static List<ProjectDashboardBusinessRiskDto> BuildBusinessRisks(
        IReadOnlyCollection<Project> projects,
        IReadOnlyDictionary<Guid, BusinessProjectSummaryDto> businessMap,
        int takeCount)
    {
        var projectIds = projects.Select(project => project.Id).ToHashSet();

        return businessMap.Values
            .Where(item => projectIds.Contains(item.Id))
            .Select(item =>
            {
                var riskLabel = ResolveBusinessRiskLabel(item);
                return new ProjectDashboardBusinessRiskDto
                {
                    BusinessCloseStatus = item.BusinessCloseStatus,
                    ContractAmount = item.ContractAmount,
                    CostAmount = item.CostAmount,
                    GrossProfitRate = item.GrossProfitRate,
                    ProjectId = item.Id,
                    ProjectName = item.Name,
                    ReceivedAmount = item.ReceivedAmount,
                    RiskLabel = riskLabel,
                    YearEndReceivableForecast = item.YearEndReceivableForecast
                };
            })
            .Where(item => item.RiskLabel != "正常")
            .OrderBy(item => item.RiskLabel == "利润预警" ? 0 : item.RiskLabel == "回款风险" ? 1 : 2)
            .ThenByDescending(item => item.ContractAmount ?? 0)
            .Take(takeCount)
            .ToList();
    }

    private static List<ProjectDashboardDepartmentPressureDto> BuildDepartmentPressures(
        IReadOnlyCollection<ProjectDashboardProjectCardDto> projects)
    {
        return projects
            .GroupBy(project => string.IsNullOrWhiteSpace(project.OrganizationUnitName) ? "未归属部门" : project.OrganizationUnitName)
            .Select(group => new ProjectDashboardDepartmentPressureDto
            {
                Name = group.Key,
                ProjectCount = group.Count(),
                RiskScore = group.Sum(project =>
                    project.HealthRiskScore +
                    project.HighRiskCount * 10 +
                    project.OverdueMilestoneCount * 5 +
                    project.OverdueTaskCount * 3 +
                    project.BlockedTaskCount * 3)
            })
            .OrderByDescending(item => item.RiskScore)
            .ThenByDescending(item => item.ProjectCount)
            .Take(6)
            .ToList();
    }

    private static List<ProjectDashboardProjectCardDto> BuildExceptionProjects(
        IReadOnlyCollection<ProjectDashboardProjectCardDto> projects,
        int takeCount)
    {
        return projects
            .Where(project =>
                project.HealthStatus != "健康" ||
                project.OverdueTaskCount > 0 ||
                project.OverdueMilestoneCount > 0 ||
                project.HighRiskCount > 0 ||
                project.BlockedTaskCount > 0)
            .OrderByDescending(project => project.HealthRiskScore)
            .ThenByDescending(project => project.HighRiskCount)
            .ThenByDescending(project => project.OverdueMilestoneCount)
            .ThenByDescending(project => project.OverdueTaskCount)
            .Take(takeCount)
            .ToList();
    }

    private async Task<Dictionary<Guid, BusinessProjectSummaryDto>> LoadBusinessSummaryMapAsync()
    {
        var businessOverview = await _businessService.GetOverviewAsync();
        return businessOverview.Projects.ToDictionary(item => item.Id);
    }

    private static ProjectPerformanceSnapshot? LatestSnapshot(Guid projectId, ProjectContext context)
    {
        return (context.PerformanceSnapshotsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(snapshot => snapshot.SnapshotDate)
            .FirstOrDefault();
    }

    private static ProjectDashboardActionItemDto ActionItem(
        Guid projectId,
        string projectName,
        Guid sourceId,
        string sourceType,
        string title,
        string status,
        string level,
        DateTime? dueDate,
        string reason,
        string actionKey)
    {
        return new ProjectDashboardActionItemDto
        {
            ActionKey = actionKey,
            DueDate = dueDate,
            Level = level,
            ProjectId = projectId,
            ProjectName = projectName,
            Reason = reason,
            SourceId = sourceId,
            SourceType = sourceType,
            Status = status,
            Title = title
        };
    }

    private static ProjectDashboardMetricDto Metric(string label, decimal value, string unit = "")
    {
        return new ProjectDashboardMetricDto
        {
            Label = label,
            Unit = unit,
            Value = value
        };
    }

    private static int CalculateHealthRiskScore(
        IReadOnlyCollection<ProjectTask> tasks,
        IReadOnlyCollection<ProjectIssue> issues,
        IReadOnlyCollection<ProjectRaidItem> raidItems,
        int progress)
    {
        return BuildHealthBreakdowns(tasks, issues, raidItems, progress).Sum(item => item.Score);
    }

    private static bool IsCurrentUserTask(ProjectTask task, Guid currentUserId, ProjectContext context)
    {
        return task.OwnerUserId == currentUserId ||
            task.ProductOwnerUserId == currentUserId ||
            task.DeveloperOwnerUserId == currentUserId ||
            task.TesterOwnerUserId == currentUserId ||
            context.TaskParticipantsByTask.GetValueOrDefault(task.Id)?.Any(item => item.UserId == currentUserId) == true;
    }

    private static bool IsHighRisk(ProjectRaidItem item)
    {
        return item.Type == ProjectRaidTypes.Risk &&
            (item.Level == ProjectPriorities.P0 || item.RiskScore >= 16) &&
            !IsClosed(item.Status);
    }

    private static bool IsIssueOverdue(ProjectIssue issue)
    {
        return issue.DueDate.HasValue &&
            issue.DueDate.Value.Date < DateTime.Today &&
            !IsClosed(issue.Status);
    }

    private static string ResolveBusinessRiskLabel(BusinessProjectSummaryDto item)
    {
        if (item.GrossProfitRate.HasValue && item.GrossProfitRate.Value < 10)
        {
            return "利润预警";
        }

        if (item.YearEndReceivableForecast.HasValue &&
            item.ReceivedAmount.HasValue &&
            item.YearEndReceivableForecast.Value > item.ReceivedAmount.Value)
        {
            return "回款风险";
        }

        if (item.BusinessCloseStatus.Contains("收尾", StringComparison.OrdinalIgnoreCase) ||
            item.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase))
        {
            return "收尾风险";
        }

        return "正常";
    }
}
