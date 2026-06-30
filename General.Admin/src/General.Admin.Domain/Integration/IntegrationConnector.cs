using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 外部系统连接器定义。用于声明连接器类型、配置 Schema 和能力清单。
/// </summary>
public class IntegrationConnector : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string CapabilitiesJson { get; private set; }

    public string ConfigSchemaJson { get; private set; }

    public string ConnectorCode { get; private set; }

    public string DisplayName { get; private set; }

    public string Icon { get; private set; }

    public bool IsBuiltin { get; private set; }

    public bool IsEnabled { get; private set; }

    public string ProviderType { get; private set; }

    public string Remark { get; private set; }

    public int Sort { get; private set; }

    public Guid? TenantId { get; private set; }

    protected IntegrationConnector()
    {
        CapabilitiesJson = "[]";
        ConfigSchemaJson = "{}";
        ConnectorCode = string.Empty;
        DisplayName = string.Empty;
        Icon = string.Empty;
        ProviderType = string.Empty;
        Remark = string.Empty;
    }

    public IntegrationConnector(
        Guid id,
        string connectorCode,
        string providerType,
        string displayName,
        string configSchemaJson,
        string capabilitiesJson,
        Guid? tenantId,
        bool isBuiltin = true,
        bool isEnabled = true,
        string? icon = null,
        int sort = 0,
        string? remark = null) : base(id)
    {
        ConnectorCode = Check.NotNullOrWhiteSpace(connectorCode, nameof(connectorCode), 64).Trim();
        ProviderType = Check.NotNullOrWhiteSpace(providerType, nameof(providerType), 64).Trim();
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), 128).Trim();
        ConfigSchemaJson = NormalizeJson(configSchemaJson, "{}");
        CapabilitiesJson = NormalizeJson(capabilitiesJson, "[]");
        TenantId = tenantId;
        IsBuiltin = isBuiltin;
        IsEnabled = isEnabled;
        Icon = Normalize(icon, 128);
        Sort = sort;
        Remark = Normalize(remark, 512);
    }

    public void UpdateDefinition(
        string providerType,
        string displayName,
        string configSchemaJson,
        string capabilitiesJson,
        bool isEnabled,
        string? icon,
        int sort,
        string? remark)
    {
        ProviderType = Check.NotNullOrWhiteSpace(providerType, nameof(providerType), 64).Trim();
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), 128).Trim();
        ConfigSchemaJson = NormalizeJson(configSchemaJson, "{}");
        CapabilitiesJson = NormalizeJson(capabilitiesJson, "[]");
        IsEnabled = isEnabled;
        Icon = Normalize(icon, 128);
        Sort = sort;
        Remark = Normalize(remark, 512);
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= 16_000 ? normalized : normalized[..16_000];
    }
}
