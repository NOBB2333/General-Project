using System.Text.Json;
using System.Threading;
using General.Admin.Business;
using General.Admin.Integration;
using General.Admin.Project;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Platform;

public class BusinessReminderNotificationJobHandler : IPlatformScheduledJobHandler
{
    private const int MaxDispatchItems = 100;

    private readonly IRepository<BusinessAcceptance, Guid> _acceptanceRepository;
    private readonly IRepository<BusinessBudgetExecution, Guid> _budgetExecutionRepository;
    private readonly IRepository<BusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<BusinessContract, Guid> _contractRepository;
    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;
    private readonly IRepository<BusinessForecastHistory, Guid> _forecastHistoryRepository;
    private readonly IRepository<BusinessInvoice, Guid> _invoiceRepository;
    private readonly IntegrationNotificationService _notificationService;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<ProjectPerformanceSnapshot, Guid> _performanceSnapshotRepository;
    private readonly IRepository<BusinessProcurement, Guid> _procurementRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<BusinessReceivable, Guid> _receivableRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly BusinessWarningBuilder _warningBuilder;

    public BusinessReminderNotificationJobHandler(
        IRepository<BusinessAcceptance, Guid> acceptanceRepository,
        IRepository<BusinessBudgetExecution, Guid> budgetExecutionRepository,
        IRepository<BusinessChain, Guid> businessChainRepository,
        IRepository<BusinessContract, Guid> contractRepository,
        PlatformSchedulerExecutionLogWriter executionLogWriter,
        IRepository<BusinessForecastHistory, Guid> forecastHistoryRepository,
        IRepository<BusinessInvoice, Guid> invoiceRepository,
        IntegrationNotificationService notificationService,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<ProjectPerformanceSnapshot, Guid> performanceSnapshotRepository,
        IRepository<BusinessProcurement, Guid> procurementRepository,
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<BusinessReceivable, Guid> receivableRepository,
        IRepository<IdentityUser, Guid> userRepository,
        BusinessWarningBuilder warningBuilder)
    {
        _acceptanceRepository = acceptanceRepository;
        _budgetExecutionRepository = budgetExecutionRepository;
        _businessChainRepository = businessChainRepository;
        _contractRepository = contractRepository;
        _executionLogWriter = executionLogWriter;
        _forecastHistoryRepository = forecastHistoryRepository;
        _invoiceRepository = invoiceRepository;
        _notificationService = notificationService;
        _organizationUnitRepository = organizationUnitRepository;
        _performanceSnapshotRepository = performanceSnapshotRepository;
        _procurementRepository = procurementRepository;
        _projectRepository = projectRepository;
        _receivableRepository = receivableRepository;
        _userRepository = userRepository;
        _warningBuilder = warningBuilder;
    }

    public string JobKey => "business-reminder-notification";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var projects = (await _projectRepository.GetListAsync(cancellationToken: cancellationToken))
            .ToDictionary(x => x.Id);
        var context = await BuildBusinessContextAsync(projects.Values.ToList(), cancellationToken);
        var summary = new BusinessReminderDispatchSummary();

        await DispatchReceivableRemindersAsync(projects, today, summary, cancellationToken);
        await DispatchContractClosingRemindersAsync(projects, summary, cancellationToken);
        await DispatchInvoiceExceptionRemindersAsync(projects, summary, cancellationToken);
        await DispatchBudgetVarianceRemindersAsync(projects, summary, cancellationToken);
        await DispatchBusinessWarningsAsync(context, summary);

        var result = $"经营提醒扫描完成：触发 {summary.EventCount} 个事件，规则投递成功 {summary.SentCount} 条，失败 {summary.FailedCount} 条，未匹配目标 {summary.NoTargetCount} 条，跳过 {summary.SkippedCount} 条。";
        await _executionLogWriter.WriteOperationAsync(JobKey, "经营提醒与预警推送", result, cancellationToken: cancellationToken);
        return $"{result} 可在日志中心的操作日志查看执行记录。";
    }

    private async Task DispatchReceivableRemindersAsync(
        IReadOnlyDictionary<Guid, ProjectEntity> projects,
        DateTime today,
        BusinessReminderDispatchSummary summary,
        CancellationToken cancellationToken)
    {
        var receivables = (await _receivableRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(IsReceivableOpen)
            .ToList();
        foreach (var receivable in receivables
                     .Where(x => x.PlannedDate.Date >= today && x.PlannedDate.Date <= today.AddDays(7))
                     .OrderBy(x => x.PlannedDate)
                     .Take(MaxDispatchItems))
        {
            if (projects.TryGetValue(receivable.ProjectId, out var project))
            {
                await DispatchReceivableAsync("business.receivable.due", project, receivable, today, summary);
            }
        }

        foreach (var receivable in receivables
                     .Where(x => x.PlannedDate.Date < today)
                     .OrderBy(x => x.PlannedDate)
                     .Take(MaxDispatchItems))
        {
            if (projects.TryGetValue(receivable.ProjectId, out var project))
            {
                await DispatchReceivableAsync("business.receivable.overdue", project, receivable, today, summary);
            }
        }
    }

    private async Task DispatchContractClosingRemindersAsync(
        IReadOnlyDictionary<Guid, ProjectEntity> projects,
        BusinessReminderDispatchSummary summary,
        CancellationToken cancellationToken)
    {
        var contracts = (await _contractRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(x => x.IsRevenueContract)
            .Where(x => !IsBusinessClosedStatus(x.Status))
            .Where(x => projects.TryGetValue(x.ProjectId, out var project) && ProjectStatuses.IsClosed(project.Status))
            .OrderBy(x => x.SignDate)
            .Take(MaxDispatchItems)
            .ToList();
        foreach (var contract in contracts)
        {
            if (projects.TryGetValue(contract.ProjectId, out var project))
            {
                var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
                {
                    EventCode = "business.contract.closing",
                    OccurredAt = DateTime.UtcNow,
                    VariablesJson = JsonSerializer.Serialize(new
                    {
                        business = new { key = contract.Id.ToString() },
                        contract = new
                        {
                            id = contract.Id,
                            code = contract.ContractCode,
                            title = contract.Title,
                            status = contract.Status,
                            signDate = contract.SignDate.ToString("yyyy-MM-dd")
                        },
                        project = BuildProjectVariable(project),
                        recipient = new { userIds = new[] { project.ManagerUserId } }
                    })
                });
                summary.Add(result);
            }
        }
    }

    private async Task DispatchInvoiceExceptionRemindersAsync(
        IReadOnlyDictionary<Guid, ProjectEntity> projects,
        BusinessReminderDispatchSummary summary,
        CancellationToken cancellationToken)
    {
        var invoices = (await _invoiceRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(x => IsInvoiceExceptionStatus(x.Status))
            .OrderByDescending(x => x.InvoiceDate)
            .Take(MaxDispatchItems)
            .ToList();
        foreach (var invoice in invoices)
        {
            if (projects.TryGetValue(invoice.ProjectId, out var project))
            {
                var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
                {
                    EventCode = "business.invoice.exception",
                    OccurredAt = DateTime.UtcNow,
                    VariablesJson = JsonSerializer.Serialize(new
                    {
                        business = new { key = invoice.Id.ToString() },
                        invoice = new
                        {
                            id = invoice.Id,
                            code = invoice.InvoiceCode,
                            title = invoice.Title,
                            status = invoice.Status,
                            invoiceDate = invoice.InvoiceDate.ToString("yyyy-MM-dd")
                        },
                        project = BuildProjectVariable(project),
                        recipient = new { userIds = new[] { project.ManagerUserId } }
                    })
                });
                summary.Add(result);
            }
        }
    }

    private async Task DispatchBudgetVarianceRemindersAsync(
        IReadOnlyDictionary<Guid, ProjectEntity> projects,
        BusinessReminderDispatchSummary summary,
        CancellationToken cancellationToken)
    {
        var budgetExecutions = (await _budgetExecutionRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(IsBudgetVarianceWarning)
            .OrderBy(x => x.ProjectId)
            .ThenBy(x => x.SortOrder)
            .Take(MaxDispatchItems)
            .ToList();
        foreach (var budget in budgetExecutions)
        {
            if (projects.TryGetValue(budget.ProjectId, out var project))
            {
                var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
                {
                    EventCode = "business.budget.variance",
                    OccurredAt = DateTime.UtcNow,
                    VariablesJson = JsonSerializer.Serialize(new
                    {
                        budget = new
                        {
                            id = budget.Id,
                            code = budget.BudgetCode,
                            category = budget.Category,
                            varianceDirection = budget.VarianceAmount < 0 ? "超支" : "结余偏差"
                        },
                        business = new { key = budget.Id.ToString() },
                        project = BuildProjectVariable(project),
                        recipient = new { userIds = new[] { project.ManagerUserId } }
                    })
                });
                summary.Add(result);
            }
        }
    }

    private async Task DispatchBusinessWarningsAsync(
        BusinessContext context,
        BusinessReminderDispatchSummary summary)
    {
        var summaries = context.Projects
            .Select(project => BusinessProjectionMapper.MapProjectSummary(project, context, new BusinessQueryAccess(true, true)))
            .ToList();
        var warnings = _warningBuilder.BuildWarningItems(summaries, context)
            .Take(MaxDispatchItems)
            .ToList();
        foreach (var warning in warnings)
        {
            var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
            {
                EventCode = "business.warning.detected",
                OccurredAt = DateTime.UtcNow,
                VariablesJson = JsonSerializer.Serialize(new
                {
                    business = new { key = warning.Id },
                    project = new
                    {
                        id = warning.ProjectId,
                        code = warning.ProjectCode,
                        detailUrl = $"/#/project/detail?projectId={warning.ProjectId}",
                        name = warning.ProjectName,
                        managerUserId = warning.OwnerUserId,
                        status = warning.Status
                    },
                    recipient = new { userIds = new[] { warning.OwnerUserId } },
                    warning = new
                    {
                        id = warning.Id,
                        type = warning.Type,
                        level = warning.Level,
                        description = warning.Description,
                        actionTab = warning.ActionTab
                    }
                })
            });
            summary.Add(result);
        }
    }

    private async Task<BusinessContext> BuildBusinessContextAsync(
        List<ProjectEntity> projects,
        CancellationToken cancellationToken)
    {
        var projectIds = projects.Select(x => x.Id).ToHashSet();
        var acceptances = (await _acceptanceRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var budgetExecutions = (await _budgetExecutionRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var businessChains = (await _businessChainRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var contracts = (await _contractRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var forecastHistories = (await _forecastHistoryRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var invoices = (await _invoiceRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var organizationUnits = await _organizationUnitRepository.GetListAsync(cancellationToken: cancellationToken);
        var performanceSnapshots = (await _performanceSnapshotRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var procurements = (await _procurementRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var receivables = (await _receivableRepository.GetListAsync(cancellationToken: cancellationToken)).Where(x => projectIds.Contains(x.ProjectId)).ToList();
        var users = await _userRepository.GetListAsync(cancellationToken: cancellationToken);
        return new BusinessContext(
            acceptances,
            budgetExecutions,
            businessChains,
            contracts,
            forecastHistories,
            invoices,
            organizationUnits.ToDictionary(x => x.Id, x => x.DisplayName),
            performanceSnapshots,
            procurements,
            projects.ToDictionary(x => x.Id, x => x.ProjectCode),
            projects.ToDictionary(x => x.Id, x => x.Name),
            projects,
            receivables,
            users.ToDictionary(x => x.Id, x => x.Name ?? x.UserName));
    }

    private async Task DispatchReceivableAsync(
        string eventCode,
        ProjectEntity project,
        BusinessReceivable receivable,
        DateTime today,
        BusinessReminderDispatchSummary summary)
    {
        var result = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
        {
            EventCode = eventCode,
            OccurredAt = DateTime.UtcNow,
            VariablesJson = JsonSerializer.Serialize(new
            {
                business = new { key = receivable.Id.ToString() },
                project = BuildProjectVariable(project),
                receivable = new
                {
                    id = receivable.Id,
                    code = receivable.ReceivableCode,
                    title = receivable.Title,
                    plannedDate = receivable.PlannedDate.ToString("yyyy-MM-dd"),
                    status = receivable.Status,
                    daysRemaining = Math.Max(0, (receivable.PlannedDate.Date - today).Days),
                    daysOverdue = Math.Max(0, (today - receivable.PlannedDate.Date).Days)
                },
                recipient = new { userIds = new[] { project.ManagerUserId } }
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

    private static bool IsReceivableOpen(BusinessReceivable item)
    {
        return item.ReceivedAmount < item.TotalAmount &&
               !item.Status.Contains("已回款", StringComparison.OrdinalIgnoreCase) &&
               !item.Status.Contains("已结清", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBusinessClosedStatus(string status)
    {
        return status.Contains("已关闭", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("已收尾", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("已归档", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("已完成", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInvoiceExceptionStatus(string status)
    {
        return status.Contains("异常", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("作废", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("退回", StringComparison.OrdinalIgnoreCase) ||
               status.Contains("冲红", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBudgetVarianceWarning(BusinessBudgetExecution item)
    {
        if (item.AdjustedAmount <= 0)
        {
            return item.ExecutedAmount > 0;
        }

        var varianceRatio = Math.Abs(item.VarianceAmount) / item.AdjustedAmount;
        return item.VarianceAmount < 0 || varianceRatio >= 0.1m;
    }

    private sealed class BusinessReminderDispatchSummary
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
