namespace General.Admin.PhaseOne;

public class PhaseOneProjectOverviewItemDto
{
    public int ActiveIssueCount { get; set; }

    public int DueSoonTaskCount { get; set; }

    public Guid Id { get; set; }

    public string MyRelation { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string NextCycleName { get; set; } = string.Empty;

    public string ProjectCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public double WeekHours { get; set; }
}
