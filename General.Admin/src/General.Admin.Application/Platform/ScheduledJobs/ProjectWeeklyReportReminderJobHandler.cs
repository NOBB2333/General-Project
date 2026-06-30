using System.Text.Json;
using System.Threading;
using General.Admin.Integration;
using General.Admin.Project;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Platform;

public class ProjectWeeklyReportReminderJobHandler : IPlatformScheduledJobHandler
{
    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;
    private readonly IntegrationNotificationService _notificationService;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectRaidItem, Guid> _projectRaidItemRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectWeeklyReportReminderJobHandler(
        PlatformSchedulerExecutionLogWriter executionLogWriter,
        IntegrationNotificationService notificationService,
        IRepository<ProjectMember, Guid> projectMemberRepository,
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<ProjectRaidItem, Guid> projectRaidItemRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository,
        IRepository<ProjectWorklog, Guid> projectWorklogRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _executionLogWriter = executionLogWriter;
        _notificationService = notificationService;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
        _projectRaidItemRepository = projectRaidItemRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _userRepository = userRepository;
    }

    public string JobKey => "weekly-report-reminder";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var weekStart = GetWeekStart(DateTime.Today);
        var activeMemberRecords = (await _projectMemberRepository.GetListAsync())
            .Where(x => x.IsActive(weekStart))
            .ToList();
        var activeMembers = activeMemberRecords
            .Select(x => x.UserId)
            .Distinct()
            .ToList();

        var groupReportResult = await DispatchProjectGroupWeeklyReportsAsync(weekStart, activeMemberRecords);
        if (activeMembers.Count == 0)
        {
            var emptyResult = $"周报提醒完成：当前没有需要提醒的项目成员；项目群周报成功 {groupReportResult.SentCount} 条，失败 {groupReportResult.FailedCount} 条，未匹配目标 {groupReportResult.NoTargetCount} 条。";
            await _executionLogWriter.WriteOperationAsync(JobKey, "周报填写通知", emptyResult, cancellationToken: cancellationToken);
            return $"{emptyResult} 可在日志中心的操作日志查看执行记录。";
        }

        var submittedUserIds = (await _projectWorklogRepository.GetListAsync())
            .Where(x => x.WeekStartDate == weekStart)
            .Select(x => x.UserId)
            .Distinct()
            .ToHashSet();

        var pendingUserIds = activeMembers
            .Where(userId => !submittedUserIds.Contains(userId))
            .Distinct()
            .ToList();

        var userLookup = (await _userRepository.GetListAsync(cancellationToken: cancellationToken))
            .ToDictionary(x => x.Id);
        var pendingUserNames = pendingUserIds
            .Select(userId => userLookup.TryGetValue(userId, out var user)
                ? string.IsNullOrWhiteSpace($"{user.Name}{user.Surname}".Trim())
                    ? user.UserName ?? userId.ToString("N")[..8]
                    : $"{user.Name}{user.Surname}".Trim()
                : userId.ToString("N")[..8])
            .Take(5)
            .ToList();

        var dispatchResult = pendingUserIds.Count == 0
            ? new IntegrationNotificationDispatchResultDto()
            : await DispatchPendingWorklogEventsAsync(weekStart, pendingUserIds, pendingUserNames);
        MergeDispatchResult(dispatchResult, groupReportResult);

        var result = pendingUserIds.Count == 0
            ? $"周报提醒完成：本周周报已全部提交（统计周期起点 {weekStart:yyyy-MM-dd}）。"
            : $"周报提醒完成：待提醒 {pendingUserIds.Count} 名成员（{string.Join("、", pendingUserNames)}{(pendingUserIds.Count > pendingUserNames.Count ? " 等" : string.Empty)}），统计周期起点 {weekStart:yyyy-MM-dd}，规则投递成功 {dispatchResult.SentCount} 条，失败 {dispatchResult.FailedCount} 条，未匹配目标 {dispatchResult.NoTargetCount} 条。";

        await _executionLogWriter.WriteOperationAsync(JobKey, "周报填写通知", result, cancellationToken: cancellationToken);
        return $"{result} 可在日志中心的操作日志查看执行记录。";
    }

    private async Task<IntegrationNotificationDispatchResultDto> DispatchProjectGroupWeeklyReportsAsync(
        DateTime weekStart,
        IReadOnlyCollection<ProjectMember> activeMemberRecords)
    {
        var result = new IntegrationNotificationDispatchResultDto();
        var projectIds = activeMemberRecords.Select(x => x.ProjectId).Distinct().ToHashSet();
        if (projectIds.Count == 0)
        {
            return result;
        }

        var weekEnd = weekStart.AddDays(6);
        var projects = (await _projectRepository.GetListAsync())
            .Where(x => projectIds.Contains(x.Id))
            .ToDictionary(x => x.Id);
        var tasks = (await _projectTaskRepository.GetListAsync())
            .Where(x => projectIds.Contains(x.ProjectId))
            .ToList();
        var risks = (await _projectRaidItemRepository.GetListAsync())
            .Where(x => projectIds.Contains(x.ProjectId) && x.Type == ProjectRaidTypes.Risk)
            .ToList();
        var worklogs = (await _projectWorklogRepository.GetListAsync())
            .Where(x => projectIds.Contains(x.ProjectId) && x.WeekStartDate == weekStart)
            .ToList();

        foreach (var project in projects.Values.OrderBy(x => x.ProjectCode))
        {
            var projectTasks = tasks.Where(x => x.ProjectId == project.Id).ToList();
            var projectRisks = risks.Where(x => x.ProjectId == project.Id && !ProjectStatuses.IsClosed(x.Status)).ToList();
            var reportResult = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
            {
                EventCode = "project.group.weekly-report",
                OccurredAt = DateTime.UtcNow,
                VariablesJson = JsonSerializer.Serialize(new
                {
                    project = new
                    {
                        id = project.Id,
                        code = project.ProjectCode,
                        detailUrl = $"/#/project/detail?projectId={project.Id}",
                        name = project.Name,
                        managerUserId = project.ManagerUserId,
                        status = project.Status
                    },
                    weeklyReport = new
                    {
                        weekStartDate = weekStart.ToString("yyyy-MM-dd"),
                        weekEndDate = weekEnd.ToString("yyyy-MM-dd"),
                        activeMemberCount = activeMemberRecords.Count(x => x.ProjectId == project.Id),
                        completedTaskCount = projectTasks.Count(x => ProjectStatuses.IsClosed(x.Status)),
                        overdueTaskCount = projectTasks.Count(x => x.PlannedEndTime.HasValue && x.PlannedEndTime.Value.Date < DateTime.Today && !ProjectStatuses.IsClosed(x.Status)),
                        highRiskCount = projectRisks.Count(x => x.RiskScore >= 16 || x.Level == ProjectPriorities.P0 || x.Level == ProjectPriorities.P1),
                        worklogHours = worklogs.Where(x => x.ProjectId == project.Id).Sum(x => x.Hours)
                    }
                })
            });
            MergeDispatchResult(result, reportResult);
        }

        return result;
    }

    private async Task<IntegrationNotificationDispatchResultDto> DispatchPendingWorklogEventsAsync(
        DateTime weekStart,
        IReadOnlyCollection<Guid> pendingUserIds,
        IReadOnlyCollection<string> pendingUserNames)
    {
        var variablesJson = JsonSerializer.Serialize(new
        {
            recipient = new
            {
                userIds = pendingUserIds,
                userNames = pendingUserNames
            },
            report = new
            {
                name = "本周项目周报",
                weekStartDate = weekStart.ToString("yyyy-MM-dd")
            },
            worklog = new
            {
                weekStartDate = weekStart.ToString("yyyy-MM-dd")
            }
        });
        var weeklyReportResult = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
        {
            EventCode = "project.weekly-report.due",
            OccurredAt = DateTime.UtcNow,
            VariablesJson = variablesJson
        });
        var worklogResult = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
        {
            EventCode = "project.worklog.reminder",
            OccurredAt = DateTime.UtcNow,
            VariablesJson = variablesJson
        });
        weeklyReportResult.FailedCount += worklogResult.FailedCount;
        weeklyReportResult.NoTargetCount += worklogResult.NoTargetCount;
        weeklyReportResult.SentCount += worklogResult.SentCount;
        weeklyReportResult.SkippedCount += worklogResult.SkippedCount + worklogResult.BlockedCount;
        weeklyReportResult.Items = weeklyReportResult.Items.Concat(worklogResult.Items).ToList();
        return weeklyReportResult;
    }

    private static void MergeDispatchResult(
        IntegrationNotificationDispatchResultDto target,
        IntegrationNotificationDispatchResultDto source)
    {
        target.FailedCount += source.FailedCount;
        target.NoTargetCount += source.NoTargetCount;
        target.SentCount += source.SentCount;
        target.SkippedCount += source.SkippedCount + source.BlockedCount;
        target.Items = target.Items.Concat(source.Items).ToList();
    }

    private static DateTime GetWeekStart(DateTime value)
    {
        var diff = ((int)value.DayOfWeek + 6) % 7;
        return value.Date.AddDays(-diff);
    }
}
