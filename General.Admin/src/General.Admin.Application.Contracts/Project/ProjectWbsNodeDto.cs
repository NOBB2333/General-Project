namespace General.Admin.Project;

public class ProjectWbsNodeDto
{
    public double ActualWorkHours { get; set; }

    public int CompletionRate { get; set; }

    public Guid? SprintId { get; set; }

    public string SprintName { get; set; } = string.Empty;

    public string DeliveryDescription { get; set; } = string.Empty;

    public string EstimateSummary { get; set; } = string.Empty;

    public double EstimatedWorkHours { get; set; }

    public Guid Id { get; set; }

    public Guid? MilestoneId { get; set; }

    public string MilestoneName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public Guid? ParentId { get; set; }

    public Guid ProjectId { get; set; }

    public int RequirementCount { get; set; }

    public string ScopeDescription { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string Status { get; set; } = string.Empty;

    public int TaskCount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string WbsCode { get; set; } = string.Empty;
}
