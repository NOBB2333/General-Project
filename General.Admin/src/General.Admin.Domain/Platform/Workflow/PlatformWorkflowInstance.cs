using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Platform;

/// <summary>
/// 平台工作流实例：屏蔽 Elsa 等第三方引擎，承载业务对象和审批状态的统一关联。
/// </summary>
public class PlatformWorkflowInstance : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? ApplicantUserId { get; private set; }

    public string BusinessCode { get; private set; }

    public string BusinessId { get; private set; }

    public string BusinessSnapshot { get; private set; }

    public string BusinessTitle { get; private set; }

    public string BusinessType { get; private set; }

    public DateTime? CompletedTime { get; private set; }

    public string CurrentNodeName { get; private set; }

    public string ElsaWorkflowInstanceId { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    public string WorkflowDefinitionKey { get; private set; }

    public int WorkflowDefinitionVersion { get; private set; }

    protected PlatformWorkflowInstance()
    {
        BusinessCode = string.Empty;
        BusinessId = string.Empty;
        BusinessSnapshot = string.Empty;
        BusinessTitle = string.Empty;
        BusinessType = string.Empty;
        CurrentNodeName = string.Empty;
        ElsaWorkflowInstanceId = string.Empty;
        Status = string.Empty;
        WorkflowDefinitionKey = string.Empty;
    }

    public PlatformWorkflowInstance(
        Guid id,
        string workflowDefinitionKey,
        int workflowDefinitionVersion,
        string businessType,
        string businessId,
        string businessCode,
        string businessTitle,
        string businessSnapshot,
        Guid? applicantUserId,
        string elsaWorkflowInstanceId,
        Guid? tenantId) : base(id)
    {
        WorkflowDefinitionKey = Check.NotNullOrWhiteSpace(workflowDefinitionKey, nameof(workflowDefinitionKey), 128);
        WorkflowDefinitionVersion = workflowDefinitionVersion <= 0 ? 1 : workflowDefinitionVersion;
        BusinessType = Check.NotNullOrWhiteSpace(businessType, nameof(businessType), 64);
        BusinessId = Check.NotNullOrWhiteSpace(businessId, nameof(businessId), 64);
        BusinessCode = businessCode.Trim();
        BusinessTitle = Check.NotNullOrWhiteSpace(businessTitle, nameof(businessTitle), 256);
        BusinessSnapshot = businessSnapshot.Trim();
        ApplicantUserId = applicantUserId;
        ElsaWorkflowInstanceId = elsaWorkflowInstanceId.Trim();
        CurrentNodeName = "审批";
        Status = PlatformWorkflowStatuses.Pending;
        TenantId = tenantId;
    }

    public void MarkApproved()
    {
        CurrentNodeName = "已完成";
        Status = PlatformWorkflowStatuses.Approved;
        CompletedTime = DateTime.Now;
    }

    public void MarkRejected()
    {
        CurrentNodeName = "已完成";
        Status = PlatformWorkflowStatuses.Rejected;
        CompletedTime = DateTime.Now;
    }

    public void MarkWithdrawn()
    {
        CurrentNodeName = "已撤回";
        Status = PlatformWorkflowStatuses.Withdrawn;
        CompletedTime = DateTime.Now;
    }

    public void UpdateElsaWorkflowInstanceId(string elsaWorkflowInstanceId)
    {
        ElsaWorkflowInstanceId = elsaWorkflowInstanceId.Trim();
    }
}
