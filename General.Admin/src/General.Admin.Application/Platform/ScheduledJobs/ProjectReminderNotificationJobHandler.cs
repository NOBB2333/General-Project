using System.Text.Json;
using System.Threading;
using General.Admin.Integration;
using General.Admin.Project;
using Volo.Abp.Domain.Repositories;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Platform;

public class ProjectReminderNotificationJobHandler : IPlatformScheduledJobHandler
{
    private const int MaxDispatchItems = 100;

    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;
    private readonly IntegrationNotificationService _notificationService;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectMeetingRecord, Guid> _meetingRecordRepository;
    private readonly IRepository<ProjectRaidItem, Guid> _raidRepository;
    private readonly IRepository<ProjectTask, Guid> _taskRepository;

    public ProjectReminderNotificationJobHandler(
        PlatformSchedulerExecutionLogWriter executionLogWriter,
        IntegrationNotificationService notificationService,
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<ProjectMeetingRecord, Guid> meetingRecordRepository,
        IRepository<ProjectRaidItem, Guid> raidRepository,
        IRepository<ProjectTask, Guid> taskRepository)
    {
        _executionLogWriter = executionLogWriter;
        _notificationService = notificationService;
        _projectRepository = projectRepository;
        _meetingRecordRepository = meetingRecordRepository;
        _raidRepository = raidRepository;
        _taskRepository = taskRepository;
    }

    public string JobKey => "project-reminder-notification";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var projects = (await _projectRepository.GetListAsync(cancellationToken: cancellationToken))
            .ToDictionary(x => x.Id);
        var activeTasks = (await _taskRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(x => x.PlannedEndTime.HasValue && !ProjectStatuses.IsClosed(x.Status))
            .ToList();
        var activeRisks = (await _raidRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(x =>
                x.Type == ProjectRaidTypes.Risk &&
                !ProjectStatuses.IsClosed(x.Status))
            .ToList();
        var meetingRecords = await _meetingRecordRepository.GetListAsync(cancellationToken: cancellationToken);

        var summary = new ReminderDispatchSummary();
        foreach (var task in activeTasks
                     .Where(x => x.PlannedEndTime!.Value.Date >= today && x.PlannedEndTime!.Value.Date <= today.AddDays(3))
                     .OrderBy(x => x.PlannedEndTime)
                     .Take(MaxDispatchItems))
        {
            if (projects.TryGetValue(task.ProjectId, out var project))
            {
                await DispatchTaskAsync("project.task.due", project, task, today, summary);
            }
        }

        foreach (var task in activeTasks
                     .Where(x => x.PlannedEndTime!.Value.Date < today)
                     .OrderBy(x => x.PlannedEndTime)
                     .Take(MaxDispatchItems))
        {
            if (projects.TryGetValue(task.ProjectId, out var project))
            {
                await DispatchTaskAsync("project.task.overdue", project, task, today, summary);
                await DispatchGovernanceActionAsync(
                    project,
                    "任务逾期",
                    task.Title,
                    task.OwnerUserId,
                    task.Id,
                    $"任务计划结束日期 {task.PlannedEndTime:yyyy-MM-dd}，已逾期 {Math.Max(0, (today - task.PlannedEndTime!.Value.Date).Days)} 天。",
                    summary);
            }
        }

        foreach (var risk in activeRisks
                     .Where(x => x.PlannedResolveDate.HasValue && x.PlannedResolveDate.Value.Date >= today && x.PlannedResolveDate.Value.Date <= today.AddDays(7))
                     .OrderBy(x => x.PlannedResolveDate)
                     .Take(MaxDispatchItems))
        {
            if (projects.TryGetValue(risk.ProjectId, out var project))
            {
                await DispatchRiskAsync("project.risk.due", project, risk, today, summary);
            }
        }

        foreach (var risk in activeRisks
                     .Where(x => x.RiskScore >= 16 || x.Level == ProjectPriorities.P0 || x.Level == ProjectPriorities.P1)
                     .OrderByDescending(x => x.RiskScore)
                     .ThenBy(x => x.PlannedResolveDate)
                     .Take(MaxDispatchItems))
        {
            if (projects.TryGetValue(risk.ProjectId, out var project))
            {
                await DispatchRiskAsync("project.risk.high", project, risk, today, summary);
                await DispatchGovernanceActionAsync(
                    project,
                    "高风险升级",
                    risk.Title,
                    risk.OwnerUserId,
                    risk.Id,
                    $"风险等级 {risk.Level}，风险分值 {risk.RiskScore}，需 PMO 关注应对计划。",
                    summary);
            }
        }

        var now = DateTime.Now;
        foreach (var meeting in meetingRecords
                     .Where(x => x.MeetingTime > now && x.MeetingTime <= now.AddDays(1))
                     .OrderBy(x => x.MeetingTime)
                     .Take(MaxDispatchItems))
        {
            if (projects.TryGetValue(meeting.ProjectId, out var project))
            {
                await DispatchMeetingAsync("project.meeting.reminder", project, meeting, summary);
            }
        }

        foreach (var meeting in meetingRecords
                     .Where(x =>
                         x.MeetingTime <= now &&
                         x.MeetingTime >= now.AddDays(-7) &&
                         !string.IsNullOrWhiteSpace(x.Minutes))
                     .OrderByDescending(x => x.MeetingTime)
                     .Take(MaxDispatchItems))
        {
            if (projects.TryGetValue(meeting.ProjectId, out var project))
            {
                await DispatchMeetingAsync("project.meeting.minutes.distribute", project, meeting, summary);
            }
        }

        var result = $"项目提醒扫描完成：触发 {summary.EventCount} 个事件，规则投递成功 {summary.SentCount} 条，失败 {summary.FailedCount} 条，未匹配目标 {summary.NoTargetCount} 条，跳过 {summary.SkippedCount} 条。";
        await _executionLogWriter.WriteOperationAsync(JobKey, "项目到期与风险提醒", result, cancellationToken: cancellationToken);
        return $"{result} 可在日志中心的操作日志查看执行记录。";
    }

    private async Task DispatchTaskAsync(
        string eventCode,
        ProjectEntity project,
        ProjectTask task,
        DateTime today,
        ReminderDispatchSummary summary)
    {
        var plannedEndDate = task.PlannedEndTime!.Value.Date;
        var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
        {
            EventCode = eventCode,
            OccurredAt = DateTime.UtcNow,
            VariablesJson = JsonSerializer.Serialize(new
            {
                business = new { key = task.Id.ToString() },
                project = BuildProjectVariable(project),
                recipient = new { userIds = new[] { task.OwnerUserId } },
                task = new
                {
                    id = task.Id,
                    code = task.TaskCode,
                    name = task.Title,
                    ownerUserId = task.OwnerUserId,
                    plannedEndDate = plannedEndDate.ToString("yyyy-MM-dd"),
                    status = task.Status,
                    daysRemaining = Math.Max(0, (plannedEndDate - today).Days),
                    daysOverdue = Math.Max(0, (today - plannedEndDate).Days)
                }
            })
        });
        summary.Add(result);
    }

    private async Task DispatchRiskAsync(
        string eventCode,
        ProjectEntity project,
        ProjectRaidItem risk,
        DateTime today,
        ReminderDispatchSummary summary)
    {
        var plannedResolveDate = risk.PlannedResolveDate?.Date;
        var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
        {
            EventCode = eventCode,
            OccurredAt = DateTime.UtcNow,
            VariablesJson = JsonSerializer.Serialize(new
            {
                business = new { key = risk.Id.ToString() },
                project = BuildProjectVariable(project),
                recipient = new { userIds = new[] { risk.OwnerUserId } },
                risk = new
                {
                    id = risk.Id,
                    name = risk.Title,
                    ownerUserId = risk.OwnerUserId,
                    status = risk.Status,
                    level = risk.Level,
                    riskScore = risk.RiskScore,
                    probability = risk.Probability,
                    impact = risk.Impact,
                    plannedResolveDate = plannedResolveDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    daysRemaining = plannedResolveDate.HasValue ? Math.Max(0, (plannedResolveDate.Value - today).Days) : 0,
                    daysOverdue = plannedResolveDate.HasValue ? Math.Max(0, (today - plannedResolveDate.Value).Days) : 0
                }
            })
        });
        summary.Add(result);
    }

    private async Task DispatchGovernanceActionAsync(
        ProjectEntity project,
        string actionType,
        string title,
        Guid ownerUserId,
        Guid sourceId,
        string reason,
        ReminderDispatchSummary summary)
    {
        var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
        {
            EventCode = "pmo.governance.action",
            OccurredAt = DateTime.UtcNow,
            VariablesJson = JsonSerializer.Serialize(new
            {
                business = new { key = sourceId.ToString() },
                governance = new
                {
                    actionType,
                    title,
                    ownerUserId,
                    reason
                },
                project = BuildProjectVariable(project),
                recipient = new { userIds = new[] { project.ManagerUserId, ownerUserId } }
            })
        });
        summary.Add(result);
    }

    private async Task DispatchMeetingAsync(
        string eventCode,
        ProjectEntity project,
        ProjectMeetingRecord meeting,
        ReminderDispatchSummary summary)
    {
        var participantUserIds = meeting.GetParticipantUserIds()
            .Append(meeting.FacilitatorUserId)
            .Distinct()
            .ToList();
        var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
        {
            EventCode = eventCode,
            OccurredAt = DateTime.UtcNow,
            VariablesJson = JsonSerializer.Serialize(new
            {
                business = new { key = meeting.Id.ToString() },
                meeting = new
                {
                    id = meeting.Id,
                    topic = meeting.Topic,
                    meetingTime = meeting.MeetingTime.ToString("yyyy-MM-dd HH:mm"),
                    facilitatorUserId = meeting.FacilitatorUserId,
                    participantUserIds,
                    status = meeting.Status,
                    minutes = meeting.Minutes,
                    decisions = meeting.Decisions
                },
                project = BuildProjectVariable(project),
                recipient = new { userIds = participantUserIds }
            })
        });
        summary.Add(result);
    }

    private static object BuildProjectVariable(ProjectEntity project)
    {
        return new
        {
            id = project.Id,
            code = project.ProjectCode,
            detailUrl = $"/#/project/detail?projectId={project.Id}",
            name = project.Name,
            managerUserId = project.ManagerUserId,
            status = project.Status
        };
    }

    private sealed class ReminderDispatchSummary
    {
        public int EventCount { get; private set; }

        public int FailedCount { get; private set; }

        public int NoTargetCount { get; private set; }

        public int SentCount { get; private set; }

        public int SkippedCount { get; private set; }

        public void Add(IntegrationNotificationDispatchResultDto result)
        {
            EventCount++;
            FailedCount += result.FailedCount;
            NoTargetCount += result.NoTargetCount;
            SentCount += result.SentCount;
            SkippedCount += result.SkippedCount + result.BlockedCount;
        }
    }
}
