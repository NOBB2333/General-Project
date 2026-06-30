using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 外部系统账号。由同步、批量导入或人工录入产生，是内部用户绑定的统一来源。
/// </summary>
public class IntegrationExternalAccount : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AvatarUrl { get; private set; }

    public Guid ConnectorId { get; private set; }

    public string ConnectorCode { get; private set; }

    public string DepartmentExternalId { get; private set; }

    public string DisplayName { get; private set; }

    public string Email { get; private set; }

    public string ExternalUnionId { get; private set; }

    public string ExternalUserId { get; private set; }

    public DateTime? LastSyncedTime { get; private set; }

    public string Mobile { get; private set; }

    public string ProviderType { get; private set; }

    public string RawJson { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    protected IntegrationExternalAccount()
    {
        AvatarUrl = string.Empty;
        ConnectorCode = string.Empty;
        DepartmentExternalId = string.Empty;
        DisplayName = string.Empty;
        Email = string.Empty;
        ExternalUnionId = string.Empty;
        ExternalUserId = string.Empty;
        Mobile = string.Empty;
        ProviderType = string.Empty;
        RawJson = "{}";
        Status = IntegrationBindingStatus.Pending;
    }

    public IntegrationExternalAccount(
        Guid id,
        Guid connectorId,
        string connectorCode,
        string providerType,
        string externalUserId,
        Guid? tenantId,
        string? displayName = null,
        string? externalUnionId = null,
        string? mobile = null,
        string? email = null,
        string? avatarUrl = null,
        string? departmentExternalId = null,
        string? rawJson = null,
        string? status = null) : base(id)
    {
        ConnectorId = connectorId;
        ConnectorCode = NormalizeRequired(connectorCode, nameof(connectorCode), 64);
        ProviderType = NormalizeRequired(providerType, nameof(providerType), 64);
        ExternalUserId = NormalizeRequired(externalUserId, nameof(externalUserId), 256);
        TenantId = tenantId;
        DisplayName = Normalize(displayName, 128);
        ExternalUnionId = Normalize(externalUnionId, 256);
        Mobile = Normalize(mobile, 64);
        Email = Normalize(email, 256);
        AvatarUrl = Normalize(avatarUrl, 512);
        DepartmentExternalId = Normalize(departmentExternalId, 256);
        RawJson = NormalizeRawJson(rawJson);
        Status = Normalize(status, 32, IntegrationBindingStatus.Pending);
        LastSyncedTime = DateTime.UtcNow;
    }

    public void UpdateProfile(
        string? displayName,
        string? externalUnionId,
        string? mobile,
        string? email,
        string? avatarUrl,
        string? departmentExternalId,
        string? rawJson,
        string? status)
    {
        DisplayName = Normalize(displayName, 128);
        ExternalUnionId = Normalize(externalUnionId, 256);
        Mobile = Normalize(mobile, 64);
        Email = Normalize(email, 256);
        AvatarUrl = Normalize(avatarUrl, 512);
        DepartmentExternalId = Normalize(departmentExternalId, 256);
        RawJson = NormalizeRawJson(rawJson);
        Status = Normalize(status, 32, Status);
        LastSyncedTime = DateTime.UtcNow;
    }

    public void MarkStatus(string status)
    {
        Status = Normalize(status, 32, IntegrationBindingStatus.Pending);
    }

    private static string NormalizeRequired(string value, string name, int maxLength)
    {
        var normalized = Check.NotNullOrWhiteSpace(value, name, maxLength).Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeRawJson(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 32_000 ? normalized : normalized[..32_000];
    }
}
