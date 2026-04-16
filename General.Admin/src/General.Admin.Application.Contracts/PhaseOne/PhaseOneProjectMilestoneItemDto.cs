namespace General.Admin.PhaseOne;

public class PhaseOneProjectMilestoneItemDto
{
    public DateTime? ActualCompletionDate { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public DateTime PlannedCompletionDate { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
