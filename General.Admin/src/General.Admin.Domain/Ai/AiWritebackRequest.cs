using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 写回确认申请。AI 只能生成申请，业务写入必须在人工确认后由具体模块执行。
/// </summary>
public class AiWritebackRequest : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ActionCode { get; private set; }

    public DateTime? ApprovedTime { get; private set; }

    public Guid? ApproverUserId { get; private set; }

    public string BusinessModule { get; private set; }

    public Guid? ConversationId { get; private set; }

    public string DiffPreviewJson { get; private set; }

    public string ErrorMessage { get; private set; }

    public Guid? MessageId { get; private set; }

    public string PayloadJson { get; private set; }

    public Guid? ProjectId { get; private set; }

    public string RejectReason { get; private set; }

    public string SourceType { get; private set; }

    public string Status { get; private set; }

    public string Summary { get; private set; }

    public string TargetEntityId { get; private set; }

    public string TargetEntityType { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiWritebackRequest()
    {
        ActionCode = string.Empty;
        BusinessModule = string.Empty;
        DiffPreviewJson = "{}";
        ErrorMessage = string.Empty;
        PayloadJson = "{}";
        RejectReason = string.Empty;
        SourceType = string.Empty;
        Status = "Pending";
        Summary = string.Empty;
        TargetEntityId = string.Empty;
        TargetEntityType = string.Empty;
    }

    public AiWritebackRequest(
        Guid id,
        string businessModule,
        string actionCode,
        string targetEntityType,
        string targetEntityId,
        string summary,
        string payloadJson,
        string diffPreviewJson,
        Guid? tenantId,
        Guid? projectId = null,
        Guid? conversationId = null,
        Guid? messageId = null,
        string? sourceType = null) : base(id)
    {
        BusinessModule = Normalize(businessModule, 64, "Project");
        ActionCode = Normalize(actionCode, 128);
        TargetEntityType = Normalize(targetEntityType, 128);
        TargetEntityId = Normalize(targetEntityId, 128);
        Summary = Normalize(summary, 512);
        PayloadJson = NormalizeJson(payloadJson);
        DiffPreviewJson = NormalizeJson(diffPreviewJson);
        TenantId = tenantId;
        ProjectId = projectId;
        ConversationId = conversationId;
        MessageId = messageId;
        SourceType = Normalize(sourceType, 64, "Ai");
        Status = "Pending";
        ErrorMessage = string.Empty;
        RejectReason = string.Empty;
    }

    public void Approve(Guid? approverUserId)
    {
        EnsurePending();
        Status = "Approved";
        ApproverUserId = approverUserId;
        ApprovedTime = DateTime.UtcNow;
        RejectReason = string.Empty;
    }

    public void Reject(string? reason, Guid? approverUserId)
    {
        EnsurePending();
        Status = "Rejected";
        ApproverUserId = approverUserId;
        ApprovedTime = DateTime.UtcNow;
        RejectReason = Normalize(reason, 512);
    }

    public void Cancel()
    {
        EnsurePending();
        Status = "Cancelled";
    }

    public void MarkFailed(string? errorMessage)
    {
        Status = "Failed";
        ErrorMessage = Normalize(errorMessage, 2000);
    }

    private void EnsurePending()
    {
        if (!Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("只有待确认的 AI 写回申请可以继续处理。");
        }
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 16_000 ? normalized : normalized[..16_000];
    }
}
