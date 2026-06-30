using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目变更审批记录：记录变更控制中的审批意见，后续可接入统一工作流。
/// </summary>
public class ProjectChangeApproval : FullAuditedAggregateRoot<Guid>
{
    public string Action { get; private set; }

    public Guid ApproverUserId { get; private set; }

    public string Comment { get; private set; }

    public DateTime DecisionTime { get; private set; }

    public Guid ProjectChangeRequestId { get; private set; }

    public Guid ProjectId { get; private set; }

    protected ProjectChangeApproval()
    {
        Action = string.Empty;
        Comment = string.Empty;
    }

    public ProjectChangeApproval(
        Guid id,
        Guid projectId,
        Guid projectChangeRequestId,
        Guid approverUserId,
        string action,
        string comment,
        DateTime? decisionTime = null) : base(id)
    {
        ProjectId = projectId;
        ProjectChangeRequestId = projectChangeRequestId;
        ApproverUserId = approverUserId;
        Action = Check.NotNullOrWhiteSpace(action, nameof(action), 32);
        Comment = comment.Trim();
        DecisionTime = decisionTime ?? DateTime.Now;
    }
}
