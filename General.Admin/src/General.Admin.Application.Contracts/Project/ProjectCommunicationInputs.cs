namespace General.Admin.Project;

public class ProjectReportSaveInput
{
    public Guid? AttachmentFileId { get; set; }

    public Guid AuthorUserId { get; set; }

    public string BusinessSummary { get; set; } = string.Empty;

    public string CompletedSummary { get; set; } = string.Empty;

    public Guid? SprintId { get; set; }

    public string HoursSummary { get; set; } = string.Empty;

    public string MilestoneSummary { get; set; } = string.Empty;

    public string NextPlan { get; set; } = string.Empty;

    public DateTime? PeriodEnd { get; set; }

    public DateTime? PeriodStart { get; set; }

    public string ReportType { get; set; } = "周报";

    public string RiskIssueSummary { get; set; } = string.Empty;

    public string Status { get; set; } = "草稿";

    public string Title { get; set; } = string.Empty;
}

public class ProjectReportDraftInput
{
    public DateTime? PeriodEnd { get; set; }

    public DateTime? PeriodStart { get; set; }

    public string ReportType { get; set; } = "周报";
}

public class ProjectMeetingRecordSaveInput
{
    public List<ProjectMeetingActionItemSaveInput> ActionItems { get; set; } = [];

    public Guid? AttachmentFileId { get; set; }

    public string Decisions { get; set; } = string.Empty;

    public Guid FacilitatorUserId { get; set; }

    public DateTime? MeetingTime { get; set; }

    public string Minutes { get; set; } = string.Empty;

    public List<Guid> ParticipantUserIds { get; set; } = [];

    public string Status { get; set; } = "草稿";

    public string Topic { get; set; } = string.Empty;
}

public class ProjectMeetingActionItemSaveInput
{
    public string ActionType { get; set; } = "行动项";

    public Guid? AssigneeUserId { get; set; }

    public DateTime? DueDate { get; set; }

    public double? EstimatedWorkHours { get; set; }

    public string GeneratedIssueType { get; set; } = "执行问题";

    public string GeneratedRiskResponseStrategy { get; set; } = string.Empty;

    public Guid? LinkedIssueId { get; set; }

    public Guid? LinkedRaidItemId { get; set; }

    public Guid? LinkedRequirementId { get; set; }

    public Guid? LinkedTaskId { get; set; }

    public string Priority { get; set; } = ProjectPriorities.P1;

    public string TaskCode { get; set; } = string.Empty;

    public string Status { get; set; } = "未开始";

    public string Title { get; set; } = string.Empty;
}
