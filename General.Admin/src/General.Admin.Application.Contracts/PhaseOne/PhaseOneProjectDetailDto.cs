namespace General.Admin.PhaseOne;

public class PhaseOneProjectDetailDto
{
    public decimal? BudgetTotalAmount { get; set; }

    public List<PhaseOneProjectCalendarItemDto> CalendarItems { get; set; } = [];

    public decimal? ContractTotalAmount { get; set; }

    public List<PhaseOneProjectCycleItemDto> Cycles { get; set; } = [];

    public string Description { get; set; } = string.Empty;

    public List<PhaseOneProjectDocumentItemDto> Documents { get; set; } = [];

    public int HighRiskCount { get; set; }

    public Guid Id { get; set; }

    public bool IsKeyProject { get; set; }

    public string ManagerName { get; set; } = string.Empty;

    public List<PhaseOneProjectMilestoneItemDto> Milestones { get; set; } = [];

    public int MemberCount { get; set; }

    public List<PhaseOneProjectMemberItemDto> Members { get; set; } = [];

    public string MyRelation { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string OrganizationUnitName { get; set; } = string.Empty;

    public int OpenIssueCount { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public string Priority { get; set; } = string.Empty;

    public string ProjectCode { get; set; } = string.Empty;

    public string ProjectSource { get; set; } = string.Empty;

    public string ProjectType { get; set; } = string.Empty;

    public List<PhaseOneProjectIssueItemDto> Issues { get; set; } = [];

    public List<PhaseOneProjectRaidItemDto> RaidItems { get; set; } = [];

    public decimal? ReceivedAmount { get; set; }

    public string SponsorName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int TaskCompletionRate { get; set; }

    public List<PhaseOneProjectTaskItemDto> Tasks { get; set; } = [];

    public double TotalWorklogHours { get; set; }

    public List<PhaseOneProjectWorklogItemDto> Worklogs { get; set; } = [];
}
