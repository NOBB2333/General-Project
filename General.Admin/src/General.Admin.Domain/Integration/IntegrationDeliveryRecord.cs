using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 外部系统消息投递记录。承接 Outbox、重试、死信和审计查询。
/// </summary>
public class IntegrationDeliveryRecord : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public int AttemptCount { get; private set; }

    public string BusinessKey { get; private set; }

    public string BusinessModule { get; private set; }

    public Guid ConnectorId { get; private set; }

    public string ConnectorCode { get; private set; }

    public string DeadLetterReason { get; private set; }

    public int DurationMilliseconds { get; private set; }

    public string ErrorMessage { get; private set; }

    public string ExternalMessageId { get; private set; }

    public DateTime? LastAttemptTime { get; private set; }

    public int MaxRetryCount { get; private set; }

    public string MessageType { get; private set; }

    public DateTime? NextRetryTime { get; private set; }

    public string PayloadPreview { get; private set; }

    public string ProviderType { get; private set; }

    public string RequestJson { get; private set; }

    public string ResponseJson { get; private set; }

    public string SourceType { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    public string TargetId { get; private set; }

    public string TargetType { get; private set; }

    protected IntegrationDeliveryRecord()
    {
        BusinessKey = string.Empty;
        BusinessModule = string.Empty;
        ConnectorCode = string.Empty;
        DeadLetterReason = string.Empty;
        ErrorMessage = string.Empty;
        ExternalMessageId = string.Empty;
        MessageType = string.Empty;
        PayloadPreview = string.Empty;
        ProviderType = string.Empty;
        RequestJson = "{}";
        ResponseJson = "{}";
        SourceType = string.Empty;
        Status = string.Empty;
        TargetId = string.Empty;
        TargetType = string.Empty;
    }

    public IntegrationDeliveryRecord(
        Guid id,
        Guid connectorId,
        string connectorCode,
        string providerType,
        string messageType,
        string targetType,
        string targetId,
        string payloadPreview,
        string requestJson,
        Guid? tenantId,
        string? sourceType = null,
        string? businessModule = null,
        string? businessKey = null,
        int maxRetryCount = 3) : base(id)
    {
        ConnectorId = connectorId;
        ConnectorCode = Normalize(connectorCode, 64);
        ProviderType = Normalize(providerType, 64);
        MessageType = Normalize(messageType, 32);
        TargetType = Normalize(targetType, 32);
        TargetId = Normalize(targetId, 256);
        PayloadPreview = Normalize(payloadPreview, 2000);
        RequestJson = NormalizeJson(requestJson);
        TenantId = tenantId;
        SourceType = Normalize(sourceType, 64, "Manual");
        BusinessModule = Normalize(businessModule, 64);
        BusinessKey = Normalize(businessKey, 128);
        MaxRetryCount = Math.Clamp(maxRetryCount, 0, 10);
        ResponseJson = "{}";
        Status = "Pending";
        DeadLetterReason = string.Empty;
        ErrorMessage = string.Empty;
        ExternalMessageId = string.Empty;
    }

    public void MarkSending()
    {
        AttemptCount++;
        LastAttemptTime = DateTime.UtcNow;
        Status = "Sending";
        ErrorMessage = string.Empty;
        DeadLetterReason = string.Empty;
        NextRetryTime = null;
    }

    public void MarkSucceeded(string? externalMessageId, string? responseJson, int durationMilliseconds)
    {
        ExternalMessageId = Normalize(externalMessageId, 256);
        ResponseJson = NormalizeJson(responseJson);
        DurationMilliseconds = Math.Max(0, durationMilliseconds);
        ErrorMessage = string.Empty;
        DeadLetterReason = string.Empty;
        NextRetryTime = null;
        Status = "Succeeded";
    }

    public void MarkFailed(string errorMessage, string? responseJson, int durationMilliseconds)
    {
        ErrorMessage = Normalize(errorMessage, 4000);
        ResponseJson = NormalizeJson(responseJson);
        DurationMilliseconds = Math.Max(0, durationMilliseconds);

        if (AttemptCount >= MaxRetryCount)
        {
            Status = "DeadLetter";
            DeadLetterReason = ErrorMessage;
            NextRetryTime = null;
            return;
        }

        Status = "Failed";
        var delayMinutes = Math.Pow(2, Math.Max(0, AttemptCount - 1));
        NextRetryTime = DateTime.UtcNow.AddMinutes(Math.Min(120, delayMinutes));
    }

    public void MarkSkipped(string? reason)
    {
        Status = "Skipped";
        DeadLetterReason = Normalize(reason, 1000);
        ErrorMessage = string.Empty;
        NextRetryTime = null;
    }

    public bool CanRetry()
    {
        return Status is "Failed" or "DeadLetter";
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
