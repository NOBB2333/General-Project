namespace General.Admin.Integration;

public record IntegrationConnectorDescriptor(
    string ConnectorCode,
    string ProviderType,
    string DisplayName,
    string Icon,
    string ConfigSchemaJson,
    IReadOnlyList<string> Capabilities,
    int Sort,
    string Remark);

public class IntegrationMessageSendInput
{
    public string Body { get; set; } = string.Empty;

    public string BusinessKey { get; set; } = string.Empty;

    public string BusinessModule { get; set; } = string.Empty;

    public Guid ConnectorId { get; set; }

    public int MaxRetryCount { get; set; } = 3;

    public string MessageType { get; set; } = "Text";

    public string SourceType { get; set; } = "Manual";

    public string TargetId { get; set; } = string.Empty;

    public string TargetType { get; set; } = "User";
}

public class IntegrationMessageSendResult
{
    public Guid? DeliveryRecordId { get; set; }

    public string ExternalMessageId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool Success { get; set; }
}

public class IntegrationSyncExecuteInput
{
    public string ConfigJson { get; set; } = "{}";

    public Guid ConnectorId { get; set; }

    public string JobCode { get; set; } = string.Empty;

    public string Mode { get; set; } = IntegrationSyncModes.Incremental;

    public string RequestJson { get; set; } = "{}";

    public string SyncType { get; set; } = IntegrationSyncTypes.User;
}

public class IntegrationSyncResult
{
    public string ConflictJson { get; set; } = "{}";

    public int ConflictCount { get; set; }

    public int CreatedCount { get; set; }

    public int FailedCount { get; set; }

    public string Message { get; set; } = string.Empty;

    public string ResultJson { get; set; } = "{}";

    public int SkippedCount { get; set; }

    public int SuccessCount { get; set; }

    public int TotalCount { get; set; }

    public int UpdatedCount { get; set; }
}

public class IntegrationWebhookParseInput
{
    public string HeadersJson { get; set; } = "{}";

    public string RawBody { get; set; } = string.Empty;
}

public class IntegrationWebhookParseResult
{
    public string ErrorMessage { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string ParsedJson { get; set; } = "{}";

    public bool SignatureValid { get; set; }

    public string SignatureStatus { get; set; } = "Unchecked";
}

public interface IIntegrationConnectorAdapter
{
    string ConnectorCode { get; }

    Task<IntegrationConnectionTestResultDto> TestConnectionAsync(
        IntegrationConnector connector,
        IntegrationTenantConfig? tenantConfig);
}

public interface IIntegrationMessageSender
{
    string ConnectorCode { get; }

    Task<IntegrationMessageSendResult> SendMessageAsync(IntegrationMessageSendInput input);
}

public interface IIntegrationOrganizationSyncAdapter
{
    string ConnectorCode { get; }

    Task<IntegrationSyncResult> SyncOrganizationsAsync(Guid connectorId);
}

public interface IIntegrationSyncAdapter
{
    string ConnectorCode { get; }

    Task<IntegrationSyncResult> ExecuteSyncAsync(IntegrationSyncExecuteInput input);
}

public interface IIntegrationWebhookParser
{
    string ConnectorCode { get; }

    Task<IntegrationWebhookParseResult> ParseAsync(Guid connectorId, IntegrationWebhookParseInput input);
}
