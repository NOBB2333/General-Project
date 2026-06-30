using System.Text.Json;
using System.Threading;
using General.Admin.Business;
using General.Admin.Integration;

namespace General.Admin.Platform;

public class BusinessReportDistributionJobHandler : IPlatformScheduledJobHandler
{
    private static readonly string[] DefaultLedgerKinds =
    [
        string.Empty,
        "receivable",
        "warning"
    ];

    private readonly BusinessReportExportBuilder _reportExportBuilder;
    private readonly BusinessService _businessService;
    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;
    private readonly IntegrationNotificationService _notificationService;

    public BusinessReportDistributionJobHandler(
        BusinessReportExportBuilder reportExportBuilder,
        BusinessService businessService,
        PlatformSchedulerExecutionLogWriter executionLogWriter,
        IntegrationNotificationService notificationService)
    {
        _reportExportBuilder = reportExportBuilder;
        _businessService = businessService;
        _executionLogWriter = executionLogWriter;
        _notificationService = notificationService;
    }

    public string JobKey => "business-report-distribution";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var summary = new BusinessReportDistributionSummary();
        foreach (var ledgerKind in DefaultLedgerKinds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var input = new BusinessReportExportInput
            {
                LedgerKind = ledgerKind
            };
            var report = await _businessService.GetReportAsync(input);
            var export = _reportExportBuilder.Build(report, input);
            var dispatchResult = await _notificationService.DispatchEventAsync(new IntegrationNotificationEventInput
            {
                EventCode = "business.report.distribute",
                OccurredAt = DateTime.UtcNow,
                VariablesJson = JsonSerializer.Serialize(new
                {
                    businessReport = new
                    {
                        contentLength = export.Content.Length,
                        contentType = export.ContentType,
                        fileName = export.FileName,
                        generatedAt = DateTime.UtcNow,
                        ledgerKind = NormalizeLedgerKind(ledgerKind),
                        ledgerName = ResolveLedgerName(ledgerKind),
                        rowCount = ResolveRowCount(report, ledgerKind)
                    },
                    report = new
                    {
                        acceptanceCount = report.AcceptanceItems.Count,
                        budgetCount = report.BudgetItems.Count,
                        contractCount = report.ContractItems.Count,
                        invoiceCount = report.InvoiceItems.Count,
                        procurementCount = report.ProcurementItems.Count,
                        projectCount = report.ProjectItems.Count,
                        receivableCount = report.ReceivableItems.Count,
                        warningCount = report.WarningItems.Count
                    }
                })
            });
            summary.Merge(dispatchResult);
            summary.ReportCount++;
        }

        var result = $"经营报表分发完成：生成 {summary.ReportCount} 份报表，规则投递成功 {summary.SentCount} 条，失败 {summary.FailedCount} 条，未匹配目标 {summary.NoTargetCount} 条，跳过 {summary.SkippedCount} 条。";
        await _executionLogWriter.WriteOperationAsync(JobKey, "经营报表分发", result, cancellationToken: cancellationToken);
        return $"{result} 可在外部系统集成的投递记录和日志中心查看执行记录。";
    }

    private static string NormalizeLedgerKind(string ledgerKind)
    {
        return string.IsNullOrWhiteSpace(ledgerKind) ? "project" : ledgerKind.Trim().ToLowerInvariant();
    }

    private static int ResolveRowCount(BusinessReportDto report, string ledgerKind)
    {
        return NormalizeLedgerKind(ledgerKind) switch
        {
            "acceptance" => report.AcceptanceItems.Count,
            "budget" => report.BudgetItems.Count,
            "contract" => report.ContractItems.Count,
            "invoice" => report.InvoiceItems.Count,
            "procurement" => report.ProcurementItems.Count,
            "receivable" => report.ReceivableItems.Count,
            "warning" => report.WarningItems.Count,
            _ => report.ProjectItems.Count
        };
    }

    private static string ResolveLedgerName(string ledgerKind)
    {
        return NormalizeLedgerKind(ledgerKind) switch
        {
            "acceptance" => "验收记录",
            "budget" => "预算台账",
            "contract" => "合同台账",
            "invoice" => "开票记录",
            "procurement" => "采购台账",
            "receivable" => "回款记录",
            "warning" => "经营预警",
            _ => "项目经营"
        };
    }

    private sealed class BusinessReportDistributionSummary
    {
        public int FailedCount { get; private set; }

        public int NoTargetCount { get; private set; }

        public int ReportCount { get; set; }

        public int SentCount { get; private set; }

        public int SkippedCount { get; private set; }

        public void Merge(IntegrationNotificationDispatchResultDto result)
        {
            FailedCount += result.FailedCount;
            NoTargetCount += result.NoTargetCount;
            SentCount += result.SentCount;
            SkippedCount += result.SkippedCount + result.BlockedCount;
        }
    }
}
