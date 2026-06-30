using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 内部用户与外部账号的绑定记录。自动匹配失败时也保留冲突记录，便于人工处理。
/// </summary>
public class IntegrationUserBinding : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public DateTime? BoundTime { get; private set; }

    public Guid ConnectorId { get; private set; }

    public string ConnectorCode { get; private set; } = string.Empty;

    public string ConflictReason { get; private set; } = string.Empty;

    public Guid ExternalAccountId { get; private set; }

    public string ExternalDisplayName { get; private set; } = string.Empty;

    public string ExternalUserId { get; private set; } = string.Empty;

    public Guid? InternalUserId { get; private set; }

    public string InternalUserName { get; private set; } = string.Empty;

    public string MatchType { get; private set; } = IntegrationBindingMatchTypes.Manual;

    public string ProviderType { get; private set; } = string.Empty;

    public string Remark { get; private set; } = string.Empty;

    public string Status { get; private set; } = IntegrationBindingStatus.Pending;

    public Guid? TenantId { get; private set; }

    protected IntegrationUserBinding()
    {
    }

    public IntegrationUserBinding(
        Guid id,
        Guid connectorId,
        string connectorCode,
        string providerType,
        Guid externalAccountId,
        string externalUserId,
        string externalDisplayName,
        Guid? tenantId,
        Guid? internalUserId = null,
        string? internalUserName = null,
        string? matchType = null,
        string? status = null,
        string? conflictReason = null,
        string? remark = null) : base(id)
    {
        ConnectorId = connectorId;
        ConnectorCode = NormalizeRequired(connectorCode, nameof(connectorCode), 64);
        ProviderType = NormalizeRequired(providerType, nameof(providerType), 64);
        ExternalAccountId = externalAccountId;
        ExternalUserId = NormalizeRequired(externalUserId, nameof(externalUserId), 256);
        ExternalDisplayName = Normalize(externalDisplayName, 128);
        TenantId = tenantId;
        Apply(internalUserId, internalUserName, matchType, status, conflictReason, remark);
    }

    public void UpdateBinding(
        Guid? internalUserId,
        string? internalUserName,
        string? matchType,
        string? status,
        string? conflictReason,
        string? remark)
    {
        Apply(internalUserId, internalUserName, matchType, status, conflictReason, remark);
    }

    public void Disable(string? remark)
    {
        Status = IntegrationBindingStatus.Disabled;
        Remark = Normalize(remark, 512);
    }

    private void Apply(
        Guid? internalUserId,
        string? internalUserName,
        string? matchType,
        string? status,
        string? conflictReason,
        string? remark)
    {
        InternalUserId = internalUserId;
        InternalUserName = Normalize(internalUserName, 128);
        MatchType = Normalize(matchType, 32, IntegrationBindingMatchTypes.Manual);
        Status = Normalize(status, 32, IntegrationBindingStatus.Pending);
        ConflictReason = Normalize(conflictReason, 1000);
        Remark = Normalize(remark, 512);
        BoundTime = Status == IntegrationBindingStatus.Bound ? DateTime.UtcNow : BoundTime;
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
}
