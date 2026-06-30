namespace General.Admin.Project;

public class ProjectRequirementDto
{
    public string AcceptanceCriteria { get; set; } = string.Empty;

    public string BusinessOwnerName { get; set; } = string.Empty;

    public Guid? BusinessOwnerUserId { get; set; }

    public string ChangeReason { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string ImpactSummary { get; set; } = string.Empty;

    public bool IsCurrent { get; set; }

    public Guid? MilestoneId { get; set; }

    public string MilestoneName { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string ProductOwnerName { get; set; } = string.Empty;

    public Guid? ProductOwnerUserId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? ReplacesRequirementId { get; set; }

    public string ReplacesRequirementTitle { get; set; } = string.Empty;

    public string RequirementCode { get; set; } = string.Empty;

    public double? SizeEstimate { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int VersionNo { get; set; }

    public Guid? WbsNodeId { get; set; }

    public string WbsNodeName { get; set; } = string.Empty;
}
