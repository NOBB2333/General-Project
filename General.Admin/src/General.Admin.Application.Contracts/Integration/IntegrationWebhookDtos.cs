using System.ComponentModel.DataAnnotations;

namespace General.Admin.Integration;

public class IntegrationWebhookEventDto
{
    public string ActionCode { get; set; } = string.Empty;

    public string BusinessKey { get; set; } = string.Empty;

    public string ConnectorCode { get; set; } = string.Empty;

    public Guid? ConnectorId { get; set; }

    public DateTime CreationTime { get; set; }

    public string DedupeKey { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public string EventId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string HeadersJson { get; set; } = "{}";

    public Guid Id { get; set; }

    public string NormalizedPayloadJson { get; set; } = "{}";

    public string ProviderType { get; set; } = string.Empty;

    public string RawBody { get; set; } = string.Empty;

    public string RawPayloadJson { get; set; } = "{}";

    public DateTime? ResolvedTime { get; set; }

    public int RetryCount { get; set; }

    public string SignatureStatus { get; set; } = "Unchecked";

    public string SourceIp { get; set; } = string.Empty;

    public string Status { get; set; } = "Received";
}

public class IntegrationWebhookEventQueryInput
{
    [MaxLength(128)]
    public string EventType { get; set; } = string.Empty;

    [MaxLength(64)]
    public string ProviderType { get; set; } = string.Empty;

    public int SkipCount { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(128)]
    public string Keyword { get; set; } = string.Empty;

    [Range(1, 200)]
    public int MaxResultCount { get; set; } = 20;
}

public class IntegrationWebhookReceiveInput
{
    [MaxLength(128)]
    public string ActionCode { get; set; } = string.Empty;

    [MaxLength(256)]
    public string BusinessKey { get; set; } = string.Empty;

    [MaxLength(64)]
    public string ConnectorCode { get; set; } = string.Empty;

    [MaxLength(256)]
    public string DedupeKey { get; set; } = string.Empty;

    [MaxLength(256)]
    public string EventId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string EventType { get; set; } = "Unknown";

    [MaxLength(16_000)]
    public string HeadersJson { get; set; } = "{}";

    [MaxLength(64)]
    public string ProviderType { get; set; } = string.Empty;

    [MaxLength(64_000)]
    public string RawBody { get; set; } = string.Empty;

    [MaxLength(64_000)]
    public string RawPayloadJson { get; set; } = "{}";

    [MaxLength(32)]
    public string SignatureStatus { get; set; } = "Unchecked";

    [MaxLength(128)]
    public string SourceIp { get; set; } = string.Empty;
}

public class IntegrationWebhookSkipInput
{
    [MaxLength(2000)]
    public string Reason { get; set; } = string.Empty;
}
