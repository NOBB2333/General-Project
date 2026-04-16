namespace General.Admin.PhaseOne;

public class PhaseOneProjectRaidItemDto
{
    public Guid Id { get; set; }

    public bool IsOverdue { get; set; }

    public string Level { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public DateTime? PlannedResolveDate { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
