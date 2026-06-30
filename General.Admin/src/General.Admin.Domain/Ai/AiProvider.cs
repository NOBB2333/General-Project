using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 模型供应商配置。密钥只保存加密值，不对外返回明文。
/// </summary>
public class AiProvider : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ApiKeyCipherText { get; private set; }

    public string ConfigJson { get; private set; }

    public string Endpoint { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Name { get; private set; }

    public string OrganizationId { get; private set; }

    public string ProviderType { get; private set; }

    public string Remark { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiProvider()
    {
        ApiKeyCipherText = string.Empty;
        ConfigJson = "{}";
        Endpoint = string.Empty;
        Name = string.Empty;
        OrganizationId = string.Empty;
        ProviderType = string.Empty;
        Remark = string.Empty;
    }

    public AiProvider(
        Guid id,
        string name,
        string providerType,
        string endpoint,
        string apiKeyCipherText,
        Guid? tenantId,
        bool isEnabled = true,
        bool isDefault = false,
        string? organizationId = null,
        string? configJson = null,
        string? remark = null) : base(id)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        ProviderType = Check.NotNullOrWhiteSpace(providerType, nameof(providerType), 32).Trim();
        Endpoint = Normalize(endpoint, 512);
        ApiKeyCipherText = Normalize(apiKeyCipherText, 2048);
        TenantId = tenantId;
        IsEnabled = isEnabled;
        IsDefault = isDefault;
        OrganizationId = Normalize(organizationId, 128);
        ConfigJson = NormalizeJson(configJson);
        Remark = Normalize(remark, 512);
    }

    public void Update(
        string name,
        string providerType,
        string endpoint,
        bool isEnabled,
        bool isDefault,
        string? organizationId,
        string? configJson,
        string? remark)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        ProviderType = Check.NotNullOrWhiteSpace(providerType, nameof(providerType), 32).Trim();
        Endpoint = Normalize(endpoint, 512);
        IsEnabled = isEnabled;
        IsDefault = isDefault;
        OrganizationId = Normalize(organizationId, 128);
        ConfigJson = NormalizeJson(configJson);
        Remark = Normalize(remark, 512);
    }

    public void ResetApiKey(string apiKeyCipherText)
    {
        ApiKeyCipherText = Normalize(apiKeyCipherText, 2048);
    }

    public void ClearDefault()
    {
        IsDefault = false;
    }

    public bool HasApiKey()
    {
        return !string.IsNullOrWhiteSpace(ApiKeyCipherText);
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 8000 ? normalized : normalized[..8000];
    }
}
