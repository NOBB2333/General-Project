using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目报告：统一承载日报、周报、月报、阶段报告和项目总结。
/// </summary>
public class ProjectReport : FullAuditedAggregateRoot<Guid>
{
    public Guid? AttachmentFileId { get; private set; }

    public Guid AuthorUserId { get; private set; }

    public string BusinessSummary { get; private set; }

    public string CompletedSummary { get; private set; }

    public Guid? SprintId { get; private set; }

    public string HoursSummary { get; private set; }

    public string MilestoneSummary { get; private set; }

    public string NextPlan { get; private set; }

    public Guid ProjectId { get; private set; }

    public DateTime PeriodEnd { get; private set; }

    public DateTime PeriodStart { get; private set; }

    public string ReportType { get; private set; }

    public string RiskIssueSummary { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    protected ProjectReport()
    {
        BusinessSummary = string.Empty;
        CompletedSummary = string.Empty;
        HoursSummary = string.Empty;
        MilestoneSummary = string.Empty;
        NextPlan = string.Empty;
        ReportType = string.Empty;
        RiskIssueSummary = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
    }

    public ProjectReport(
        Guid id,
        Guid projectId,
        string reportType,
        string title,
        Guid authorUserId,
        DateTime periodStart,
        DateTime periodEnd,
        string status,
        string? businessSummary = null,
        string? completedSummary = null,
        string? nextPlan = null,
        string? riskIssueSummary = null,
        string? milestoneSummary = null,
        string? hoursSummary = null,
        Guid? sprintId = null,
        Guid? attachmentFileId = null) : base(id)
    {
        ProjectId = projectId;
        ReportType = Check.NotNullOrWhiteSpace(reportType, nameof(reportType), 32);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        AuthorUserId = authorUserId;
        PeriodStart = periodStart.Date;
        PeriodEnd = periodEnd.Date;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        BusinessSummary = businessSummary?.Trim() ?? string.Empty;
        CompletedSummary = completedSummary?.Trim() ?? string.Empty;
        NextPlan = nextPlan?.Trim() ?? string.Empty;
        RiskIssueSummary = riskIssueSummary?.Trim() ?? string.Empty;
        MilestoneSummary = milestoneSummary?.Trim() ?? string.Empty;
        HoursSummary = hoursSummary?.Trim() ?? string.Empty;
        SprintId = sprintId;
        AttachmentFileId = attachmentFileId;
    }
}
