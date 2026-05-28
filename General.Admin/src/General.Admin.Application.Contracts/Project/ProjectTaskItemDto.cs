namespace General.Admin.Project;

public class ProjectTaskItemDto
{
    public DateTime? ActualEndTime { get; set; }

    public DateTime? BaselineEndTime { get; set; }

    public DateTime? BaselineStartTime { get; set; }

    public string? BlockReason { get; set; }

    public string? ContractClause { get; set; }

    public string? DeveloperOwnerName { get; set; }

    public Guid? DeveloperOwnerUserId { get; set; }

    public Guid Id { get; set; }

    public bool IsBlocked { get; set; }

    public string OrganizationUnitName { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public Guid? ParentTaskId { get; set; }

    public DateTime? PlannedEndTime { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public string Priority { get; set; } = string.Empty;

    public string? ProductOwnerName { get; set; }

    public Guid? ProductOwnerUserId { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string TaskCode { get; set; } = string.Empty;

    public string? TesterOwnerName { get; set; }

    public Guid? TesterOwnerUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string WbsCode { get; set; } = string.Empty;
}
