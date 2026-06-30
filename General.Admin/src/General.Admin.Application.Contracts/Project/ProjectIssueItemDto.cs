namespace General.Admin.Project;

public class ProjectIssueItemDto
{
    public Guid? AcceptanceCriteriaId { get; set; }

    public string AcceptanceCriteriaTitle { get; set; } = string.Empty;

    public string? DeveloperOwnerName { get; set; }

    public Guid? DeveloperOwnerUserId { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid Id { get; set; }

    public bool IsOverdue { get; set; }

    public string Level { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public Guid OwnerUserId { get; set; }

    public string? ProductOwnerName { get; set; }

    public Guid? ProductOwnerUserId { get; set; }

    public Guid? QualityChecklistId { get; set; }

    public string QualityChecklistTitle { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string? RequirementTitle { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? TesterOwnerName { get; set; }

    public Guid? TesterOwnerUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
