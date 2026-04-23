using Volo.Abp.Auditing;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.PhaseOne;

public class PhaseOneBusinessService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<PhaseOneBusinessBudgetExecution, Guid> _budgetExecutionRepository;
    private readonly IRepository<PhaseOneBusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<PhaseOneBusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<PhaseOneBusinessForecastHistory, Guid> _forecastHistoryRepository;
    private readonly IRepository<PhaseOneBusinessProcurement, Guid> _procurementRepository;
    private readonly IRepository<PhaseOneBusinessReceivable, Guid> _receivableRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<PhaseOneProject, Guid> _projectRepository;
    private readonly IRepository<PhaseOneProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PhaseOneBusinessService(
        ICurrentUser currentUser,
        CurrentUserDataScopeService dataScopeService,
        IRepository<PhaseOneBusinessBudgetExecution, Guid> budgetExecutionRepository,
        IRepository<PhaseOneBusinessChain, Guid> businessChainRepository,
        IRepository<PhaseOneBusinessContract, Guid> businessContractRepository,
        IRepository<PhaseOneBusinessForecastHistory, Guid> forecastHistoryRepository,
        IRepository<PhaseOneBusinessProcurement, Guid> procurementRepository,
        IRepository<PhaseOneBusinessReceivable, Guid> receivableRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<PhaseOneProject, Guid> projectRepository,
        IRepository<PhaseOneProjectMember, Guid> projectMemberRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _dataScopeService = dataScopeService;
        _budgetExecutionRepository = budgetExecutionRepository;
        _businessChainRepository = businessChainRepository;
        _businessContractRepository = businessContractRepository;
        _forecastHistoryRepository = forecastHistoryRepository;
        _procurementRepository = procurementRepository;
        _receivableRepository = receivableRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<PhaseOneBusinessOverviewDto> GetOverviewAsync()
    {
        var context = await BuildContextAsync();
        var canViewSensitiveAmounts = CanViewSensitiveAmounts();
        var canViewProfitAnalysis   = CanViewProfitAnalysis();
        var summaries = context.Projects.Select(project => MapProjectSummary(project, context, canViewSensitiveAmounts, canViewProfitAnalysis)).ToList();
        var warnings = BuildWarningItems(summaries, context);

        var contractAmount = summaries.Sum(item => item.ContractAmount ?? 0m);
        var costAmount = summaries.Sum(item => item.CostAmount ?? 0m);
        var procurementAmount = summaries.Sum(item => item.ProcurementAmount ?? 0m);
        var receivedAmount = summaries.Sum(item => item.ReceivedAmount ?? 0m);
        var yearEndReceivableForecast = summaries.Sum(item => item.YearEndReceivableForecast ?? 0m);
        var grossProfitAmount = summaries.Sum(item => item.GrossProfitAmount ?? 0m);
        var grossProfitRate = contractAmount <= 0 ? 0 : Math.Round(grossProfitAmount * 100m / contractAmount, 1);

        return new PhaseOneBusinessOverviewDto
        {
            CanViewProfitAnalysis = canViewProfitAnalysis,
            CanViewSensitiveAmounts = canViewSensitiveAmounts,
            ContractAmount = MaskAmount(contractAmount, canViewSensitiveAmounts),
            CostAmount = MaskAmount(costAmount, canViewSensitiveAmounts),
            FocusProjects = BuildFocusItems(warnings),
            GrossProfitAmount = MaskAmount(grossProfitAmount, canViewProfitAnalysis),
            GrossProfitRate = canViewProfitAnalysis ? grossProfitRate : null,
            LowProfitProjectCount = warnings.Count(item => item.Type == "利润风险"),
            ProcurementAmount = MaskAmount(procurementAmount, canViewSensitiveAmounts),
            Projects = summaries,
            ReceivableRiskProjectCount = summaries.Count(item => (item.YearEndReceivableForecast ?? 0) > (item.ReceivedAmount ?? 0)),
            ReceivedAmount = MaskAmount(receivedAmount, canViewSensitiveAmounts),
            TailClosingProjectCount = summaries.Count(item => item.BusinessCloseStatus.Contains("收尾", StringComparison.OrdinalIgnoreCase)
                || item.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase)),
            WarningItems = warnings.Take(6).ToList(),
            YearEndReceivableForecast = MaskAmount(yearEndReceivableForecast, canViewSensitiveAmounts)
        };
    }

    public async Task<List<PhaseOneBusinessProjectSummaryDto>> GetProjectListAsync(PhaseOneBusinessProjectListInput input)
    {
        var context = await BuildContextAsync();
        var keyword = input.Keyword?.Trim();
        var canViewSensitiveAmounts = CanViewSensitiveAmounts();
        var canViewProfitAnalysis   = CanViewProfitAnalysis();

        return context.Projects
            .Select(project => MapProjectSummary(project, context, canViewSensitiveAmounts, canViewProfitAnalysis))
            .Where(item => string.IsNullOrWhiteSpace(keyword)
                || item.ProjectCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.CustomerName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.ManagerName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.YearEndReceivableForecast ?? 0m)
            .ThenBy(item => item.ProjectCode)
            .ToList();
    }

    public async Task<PhaseOneBusinessProjectDetailDto> GetProjectDetailAsync(Guid projectId)
    {
        var context = await BuildContextAsync();
        var project = context.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project == null)
        {
            throw new AbpAuthorizationException("项目不存在或当前用户无权查看经营信息。");
        }

        var canViewSensitiveAmounts = CanViewSensitiveAmounts();
        var canViewProfitAnalysis = CanViewProfitAnalysis();

        return new PhaseOneBusinessProjectDetailDto
        {
            BudgetExecutions = context.BudgetExecutions
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.SortOrder)
                .Select(item => new PhaseOneBusinessBudgetExecutionItemDto
                {
                    AdjustedAmount = MaskAmount(item.AdjustedAmount, canViewSensitiveAmounts),
                    BudgetCode = item.BudgetCode,
                    Category = item.Category,
                    ExecutedAmount = MaskAmount(item.ExecutedAmount, canViewSensitiveAmounts),
                    Id = item.Id,
                    VarianceAmount = MaskAmount(item.VarianceAmount, canViewSensitiveAmounts),
                })
                .ToList(),
            BusinessChains = context.BusinessChains
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.Stage)
                .ThenBy(item => item.ChainCode)
                .Select(item => new PhaseOneBusinessChainItemDto
                {
                    ChainCode = item.ChainCode,
                    Id = item.Id,
                    LinkedContractCode = item.LinkedContractCode,
                    OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
                    SourceChangeCode = item.SourceChangeCode,
                    Stage = item.Stage,
                    Status = item.Status,
                    Summary = item.Summary,
                    Title = item.Title,
                    Type = item.Type
                })
                .ToList(),
            CanViewProfitAnalysis = canViewProfitAnalysis,
            CanViewSensitiveAmounts = canViewSensitiveAmounts,
            Contracts = context.Contracts
                .Where(item => item.ProjectId == projectId && item.IsRevenueContract)
                .OrderBy(item => item.SignDate)
                .ThenBy(item => item.ContractCode)
                .Select(item => MapContract(item, context, canViewSensitiveAmounts))
                .ToList(),
            ForecastHistories = context.ForecastHistories
                .Where(item => item.ProjectId == projectId)
                .OrderByDescending(item => item.CreationTime)
                .Select(item => new PhaseOneBusinessForecastHistoryItemDto
                {
                    ChangeType = item.ChangeType,
                    ChangedAt = item.CreationTime.ToString("yyyy-MM-dd HH:mm"),
                    ChangedBy = context.UserNameMap.GetValueOrDefault(item.ChangedByUserId) ?? "-",
                    Id = item.Id,
                    Metric = item.Metric,
                    NewValue = item.NewValue,
                    OldValue = item.OldValue,
                    Reason = item.Reason,
                    RelatedCode = item.RelatedCode
                })
                .ToList(),
            Procurements = context.Procurements
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.SignDate)
                .Select(item => new PhaseOneBusinessProcurementItemDto
                {
                    Amount = MaskAmount(item.Amount, canViewSensitiveAmounts),
                    Id = item.Id,
                    LinkedContractCode = item.LinkedContractCode,
                    ProcurementCode = item.ProcurementCode,
                    ProjectId = item.ProjectId,
                    ProjectName = project.Name,
                    SignDate = item.SignDate,
                    SourceChangeCode = item.SourceChangeCode,
                    Stage = item.Stage,
                    Status = item.Status,
                    SupplierName = item.SupplierName,
                    Title = item.Title
                })
                .ToList(),
            Receivables = context.Receivables
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.PlannedDate)
                .Select(item => new PhaseOneBusinessReceivableItemDto
                {
                    Id = item.Id,
                    InvoiceCode = item.InvoiceCode,
                    LinkedContractCode = item.LinkedContractCode,
                    PlannedDate = item.PlannedDate,
                    ProjectId = item.ProjectId,
                    ProjectName = project.Name,
                    ReceivedAmount = MaskAmount(item.ReceivedAmount, canViewSensitiveAmounts),
                    ReceivableCode = item.ReceivableCode,
                    Status = item.Status,
                    Title = item.Title,
                    TotalAmount = MaskAmount(item.TotalAmount, canViewSensitiveAmounts)
                })
                .ToList(),
            Summary = MapProjectSummary(project, context, canViewSensitiveAmounts, canViewProfitAnalysis)
        };
    }

    public async Task<PhaseOneBusinessReportDto> GetReportAsync(PhaseOneBusinessReportQueryInput input)
    {
        var context = await BuildContextAsync();
        var keyword = input.Keyword?.Trim();
        var projectCode = input.ProjectCode?.Trim();
        var counterpartyName = input.CounterpartyName?.Trim();
        var supplierName = input.SupplierName?.Trim();
        var businessCloseStatus = input.BusinessCloseStatus?.Trim();
        var contractType = input.ContractType?.Trim();
        var receivableStatus = input.ReceivableStatus?.Trim();

        var canViewSensitiveAmounts = CanViewSensitiveAmounts();
        var canViewProfitAnalysis   = CanViewProfitAnalysis();

        var projectItems = context.Projects
            .Select(project => MapProjectSummary(project, context, canViewSensitiveAmounts, canViewProfitAnalysis))
            .Where(item => MatchKeyword(keyword, item.ProjectCode, item.Name, item.CustomerName, item.ManagerName))
            .Where(item => string.IsNullOrWhiteSpace(projectCode) || item.ProjectCode.Contains(projectCode, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(counterpartyName) || item.CustomerName.Contains(counterpartyName, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(businessCloseStatus) || item.BusinessCloseStatus.Equals(businessCloseStatus, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.ProjectCode)
            .ToList();
        var warningItems = BuildWarningItems(projectItems, context)
            .Where(item => MatchKeyword(keyword, item.ProjectCode, item.ProjectName, item.Description, item.Type, item.Status))
            .Where(item => string.IsNullOrWhiteSpace(projectCode) || item.ProjectCode.Contains(projectCode, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(businessCloseStatus) || item.Status.Equals(businessCloseStatus, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new PhaseOneBusinessReportDto
        {
            ContractItems = context.Contracts
                .Where(item => item.IsRevenueContract)
                .Where(item => MatchKeyword(keyword, item.ContractCode, item.Title, item.CounterpartyName, item.Type, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(counterpartyName) || item.CounterpartyName.Contains(counterpartyName, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(contractType) || item.Type.Equals(contractType, StringComparison.OrdinalIgnoreCase))
                .Where(item => !input.SignDateFrom.HasValue || item.SignDate.Date >= input.SignDateFrom.Value.Date)
                .Where(item => !input.SignDateTo.HasValue || item.SignDate.Date <= input.SignDateTo.Value.Date)
                .OrderByDescending(item => item.SignDate)
                .Select(item => MapContract(item, context, canViewSensitiveAmounts))
                .ToList(),
            ProcurementItems = context.Procurements
                .Where(item => MatchKeyword(keyword, item.ProcurementCode, item.Title, item.SupplierName, item.Stage, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(supplierName) || item.SupplierName.Contains(supplierName, StringComparison.OrdinalIgnoreCase))
                .Where(item => !input.SignDateFrom.HasValue || item.SignDate.Date >= input.SignDateFrom.Value.Date)
                .Where(item => !input.SignDateTo.HasValue || item.SignDate.Date <= input.SignDateTo.Value.Date)
                .OrderByDescending(item => item.SignDate)
                .Select(item => new PhaseOneBusinessProcurementItemDto
                {
                    Amount = MaskAmount(item.Amount, canViewSensitiveAmounts),
                    Id = item.Id,
                    LinkedContractCode = item.LinkedContractCode,
                    ProcurementCode = item.ProcurementCode,
                    ProjectId = item.ProjectId,
                    ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
                    SignDate = item.SignDate,
                    SourceChangeCode = item.SourceChangeCode,
                    Stage = item.Stage,
                    Status = item.Status,
                    SupplierName = item.SupplierName,
                    Title = item.Title
                })
                .ToList(),
            ProfitItems = projectItems.OrderBy(item => item.GrossProfitRate ?? decimal.MaxValue).ToList(),
            ProjectItems = projectItems,
            ReceivableItems = context.Receivables
                .Where(item => MatchKeyword(keyword, item.ReceivableCode, item.Title, item.Status, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(receivableStatus) || item.Status.Equals(receivableStatus, StringComparison.OrdinalIgnoreCase))
                .Where(item => !input.PlannedDateFrom.HasValue || item.PlannedDate.Date >= input.PlannedDateFrom.Value.Date)
                .Where(item => !input.PlannedDateTo.HasValue || item.PlannedDate.Date <= input.PlannedDateTo.Value.Date)
                .OrderBy(item => item.PlannedDate)
                .Select(item => new PhaseOneBusinessReceivableItemDto
                {
                    Id = item.Id,
                    InvoiceCode = item.InvoiceCode,
                    LinkedContractCode = item.LinkedContractCode,
                    PlannedDate = item.PlannedDate,
                    ProjectId = item.ProjectId,
                    ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
                    ReceivedAmount = MaskAmount(item.ReceivedAmount, canViewSensitiveAmounts),
                    ReceivableCode = item.ReceivableCode,
                    Status = item.Status,
                    Title = item.Title,
                    TotalAmount = MaskAmount(item.TotalAmount, canViewSensitiveAmounts)
                })
                .ToList(),
            WarningItems = warningItems
        };
    }

    private static List<PhaseOneBusinessFocusItemDto> BuildFocusItems(List<PhaseOneBusinessWarningItemDto> warnings)
    {
        return warnings
            .Take(4)
            .Select(item => new PhaseOneBusinessFocusItemDto
        {
            ActionTab = item.ActionTab,
            Description = item.Description,
            ProjectId = item.ProjectId,
            ProjectName = item.ProjectName,
            Type = item.Type
        })
            .ToList();
    }

    private List<PhaseOneBusinessWarningItemDto> BuildWarningItems(List<PhaseOneBusinessProjectSummaryDto> summaries, BusinessContext context)
    {
        var warnings = new List<PhaseOneBusinessWarningItemDto>();

        foreach (var summary in summaries)
        {
            if (!context.ProjectById.TryGetValue(summary.Id, out var project))
            {
                continue;
            }

            if (summary.GrossProfitRate.HasValue && summary.GrossProfitRate.Value < 25m)
            {
                warnings.Add(BuildWarningItem(
                    context,
                    project,
                    summary,
                    type: "利润风险",
                    level: summary.GrossProfitRate.Value < 15m ? PhaseOnePriorities.High : PhaseOnePriorities.Medium,
                    actionTab: "profit",
                    description: $"当前毛利率 {summary.GrossProfitRate.Value:0.0}% ，建议优先复核分包成本和补充合同变化。"));
            }

            if (summary.BusinessCloseStatus.Contains("收尾", StringComparison.OrdinalIgnoreCase)
                || summary.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add(BuildWarningItem(
                    context,
                    project,
                    summary,
                    type: "收尾风险",
                    level: summary.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase) ? PhaseOnePriorities.High : PhaseOnePriorities.Medium,
                    actionTab: "receivable",
                    description: "项目执行已接近结束，但经营收尾或尾款回收仍未闭环。"));
            }

            if ((summary.YearEndReceivableForecast ?? 0m) > (summary.ReceivedAmount ?? 0m))
            {
                warnings.Add(BuildWarningItem(
                    context,
                    project,
                    summary,
                    type: "回款风险",
                    level: summary.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase) ? PhaseOnePriorities.High : PhaseOnePriorities.Medium,
                    actionTab: "receivable",
                    description: $"预计回款 {FormatAmount(summary.YearEndReceivableForecast)}，当前已回款 {FormatAmount(summary.ReceivedAmount)}，请复核回款计划。"));
            }
        }

        return warnings
            .DistinctBy(item => new { item.ProjectId, item.Type })
            .OrderByDescending(item => ResolveWarningRank(item.Level))
            .ThenBy(item => item.ProjectCode)
            .ToList();
    }

    private PhaseOneBusinessWarningItemDto BuildWarningItem(
        BusinessContext context,
        PhaseOneProject project,
        PhaseOneBusinessProjectSummaryDto summary,
        string type,
        string level,
        string actionTab,
        string description)
    {
        return new PhaseOneBusinessWarningItemDto
        {
            ActionTab = actionTab,
            Description = description,
            Id = $"{project.Id:N}:{type}",
            LastUpdatedTime = context.LastUpdatedByProject.GetValueOrDefault(project.Id, DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm"),
            Level = level,
            OwnerName = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            OwnerUserId = project.ManagerUserId,
            ProjectCode = summary.ProjectCode,
            ProjectId = project.Id,
            ProjectName = project.Name,
            Status = summary.BusinessCloseStatus,
            Type = type
        };
    }

    private static int ResolveWarningRank(string level)
    {
        return level switch
        {
            var l when l == PhaseOnePriorities.High   => 3,
            var l when l == PhaseOnePriorities.Medium => 2,
            _ => 1
        };
    }

    private static string FormatAmount(decimal? amount)
    {
        return amount.HasValue ? amount.Value.ToString("0.##") : "--";
    }

    private sealed record BusinessContext(
        List<PhaseOneBusinessBudgetExecution>    BudgetExecutions,
        List<PhaseOneBusinessChain>              BusinessChains,
        List<PhaseOneBusinessContract>           Contracts,
        List<PhaseOneBusinessForecastHistory>    ForecastHistories,
        Dictionary<Guid, string>                 OrganizationUnitNameMap,
        List<PhaseOneBusinessProcurement>        Procurements,
        Dictionary<Guid, string>                 ProjectCodeMap,
        Dictionary<Guid, string>                 ProjectNameMap,
        List<PhaseOneProject>                    Projects,
        List<PhaseOneBusinessReceivable>         Receivables,
        Dictionary<Guid, string>                 UserNameMap)
    {
        // O(1) 单条项目查找
        public Dictionary<Guid, PhaseOneProject> ProjectById { get; } =
            Projects.ToDictionary(p => p.Id);

        // 预分组字典：O(1) 按项目取子集
        public Dictionary<Guid, List<PhaseOneBusinessContract>>        ContractsByProject        { get; } = Contracts.GroupBy(c => c.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<PhaseOneBusinessProcurement>>     ProcurementsByProject     { get; } = Procurements.GroupBy(p => p.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<PhaseOneBusinessReceivable>>      ReceivablesByProject      { get; } = Receivables.GroupBy(r => r.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<PhaseOneBusinessBudgetExecution>> BudgetExecutionsByProject { get; } = BudgetExecutions.GroupBy(b => b.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<PhaseOneBusinessChain>>           BusinessChainsByProject   { get; } = BusinessChains.GroupBy(b => b.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<PhaseOneBusinessForecastHistory>> ForecastHistoriesByProject { get; } = ForecastHistories.GroupBy(f => f.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

        // 预计算每个项目的最新操作时间，避免 BuildWarningItems 的 O(N×M) 扫描
        public Dictionary<Guid, DateTime> LastUpdatedByProject { get; } =
            ComputeLastUpdated(Projects, BudgetExecutions, BusinessChains, Contracts, ForecastHistories, Procurements, Receivables);

        private static Dictionary<Guid, DateTime> ComputeLastUpdated(
            List<PhaseOneProject> projects,
            List<PhaseOneBusinessBudgetExecution> budgetExecutions,
            List<PhaseOneBusinessChain> businessChains,
            List<PhaseOneBusinessContract> contracts,
            List<PhaseOneBusinessForecastHistory> forecastHistories,
            List<PhaseOneBusinessProcurement> procurements,
            List<PhaseOneBusinessReceivable> receivables)
        {
            var result = projects.ToDictionary(p => p.Id, p => ResolveEntityTimestamp(p));

            void MergeMax<T>(IEnumerable<T> items, Func<T, Guid> getId, Func<T, DateTime> getTime)
            {
                foreach (var item in items)
                {
                    var id = getId(item);
                    var t  = getTime(item);
                    if (!result.TryGetValue(id, out var existing) || t > existing)
                        result[id] = t;
                }
            }

            MergeMax(budgetExecutions,  i => i.ProjectId, i => ResolveEntityTimestamp(i));
            MergeMax(businessChains,    i => i.ProjectId, i => ResolveEntityTimestamp(i));
            MergeMax(contracts,         i => i.ProjectId, i => ResolveEntityTimestamp(i));
            MergeMax(forecastHistories, i => i.ProjectId, i => ResolveEntityTimestamp(i));
            MergeMax(procurements,      i => i.ProjectId, i => ResolveEntityTimestamp(i));
            MergeMax(receivables,       i => i.ProjectId, i => ResolveEntityTimestamp(i));

            return result;
        }

        private static DateTime ResolveEntityTimestamp<T>(T item)
            where T : IHasCreationTime, IHasModificationTime
            => item.LastModificationTime ?? item.CreationTime;
    }

    private async Task<BusinessContext> BuildContextAsync()
    {
        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();

        // 只查一次项目表（修复 #1 双重查询）
        var allProjects = await _projectRepository.GetListAsync();
        var projects = allProjects
            .Where(project => _currentUser.IsInRole(PhaseOneRoleNames.Admin) || accessibleOrganizationUnitIds.Contains(project.OrganizationUnitId))
            .ToList();

        if (_currentUser.Id.HasValue && !_currentUser.IsInRole(PhaseOneRoleNames.Admin))
        {
            var projectIdsByMembership = (await _projectMemberRepository.GetListAsync())
                .Where(item => item.UserId == _currentUser.Id.Value)
                .Select(item => item.ProjectId)
                .ToHashSet();

            projects = projects
                .Concat(allProjects.Where(item => projectIdsByMembership.Contains(item.Id)))
                .DistinctBy(item => item.Id)
                .ToList();
        }

        var projectIds = projects.Select(item => item.Id).ToHashSet();
        var users = await _userRepository.GetListAsync();
        var organizationUnits = await _organizationUnitRepository.GetListAsync();

        var budgetExecutions   = (await _budgetExecutionRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var businessChains     = (await _businessChainRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var contracts          = (await _businessContractRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var forecastHistories  = (await _forecastHistoryRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var procurements       = (await _procurementRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();
        var receivables        = (await _receivableRepository.GetListAsync()).Where(item => projectIds.Contains(item.ProjectId)).ToList();

        return new BusinessContext(
            budgetExecutions,
            businessChains,
            contracts,
            forecastHistories,
            organizationUnits.ToDictionary(item => item.Id, item => item.DisplayName),
            procurements,
            projects.ToDictionary(item => item.Id, item => item.ProjectCode),
            projects.ToDictionary(item => item.Id, item => item.Name),
            projects,
            receivables,
            users.ToDictionary(item => item.Id, item => item.Name ?? item.UserName));
    }

    private PhaseOneBusinessContractItemDto MapContract(PhaseOneBusinessContract item, BusinessContext context, bool canViewSensitiveAmounts)
    {
        return new PhaseOneBusinessContractItemDto
        {
            Amount = MaskAmount(item.Amount, canViewSensitiveAmounts),
            ContractCode = item.ContractCode,
            CounterpartyName = item.CounterpartyName,
            Id = item.Id,
            IsRevenueContract = item.IsRevenueContract,
            ParentContractCode = item.ParentContractCode,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            SignDate = item.SignDate,
            SourceChangeCode = item.SourceChangeCode,
            Status = item.Status,
            Title = item.Title,
            Type = item.Type
        };
    }

    private PhaseOneBusinessProjectSummaryDto MapProjectSummary(PhaseOneProject project, BusinessContext context, bool canViewSensitiveAmounts, bool canViewProfitAnalysis)
    {
        var revenueContracts = context.ContractsByProject.GetValueOrDefault(project.Id) is { } allContracts
            ? allContracts.Where(c => c.IsRevenueContract).ToList()
            : [];
        var procurements = context.ProcurementsByProject.GetValueOrDefault(project.Id) ?? [];
        var receivables  = context.ReceivablesByProject.GetValueOrDefault(project.Id) ?? [];
        var budgetExecs  = context.BudgetExecutionsByProject.GetValueOrDefault(project.Id) ?? [];

        var contractAmount            = revenueContracts.Sum(item => item.Amount);
        var procurementAmount         = procurements.Sum(item => item.Amount);
        var receivedAmount            = receivables.Sum(item => item.ReceivedAmount);
        var yearEndReceivableForecast = receivables.Sum(item => item.TotalAmount);
        var costAmount                = procurementAmount + budgetExecs.Sum(item => item.ExecutedAmount);
        var grossProfitAmount         = contractAmount - costAmount;
        var grossProfitRate           = contractAmount <= 0 ? 0m : Math.Round(grossProfitAmount * 100m / contractAmount, 1);

        return new PhaseOneBusinessProjectSummaryDto
        {
            BusinessCloseStatus       = ResolveBusinessCloseStatus(project, receivables),
            ContractAmount            = MaskAmount(contractAmount, canViewSensitiveAmounts),
            CostAmount                = MaskAmount(costAmount, canViewSensitiveAmounts),
            CustomerName              = revenueContracts.FirstOrDefault()?.CounterpartyName ?? project.ProjectSource ?? "-",
            GrossProfitAmount         = MaskAmount(grossProfitAmount, canViewProfitAnalysis),
            GrossProfitRate           = canViewProfitAnalysis ? grossProfitRate : null,
            Id                        = project.Id,
            InvoicedAmount            = MaskAmount(receivables.Sum(item => string.IsNullOrWhiteSpace(item.InvoiceCode) ? item.ReceivedAmount : item.TotalAmount), canViewSensitiveAmounts),
            ManagerName               = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            Name                      = project.Name,
            ProcurementAmount         = MaskAmount(procurementAmount, canViewSensitiveAmounts),
            ProjectCode               = project.ProjectCode,
            ReceivedAmount            = MaskAmount(receivedAmount, canViewSensitiveAmounts),
            Status                    = project.Status,
            YearEndReceivableForecast = MaskAmount(yearEndReceivableForecast, canViewSensitiveAmounts)
        };
    }

    private bool CanViewProfitAnalysis() => IsPrivilegedRole();

    private bool CanViewSensitiveAmounts() => IsPrivilegedRole();

    private bool IsPrivilegedRole()
    {
        return _currentUser.IsInRole(PhaseOneRoleNames.Admin)
            || _currentUser.IsInRole(PhaseOneRoleNames.Pmo)
            || _currentUser.IsInRole(PhaseOneRoleNames.Viewer)
            || _currentUser.IsInRole(PhaseOneRoleNames.Pm);
    }

    private static decimal? MaskAmount(decimal amount, bool canView)
    {
        return canView ? amount : null;
    }

    private static bool MatchKeyword(string? keyword, params string[] candidates)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        return candidates.Any(item => item.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolveBusinessCloseStatus(PhaseOneProject project, List<PhaseOneBusinessReceivable> receivables)
    {
        var totalReceivable = receivables.Sum(item => item.TotalAmount);
        var receivedAmount = receivables.Sum(item => item.ReceivedAmount);
        if (receivedAmount < totalReceivable && IsClosed(project.Status))
        {
            return "执行关闭待回款";
        }

        if (receivedAmount < totalReceivable)
        {
            return "经营收尾中";
        }

        return "经营执行中";
    }

    private static bool IsClosed(string status) => PhaseOneProjectStatuses.IsClosed(status);
}
