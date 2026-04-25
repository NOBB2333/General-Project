namespace General.Admin.Project;

public class ProjectDetailDto
{
    public decimal? BudgetTotalAmount { get; set; }

    public List<ProjectCalendarItemDto> CalendarItems { get; set; } = [];

    public decimal? ContractTotalAmount { get; set; }

    public List<ProjectCycleItemDto> Cycles { get; set; } = [];

    public string Description { get; set; } = string.Empty;

    public List<ProjectDocumentItemDto> Documents { get; set; } = [];

    public int HighRiskCount { get; set; }

    public Guid Id { get; set; }

    public bool IsKeyProject { get; set; }

    public string ManagerName { get; set; } = string.Empty;

    public List<ProjectMilestoneItemDto> Milestones { get; set; } = [];

    public int MemberCount { get; set; }

    public List<ProjectMemberItemDto> Members { get; set; } = [];

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

    public List<ProjectIssueItemDto> Issues { get; set; } = [];

    public List<ProjectRaidItemDto> RaidItems { get; set; } = [];

    public decimal? ReceivedAmount { get; set; }

    public string SponsorName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int TaskCompletionRate { get; set; }

    public List<ProjectTaskItemDto> Tasks { get; set; } = [];

    public double TotalWorklogHours { get; set; }

    public List<ProjectWorklogItemDto> Worklogs { get; set; } = [];
}
