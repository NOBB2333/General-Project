using Volo.Abp.DependencyInjection;

namespace General.Admin.Business;

public class BusinessReportExportBuilder : ITransientDependency
{
    public BusinessReportExportResultDto Build(BusinessReportDto report, BusinessReportExportInput input)
    {
        var kind = NormalizeLedgerKind(input.LedgerKind);
        var rows = BuildExportRows(report, kind);

        return new BusinessReportExportResultDto
        {
            Content = BuildCsv(rows),
            ContentType = "text/csv;charset=utf-8",
            FileName = $"经营{ResolveExportTitle(kind)}_{DateTime.Now:yyyyMMddHHmmss}.csv"
        };
    }

    private static List<string[]> BuildExportRows(BusinessReportDto report, string kind)
    {
        return kind switch
        {
            "acceptance" =>
            [
                ["验收编号", "验收标题", "所属项目", "验收日期", "关联合同", "质量检查编号", "结果", "状态"],
                .. report.AcceptanceItems.Select(item => new[]
                {
                    item.AcceptanceCode,
                    item.Title,
                    item.ProjectName,
                    FormatDate(item.AcceptanceDate),
                    item.LinkedContractCode,
                    item.QualityInspectionCode,
                    item.AcceptanceResult,
                    item.Status
                })
            ],
            "budget" =>
            [
                ["预算编号", "预算项", "所属项目", "调整后预算", "已执行金额", "偏差金额"],
                .. report.BudgetItems.Select(item => new[]
                {
                    item.BudgetCode,
                    item.Category,
                    item.ProjectName,
                    FormatExportAmount(item.AdjustedAmount),
                    FormatExportAmount(item.ExecutedAmount),
                    FormatExportAmount(item.VarianceAmount)
                })
            ],
            "contract" =>
            [
                ["合同编号", "合同名称", "类型", "所属项目", "相对方", "签订日期", "金额", "状态"],
                .. report.ContractItems.Select(item => new[]
                {
                    item.ContractCode,
                    item.Title,
                    item.Type,
                    item.ProjectName,
                    item.CounterpartyName,
                    FormatDate(item.SignDate),
                    FormatExportAmount(item.Amount),
                    item.Status
                })
            ],
            "invoice" =>
            [
                ["发票编号", "开票标题", "所属项目", "开票日期", "关联合同", "关联回款", "金额", "税额", "状态"],
                .. report.InvoiceItems.Select(item => new[]
                {
                    item.InvoiceCode,
                    item.Title,
                    item.ProjectName,
                    FormatDate(item.InvoiceDate),
                    item.LinkedContractCode,
                    item.ReceivableCode,
                    FormatExportAmount(item.Amount),
                    FormatExportAmount(item.TaxAmount),
                    item.Status
                })
            ],
            "procurement" =>
            [
                ["采购编号", "采购/分包事项", "所属项目", "供应商/分包方", "来源合同", "阶段", "签订日期", "金额", "状态"],
                .. report.ProcurementItems.Select(item => new[]
                {
                    item.ProcurementCode,
                    item.Title,
                    item.ProjectName,
                    item.SupplierName,
                    item.LinkedContractCode,
                    item.Stage,
                    FormatDate(item.SignDate),
                    FormatExportAmount(item.Amount),
                    item.Status
                })
            ],
            "profit" =>
            [
                ["项目编号", "项目名称", "客户", "项目经理", "经营状态", "毛利额", "毛利率", "年底预计回款", "状态"],
                .. report.ProfitItems.Select(item => new[]
                {
                    item.ProjectCode,
                    item.Name,
                    item.CustomerName,
                    item.ManagerName,
                    item.BusinessCloseStatus,
                    FormatExportAmount(item.GrossProfitAmount),
                    FormatExportRate(item.GrossProfitRate),
                    FormatExportAmount(item.YearEndReceivableForecast),
                    item.Status
                })
            ],
            "receivable" =>
            [
                ["回款编号", "回款事项", "所属项目", "计划日期", "关联合同", "开票编号", "应回金额", "已回金额", "状态"],
                .. report.ReceivableItems.Select(item => new[]
                {
                    item.ReceivableCode,
                    item.Title,
                    item.ProjectName,
                    FormatDate(item.PlannedDate),
                    item.LinkedContractCode,
                    item.InvoiceCode,
                    FormatExportAmount(item.TotalAmount),
                    FormatExportAmount(item.ReceivedAmount),
                    item.Status
                })
            ],
            "warning" =>
            [
                ["预警类型", "等级", "项目编号", "项目名称", "预警说明", "责任人", "经营状态", "最近更新"],
                .. report.WarningItems.Select(item => new[]
                {
                    item.Type,
                    item.Level,
                    item.ProjectCode,
                    item.ProjectName,
                    item.Description,
                    item.OwnerName,
                    item.Status,
                    item.LastUpdatedTime
                })
            ],
            _ =>
            [
                ["项目编号", "项目名称", "客户", "项目经理", "经营状态", "合同金额", "回款金额", "年末预计回款", "状态"],
                .. report.ProjectItems.Select(item => new[]
                {
                    item.ProjectCode,
                    item.Name,
                    item.CustomerName,
                    item.ManagerName,
                    item.BusinessCloseStatus,
                    FormatExportAmount(item.ContractAmount),
                    FormatExportAmount(item.ReceivedAmount),
                    FormatExportAmount(item.YearEndReceivableForecast),
                    item.Status
                })
            ]
        };
    }

    private static byte[] BuildCsv(List<string[]> rows)
    {
        var lines = rows.Select(row => string.Join(",", row.Select(EscapeCsv)));
        var content = "\uFEFF" + string.Join(Environment.NewLine, lines);
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    private static string EscapeCsv(string? value)
    {
        var text = value ?? string.Empty;
        return text.Contains(',') || text.Contains('"') || text.Contains('\r') || text.Contains('\n')
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }

    private static string FormatDate(DateTime value)
    {
        return value.ToString("yyyy-MM-dd");
    }

    private static string FormatExportAmount(decimal? amount)
    {
        return amount.HasValue ? amount.Value.ToString("0.##") : string.Empty;
    }

    private static string FormatExportRate(decimal? rate)
    {
        return rate.HasValue ? $"{rate.Value:0.##}%" : string.Empty;
    }

    private static string NormalizeLedgerKind(string? kind)
    {
        return (kind ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static string ResolveExportTitle(string kind)
    {
        return kind switch
        {
            "acceptance" => "验收记录",
            "budget" => "预算台账",
            "contract" => "合同台账",
            "invoice" => "开票记录",
            "profit" => "利润分析",
            "procurement" => "采购台账",
            "receivable" => "回款记录",
            "warning" => "经营预警",
            _ => "项目经营"
        };
    }
}
