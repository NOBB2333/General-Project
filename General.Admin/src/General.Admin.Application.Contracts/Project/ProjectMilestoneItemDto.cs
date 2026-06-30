namespace General.Admin.Project;

public class ProjectMilestoneItemDto
{
    public DateTime? ActualCompletionDate { get; set; }

    public Guid? SprintId { get; set; }

    public string SprintName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public Guid OwnerUserId { get; set; }

    public DateTime PlannedCompletionDate { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
