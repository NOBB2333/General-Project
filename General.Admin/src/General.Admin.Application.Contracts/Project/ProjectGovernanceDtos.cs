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

public class ProjectCharterDto
{
    public string ApprovalStatus { get; set; } = string.Empty;

    public string Assumptions { get; set; } = string.Empty;

    public string BusinessCase { get; set; } = string.Empty;

    public string Constraints { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Objectives { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ScopeBoundary { get; set; } = string.Empty;

    public string SuccessCriteria { get; set; } = string.Empty;
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
