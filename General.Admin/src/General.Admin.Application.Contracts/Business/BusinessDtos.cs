namespace General.Admin.Business;

public class BusinessOverviewDto
{
    public bool CanViewProfitAnalysis { get; set; }

    public bool CanViewSensitiveAmounts { get; set; }

    public decimal? ContractAmount { get; set; }

    public decimal? CostAmount { get; set; }

    public List<BusinessFocusItemDto> FocusProjects { get; set; } = [];

    public decimal? GrossProfitAmount { get; set; }

    public decimal? GrossProfitRate { get; set; }

    public int LowProfitProjectCount { get; set; }

    public List<BusinessProjectSummaryDto> Projects { get; set; } = [];

    public decimal? ProcurementAmount { get; set; }

    public int ReceivableRiskProjectCount { get; set; }

    public decimal? ReceivedAmount { get; set; }

    public int TailClosingProjectCount { get; set; }

    public List<BusinessWarningItemDto> WarningItems { get; set; } = [];

    public decimal? YearEndReceivableForecast { get; set; }
}

public class BusinessProjectListInput
{
    public string? Keyword { get; set; }
}

public class BusinessProjectSummaryDto
{
    public string BusinessCloseStatus { get; set; } = string.Empty;

    public decimal? ContractAmount { get; set; }

    public decimal? CostAmount { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public decimal? GrossProfitAmount { get; set; }

    public decimal? GrossProfitRate { get; set; }

    public Guid Id { get; set; }

    public decimal? InvoicedAmount { get; set; }

    public string ManagerName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal? ProcurementAmount { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public decimal? ReceivedAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal? YearEndReceivableForecast { get; set; }
}

public class BusinessProjectDetailDto
{
    public List<BusinessBudgetExecutionItemDto> BudgetExecutions { get; set; } = [];

    public List<BusinessChainItemDto> BusinessChains { get; set; } = [];

    public bool CanViewProfitAnalysis { get; set; }

    public bool CanViewSensitiveAmounts { get; set; }

    public List<BusinessContractItemDto> Contracts { get; set; } = [];

    public List<BusinessForecastHistoryItemDto> ForecastHistories { get; set; } = [];

    public List<BusinessProcurementItemDto> Procurements { get; set; } = [];

    public List<BusinessReceivableItemDto> Receivables { get; set; } = [];

    public BusinessProjectSummaryDto Summary { get; set; } = new();
}

public class BusinessFocusItemDto
{
    public string ActionTab { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

public class BusinessWarningItemDto
{
    public string ActionTab { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;

    public string LastUpdatedTime { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

public class BusinessBudgetExecutionItemDto
{
    public decimal? AdjustedAmount { get; set; }

    public string BudgetCode { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal? ExecutedAmount { get; set; }

    public Guid Id { get; set; }

    public decimal? VarianceAmount { get; set; }
}

public class BusinessChainItemDto
{
    public string ChainCode { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string LinkedContractCode { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public string SourceChangeCode { get; set; } = string.Empty;

    public string Stage { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

public class BusinessContractItemDto
{
    public decimal? Amount { get; set; }

    public string ContractCode { get; set; } = string.Empty;

    public string CounterpartyName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsRevenueContract { get; set; }

    public string ParentContractCode { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public DateTime SignDate { get; set; }

    public string SourceChangeCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}

public class BusinessProcurementItemDto
{
    public decimal? Amount { get; set; }

    public Guid Id { get; set; }

    public string LinkedContractCode { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string ProcurementCode { get; set; } = string.Empty;

    public DateTime SignDate { get; set; }

    public string SourceChangeCode { get; set; } = string.Empty;

    public string Stage { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class BusinessReceivableItemDto
{
    public Guid Id { get; set; }

    public string InvoiceCode { get; set; } = string.Empty;

    public string LinkedContractCode { get; set; } = string.Empty;

    public DateTime PlannedDate { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public decimal? ReceivedAmount { get; set; }

    public string ReceivableCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public decimal? TotalAmount { get; set; }
}

public class BusinessForecastHistoryItemDto
{
    public string ChangeType { get; set; } = string.Empty;

    public string ChangedAt { get; set; } = string.Empty;

    public string ChangedBy { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Metric { get; set; } = string.Empty;

    public string NewValue { get; set; } = string.Empty;

    public string OldValue { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string RelatedCode { get; set; } = string.Empty;
}

public class BusinessReportDto
{
    public List<BusinessContractItemDto> ContractItems { get; set; } = [];

    public List<BusinessProjectSummaryDto> ProfitItems { get; set; } = [];

    public List<BusinessProcurementItemDto> ProcurementItems { get; set; } = [];

    public List<BusinessProjectSummaryDto> ProjectItems { get; set; } = [];

    public List<BusinessReceivableItemDto> ReceivableItems { get; set; } = [];

    public List<BusinessWarningItemDto> WarningItems { get; set; } = [];
}

public class BusinessReportQueryInput
{
    public string? BusinessCloseStatus { get; set; }

    public string? ContractType { get; set; }

    public string? CounterpartyName { get; set; }

    public string? Keyword { get; set; }

    public DateTime? PlannedDateFrom { get; set; }

    public DateTime? PlannedDateTo { get; set; }

    public string? ProjectCode { get; set; }

    public string? ReceivableStatus { get; set; }

    public DateTime? SignDateFrom { get; set; }

    public DateTime? SignDateTo { get; set; }

    public string? SupplierName { get; set; }
}
