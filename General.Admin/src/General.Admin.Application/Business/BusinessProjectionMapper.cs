using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Business;

internal static class BusinessProjectionMapper
{
    public static BusinessProjectSummaryDto MapProjectSummary(
        ProjectEntity project,
        BusinessContext context,
        BusinessQueryAccess access)
    {
        var revenueContracts = context.ContractsByProject.GetValueOrDefault(project.Id) is { } allContracts
            ? allContracts.Where(c => c.IsRevenueContract).ToList()
            : [];
        var invoices = context.InvoicesByProject.GetValueOrDefault(project.Id) ?? [];
        var procurements = context.ProcurementsByProject.GetValueOrDefault(project.Id) ?? [];
        var receivables = context.ReceivablesByProject.GetValueOrDefault(project.Id) ?? [];
        var budgetExecs = context.BudgetExecutionsByProject.GetValueOrDefault(project.Id) ?? [];

        var contractAmount = revenueContracts.Sum(item => item.Amount);
        var procurementAmount = procurements.Sum(item => item.Amount);
        var receivedAmount = receivables.Sum(item => item.ReceivedAmount);
        var yearEndReceivableForecast = receivables.Sum(item => item.TotalAmount);
        var costAmount = procurementAmount + budgetExecs.Sum(item => item.ExecutedAmount);
        var grossProfitAmount = contractAmount - costAmount;
        var grossProfitRate = contractAmount <= 0 ? 0m : Math.Round(grossProfitAmount * 100m / contractAmount, 1);
        context.LatestPerformanceSnapshotByProject.TryGetValue(project.Id, out var performanceSnapshot);

        return new BusinessProjectSummaryDto
        {
            BusinessCloseStatus = ResolveBusinessCloseStatus(project, receivables),
            ContractAmount = MaskAmount(contractAmount, access.CanViewSensitiveAmounts),
            CostPerformanceIndex = performanceSnapshot?.CostPerformanceIndex,
            CostAmount = MaskAmount(costAmount, access.CanViewSensitiveAmounts),
            CustomerName = revenueContracts.FirstOrDefault()?.CounterpartyName ?? project.ProjectSource ?? "-",
            GrossProfitAmount = MaskAmount(grossProfitAmount, access.CanViewProfitAnalysis),
            GrossProfitRate = access.CanViewProfitAnalysis ? grossProfitRate : null,
            Id = project.Id,
            InvoicedAmount = MaskAmount(invoices.Sum(item => item.Amount), access.CanViewSensitiveAmounts),
            ManagerName = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            Name = project.Name,
            ProcurementAmount = MaskAmount(procurementAmount, access.CanViewSensitiveAmounts),
            ProjectCode = project.ProjectCode,
            ReceivedAmount = MaskAmount(receivedAmount, access.CanViewSensitiveAmounts),
            SchedulePerformanceIndex = performanceSnapshot?.SchedulePerformanceIndex,
            Status = project.Status,
            YearEndReceivableForecast = MaskAmount(yearEndReceivableForecast, access.CanViewSensitiveAmounts)
        };
    }

    public static BusinessAcceptanceItemDto MapAcceptance(BusinessAcceptance item, BusinessContext context)
    {
        return new BusinessAcceptanceItemDto
        {
            AcceptanceCode = item.AcceptanceCode,
            AcceptanceDate = item.AcceptanceDate,
            AcceptanceResult = item.AcceptanceResult,
            Id = item.Id,
            LinkedContractCode = item.LinkedContractCode,
            MilestoneId = item.MilestoneId,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            QualityInspectionCode = item.QualityInspectionCode,
            Status = item.Status,
            Title = item.Title
        };
    }

    public static BusinessContractItemDto MapContract(
        BusinessContract item,
        BusinessContext context,
        bool canViewSensitiveAmounts)
    {
        return new BusinessContractItemDto
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

    public static BusinessBudgetExecutionItemDto MapBudgetExecution(
        BusinessBudgetExecution item,
        BusinessContext context,
        bool canViewSensitiveAmounts)
    {
        return new BusinessBudgetExecutionItemDto
        {
            AdjustedAmount = MaskAmount(item.AdjustedAmount, canViewSensitiveAmounts),
            BudgetCode = item.BudgetCode,
            Category = item.Category,
            ExecutedAmount = MaskAmount(item.ExecutedAmount, canViewSensitiveAmounts),
            Id = item.Id,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            SortOrder = item.SortOrder,
            VarianceAmount = MaskAmount(item.VarianceAmount, canViewSensitiveAmounts),
        };
    }

    public static BusinessInvoiceItemDto MapInvoice(
        BusinessInvoice item,
        BusinessContext context,
        bool canViewSensitiveAmounts)
    {
        return new BusinessInvoiceItemDto
        {
            Amount = MaskAmount(item.Amount, canViewSensitiveAmounts),
            Id = item.Id,
            InvoiceCode = item.InvoiceCode,
            InvoiceDate = item.InvoiceDate,
            LinkedContractCode = item.LinkedContractCode,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            ReceivableCode = item.ReceivableCode,
            Status = item.Status,
            TaxAmount = MaskAmount(item.TaxAmount, canViewSensitiveAmounts),
            Title = item.Title
        };
    }

    public static BusinessProcurementItemDto MapProcurement(
        BusinessProcurement item,
        BusinessContext context,
        bool canViewSensitiveAmounts,
        string? projectName = null)
    {
        return new BusinessProcurementItemDto
        {
            Amount = MaskAmount(item.Amount, canViewSensitiveAmounts),
            Id = item.Id,
            LinkedContractCode = item.LinkedContractCode,
            ProcurementCode = item.ProcurementCode,
            ProjectId = item.ProjectId,
            ProjectName = projectName ?? context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            SignDate = item.SignDate,
            SourceChangeCode = item.SourceChangeCode,
            Stage = item.Stage,
            Status = item.Status,
            SupplierName = item.SupplierName,
            Title = item.Title
        };
    }

    public static BusinessReceivableItemDto MapReceivable(
        BusinessReceivable item,
        BusinessContext context,
        bool canViewSensitiveAmounts,
        string? projectName = null)
    {
        return new BusinessReceivableItemDto
        {
            Id = item.Id,
            InvoiceCode = item.InvoiceCode,
            LinkedContractCode = item.LinkedContractCode,
            PlannedDate = item.PlannedDate,
            ProjectId = item.ProjectId,
            ProjectName = projectName ?? context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            ReceivedAmount = MaskAmount(item.ReceivedAmount, canViewSensitiveAmounts),
            ReceivableCode = item.ReceivableCode,
            Status = item.Status,
            Title = item.Title,
            TotalAmount = MaskAmount(item.TotalAmount, canViewSensitiveAmounts)
        };
    }

    public static decimal? MaskAmount(decimal amount, bool canView)
    {
        return canView ? amount : null;
    }

    public static bool MatchKeyword(string? keyword, params string[] candidates)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        return candidates.Any(item => item.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolveBusinessCloseStatus(ProjectEntity project, List<BusinessReceivable> receivables)
    {
        var totalReceivable = receivables.Sum(item => item.TotalAmount);
        var receivedAmount = receivables.Sum(item => item.ReceivedAmount);
        if (receivedAmount < totalReceivable && ProjectStatuses.IsClosed(project.Status))
        {
            return "执行关闭待回款";
        }

        if (receivedAmount < totalReceivable)
        {
            return "经营收尾中";
        }

        return "经营执行中";
    }
}
