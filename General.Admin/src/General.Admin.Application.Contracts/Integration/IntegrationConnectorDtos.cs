using System.ComponentModel.DataAnnotations;

namespace General.Admin.Integration;

public class IntegrationConnectorDto
{
    public List<string> Capabilities { get; set; } = [];

    public string ConfigJson { get; set; } = "{}";

    public string ConfigSchemaJson { get; set; } = "{}";

    public string ConnectorCode { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool HasSecret { get; set; }

    public string Icon { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsBuiltin { get; set; }

    public bool IsConfigured { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsTenantEnabled { get; set; }

    public string LastTestMessage { get; set; } = string.Empty;

    public string LastTestStatus { get; set; } = string.Empty;

    public DateTime? LastTestTime { get; set; }

    public string ProviderType { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public int Sort { get; set; }

    public Guid? TenantConfigId { get; set; }
}

public class IntegrationConnectorQueryInput
{
    public bool IncludeDisabled { get; set; } = true;

    [MaxLength(128)]
    public string Keyword { get; set; } = string.Empty;
}

public class IntegrationTenantConfigSaveInput
{
    [MaxLength(512)]
    public string CallbackPath { get; set; } = string.Empty;

    [MaxLength(16_000)]
    public string ConfigJson { get; set; } = "{}";

    public bool IsEnabled { get; set; } = true;

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [MaxLength(4096)]
    public string Secret { get; set; } = string.Empty;
}

public class IntegrationToggleInput
{
    public bool IsEnabled { get; set; }
}

public class IntegrationSecretRotateInput
{
    [Required]
    [MaxLength(4096)]
    public string Secret { get; set; } = string.Empty;
}

public class IntegrationConnectionTestResultDto
{
    public string Message { get; set; } = string.Empty;

    public bool Success { get; set; }
}

public class IntegrationConnectorHealthDto
{
    public string ConnectorCode { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool HasSecret { get; set; }

    public string HealthMessage { get; set; } = string.Empty;

    public string HealthStatus { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsConfigured { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsTenantEnabled { get; set; }

    public string LastTestMessage { get; set; } = string.Empty;

    public string LastTestStatus { get; set; } = string.Empty;

    public DateTime? LastTestTime { get; set; }

    public string ProviderType { get; set; } = string.Empty;
}

public class IntegrationConnectorHealthSummaryDto
{
    public int DisabledCount { get; set; }

    public int HealthyCount { get; set; }

    public IReadOnlyList<IntegrationConnectorHealthDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int UnconfiguredCount { get; set; }

    public int UnhealthyCount { get; set; }

    public int WarningCount { get; set; }
}

public class IntegrationMessageSendTestInput
{
    [MaxLength(16_000)]
    public string Body { get; set; } = string.Empty;

    [MaxLength(32)]
    public string MessageType { get; set; } = "Text";

    [MaxLength(256)]
    public string TargetId { get; set; } = string.Empty;

    [MaxLength(32)]
    public string TargetType { get; set; } = "OpenId";
}
