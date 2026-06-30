namespace General.Admin.Project;

public class ProjectDashboardMetricDto
{
    public string Label { get; set; } = string.Empty;

    public string Trend { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public decimal Value { get; set; }
}

public class ProjectDashboardHealthBucketDto
{
    public int Count { get; set; }

    public string HealthStatus { get; set; } = string.Empty;
}

public class ProjectDashboardProjectCardDto
{
    public int BlockedTaskCount { get; set; }

    public decimal? CostAmount { get; set; }

    public decimal? ContractAmount { get; set; }

    public decimal? CostPerformanceIndex { get; set; }

    public decimal? EarnedValue { get; set; }

    public int CompletedTaskCount { get; set; }

    public int HealthRiskScore { get; set; }

    public string HealthStatus { get; set; } = string.Empty;

    public int HighRiskCount { get; set; }

    public Guid Id { get; set; }

    public bool IsKeyProject { get; set; }

    public string ManagerName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int OpenIssueCount { get; set; }

    public string OrganizationUnitName { get; set; } = string.Empty;

    public int OverdueMilestoneCount { get; set; }

    public int OverdueTaskCount { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public decimal? PlannedValue { get; set; }

    public string Priority { get; set; } = string.Empty;

    public int Progress { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public decimal? ReceivedAmount { get; set; }

    public decimal? SchedulePerformanceIndex { get; set; }

    public string Status { get; set; } = string.Empty;

    public int TaskTotalCount { get; set; }
}

public class ProjectDashboardActionItemDto
{
    public string ActionKey { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public string Level { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public Guid SourceId { get; set; }

    public string SourceType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ProjectDashboardPerformancePointDto
{
    public decimal ActualCost { get; set; }

    public decimal CostPerformanceIndex { get; set; }

    public decimal EarnedValue { get; set; }

    public decimal PlannedValue { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public decimal SchedulePerformanceIndex { get; set; }

    public DateTime SnapshotDate { get; set; }
}

public class ProjectDashboardBusinessRiskDto
{
    public string BusinessCloseStatus { get; set; } = string.Empty;

    public decimal? ContractAmount { get; set; }

    public decimal? CostAmount { get; set; }

    public decimal? GrossProfitRate { get; set; }

    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public decimal? ReceivedAmount { get; set; }

    public string RiskLabel { get; set; } = string.Empty;

    public decimal? YearEndReceivableForecast { get; set; }
}

public class ProjectDashboardEarnedValueSummaryDto
{
    public decimal ActualCost { get; set; }

    public decimal CostPerformanceIndex { get; set; }

    public decimal EarnedValue { get; set; }

    public decimal PlannedValue { get; set; }

    public decimal SchedulePerformanceIndex { get; set; }

    public DateTime? SnapshotDate { get; set; }
}

public class ProjectDashboardDepartmentPressureDto
{
    public int ProjectCount { get; set; }

    public string Name { get; set; } = string.Empty;

    public int RiskScore { get; set; }
}

public class ProjectMyExecutionDashboardDto
{
    public List<ProjectDashboardActionItemDto> ActionItems { get; set; } = [];

    public List<ProjectIssueItemDto> MyIssues { get; set; } = [];

    public List<ProjectDashboardMetricDto> Metrics { get; set; } = [];

    public List<ProjectOverviewItemDto> MyProjects { get; set; } = [];

    public List<ProjectTaskItemDto> MyTasks { get; set; } = [];

    public List<ProjectWorklogItemDto> RecentWorklogs { get; set; } = [];
}

public class ProjectPmDashboardDto
{
    public List<ProjectDashboardActionItemDto> ActionItems { get; set; } = [];

    public List<ProjectDashboardBusinessRiskDto> BusinessRisks { get; set; } = [];

    public ProjectDashboardEarnedValueSummaryDto EarnedValueSummary { get; set; } = new();

    public List<ProjectDashboardHealthBucketDto> HealthBuckets { get; set; } = [];

    public List<ProjectDashboardMetricDto> Metrics { get; set; } = [];

    public List<ProjectDashboardPerformancePointDto> PerformancePoints { get; set; } = [];

    public List<ProjectDashboardProjectCardDto> Projects { get; set; } = [];
}

public class ProjectPmoDashboardDto
{
    public List<ProjectDashboardActionItemDto> ActionItems { get; set; } = [];

    public List<ProjectDashboardBusinessRiskDto> BusinessRisks { get; set; } = [];

    public List<ProjectDashboardDepartmentPressureDto> DepartmentPressures { get; set; } = [];

    public ProjectDashboardEarnedValueSummaryDto EarnedValueSummary { get; set; } = new();

    public List<ProjectDashboardProjectCardDto> ExceptionProjects { get; set; } = [];

    public List<ProjectDashboardHealthBucketDto> HealthBuckets { get; set; } = [];

    public List<ProjectDashboardMetricDto> Metrics { get; set; } = [];

    public List<ProjectDashboardPerformancePointDto> PerformancePoints { get; set; } = [];

    public List<ProjectDashboardProjectCardDto> Projects { get; set; } = [];
}
