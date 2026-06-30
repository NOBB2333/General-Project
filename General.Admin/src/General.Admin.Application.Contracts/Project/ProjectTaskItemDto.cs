namespace General.Admin.Project;

public class ProjectTaskItemDto
{
    public DateTime? ActualEndTime { get; set; }

    public DateTime? ActualStartTime { get; set; }

    public double? ActualWorkHours { get; set; }

    public DateTime? BaselineEndTime { get; set; }

    public DateTime? BaselineStartTime { get; set; }

    public string? BlockReason { get; set; }

    public string? ContractClause { get; set; }

    public Guid? SprintId { get; set; }

    public string SprintName { get; set; } = string.Empty;

    public string? DeveloperOwnerName { get; set; }

    public List<Guid> DeveloperOwnerUserIds { get; set; } = [];

    public List<string> DeveloperOwnerNames { get; set; } = [];

    public Guid? DeveloperOwnerUserId { get; set; }

    public string Description { get; set; } = string.Empty;

    public double? EstimatedWorkHours { get; set; }

    public Guid Id { get; set; }

    public bool IsBlocked { get; set; }

    public bool IsOutOfContract { get; set; }

    public string OrganizationUnitName { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public Guid OwnerUserId { get; set; }

    public Guid? ParentTaskId { get; set; }

    public DateTime? PlannedEndTime { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public string Priority { get; set; } = string.Empty;

    public string? ProductOwnerName { get; set; }

    public List<Guid> ProductOwnerUserIds { get; set; } = [];

    public List<string> ProductOwnerNames { get; set; } = [];

    public Guid? ProductOwnerUserId { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public Guid? RequirementId { get; set; }

    public string RequirementTitle { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string TaskCode { get; set; } = string.Empty;

    public string? TesterOwnerName { get; set; }

    public List<Guid> TesterOwnerUserIds { get; set; } = [];

    public List<string> TesterOwnerNames { get; set; } = [];

    public Guid? TesterOwnerUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string WbsCode { get; set; } = string.Empty;

    public Guid? WbsNodeId { get; set; }

    public string WbsNodeName { get; set; } = string.Empty;
}
