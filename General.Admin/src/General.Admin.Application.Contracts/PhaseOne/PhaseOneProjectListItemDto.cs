namespace General.Admin.PhaseOne;

public class PhaseOneProjectListItemDto
{
    public int BlockedTaskCount { get; set; }

    public int CompletedTaskCount { get; set; }

    public int HighRiskCount { get; set; }

    public Guid Id { get; set; }

    public bool IsKeyProject { get; set; }

    public string ManagerName { get; set; } = string.Empty;

    public int MemberCount { get; set; }

    public int MilestoneTotalCount { get; set; }

    public string MyRelation { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string OrganizationUnitName { get; set; } = string.Empty;

    public int OverdueMilestoneCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public string Priority { get; set; } = string.Empty;

    public string ProjectCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int TaskTotalCount { get; set; }
}
