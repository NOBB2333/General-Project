namespace General.Admin.Project;

public class ProjectAcceptanceCriteriaSaveInput
{
    public string Description { get; set; } = string.Empty;

    public Guid? MilestoneId { get; set; }

    public string Status { get; set; } = "待确认";

    public Guid? TaskId { get; set; }

    public string Title { get; set; } = string.Empty;
}

public class ProjectAcceptanceRecordSaveInput
{
    public Guid? AcceptanceFileId { get; set; }

    public string AcceptanceResult { get; set; } = "通过";

    public DateTime? AcceptanceTime { get; set; }

    public List<Guid> AcceptanceCriteriaIds { get; set; } = [];

    public Guid AcceptorUserId { get; set; }

    public Guid? MilestoneId { get; set; }

    public string RectificationRequirement { get; set; } = string.Empty;

    public Guid? SprintId { get; set; }

    public List<Guid> RequirementIds { get; set; } = [];

    public string Remark { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public bool CreateIssueWhenRejected { get; set; }

    public Guid? IssueOwnerUserId { get; set; }

    public DateTime? IssueDueDate { get; set; }

    public string IssueLevel { get; set; } = ProjectPriorities.P0;
}

public class ProjectCharterSaveInput
{
    public string ApprovalStatus { get; set; } = "草稿";

    public string Assumptions { get; set; } = string.Empty;

    public string BusinessCase { get; set; } = string.Empty;

    public string Constraints { get; set; } = string.Empty;

    public string Objectives { get; set; } = string.Empty;

    public string ScopeBoundary { get; set; } = string.Empty;

    public string SuccessCriteria { get; set; } = string.Empty;
}

public class ProjectCharterApprovalActionInput
{
    public string Comment { get; set; } = string.Empty;
}

public class ProjectWorkflowActionInput
{
    public string Comment { get; set; } = string.Empty;
}

public class ProjectSprintSaveInput
{
    public DateTime? EndDate { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid OwnerUserId { get; set; }

    public int Progress { get; set; }

    public DateTime? StartDate { get; set; }

    public string Status { get; set; } = "规划中";

    public string Summary { get; set; } = string.Empty;
}

public class ProjectMilestoneSaveInput
{
    public DateTime? ActualCompletionDate { get; set; }

    public Guid? SprintId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid OwnerUserId { get; set; }

    public DateTime PlannedCompletionDate { get; set; }

    public string Status { get; set; } = "未开始";
}

public class ProjectBaselineSaveInput
{
    public decimal BudgetAtCompletion { get; set; }

    public string CostBaselineSummary { get; set; } = string.Empty;

    public Guid? CreatedFromChangeRequestId { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string ScheduleBaselineSummary { get; set; } = string.Empty;

    public string ScopeBaselineSummary { get; set; } = string.Empty;
}

public class ProjectChangeImpactSaveInput
{
    public string Analysis { get; set; } = string.Empty;

    public decimal? EstimatedCostDelta { get; set; }

    public int? EstimatedScheduleDeltaDays { get; set; }

    public string ImpactType { get; set; } = string.Empty;

    public string RelatedBusinessCode { get; set; } = string.Empty;

    public string RelatedBudgetCode { get; set; } = string.Empty;

    public string RelatedContractCode { get; set; } = string.Empty;

    public Guid? RelatedMilestoneId { get; set; }

    public Guid? RelatedTaskId { get; set; }

    public string Severity { get; set; } = ProjectPriorities.P1;
}

public class ProjectChangeRequestSaveInput
{
    public Guid? ApplicantUserId { get; set; }

    public string ChangeType { get; set; } = string.Empty;

    public string ExpectedResult { get; set; } = string.Empty;

    public string ImpactScope { get; set; } = string.Empty;

    public List<ProjectChangeImpactSaveInput> Impacts { get; set; } = [];

    public string Reason { get; set; } = string.Empty;

    public string RequestCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ProjectPerformanceSnapshotSaveInput
{
    public decimal? ActualCost { get; set; }

    public decimal? EarnedValue { get; set; }

    public decimal? PlannedValue { get; set; }

    public Guid? ProjectBaselineId { get; set; }

    public DateTime? SnapshotDate { get; set; }

    public string SnapshotPeriod { get; set; } = "Weekly";
}

public class ProjectRequirementSaveInput
{
    public string AcceptanceCriteria { get; set; } = string.Empty;

    public Guid? BusinessOwnerUserId { get; set; }

    public string ChangeReason { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImpactSummary { get; set; } = string.Empty;

    public bool IsCurrent { get; set; } = true;

    public Guid? MilestoneId { get; set; }

    public string Priority { get; set; } = ProjectPriorities.P1;

    public Guid? ProductOwnerUserId { get; set; }

    public Guid? ReplacesRequirementId { get; set; }

    public string RequirementCode { get; set; } = string.Empty;

    public double? SizeEstimate { get; set; }

    public string Source { get; set; } = "内部";

    public string Status { get; set; } = "草稿";

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = "功能";

    public int VersionNo { get; set; } = 1;

    public Guid? WbsNodeId { get; set; }
}

public class ProjectRequirementFromWbsInput
{
    public string AcceptanceCriteria { get; set; } = string.Empty;

    public Guid? BusinessOwnerUserId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string ImpactSummary { get; set; } = string.Empty;

    public string Priority { get; set; } = ProjectPriorities.P1;

    public Guid? ProductOwnerUserId { get; set; }

    public string RequirementCode { get; set; } = string.Empty;

    public double? SizeEstimate { get; set; }

    public string Source { get; set; } = "内部";

    public string Status { get; set; } = "草稿";

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = "功能";

    public int VersionNo { get; set; } = 1;
}

public class ProjectQualityChecklistSaveInput
{
    public string Category { get; set; } = "通用";

    public string Description { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ProjectQualityInspectionSaveInput
{
    public string FindingSummary { get; set; } = string.Empty;

    public string InspectionResult { get; set; } = "通过";

    public DateTime? InspectionTime { get; set; }

    public Guid InspectorUserId { get; set; }

    public Guid QualityChecklistId { get; set; }

    public string RectificationRequirement { get; set; } = string.Empty;
}

public class ProjectRaidRiskUpdateInput
{
    public string ContingencyPlan { get; set; } = string.Empty;

    public int Impact { get; set; }

    public int Probability { get; set; }

    public string ResponseStrategy { get; set; } = string.Empty;

    public string TriggerCondition { get; set; } = string.Empty;
}

public class ProjectTaskDependencySaveInput
{
    public string DependencyType { get; set; } = "FinishToStart";

    public Guid PredecessorTaskId { get; set; }

    public Guid SuccessorTaskId { get; set; }
}

public class ProjectTaskFromRequirementInput
{
    public Guid? DeveloperOwnerUserId { get; set; }

    public List<Guid> DeveloperOwnerUserIds { get; set; } = [];

    public string Description { get; set; } = string.Empty;

    public double? EstimatedWorkHours { get; set; }

    public bool IsOutOfContract { get; set; }

    public Guid OwnerUserId { get; set; }

    public DateTime? PlannedEndTime { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public string Priority { get; set; } = ProjectPriorities.P1;

    public Guid? ProductOwnerUserId { get; set; }

    public List<Guid> ProductOwnerUserIds { get; set; } = [];

    public string TaskCode { get; set; } = string.Empty;

    public Guid? TesterOwnerUserId { get; set; }

    public List<Guid> TesterOwnerUserIds { get; set; } = [];

    public string Title { get; set; } = string.Empty;
}

public class ProjectTaskFromWbsInput
{
    public Guid? SprintId { get; set; }

    public Guid? DeveloperOwnerUserId { get; set; }

    public List<Guid> DeveloperOwnerUserIds { get; set; } = [];

    public string Description { get; set; } = string.Empty;

    public double? EstimatedWorkHours { get; set; }

    public bool IsOutOfContract { get; set; }

    public Guid OwnerUserId { get; set; }

    public DateTime? PlannedEndTime { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public string Priority { get; set; } = ProjectPriorities.P1;

    public Guid? ProductOwnerUserId { get; set; }

    public List<Guid> ProductOwnerUserIds { get; set; } = [];

    public string TaskCode { get; set; } = string.Empty;

    public Guid? TesterOwnerUserId { get; set; }

    public List<Guid> TesterOwnerUserIds { get; set; } = [];

    public string Title { get; set; } = string.Empty;
}

public class ProjectTaskSaveInput
{
    public Guid? SprintId { get; set; }

    public Guid? DeveloperOwnerUserId { get; set; }

    public List<Guid> DeveloperOwnerUserIds { get; set; } = [];

    public string Description { get; set; } = string.Empty;

    public double? EstimatedWorkHours { get; set; }

    public bool IsOutOfContract { get; set; }

    public Guid OwnerUserId { get; set; }

    public DateTime? PlannedEndTime { get; set; }

    public DateTime? PlannedStartTime { get; set; }

    public string Priority { get; set; } = ProjectPriorities.P1;

    public Guid? ProductOwnerUserId { get; set; }

    public List<Guid> ProductOwnerUserIds { get; set; } = [];

    public string Status { get; set; } = "未开始";

    public string TaskCode { get; set; } = string.Empty;

    public Guid? TesterOwnerUserId { get; set; }

    public List<Guid> TesterOwnerUserIds { get; set; } = [];

    public string Title { get; set; } = string.Empty;

    public Guid? WbsNodeId { get; set; }
}

public class ProjectTaskWbsUpdateInput
{
    public DateTime? BaselineEndTime { get; set; }

    public DateTime? BaselineStartTime { get; set; }

    public Guid? SprintId { get; set; }

    public Guid? ParentTaskId { get; set; }

    public Guid? RequirementId { get; set; }

    public int SortOrder { get; set; }

    public string WbsCode { get; set; } = string.Empty;

    public Guid? WbsNodeId { get; set; }
}

public class ProjectTaskProgressInput
{
    public string BlockReason { get; set; } = string.Empty;

    public double Hours { get; set; }

    public bool IsBlocked { get; set; }

    public string Status { get; set; } = "进行中";

    public string Summary { get; set; } = string.Empty;

    public DateTime? WorkDate { get; set; }
}

public class ProjectWbsNodeSaveInput
{
    public Guid? SprintId { get; set; }

    public string DeliveryDescription { get; set; } = string.Empty;

    public string EstimateSummary { get; set; } = string.Empty;

    public Guid? MilestoneId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public Guid? ParentId { get; set; }

    public string ScopeDescription { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string Status { get; set; } = "规划中";

    public string Type { get; set; } = "工作包";

    public string WbsCode { get; set; } = string.Empty;
}

public class ProjectMemberRolesUpdateInput
{
    public List<string> RoleNames { get; set; } = [];
}

public class ProjectIssueQualityLinkInput
{
    public Guid? AcceptanceCriteriaId { get; set; }

    public Guid? QualityChecklistId { get; set; }
}

public class ProjectIssueSaveInput
{
    public Guid? AcceptanceCriteriaId { get; set; }

    public Guid? DeveloperOwnerUserId { get; set; }

    public DateTime? DueDate { get; set; }

    public string Level { get; set; } = ProjectPriorities.P1;

    public Guid OwnerUserId { get; set; }

    public Guid? ProductOwnerUserId { get; set; }

    public Guid? QualityChecklistId { get; set; }

    public string RequirementTitle { get; set; } = string.Empty;

    public string Status { get; set; } = "待处理";

    public Guid? TesterOwnerUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = "缺陷";
}

public class ProjectStatusUpdateInput
{
    public string Comment { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class ProjectMilestoneStatusUpdateInput
{
    public DateTime? ActualCompletionDate { get; set; }

    public Guid? SprintId { get; set; }

    public string Status { get; set; } = string.Empty;
}
