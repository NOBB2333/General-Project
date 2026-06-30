using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Platform;

/// <summary>
/// 平台审批动作记录：记录每一次提交、通过、驳回、撤回等动作。
/// </summary>
public class PlatformApprovalRecord : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string Action { get; private set; }

    public Guid? ActorUserId { get; private set; }

    public string Comment { get; private set; }

    public string FromStatus { get; private set; }

    public string NodeName { get; private set; }

    public Guid ApprovalInstanceId { get; private set; }

    public Guid? TenantId { get; private set; }

    public string ToStatus { get; private set; }

    protected PlatformApprovalRecord()
    {
        Action = string.Empty;
        Comment = string.Empty;
        FromStatus = string.Empty;
        NodeName = string.Empty;
        ToStatus = string.Empty;
    }

    public PlatformApprovalRecord(
        Guid id,
        Guid approvalInstanceId,
        string action,
        string fromStatus,
        string toStatus,
        Guid? actorUserId,
        string comment,
        string nodeName,
        Guid? tenantId) : base(id)
    {
        ApprovalInstanceId = approvalInstanceId;
        Action = Check.NotNullOrWhiteSpace(action, nameof(action), 32);
        FromStatus = Check.NotNullOrWhiteSpace(fromStatus, nameof(fromStatus), 32);
        ToStatus = Check.NotNullOrWhiteSpace(toStatus, nameof(toStatus), 32);
        ActorUserId = actorUserId;
        Comment = comment.Trim();
        NodeName = Check.NotNullOrWhiteSpace(nodeName, nameof(nodeName), 64);
        TenantId = tenantId;
    }
}
