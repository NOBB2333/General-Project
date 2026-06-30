namespace General.Admin.Project;

public class ProjectAcceptanceCriteriaDto
{
    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid? MilestoneId { get; set; }

    public string MilestoneName { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? TaskId { get; set; }

    public string TaskTitle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ProjectAcceptanceRecordDto
{
    public Guid? AcceptanceFileId { get; set; }

    public string AcceptanceFileName { get; set; } = string.Empty;

    public string AcceptanceResult { get; set; } = string.Empty;

    public DateTime AcceptanceTime { get; set; }

    public List<Guid> AcceptanceCriteriaIds { get; set; } = [];

    public List<string> AcceptanceCriteriaTitles { get; set; } = [];

    public string AcceptorName { get; set; } = string.Empty;

    public Guid AcceptorUserId { get; set; }

    public Guid Id { get; set; }

    public Guid? MilestoneId { get; set; }

    public string MilestoneName { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string RectificationRequirement { get; set; } = string.Empty;

    public Guid? SprintId { get; set; }

    public string SprintName { get; set; } = string.Empty;

    public List<Guid> RequirementIds { get; set; } = [];

    public List<string> RequirementTitles { get; set; } = [];

    public string Remark { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ProjectCharterDto
{
    public string ApprovalStatus { get; set; } = string.Empty;

    public PlatformWorkflowInstanceDto? ApprovalWorkflow { get; set; }

    public string Assumptions { get; set; } = string.Empty;

    public string BusinessCase { get; set; } = string.Empty;

    public string Constraints { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Objectives { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ScopeBoundary { get; set; } = string.Empty;

    public string SuccessCriteria { get; set; } = string.Empty;
}

public class ProjectBaselineDto
{
    public decimal BudgetAtCompletion { get; set; }

    public string CostBaselineSummary { get; set; } = string.Empty;

    public Guid? CreatedFromChangeRequestId { get; set; }

    public string CreatedFromChangeRequestCode { get; set; } = string.Empty;

    public DateTime EffectiveDate { get; set; }

    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public Guid ProjectId { get; set; }

    public string ScheduleBaselineSummary { get; set; } = string.Empty;

    public string ScopeBaselineSummary { get; set; } = string.Empty;

    public int VersionNo { get; set; }
}

public class ProjectChangeApprovalDto
{
    public string Action { get; set; } = string.Empty;

    public string ApproverName { get; set; } = string.Empty;

    public Guid ApproverUserId { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime DecisionTime { get; set; }

    public Guid Id { get; set; }
}

public class ProjectChangeImpactDto
{
    public string Analysis { get; set; } = string.Empty;

    public decimal? EstimatedCostDelta { get; set; }

    public int? EstimatedScheduleDeltaDays { get; set; }

    public Guid Id { get; set; }

    public string ImpactType { get; set; } = string.Empty;

    public string RelatedBusinessCode { get; set; } = string.Empty;

    public string RelatedBudgetCode { get; set; } = string.Empty;

    public string RelatedContractCode { get; set; } = string.Empty;

    public Guid? RelatedMilestoneId { get; set; }

    public string RelatedMilestoneName { get; set; } = string.Empty;

    public Guid? RelatedTaskId { get; set; }

    public string RelatedTaskTitle { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;
}

public class ProjectChangeRequestDto
{
    public string ApplicantName { get; set; } = string.Empty;

    public Guid ApplicantUserId { get; set; }

    public PlatformWorkflowInstanceDto? ApprovalWorkflow { get; set; }

    public List<ProjectChangeApprovalDto> Approvals { get; set; } = [];

    public string ChangeType { get; set; } = string.Empty;

    public string ExpectedResult { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string ImpactScope { get; set; } = string.Empty;

    public List<ProjectChangeImpactDto> Impacts { get; set; } = [];

    public Guid ProjectId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string RequestCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ProjectPerformanceSnapshotDto
{
    public decimal ActualCost { get; set; }

    public decimal CostPerformanceIndex { get; set; }

    public decimal EarnedValue { get; set; }

    public Guid Id { get; set; }

    public decimal PlannedValue { get; set; }

    public Guid? ProjectBaselineId { get; set; }

    public Guid ProjectId { get; set; }

    public decimal SchedulePerformanceIndex { get; set; }

    public DateTime SnapshotDate { get; set; }

    public string SnapshotPeriod { get; set; } = string.Empty;
}

public class ProjectQualityChecklistDto
{
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public int InspectionCount { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;
}

public class ProjectQualityInspectionDto
{
    public string FindingSummary { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InspectionResult { get; set; } = string.Empty;

    public DateTime InspectionTime { get; set; }

    public string InspectorName { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public Guid QualityChecklistId { get; set; }

    public string QualityChecklistTitle { get; set; } = string.Empty;

    public string RectificationRequirement { get; set; } = string.Empty;
}

public class ProjectTaskDependencyDto
{
    public string DependencyType { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid PredecessorTaskId { get; set; }

    public string PredecessorTaskTitle { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public Guid SuccessorTaskId { get; set; }

    public string SuccessorTaskTitle { get; set; } = string.Empty;
}

public class ProjectTaskScheduleImpactDto
{
    public string ImpactReason { get; set; } = string.Empty;

    public Guid PredecessorTaskId { get; set; }

    public string PredecessorTaskTitle { get; set; } = string.Empty;

    public DateTime? PredecessorPlannedEndTime { get; set; }

    public Guid SuccessorTaskId { get; set; }

    public string SuccessorTaskTitle { get; set; } = string.Empty;

    public DateTime? SuccessorPlannedStartTime { get; set; }
}
