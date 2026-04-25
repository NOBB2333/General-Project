namespace General.Admin.Project;

public class ProjectTaskItemDto
{
    public DateTime? ActualEndTime { get; set; }

    public string? BlockReason { get; set; }

    public string? ContractClause { get; set; }

    public string? DeveloperOwnerName { get; set; }

    public Guid Id { get; set; }

    public bool IsBlocked { get; set; }

    public string OrganizationUnitName { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public DateTime? PlannedEndTime { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public string Priority { get; set; } = string.Empty;

    public string? ProductOwnerName { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string TaskCode { get; set; } = string.Empty;

    public string? TesterOwnerName { get; set; }

    public string Title { get; set; } = string.Empty;
}
