using General.Admin.Business;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.Project;

public class ProjectCommunicationService : ITransientDependency
{
    private static readonly string[] ReportTypes = ["日报", "周报", "月报", "阶段报告", "项目总结"];
    private static readonly string[] ReportStatuses = ["草稿", "已提交", "已发布", "已归档"];
    private static readonly string[] MeetingStatuses = ["草稿", "已确认", "已归档"];
    private static readonly string[] ActionTypes = ["行动项", "转任务", "关联任务", "关联需求", "关联风险", "关联问题", "生成需求", "生成风险", "生成问题"];
    private static readonly string[] ActionStatuses = ["未开始", "进行中", "已完成", "已关闭"];

    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<BusinessBudgetExecution, Guid> _budgetExecutionRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<BusinessInvoice, Guid> _invoiceRepository;
    private readonly IRepository<BusinessProcurement, Guid> _procurementRepository;
    private readonly IRepository<BusinessReceivable, Guid> _receivableRepository;
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<ProjectMeetingActionItem, Guid> _projectMeetingActionItemRepository;
    private readonly IRepository<ProjectMeetingRecord, Guid> _projectMeetingRecordRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectRaidItem, Guid> _projectRaidItemRepository;
    private readonly IRepository<ProjectReport, Guid> _projectReportRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectCommunicationService(
        ProjectAccessGuard projectAccessGuard,
        ICurrentUser currentUser,
        IRepository<BusinessBudgetExecution, Guid> budgetExecutionRepository,
        IRepository<BusinessContract, Guid> businessContractRepository,
        IRepository<BusinessInvoice, Guid> invoiceRepository,
        IRepository<BusinessProcurement, Guid> procurementRepository,
        IRepository<BusinessReceivable, Guid> receivableRepository,
        IRepository<AppPlatformFile, Guid> fileRepository,
        IRepository<ProjectSprint, Guid> projectSprintRepository,
        IRepository<ProjectIssue, Guid> projectIssueRepository,
        IRepository<ProjectMeetingActionItem, Guid> projectMeetingActionItemRepository,
        IRepository<ProjectMeetingRecord, Guid> projectMeetingRecordRepository,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<ProjectRaidItem, Guid> projectRaidItemRepository,
        IRepository<ProjectReport, Guid> projectReportRepository,
        IRepository<ProjectRequirement, Guid> projectRequirementRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository,
        IRepository<ProjectWorklog, Guid> projectWorklogRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _projectAccessGuard = projectAccessGuard;
        _currentUser = currentUser;
        _budgetExecutionRepository = budgetExecutionRepository;
        _businessContractRepository = businessContractRepository;
        _invoiceRepository = invoiceRepository;
        _procurementRepository = procurementRepository;
        _receivableRepository = receivableRepository;
        _fileRepository = fileRepository;
        _projectSprintRepository = projectSprintRepository;
        _projectIssueRepository = projectIssueRepository;
        _projectMeetingActionItemRepository = projectMeetingActionItemRepository;
        _projectMeetingRecordRepository = projectMeetingRecordRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectRaidItemRepository = projectRaidItemRepository;
        _projectReportRepository = projectReportRepository;
        _projectRequirementRepository = projectRequirementRepository;
        _projectRepository = projectRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _userRepository = userRepository;
    }

    public async Task<Guid> CreateMeetingRecordAsync(Guid projectId, ProjectMeetingRecordSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        await ValidateMeetingInputAsync(projectId, input);

        var meetingRecord = new ProjectMeetingRecord(
            Guid.NewGuid(),
            projectId,
            input.Topic.Trim(),
            input.MeetingTime ?? DateTime.Now,
            input.FacilitatorUserId,
            input.ParticipantUserIds,
            input.Status.Trim(),
            input.Minutes,
            input.Decisions,
            input.AttachmentFileId);

        await _projectMeetingRecordRepository.InsertAsync(meetingRecord, autoSave: true);

        var actionItems = new List<ProjectMeetingActionItem>();
        foreach (var item in input.ActionItems.Where(item => !string.IsNullOrWhiteSpace(item.Title)))
        {
            var actionType = item.ActionType.Trim();
            var linkedTaskId = actionType == "转任务"
                ? await CreateTaskFromActionItemAsync(projectId, item)
                : item.LinkedTaskId;
            var linkedRaidItemId = actionType == "生成风险"
                ? await CreateRiskFromActionItemAsync(projectId, item)
                : item.LinkedRaidItemId;
            var linkedIssueId = actionType == "生成问题"
                ? await CreateIssueFromActionItemAsync(projectId, item)
                : item.LinkedIssueId;
            var linkedRequirementId = actionType == "生成需求"
                ? await CreateRequirementFromActionItemAsync(projectId, item)
                : item.LinkedRequirementId;
            actionItems.Add(new ProjectMeetingActionItem(
                Guid.NewGuid(),
                projectId,
                meetingRecord.Id,
                item.Title.Trim(),
                actionType,
                item.Status.Trim(),
                item.AssigneeUserId,
                item.DueDate,
                linkedTaskId,
                linkedRaidItemId,
                linkedIssueId,
                linkedRequirementId));
        }

        if (actionItems.Count > 0)
        {
            await _projectMeetingActionItemRepository.InsertManyAsync(actionItems, autoSave: true);
        }

        return meetingRecord.Id;
    }

    public async Task<Guid> CreateReportAsync(Guid projectId, ProjectReportSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        await ValidateReportInputAsync(projectId, input);

        var (periodStart, periodEnd) = ResolveReportPeriod(input.ReportType, input.PeriodStart, input.PeriodEnd);
        var report = new ProjectReport(
            Guid.NewGuid(),
            projectId,
            input.ReportType.Trim(),
            string.IsNullOrWhiteSpace(input.Title)
                ? ResolveReportTitle(input.ReportType, periodStart, periodEnd)
                : input.Title.Trim(),
            input.AuthorUserId,
            periodStart,
            periodEnd,
            input.Status.Trim(),
            input.BusinessSummary,
            input.CompletedSummary,
            input.NextPlan,
            input.RiskIssueSummary,
            input.MilestoneSummary,
            input.HoursSummary,
            input.SprintId,
            input.AttachmentFileId);

        await _projectReportRepository.InsertAsync(report, autoSave: true);
        return report.Id;
    }

    public async Task<ProjectReportDraftDto> GetReportDraftAsync(Guid projectId, ProjectReportDraftInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);

        var reportType = string.IsNullOrWhiteSpace(input.ReportType)
            ? "周报"
            : input.ReportType.Trim();
        if (!ReportTypes.Contains(reportType))
        {
            throw new BusinessException("报告类型不支持。");
        }

        var (periodStart, periodEnd) = ResolveReportPeriod(reportType, input.PeriodStart, input.PeriodEnd);
        var tasks = await _projectTaskRepository.GetListAsync(item => item.ProjectId == projectId);
        var worklogs = await _projectWorklogRepository.GetListAsync(item => item.ProjectId == projectId);
        var milestones = await _projectMilestoneRepository.GetListAsync(item => item.ProjectId == projectId);
        var risks = await _projectRaidItemRepository.GetListAsync(item => item.ProjectId == projectId);
        var issues = await _projectIssueRepository.GetListAsync(item => item.ProjectId == projectId);
        var businessSummary = reportType == "月报"
            ? await BuildMonthlyBusinessSummaryAsync(projectId, periodStart, periodEnd)
            : string.Empty;

        return new ProjectReportDraftDto
        {
            BusinessSummary = businessSummary,
            CompletedSummary = BuildCompletedSummary(tasks, periodStart, periodEnd, reportType),
            HoursSummary = await BuildHoursSummaryAsync(worklogs, periodStart, periodEnd, reportType),
            MilestoneSummary = BuildMilestoneSummary(milestones, periodStart, periodEnd),
            NextPlan = BuildNextPlan(tasks, periodEnd),
            PeriodEnd = periodEnd,
            PeriodStart = periodStart,
            ReportType = reportType,
            RiskIssueSummary = BuildRiskIssueSummary(risks, issues, periodStart, periodEnd, reportType),
            Title = ResolveReportTitle(reportType, periodStart, periodEnd)
        };
    }

    private static string BuildCompletedSummary(
        IReadOnlyCollection<ProjectTask> tasks,
        DateTime periodStart,
        DateTime periodEnd,
        string reportType)
    {
        var completedTasks = tasks
            .Where(item => ProjectStatuses.IsClosed(item.Status))
            .Where(item => IsInRange(item.ActualEndTime ?? item.PlannedEndTime, periodStart, periodEnd))
            .OrderBy(item => item.PlannedEndTime ?? DateTime.MaxValue)
            .Take(12)
            .Select(item => $"完成 {item.TaskCode} {item.Title}")
            .ToList();

        if (completedTasks.Count == 0 && reportType == "日报")
        {
            completedTasks = tasks
                .Where(item => IsInRange(item.PlannedEndTime, periodStart, periodEnd))
                .OrderBy(item => item.PlannedEndTime ?? DateTime.MaxValue)
                .Take(8)
                .Select(item => $"推进 {item.TaskCode} {item.Title}，当前状态 {item.Status}")
                .ToList();
        }

        return completedTasks.Count == 0
            ? "本期暂无已完成任务，请补充实际完成事项。"
            : string.Join(Environment.NewLine, completedTasks);
    }

    private async Task<string> BuildHoursSummaryAsync(
        IReadOnlyCollection<ProjectWorklog> worklogs,
        DateTime periodStart,
        DateTime periodEnd,
        string reportType)
    {
        var periodWorklogs = worklogs
            .Where(item => item.WorkDate.Date >= periodStart.Date && item.WorkDate.Date <= periodEnd.Date)
            .ToList();
        if (periodWorklogs.Count == 0)
        {
            return "本期暂无工时记录。";
        }

        var totalHours = Math.Round(periodWorklogs.Sum(item => item.Hours), 1);
        var userCount = periodWorklogs.Select(item => item.UserId).Distinct().Count();
        if (reportType != "月报")
        {
            return $"本期累计工时 {totalHours} 小时，涉及 {userCount} 人。";
        }

        return await BuildMonthlyResourceSummaryAsync(worklogs, periodWorklogs, periodStart, periodEnd, totalHours, userCount);
    }

    private async Task<string> BuildMonthlyResourceSummaryAsync(
        IReadOnlyCollection<ProjectWorklog> allWorklogs,
        IReadOnlyCollection<ProjectWorklog> periodWorklogs,
        DateTime periodStart,
        DateTime periodEnd,
        double totalHours,
        int userCount)
    {
        var periodDays = Math.Max(1, (periodEnd.Date - periodStart.Date).Days + 1);
        var previousStart = periodStart.Date.AddDays(-periodDays);
        var previousEnd = periodStart.Date.AddDays(-1);
        var previousWorklogs = allWorklogs
            .Where(item => item.WorkDate.Date >= previousStart.Date && item.WorkDate.Date <= previousEnd.Date)
            .ToList();
        var previousHours = Math.Round(previousWorklogs.Sum(item => item.Hours), 1);
        var previousUserCount = previousWorklogs.Select(item => item.UserId).Distinct().Count();
        var taskLinkedHours = Math.Round(periodWorklogs.Where(item => item.TaskId.HasValue).Sum(item => item.Hours), 1);
        var taskLinkedRate = totalHours <= 0
            ? 0
            : Math.Round(taskLinkedHours / totalHours * 100, 1);

        var topUserHours = periodWorklogs
            .GroupBy(item => item.UserId)
            .Select(item => new { UserId = item.Key, Hours = Math.Round(item.Sum(worklog => worklog.Hours), 1) })
            .OrderByDescending(item => item.Hours)
            .Take(5)
            .ToList();
        var userIds = topUserHours.Select(item => item.UserId).ToList();
        var users = userIds.Count == 0
            ? new List<IdentityUser>()
            : await _userRepository.GetListAsync(item => userIds.Contains(item.Id));
        var userNameMap = users.ToDictionary(item => item.Id, ResolveDisplayName);
        var topUserText = topUserHours.Count == 0
            ? "暂无成员工时记录。"
            : string.Join("；", topUserHours.Select(item =>
                $"{userNameMap.GetValueOrDefault(item.UserId) ?? "-"} {item.Hours:0.#} 小时"));

        return string.Join(Environment.NewLine, new[]
        {
            $"本月累计工时 {totalHours:0.#} 小时，参与成员 {userCount} 人；较上期工时 {FormatDelta(totalHours - previousHours)} 小时，参与人数 {FormatDelta(userCount - previousUserCount)} 人。",
            $"有任务关联的工时 {taskLinkedHours:0.#} 小时，占比 {taskLinkedRate:0.#}%，用于判断填报是否能回到具体执行任务。",
            $"投入 Top 成员：{topUserText}。",
            "以上仅反映项目资源投入和填报完整性，不作为个人绩效评价。"
        });
    }

    private static string BuildMilestoneSummary(
        IReadOnlyCollection<ProjectMilestone> milestones,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var periodMilestones = milestones
            .Where(item => item.PlannedCompletionDate.Date >= periodStart.Date && item.PlannedCompletionDate.Date <= periodEnd.Date)
            .OrderBy(item => item.PlannedCompletionDate)
            .Take(8)
            .Select(item => $"{item.Name}：{item.Status}，计划 {item.PlannedCompletionDate:yyyy-MM-dd}")
            .ToList();

        return periodMilestones.Count == 0
            ? "本期无计划完成里程碑。"
            : string.Join(Environment.NewLine, periodMilestones);
    }

    private static string BuildNextPlan(IReadOnlyCollection<ProjectTask> tasks, DateTime periodEnd)
    {
        var nextPeriodEnd = periodEnd.Date.AddDays(7);
        var nextTasks = tasks
            .Where(item => !ProjectStatuses.IsClosed(item.Status))
            .Where(item => item.PlannedEndTime.HasValue &&
                item.PlannedEndTime.Value.Date > periodEnd.Date &&
                item.PlannedEndTime.Value.Date <= nextPeriodEnd)
            .OrderBy(item => item.PlannedEndTime)
            .Take(8)
            .Select(item => $"{item.TaskCode} {item.Title}，计划 {item.PlannedEndTime:yyyy-MM-dd}")
            .ToList();

        return nextTasks.Count == 0
            ? "下期暂无明确到期任务，请补充计划。"
            : string.Join(Environment.NewLine, nextTasks);
    }

    private static string BuildRiskIssueSummary(
        IReadOnlyCollection<ProjectRaidItem> risks,
        IReadOnlyCollection<ProjectIssue> issues,
        DateTime periodStart,
        DateTime periodEnd,
        string reportType)
    {
        if (reportType == "月报")
        {
            return BuildMonthlyRiskIssueTrend(risks, issues, periodStart, periodEnd);
        }

        var activeRisks = risks
            .Where(item => item.Type == ProjectRaidTypes.Risk)
            .Where(item => !ProjectStatuses.IsClosed(item.Status))
            .Where(item => !item.PlannedResolveDate.HasValue || IsInRange(item.PlannedResolveDate, periodStart, periodEnd))
            .OrderByDescending(item => item.Level)
            .Take(6)
            .Select(item => $"风险：{item.Title}（{item.Level} / {item.Status}）");
        var activeIssues = issues
            .Where(item => !ProjectStatuses.IsClosed(item.Status))
            .Where(item => !item.DueDate.HasValue || IsInRange(item.DueDate, periodStart, periodEnd))
            .OrderByDescending(item => item.Level)
            .Take(6)
            .Select(item => $"问题：{item.Title}（{item.Level} / {item.Status}）");
        var items = activeRisks.Concat(activeIssues).ToList();

        return items.Count == 0
            ? "本期未触发待处理风险或问题。"
            : string.Join(Environment.NewLine, items);
    }

    private static string BuildMonthlyRiskIssueTrend(
        IReadOnlyCollection<ProjectRaidItem> risks,
        IReadOnlyCollection<ProjectIssue> issues,
        DateTime periodStart,
        DateTime periodEnd)
    {
        var riskItems = risks
            .Where(item => item.Type == ProjectRaidTypes.Risk)
            .ToList();
        var issueItems = issues.ToList();
        var periodDays = Math.Max(1, (periodEnd.Date - periodStart.Date).Days + 1);
        var previousStart = periodStart.Date.AddDays(-periodDays);
        var previousEnd = periodStart.Date.AddDays(-1);

        var currentNewRiskCount = riskItems.Count(item => IsCreatedInRange(item.CreationTime, periodStart, periodEnd));
        var previousNewRiskCount = riskItems.Count(item => IsCreatedInRange(item.CreationTime, previousStart, previousEnd));
        var currentNewIssueCount = issueItems.Count(item => IsCreatedInRange(item.CreationTime, periodStart, periodEnd));
        var previousNewIssueCount = issueItems.Count(item => IsCreatedInRange(item.CreationTime, previousStart, previousEnd));

        var openRisks = riskItems
            .Where(item => !ProjectStatuses.IsClosed(item.Status))
            .ToList();
        var openIssues = issueItems
            .Where(item => !ProjectStatuses.IsClosed(item.Status))
            .ToList();
        var overdueRiskCount = openRisks.Count(item =>
            item.PlannedResolveDate.HasValue && item.PlannedResolveDate.Value.Date < periodEnd.Date);
        var overdueIssueCount = openIssues.Count(item =>
            item.DueDate.HasValue && item.DueDate.Value.Date < periodEnd.Date);

        var focusRisks = openRisks
            .OrderByDescending(item => IsHighRisk(item))
            .ThenByDescending(item => item.RiskScore)
            .ThenBy(item => item.PlannedResolveDate ?? DateTime.MaxValue)
            .Take(3)
            .Select(item => $"风险：{item.Title}（{item.Level} / {item.Status}）")
            .ToList();
        var focusIssues = openIssues
            .OrderByDescending(item => ResolveLevelWeight(item.Level))
            .ThenBy(item => item.DueDate ?? DateTime.MaxValue)
            .Take(Math.Max(0, 6 - focusRisks.Count))
            .Select(item => $"问题：{item.Title}（{item.Level} / {item.Status}）")
            .ToList();
        var focusItems = focusRisks.Concat(focusIssues).ToList();

        return string.Join(Environment.NewLine, new[]
        {
            $"本月新增风险 {currentNewRiskCount} 个（较上期 {FormatDelta(currentNewRiskCount - previousNewRiskCount)}），新增问题 {currentNewIssueCount} 个（较上期 {FormatDelta(currentNewIssueCount - previousNewIssueCount)}）。",
            $"当前待闭环风险 {openRisks.Count} 个、待闭环问题 {openIssues.Count} 个；其中高等级风险 {openRisks.Count(IsHighRisk)} 个、高等级问题 {openIssues.Count(IsHighIssue)} 个。",
            $"已逾期未闭环风险 {overdueRiskCount} 个、问题 {overdueIssueCount} 个；当前已关闭风险 {riskItems.Count(item => ProjectStatuses.IsClosed(item.Status))} 个、问题 {issueItems.Count(item => ProjectStatuses.IsClosed(item.Status))} 个。",
            focusItems.Count == 0
                ? "本月无重点待跟进风险或问题。"
                : $"重点跟进：{string.Join("；", focusItems)}。"
        });
    }

    private static bool IsCreatedInRange(DateTime creationTime, DateTime periodStart, DateTime periodEnd)
    {
        return creationTime.Date >= periodStart.Date && creationTime.Date <= periodEnd.Date;
    }

    private static bool IsHighRisk(ProjectRaidItem item)
    {
        return item.Level == ProjectPriorities.P0 || item.RiskScore >= 16;
    }

    private static bool IsHighIssue(ProjectIssue item)
    {
        return item.Level == ProjectPriorities.P0;
    }

    private static int ResolveLevelWeight(string level)
    {
        return level switch
        {
            ProjectPriorities.P0 => 4,
            ProjectPriorities.P1 => 3,
            ProjectPriorities.P2 => 2,
            ProjectPriorities.P3 => 1,
            _ => 0
        };
    }

    private static string FormatDelta(int delta)
    {
        return delta switch
        {
            > 0 => $"+{delta}",
            < 0 => delta.ToString(),
            _ => "持平"
        };
    }

    private static string FormatDelta(double delta)
    {
        var rounded = Math.Round(delta, 1);
        return rounded switch
        {
            > 0 => $"+{rounded:0.#}",
            < 0 => $"{rounded:0.#}",
            _ => "持平"
        };
    }

    private static string ResolveDisplayName(IdentityUser user)
    {
        var name = $"{user.Name}{user.Surname}".Trim();
        return string.IsNullOrWhiteSpace(name)
            ? user.UserName ?? string.Empty
            : name;
    }

    private async Task<string> BuildMonthlyBusinessSummaryAsync(Guid projectId, DateTime periodStart, DateTime periodEnd)
    {
        if (!CanViewSensitiveBusinessSummary())
        {
            return "经营摘要已按权限脱敏。当前账号可查看任务、里程碑、风险和工时摘要，如需查看合同、采购、预算、回款、开票金额，请联系 PM/PMO 或经营查看角色。";
        }

        var contracts = (await _businessContractRepository.GetListAsync(item => item.ProjectId == projectId))
            .Where(item => item.IsRevenueContract)
            .ToList();
        var procurements = await _procurementRepository.GetListAsync(item => item.ProjectId == projectId);
        var receivables = await _receivableRepository.GetListAsync(item => item.ProjectId == projectId);
        var invoices = await _invoiceRepository.GetListAsync(item => item.ProjectId == projectId);
        var budgets = await _budgetExecutionRepository.GetListAsync(item => item.ProjectId == projectId);

        var signedContracts = contracts
            .Where(item => item.SignDate.Date >= periodStart.Date && item.SignDate.Date <= periodEnd.Date)
            .ToList();
        var signedProcurements = procurements
            .Where(item => item.SignDate.Date >= periodStart.Date && item.SignDate.Date <= periodEnd.Date)
            .ToList();
        var plannedReceivables = receivables
            .Where(item => item.PlannedDate.Date >= periodStart.Date && item.PlannedDate.Date <= periodEnd.Date)
            .ToList();
        var issuedInvoices = invoices
            .Where(item => item.InvoiceDate.Date >= periodStart.Date && item.InvoiceDate.Date <= periodEnd.Date)
            .ToList();

        var totalContractAmount = contracts.Sum(item => item.Amount);
        var totalProcurementAmount = procurements.Sum(item => item.Amount);
        var totalReceivedAmount = receivables.Sum(item => item.ReceivedAmount);
        var totalReceivableAmount = receivables.Sum(item => item.TotalAmount);
        var totalInvoiceAmount = invoices.Sum(item => item.Amount);
        var totalBudgetAmount = budgets.Sum(item => item.AdjustedAmount);
        var totalExecutedBudget = budgets.Sum(item => item.ExecutedAmount);
        var grossProfitAmount = totalContractAmount - totalProcurementAmount - totalExecutedBudget;
        var grossProfitRate = totalContractAmount == 0
            ? 0
            : Math.Round(grossProfitAmount / totalContractAmount * 100, 2);

        return string.Join(Environment.NewLine, new[]
        {
            $"本月新增收入合同 {signedContracts.Count} 个，签约金额 {FormatAmount(signedContracts.Sum(item => item.Amount))}。",
            $"本月新增采购 {signedProcurements.Count} 个，采购金额 {FormatAmount(signedProcurements.Sum(item => item.Amount))}。",
            $"本月计划回款 {FormatAmount(plannedReceivables.Sum(item => item.TotalAmount))}，已回款 {FormatAmount(plannedReceivables.Sum(item => item.ReceivedAmount))}。",
            $"本月开票 {issuedInvoices.Count} 笔，开票金额 {FormatAmount(issuedInvoices.Sum(item => item.Amount))}。",
            $"累计收入合同 {FormatAmount(totalContractAmount)}，累计采购 {FormatAmount(totalProcurementAmount)}，累计已回款 {FormatAmount(totalReceivedAmount)} / 应回款 {FormatAmount(totalReceivableAmount)}。",
            $"预算执行 {FormatAmount(totalExecutedBudget)} / {FormatAmount(totalBudgetAmount)}，估算项目毛利 {FormatAmount(grossProfitAmount)}，毛利率 {grossProfitRate:0.##}% 。"
        });
    }

    private bool CanViewSensitiveBusinessSummary()
    {
        return _currentUser.IsInRole(PlatformRoleNames.Admin)
            || _currentUser.IsInRole(PlatformRoleNames.Pmo)
            || _currentUser.IsInRole(PlatformRoleNames.Viewer)
            || _currentUser.IsInRole(PlatformRoleNames.Pm);
    }

    private static string FormatAmount(decimal amount)
    {
        return $"{amount:N2} 元";
    }

    private async Task ValidateMeetingInputAsync(Guid projectId, ProjectMeetingRecordSaveInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Topic))
        {
            throw new BusinessException("会议主题不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Status) || !MeetingStatuses.Contains(input.Status.Trim()))
        {
            throw new BusinessException("会议记录状态不支持。");
        }

        await EnsureUserExistsAsync(input.FacilitatorUserId, "主持人不存在。");
        foreach (var participantUserId in input.ParticipantUserIds.Distinct())
        {
            await EnsureUserExistsAsync(participantUserId, "参会人不存在。");
        }

        if (input.AttachmentFileId.HasValue)
        {
            await EnsureFileBelongsToProjectAsync(projectId, input.AttachmentFileId.Value);
        }

        foreach (var actionItem in input.ActionItems)
        {
            await ValidateActionItemAsync(projectId, actionItem);
        }
    }

    private async Task ValidateReportInputAsync(Guid projectId, ProjectReportSaveInput input)
    {
        if (string.IsNullOrWhiteSpace(input.ReportType) || !ReportTypes.Contains(input.ReportType.Trim()))
        {
            throw new BusinessException("报告类型不支持。");
        }

        if (string.IsNullOrWhiteSpace(input.Status) || !ReportStatuses.Contains(input.Status.Trim()))
        {
            throw new BusinessException("报告状态不支持。");
        }

        if (input.PeriodStart.HasValue && input.PeriodEnd.HasValue && input.PeriodStart.Value.Date > input.PeriodEnd.Value.Date)
        {
            throw new BusinessException("报告开始时间不能晚于结束时间。");
        }

        await EnsureUserExistsAsync(input.AuthorUserId, "报告创建人不存在。");
        if (input.SprintId.HasValue && !await _projectSprintRepository.AnyAsync(item => item.Id == input.SprintId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("报告关联 Sprint 必须属于当前项目。");
        }

        if (input.AttachmentFileId.HasValue)
        {
            await EnsureFileBelongsToProjectAsync(projectId, input.AttachmentFileId.Value);
        }
    }

    private async Task ValidateActionItemAsync(Guid projectId, ProjectMeetingActionItemSaveInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new BusinessException("会议行动项标题不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.ActionType) || !ActionTypes.Contains(input.ActionType.Trim()))
        {
            throw new BusinessException("会议行动项类型不支持。");
        }

        if (string.IsNullOrWhiteSpace(input.Status) || !ActionStatuses.Contains(input.Status.Trim()))
        {
            throw new BusinessException("会议行动项状态不支持。");
        }

        if (input.AssigneeUserId.HasValue)
        {
            await EnsureUserExistsAsync(input.AssigneeUserId.Value, "会议行动项负责人不存在。");
        }

        if (input.ActionType.Trim() == "转任务")
        {
            if (!input.AssigneeUserId.HasValue)
            {
                throw new BusinessException("转任务行动项必须指定负责人。");
            }

            if (input.LinkedTaskId.HasValue)
            {
                throw new BusinessException("转任务行动项不能同时关联已有任务。");
            }

            if (input.EstimatedWorkHours.HasValue && input.EstimatedWorkHours.Value < 0)
            {
                throw new BusinessException("转任务预计工时不能为负数。");
            }

            if (!string.IsNullOrWhiteSpace(input.Priority) && !IsSupportedTaskPriority(input.Priority))
            {
                throw new BusinessException("转任务优先级不支持。");
            }
        }

        if (input.ActionType.Trim() == "生成需求")
        {
            if (input.LinkedTaskId.HasValue
                || input.LinkedRaidItemId.HasValue
                || input.LinkedIssueId.HasValue
                || input.LinkedRequirementId.HasValue)
            {
                throw new BusinessException("生成需求的行动项不能同时关联已有任务、风险、问题或需求。");
            }

            if (!string.IsNullOrWhiteSpace(input.Priority) && !ProjectPriorities.IsSupported(input.Priority))
            {
                throw new BusinessException("生成需求的优先级不支持。");
            }
        }

        if (input.ActionType.Trim() is "生成风险" or "生成问题")
        {
            if (!input.AssigneeUserId.HasValue)
            {
                throw new BusinessException("生成风险或问题的行动项必须指定负责人。");
            }

            if (input.LinkedRaidItemId.HasValue
                || input.LinkedIssueId.HasValue
                || input.LinkedRequirementId.HasValue
                || input.LinkedTaskId.HasValue)
            {
                throw new BusinessException("生成风险或问题的行动项不能同时关联已有任务、需求、风险或问题。");
            }

            if (!string.IsNullOrWhiteSpace(input.Priority) && !ProjectPriorities.IsSupported(input.Priority))
            {
                throw new BusinessException("生成风险或问题的级别不支持。");
            }
        }

        if (input.LinkedTaskId.HasValue && !await _projectTaskRepository.AnyAsync(item => item.Id == input.LinkedTaskId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("会议行动项关联任务必须属于当前项目。");
        }

        if (input.LinkedRaidItemId.HasValue && !await _projectRaidItemRepository.AnyAsync(item => item.Id == input.LinkedRaidItemId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("会议行动项关联风险必须属于当前项目。");
        }

        if (input.LinkedIssueId.HasValue && !await _projectIssueRepository.AnyAsync(item => item.Id == input.LinkedIssueId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("会议行动项关联问题必须属于当前项目。");
        }

        if (input.LinkedRequirementId.HasValue && !await _projectRequirementRepository.AnyAsync(item => item.Id == input.LinkedRequirementId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("会议行动项关联需求必须属于当前项目。");
        }
    }

    private async Task<Guid> CreateTaskFromActionItemAsync(Guid projectId, ProjectMeetingActionItemSaveInput input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        var projectTasks = await _projectTaskRepository.GetListAsync(item => item.ProjectId == projectId);
        var taskCode = string.IsNullOrWhiteSpace(input.TaskCode)
            ? ResolveNextTaskCode(projectTasks)
            : input.TaskCode.Trim();
        if (projectTasks.Any(item => item.TaskCode.Equals(taskCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("转任务编号在当前项目中已存在。");
        }

        var task = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            project.OrganizationUnitId,
            taskCode,
            input.Title.Trim(),
            input.AssigneeUserId!.Value,
            ProjectStatuses.InProgress,
            NormalizeTaskPriority(input.Priority),
            plannedStartTime: null,
            plannedEndTime: input.DueDate,
            estimatedWorkHours: input.EstimatedWorkHours);

        await _projectTaskRepository.InsertAsync(task, autoSave: true);
        return task.Id;
    }

    private async Task<Guid> CreateRiskFromActionItemAsync(Guid projectId, ProjectMeetingActionItemSaveInput input)
    {
        var risk = new ProjectRaidItem(
            Guid.NewGuid(),
            projectId,
            ProjectRaidTypes.Risk,
            input.Title.Trim(),
            ResolvePriority(input.Priority),
            input.AssigneeUserId!.Value,
            ProjectStatuses.InProgress,
            input.DueDate,
            probability: 3,
            impact: ResolveRiskImpact(input.Priority),
            responseStrategy: string.IsNullOrWhiteSpace(input.GeneratedRiskResponseStrategy)
                ? "来源会议行动项，请补充应对策略。"
                : input.GeneratedRiskResponseStrategy.Trim(),
            triggerCondition: "会议行动项识别",
            contingencyPlan: string.Empty);

        await _projectRaidItemRepository.InsertAsync(risk, autoSave: true);
        return risk.Id;
    }

    private async Task<Guid> CreateIssueFromActionItemAsync(Guid projectId, ProjectMeetingActionItemSaveInput input)
    {
        var issueType = string.IsNullOrWhiteSpace(input.GeneratedIssueType)
            ? "执行问题"
            : input.GeneratedIssueType.Trim();
        var issue = new ProjectIssue(
            Guid.NewGuid(),
            projectId,
            issueType,
            input.Title.Trim(),
            ResolvePriority(input.Priority),
            input.AssigneeUserId!.Value,
            "待处理",
            input.DueDate,
            requirementTitle: "来源会议行动项");

        await _projectIssueRepository.InsertAsync(issue, autoSave: true);
        return issue.Id;
    }

    private async Task<Guid> CreateRequirementFromActionItemAsync(Guid projectId, ProjectMeetingActionItemSaveInput input)
    {
        var projectRequirements = await _projectRequirementRepository.GetListAsync(item => item.ProjectId == projectId);
        var requirement = new ProjectRequirement(
            Guid.NewGuid(),
            projectId,
            ResolveNextRequirementCode(projectRequirements),
            input.Title.Trim(),
            "来源会议行动项，请补充需求描述。",
            "草稿",
            1,
            ResolvePriority(input.Priority),
            "会议行动项",
            "功能",
            acceptanceCriteria: "待补充",
            productOwnerUserId: input.AssigneeUserId,
            isCurrent: true,
            changeReason: "会议行动项生成",
            impactSummary: input.DueDate.HasValue
                ? $"会议行动项要求完成日期：{input.DueDate.Value:yyyy-MM-dd}"
                : "会议行动项生成");

        await _projectRequirementRepository.InsertAsync(requirement, autoSave: true);
        return requirement.Id;
    }

    private static string ResolveNextTaskCode(IReadOnlyCollection<ProjectTask> projectTasks)
    {
        var maxNumber = projectTasks
            .Select(item => item.TaskCode)
            .Where(item => item.StartsWith("TASK-", StringComparison.OrdinalIgnoreCase))
            .Select(item => int.TryParse(item["TASK-".Length..], out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"TASK-{maxNumber + 1:000}";
    }

    private static string ResolveNextRequirementCode(IReadOnlyCollection<ProjectRequirement> projectRequirements)
    {
        var maxNumber = projectRequirements
            .Select(item => item.RequirementCode)
            .Where(item => item.StartsWith("REQ-", StringComparison.OrdinalIgnoreCase))
            .Select(item => int.TryParse(item["REQ-".Length..], out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"REQ-{maxNumber + 1:000}";
    }

    private static string ResolvePriority(string? priority)
    {
        return ProjectPriorities.Normalize(priority);
    }

    private static bool IsSupportedTaskPriority(string? priority)
    {
        return ProjectPriorities.IsSupported(priority);
    }

    private static string NormalizeTaskPriority(string? priority)
    {
        return ProjectPriorities.Normalize(priority);
    }

    private static int ResolveRiskImpact(string? priority)
    {
        return ProjectPriorities.ResolveRiskImpact(priority);
    }

    private async Task EnsureUserExistsAsync(Guid userId, string message)
    {
        if (!await _userRepository.AnyAsync(item => item.Id == userId))
        {
            throw new BusinessException(message);
        }
    }

    private async Task EnsureFileBelongsToProjectAsync(Guid projectId, Guid fileId)
    {
        var file = await _fileRepository.FindAsync(fileId);
        if (file == null)
        {
            throw new BusinessException("附件文件不存在。");
        }

        if (file.BusinessType == "Project" && file.BusinessId == projectId.ToString())
        {
            return;
        }

        if (file.BusinessType == "ProjectReport"
            && Guid.TryParse(file.BusinessId, out var reportId)
            && await _projectReportRepository.AnyAsync(item => item.Id == reportId && item.ProjectId == projectId))
        {
            return;
        }

        if (file.BusinessType == "ProjectMeetingRecord"
            && Guid.TryParse(file.BusinessId, out var meetingRecordId)
            && await _projectMeetingRecordRepository.AnyAsync(item => item.Id == meetingRecordId && item.ProjectId == projectId))
        {
            return;
        }

        throw new BusinessException("附件文件必须属于当前项目。");
    }

    private static (DateTime PeriodStart, DateTime PeriodEnd) ResolveReportPeriod(
        string reportType,
        DateTime? inputStart,
        DateTime? inputEnd)
    {
        if (inputStart.HasValue && inputEnd.HasValue)
        {
            return (inputStart.Value.Date, inputEnd.Value.Date);
        }

        var today = DateTime.Today;
        return reportType.Trim() switch
        {
            "日报" => (today, today),
            "月报" => (new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1)),
            "阶段报告" or "项目总结" => (inputStart?.Date ?? today.AddDays(-30), inputEnd?.Date ?? today),
            _ => (GetWeekStart(today), GetWeekStart(today).AddDays(6))
        };
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = ((int)date.DayOfWeek + 6) % 7;
        return date.Date.AddDays(-diff);
    }

    private static string ResolveReportTitle(string reportType, DateTime periodStart, DateTime periodEnd)
    {
        return reportType.Trim() switch
        {
            "日报" => $"{periodStart:yyyy-MM-dd} 日报",
            "月报" => $"{periodStart:yyyy年MM月} 月报",
            _ => $"{periodStart:yyyy-MM-dd} 至 {periodEnd:yyyy-MM-dd} {reportType.Trim()}"
        };
    }

    private static bool IsInRange(DateTime? date, DateTime periodStart, DateTime periodEnd)
    {
        return date.HasValue && date.Value.Date >= periodStart.Date && date.Value.Date <= periodEnd.Date;
    }
}
