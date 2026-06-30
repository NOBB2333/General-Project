using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目验收记录：记录一次真实验收动作，关联里程碑、验收标准和验收文件。
/// </summary>
public class ProjectAcceptanceRecord : FullAuditedAggregateRoot<Guid>
{
    public Guid? AcceptanceFileId { get; private set; }

    public string AcceptanceResult { get; private set; }

    public DateTime AcceptanceTime { get; private set; }

    public Guid AcceptorUserId { get; private set; }

    public string RectificationRequirement { get; private set; }

    public Guid? MilestoneId { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid? SprintId { get; private set; }

    public string Remark { get; private set; }

    public string Title { get; private set; }

    protected ProjectAcceptanceRecord()
    {
        AcceptanceResult = string.Empty;
        RectificationRequirement = string.Empty;
        Remark = string.Empty;
        Title = string.Empty;
    }

    public ProjectAcceptanceRecord(
        Guid id,
        Guid projectId,
        string title,
        string acceptanceResult,
        Guid acceptorUserId,
        DateTime acceptanceTime,
        Guid? milestoneId = null,
        Guid? sprintId = null,
        Guid? acceptanceFileId = null,
        string? rectificationRequirement = null,
        string? remark = null) : base(id)
    {
        ProjectId = projectId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        AcceptanceResult = Check.NotNullOrWhiteSpace(acceptanceResult, nameof(acceptanceResult), 32);
        AcceptorUserId = acceptorUserId;
        AcceptanceTime = acceptanceTime;
        MilestoneId = milestoneId;
        SprintId = sprintId;
        AcceptanceFileId = acceptanceFileId;
        RectificationRequirement = (rectificationRequirement ?? string.Empty).Trim();
        Remark = (remark ?? string.Empty).Trim();
    }
}
