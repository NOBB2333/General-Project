using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 会议行动项：可保留为行动项，也可关联任务、风险或问题。
/// </summary>
public class ProjectMeetingActionItem : FullAuditedAggregateRoot<Guid>
{
    public string ActionType { get; private set; }

    public Guid? AssigneeUserId { get; private set; }

    public DateTime? DueDate { get; private set; }

    public Guid? LinkedIssueId { get; private set; }

    public Guid? LinkedRaidItemId { get; private set; }

    public Guid? LinkedRequirementId { get; private set; }

    public Guid? LinkedTaskId { get; private set; }

    public Guid MeetingRecordId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    protected ProjectMeetingActionItem()
    {
        ActionType = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
    }

    public ProjectMeetingActionItem(
        Guid id,
        Guid projectId,
        Guid meetingRecordId,
        string title,
        string actionType,
        string status,
        Guid? assigneeUserId = null,
        DateTime? dueDate = null,
        Guid? linkedTaskId = null,
        Guid? linkedRaidItemId = null,
        Guid? linkedIssueId = null,
        Guid? linkedRequirementId = null) : base(id)
    {
        ProjectId = projectId;
        MeetingRecordId = meetingRecordId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256);
        ActionType = Check.NotNullOrWhiteSpace(actionType, nameof(actionType), 32);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        AssigneeUserId = assigneeUserId;
        DueDate = dueDate?.Date;
        LinkedTaskId = linkedTaskId;
        LinkedRaidItemId = linkedRaidItemId;
        LinkedIssueId = linkedIssueId;
        LinkedRequirementId = linkedRequirementId;
    }
}
