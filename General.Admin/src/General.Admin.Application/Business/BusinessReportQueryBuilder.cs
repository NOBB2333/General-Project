using Volo.Abp.DependencyInjection;

namespace General.Admin.Business;

public class BusinessReportQueryBuilder : ITransientDependency
{
    private readonly BusinessWarningBuilder _warningBuilder;

    public BusinessReportQueryBuilder(BusinessWarningBuilder warningBuilder)
    {
        _warningBuilder = warningBuilder;
    }

    public BusinessReportDto BuildReport(
        BusinessContext context,
        BusinessReportQueryInput input,
        BusinessQueryAccess access)
    {
        var keyword = input.Keyword?.Trim();
        var projectCode = input.ProjectCode?.Trim();
        var counterpartyName = input.CounterpartyName?.Trim();
        var supplierName = input.SupplierName?.Trim();
        var businessCloseStatus = input.BusinessCloseStatus?.Trim();
        var contractType = input.ContractType?.Trim();
        var receivableStatus = input.ReceivableStatus?.Trim();
        var warningLevel = input.WarningLevel?.Trim();

        var mappedProjectItems = context.Projects
            .Select(project => BusinessProjectionMapper.MapProjectSummary(project, context, access));
        var projectItems = BusinessProjectFilter.Apply(mappedProjectItems, keyword, projectCode, counterpartyName, businessCloseStatus)
            .OrderBy(item => item.ProjectCode)
            .ToList();
        var warningItems = _warningBuilder.BuildWarningItems(projectItems, context)
            .Where(item => BusinessProjectionMapper.MatchKeyword(keyword, item.ProjectCode, item.ProjectName, item.Description, item.Type, item.Status))
            .Where(item => string.IsNullOrWhiteSpace(projectCode) || item.ProjectCode.Contains(projectCode, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(businessCloseStatus) || item.Status.Equals(businessCloseStatus, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(warningLevel) || item.Level.Equals(warningLevel, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new BusinessReportDto
        {
            AcceptanceItems = context.Acceptances
                .Where(item => BusinessProjectionMapper.MatchKeyword(keyword, item.AcceptanceCode, item.Title, item.Status, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .Where(item => !input.PlannedDateFrom.HasValue || item.AcceptanceDate.Date >= input.PlannedDateFrom.Value.Date)
                .Where(item => !input.PlannedDateTo.HasValue || item.AcceptanceDate.Date <= input.PlannedDateTo.Value.Date)
                .OrderByDescending(item => item.AcceptanceDate)
                .Select(item => BusinessProjectionMapper.MapAcceptance(item, context))
                .ToList(),
            BudgetItems = context.BudgetExecutions
                .Where(item => BusinessProjectionMapper.MatchKeyword(keyword, item.BudgetCode, item.Category, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => Math.Abs(item.VarianceAmount))
                .ThenBy(item => item.SortOrder)
                .Select(item => BusinessProjectionMapper.MapBudgetExecution(item, context, access.CanViewSensitiveAmounts))
                .ToList(),
            ContractItems = context.Contracts
                .Where(item => item.IsRevenueContract)
                .Where(item => BusinessProjectionMapper.MatchKeyword(keyword, item.ContractCode, item.Title, item.CounterpartyName, item.Type, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(counterpartyName) || item.CounterpartyName.Contains(counterpartyName, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(contractType) || item.Type.Equals(contractType, StringComparison.OrdinalIgnoreCase))
                .Where(item => !input.SignDateFrom.HasValue || item.SignDate.Date >= input.SignDateFrom.Value.Date)
                .Where(item => !input.SignDateTo.HasValue || item.SignDate.Date <= input.SignDateTo.Value.Date)
                .OrderByDescending(item => item.SignDate)
                .Select(item => BusinessProjectionMapper.MapContract(item, context, access.CanViewSensitiveAmounts))
                .ToList(),
            InvoiceItems = context.Invoices
                .Where(item => BusinessProjectionMapper.MatchKeyword(keyword, item.InvoiceCode, item.Title, item.Status, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .Where(item => !input.PlannedDateFrom.HasValue || item.InvoiceDate.Date >= input.PlannedDateFrom.Value.Date)
                .Where(item => !input.PlannedDateTo.HasValue || item.InvoiceDate.Date <= input.PlannedDateTo.Value.Date)
                .OrderByDescending(item => item.InvoiceDate)
                .Select(item => BusinessProjectionMapper.MapInvoice(item, context, access.CanViewSensitiveAmounts))
                .ToList(),
            ProcurementItems = context.Procurements
                .Where(item => BusinessProjectionMapper.MatchKeyword(keyword, item.ProcurementCode, item.Title, item.SupplierName, item.Stage, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(supplierName) || item.SupplierName.Contains(supplierName, StringComparison.OrdinalIgnoreCase))
                .Where(item => !input.SignDateFrom.HasValue || item.SignDate.Date >= input.SignDateFrom.Value.Date)
                .Where(item => !input.SignDateTo.HasValue || item.SignDate.Date <= input.SignDateTo.Value.Date)
                .OrderByDescending(item => item.SignDate)
                .Select(item => BusinessProjectionMapper.MapProcurement(item, context, access.CanViewSensitiveAmounts))
                .ToList(),
            ProfitItems = projectItems.OrderBy(item => item.GrossProfitRate ?? decimal.MaxValue).ToList(),
            ProjectItems = projectItems,
            ReceivableItems = context.Receivables
                .Where(item => BusinessProjectionMapper.MatchKeyword(keyword, item.ReceivableCode, item.Title, item.Status, context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? string.Empty))
                .Where(item => string.IsNullOrWhiteSpace(projectCode) || (context.ProjectCodeMap.GetValueOrDefault(item.ProjectId) ?? string.Empty).Contains(projectCode, StringComparison.OrdinalIgnoreCase))
                .Where(item => string.IsNullOrWhiteSpace(receivableStatus) || item.Status.Equals(receivableStatus, StringComparison.OrdinalIgnoreCase))
                .Where(item => !input.PlannedDateFrom.HasValue || item.PlannedDate.Date >= input.PlannedDateFrom.Value.Date)
                .Where(item => !input.PlannedDateTo.HasValue || item.PlannedDate.Date <= input.PlannedDateTo.Value.Date)
                .OrderBy(item => item.PlannedDate)
                .Select(item => BusinessProjectionMapper.MapReceivable(item, context, access.CanViewSensitiveAmounts))
                .ToList(),
            WarningItems = warningItems
        };
    }
}
