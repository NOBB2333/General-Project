namespace General.Admin.Project;

public class ProjectDetailDto
{
    public List<ProjectAcceptanceCriteriaDto> AcceptanceCriteria { get; set; } = [];

    public List<ProjectAcceptanceRecordDto> AcceptanceRecords { get; set; } = [];

    public decimal? BudgetTotalAmount { get; set; }

    public ProjectBaselineDto? ActiveBaseline { get; set; }

    public List<ProjectCalendarItemDto> CalendarItems { get; set; } = [];

    public List<ProjectChangeRequestDto> ChangeRequests { get; set; } = [];

    public ProjectCharterDto? Charter { get; set; }

    public decimal? ContractTotalAmount { get; set; }

    public List<ProjectSprintItemDto> Sprints { get; set; } = [];

    public string Description { get; set; } = string.Empty;

    public List<ProjectDocumentItemDto> Documents { get; set; } = [];

    public List<ProjectMeetingRecordDto> MeetingRecords { get; set; } = [];

    public int HighRiskCount { get; set; }

    public int HealthRiskScore { get; set; }

    public string HealthStatus { get; set; } = string.Empty;

    public List<ProjectHealthBreakdownDto> HealthBreakdowns { get; set; } = [];

    public DateTime HealthCalculatedTime { get; set; }

    public string HealthDataSource { get; set; } = string.Empty;

    public List<string> HealthReasons { get; set; } = [];

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

    public ProjectPerformanceSnapshotDto? PerformanceSnapshot { get; set; }

    public List<ProjectPerformanceSnapshotDto> PerformanceSnapshots { get; set; } = [];

    public DateTime? PlannedEndDate { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public string Priority { get; set; } = string.Empty;

    public string ProjectCode { get; set; } = string.Empty;

    public string ProjectSource { get; set; } = string.Empty;

    public string ProjectType { get; set; } = string.Empty;

    public List<ProjectRequirementDto> Requirements { get; set; } = [];

    public List<ProjectReportDto> Reports { get; set; } = [];

    public List<ProjectIssueItemDto> Issues { get; set; } = [];

    public List<ProjectQualityChecklistDto> QualityChecklists { get; set; } = [];

    public List<ProjectQualityInspectionDto> QualityInspections { get; set; } = [];

    public List<ProjectRaidItemDto> RaidItems { get; set; } = [];

    public decimal? ReceivedAmount { get; set; }

    public string SponsorName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int TaskCompletionRate { get; set; }

    public List<ProjectTaskDependencyDto> TaskDependencies { get; set; } = [];

    public List<ProjectTaskScheduleImpactDto> TaskScheduleImpacts { get; set; } = [];

    public List<ProjectTaskItemDto> Tasks { get; set; } = [];

    public double TotalWorklogHours { get; set; }

    public List<ProjectWbsNodeDto> WbsNodes { get; set; } = [];

    public List<ProjectWorklogItemDto> Worklogs { get; set; } = [];
}

public static class ProjectDetailSections
{
    public const string Control = "control";

    public const string Delivery = "delivery";

    public const string Docs = "docs";

    public const string Issues = "issues";

    public const string Overview = "overview";

    public const string Plan = "plan";

    public const string Resources = "resources";

    public const string Tasks = "tasks";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Control,
        Delivery,
        Docs,
        Issues,
        Overview,
        Plan,
        Resources,
        Tasks
    };
}

public class ProjectHealthBreakdownDto
{
    public string Code { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string ActionKey { get; set; } = string.Empty;

    public string ActionLabel { get; set; } = string.Empty;

    public string RuleVersion { get; set; } = string.Empty;

    public string DataSource { get; set; } = string.Empty;

    public int Score { get; set; }
}
