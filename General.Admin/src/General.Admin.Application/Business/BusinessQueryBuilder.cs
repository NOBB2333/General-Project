using Volo.Abp.DependencyInjection;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Business;

public class BusinessQueryBuilder : ITransientDependency
{
    private readonly BusinessWarningBuilder _warningBuilder;

    public BusinessQueryBuilder(BusinessWarningBuilder warningBuilder)
    {
        _warningBuilder = warningBuilder;
    }

    public BusinessOverviewDto BuildOverview(BusinessContext context, BusinessQueryAccess access)
    {
        var summaries = context.Projects
            .Select(project => BusinessProjectionMapper.MapProjectSummary(project, context, access))
            .ToList();
        var warnings = _warningBuilder.BuildWarningItems(summaries, context);

        var contractAmount = summaries.Sum(item => item.ContractAmount ?? 0m);
        var costAmount = summaries.Sum(item => item.CostAmount ?? 0m);
        var procurementAmount = summaries.Sum(item => item.ProcurementAmount ?? 0m);
        var receivedAmount = summaries.Sum(item => item.ReceivedAmount ?? 0m);
        var yearEndReceivableForecast = summaries.Sum(item => item.YearEndReceivableForecast ?? 0m);
        var grossProfitAmount = summaries.Sum(item => item.GrossProfitAmount ?? 0m);
        var grossProfitRate = contractAmount <= 0 ? 0 : Math.Round(grossProfitAmount * 100m / contractAmount, 1);

        return new BusinessOverviewDto
        {
            CanViewProfitAnalysis = access.CanViewProfitAnalysis,
            CanViewSensitiveAmounts = access.CanViewSensitiveAmounts,
            ContractAmount = BusinessProjectionMapper.MaskAmount(contractAmount, access.CanViewSensitiveAmounts),
            CostAmount = BusinessProjectionMapper.MaskAmount(costAmount, access.CanViewSensitiveAmounts),
            FocusProjects = _warningBuilder.BuildFocusItems(warnings),
            GrossProfitAmount = BusinessProjectionMapper.MaskAmount(grossProfitAmount, access.CanViewProfitAnalysis),
            GrossProfitRate = access.CanViewProfitAnalysis ? grossProfitRate : null,
            LowProfitProjectCount = warnings.Count(item => item.Type == "利润风险"),
            ProcurementAmount = BusinessProjectionMapper.MaskAmount(procurementAmount, access.CanViewSensitiveAmounts),
            Projects = summaries,
            ReceivableRiskProjectCount = summaries.Count(item => (item.YearEndReceivableForecast ?? 0) > (item.ReceivedAmount ?? 0)),
            ReceivedAmount = BusinessProjectionMapper.MaskAmount(receivedAmount, access.CanViewSensitiveAmounts),
            TailClosingProjectCount = summaries.Count(item => item.BusinessCloseStatus.Contains("收尾", StringComparison.OrdinalIgnoreCase)
                || item.BusinessCloseStatus.Contains("待回款", StringComparison.OrdinalIgnoreCase)),
            WarningItems = warnings.Take(6).ToList(),
            YearEndReceivableForecast = BusinessProjectionMapper.MaskAmount(yearEndReceivableForecast, access.CanViewSensitiveAmounts)
        };
    }

    public List<BusinessProjectSummaryDto> BuildProjectList(
        BusinessContext context,
        BusinessProjectListInput input,
        BusinessQueryAccess access)
    {
        var projectItems = context.Projects
            .Select(project => BusinessProjectionMapper.MapProjectSummary(project, context, access));

        return BusinessProjectFilter.Apply(projectItems, input.Keyword)
            .OrderByDescending(item => item.YearEndReceivableForecast ?? 0m)
            .ThenBy(item => item.ProjectCode)
            .ToList();
    }

    public BusinessProjectDetailDto BuildProjectDetail(
        BusinessContext context,
        ProjectEntity project,
        BusinessQueryAccess access)
    {
        var projectId = project.Id;

        return new BusinessProjectDetailDto
        {
            Acceptances = context.Acceptances
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.AcceptanceDate)
                .Select(item => BusinessProjectionMapper.MapAcceptance(item, context))
                .ToList(),
            BudgetExecutions = context.BudgetExecutions
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.SortOrder)
                .Select(item => BusinessProjectionMapper.MapBudgetExecution(item, context, access.CanViewSensitiveAmounts))
                .ToList(),
            BusinessChains = context.BusinessChains
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.Stage)
                .ThenBy(item => item.ChainCode)
                .Select(item => new BusinessChainItemDto
                {
                    ChainCode = item.ChainCode,
                    Id = item.Id,
                    LinkedContractCode = item.LinkedContractCode,
                    OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
                    OwnerUserId = item.OwnerUserId,
                    SourceChangeCode = item.SourceChangeCode,
                    Stage = item.Stage,
                    Status = item.Status,
                    Summary = item.Summary,
                    Title = item.Title,
                    Type = item.Type
                })
                .ToList(),
            CanViewProfitAnalysis = access.CanViewProfitAnalysis,
            CanViewSensitiveAmounts = access.CanViewSensitiveAmounts,
            Contracts = context.Contracts
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.SignDate)
                .ThenBy(item => item.ContractCode)
                .Select(item => BusinessProjectionMapper.MapContract(item, context, access.CanViewSensitiveAmounts))
                .ToList(),
            ForecastHistories = context.ForecastHistories
                .Where(item => item.ProjectId == projectId)
                .OrderByDescending(item => item.CreationTime)
                .Select(item => new BusinessForecastHistoryItemDto
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
            Invoices = context.Invoices
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.InvoiceDate)
                .Select(item => BusinessProjectionMapper.MapInvoice(item, context, access.CanViewSensitiveAmounts))
                .ToList(),
            Procurements = context.Procurements
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.SignDate)
                .Select(item => BusinessProjectionMapper.MapProcurement(item, context, access.CanViewSensitiveAmounts, project.Name))
                .ToList(),
            Receivables = context.Receivables
                .Where(item => item.ProjectId == projectId)
                .OrderBy(item => item.PlannedDate)
                .Select(item => BusinessProjectionMapper.MapReceivable(item, context, access.CanViewSensitiveAmounts, project.Name))
                .ToList(),
            Summary = BusinessProjectionMapper.MapProjectSummary(project, context, access)
        };
    }

}

public sealed record BusinessQueryAccess(
    bool CanViewProfitAnalysis,
    bool CanViewSensitiveAmounts);
