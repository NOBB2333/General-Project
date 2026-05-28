namespace General.Admin.Project;

public class ProjectRaidItemDto
{
    public Guid Id { get; set; }

    public string ContingencyPlan { get; set; } = string.Empty;

    public int Impact { get; set; }

    public bool IsOverdue { get; set; }

    public string Level { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public DateTime? PlannedResolveDate { get; set; }

    public int Probability { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string ResponseStrategy { get; set; } = string.Empty;

    public int RiskScore { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string TriggerCondition { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
