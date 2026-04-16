namespace General.Admin.PhaseOne;

public class PhaseOneProjectIssueItemDto
{
    public string? DeveloperOwnerName { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid Id { get; set; }

    public bool IsOverdue { get; set; }

    public string Level { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public string? ProductOwnerName { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string? RequirementTitle { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? TesterOwnerName { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
