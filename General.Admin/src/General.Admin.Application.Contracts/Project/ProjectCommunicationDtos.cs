namespace General.Admin.Project;

public class ProjectReportDto
{
    public Guid? AttachmentFileId { get; set; }

    public string AttachmentFileName { get; set; } = string.Empty;

    public string AuthorName { get; set; } = string.Empty;

    public Guid AuthorUserId { get; set; }

    public string BusinessSummary { get; set; } = string.Empty;

    public string CompletedSummary { get; set; } = string.Empty;

    public Guid? SprintId { get; set; }

    public string SprintName { get; set; } = string.Empty;

    public string HoursSummary { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string MilestoneSummary { get; set; } = string.Empty;

    public string NextPlan { get; set; } = string.Empty;

    public DateTime PeriodEnd { get; set; }

    public DateTime PeriodStart { get; set; }

    public Guid ProjectId { get; set; }

    public string ReportType { get; set; } = string.Empty;

    public string RiskIssueSummary { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ProjectReportDraftDto
{
    public string BusinessSummary { get; set; } = string.Empty;

    public string CompletedSummary { get; set; } = string.Empty;

    public string HoursSummary { get; set; } = string.Empty;

    public string MilestoneSummary { get; set; } = string.Empty;

    public string NextPlan { get; set; } = string.Empty;

    public DateTime PeriodEnd { get; set; }

    public DateTime PeriodStart { get; set; }

    public string ReportType { get; set; } = string.Empty;

    public string RiskIssueSummary { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}

public class ProjectMeetingRecordDto
{
    public List<ProjectMeetingActionItemDto> ActionItems { get; set; } = [];

    public Guid? AttachmentFileId { get; set; }

    public string AttachmentFileName { get; set; } = string.Empty;

    public string Decisions { get; set; } = string.Empty;

    public string FacilitatorName { get; set; } = string.Empty;

    public Guid FacilitatorUserId { get; set; }

    public Guid Id { get; set; }

    public DateTime MeetingTime { get; set; }

    public string Minutes { get; set; } = string.Empty;

    public List<Guid> ParticipantUserIds { get; set; } = [];

    public List<string> ParticipantNames { get; set; } = [];

    public Guid ProjectId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;
}

public class ProjectMeetingActionItemDto
{
    public string ActionType { get; set; } = string.Empty;

    public string AssigneeName { get; set; } = string.Empty;

    public Guid? AssigneeUserId { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid Id { get; set; }

    public Guid? LinkedIssueId { get; set; }

    public string LinkedIssueTitle { get; set; } = string.Empty;

    public Guid? LinkedRaidItemId { get; set; }

    public string LinkedRaidItemTitle { get; set; } = string.Empty;

    public Guid? LinkedRequirementId { get; set; }

    public string LinkedRequirementTitle { get; set; } = string.Empty;

    public Guid? LinkedTaskId { get; set; }

    public string LinkedTaskTitle { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
