using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 外部系统 Webhook 原始事件。用于回调验签、幂等去重、解析状态和人工处理。
/// </summary>
public class IntegrationWebhookEvent : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ActionCode { get; private set; }

    public string BusinessKey { get; private set; }

    public string ConnectorCode { get; private set; }

    public Guid? ConnectorId { get; private set; }

    public string DedupeKey { get; private set; }

    public string ErrorMessage { get; private set; }

    public string EventId { get; private set; }

    public string EventType { get; private set; }

    public string HeadersJson { get; private set; }

    public string NormalizedPayloadJson { get; private set; }

    public string ProviderType { get; private set; }

    public string RawBody { get; private set; }

    public string RawPayloadJson { get; private set; }

    public DateTime? ResolvedTime { get; private set; }

    public int RetryCount { get; private set; }

    public string SignatureStatus { get; private set; }

    public string SourceIp { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    protected IntegrationWebhookEvent()
    {
        ActionCode = string.Empty;
        BusinessKey = string.Empty;
        ConnectorCode = string.Empty;
        DedupeKey = string.Empty;
        ErrorMessage = string.Empty;
        EventId = string.Empty;
        EventType = string.Empty;
        HeadersJson = "{}";
        NormalizedPayloadJson = "{}";
        ProviderType = string.Empty;
        RawBody = string.Empty;
        RawPayloadJson = "{}";
        SignatureStatus = "Unchecked";
        SourceIp = string.Empty;
        Status = "Received";
    }

    public IntegrationWebhookEvent(
        Guid id,
        string providerType,
        string connectorCode,
        string eventType,
        string eventId,
        string dedupeKey,
        string rawPayloadJson,
        string rawBody,
        string headersJson,
        Guid? tenantId,
        Guid? connectorId = null,
        string? sourceIp = null,
        string? signatureStatus = null,
        string? actionCode = null,
        string? businessKey = null) : base(id)
    {
        ProviderType = Normalize(providerType, 64);
        ConnectorCode = Normalize(connectorCode, 64);
        ConnectorId = connectorId;
        EventType = Normalize(eventType, 128, "Unknown");
        EventId = Normalize(eventId, 256);
        DedupeKey = Normalize(dedupeKey, 256);
        RawPayloadJson = NormalizeJson(rawPayloadJson, 64_000);
        RawBody = Normalize(rawBody, 64_000);
        HeadersJson = NormalizeJson(headersJson, 16_000);
        TenantId = tenantId;
        SourceIp = Normalize(sourceIp, 128);
        SignatureStatus = Normalize(signatureStatus, 32, "Unchecked");
        ActionCode = Normalize(actionCode, 128);
        BusinessKey = Normalize(businessKey, 256);
        NormalizedPayloadJson = "{}";
        ErrorMessage = string.Empty;
        Status = "Received";
    }

    public void MarkDuplicate(string? reason)
    {
        Status = "Duplicate";
        ErrorMessage = Normalize(reason, 2000, "幂等键已存在，事件已忽略。");
        ResolvedTime = DateTime.UtcNow;
    }

    public void MarkParsed(string? normalizedPayloadJson, string? actionCode, string? businessKey)
    {
        NormalizedPayloadJson = NormalizeJson(normalizedPayloadJson, 64_000);
        ActionCode = Normalize(actionCode, 128);
        BusinessKey = Normalize(businessKey, 256);
        ErrorMessage = string.Empty;
        Status = "Parsed";
    }

    public void MarkProcessed()
    {
        ErrorMessage = string.Empty;
        Status = "Processed";
        ResolvedTime = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage)
    {
        RetryCount++;
        ErrorMessage = Normalize(errorMessage, 2000);
        Status = "Failed";
    }

    public void MarkSkipped(string? reason)
    {
        ErrorMessage = Normalize(reason, 2000);
        Status = "Skipped";
        ResolvedTime = DateTime.UtcNow;
    }

    public void ResetForRetry()
    {
        RetryCount++;
        ErrorMessage = string.Empty;
        Status = "Received";
        ResolvedTime = null;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
