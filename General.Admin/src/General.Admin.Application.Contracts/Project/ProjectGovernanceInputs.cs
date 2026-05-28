namespace General.Admin.Project;

public class ProjectAcceptanceCriteriaSaveInput
{
    public string Description { get; set; } = string.Empty;

    public Guid? MilestoneId { get; set; }

    public string Status { get; set; } = "待确认";

    public Guid? TaskId { get; set; }

    public string Title { get; set; } = string.Empty;
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

public class ProjectTaskWbsUpdateInput
{
    public DateTime? BaselineEndTime { get; set; }

    public DateTime? BaselineStartTime { get; set; }

    public Guid? ParentTaskId { get; set; }

    public int SortOrder { get; set; }

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

public class ProjectStatusUpdateInput
{
    public string Status { get; set; } = string.Empty;
}

public class ProjectMilestoneStatusUpdateInput
{
    public DateTime? ActualCompletionDate { get; set; }

    public string Status { get; set; } = string.Empty;
}
