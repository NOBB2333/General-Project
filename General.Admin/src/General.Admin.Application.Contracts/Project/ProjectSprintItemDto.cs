namespace General.Admin.Project;

public class ProjectSprintItemDto
{
    public DateTime? EndDate { get; set; }

    public int ClosedMilestoneCount { get; set; }

    public int ClosedTaskCount { get; set; }

    public Guid Id { get; set; }

    public int HighRiskCount { get; set; }

    public int MilestoneCount { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public Guid OwnerUserId { get; set; }

    public int OverdueMilestoneCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public int Progress { get; set; }

    public int OpenIssueCount { get; set; }

    public string LatestReportStatus { get; set; } = string.Empty;

    public int ReportCount { get; set; }

    public string SprintCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public string Summary { get; set; } = string.Empty;

    public int RiskCount { get; set; }

    public int TaskCount { get; set; }
}
