using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 内部业务对象与外部群或机器人的绑定。
/// </summary>
public class IntegrationGroupBinding : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid ConnectorId { get; private set; }

    public string ConnectorCode { get; private set; } = string.Empty;

    public string ExternalGroupId { get; private set; } = string.Empty;

    public string ExternalGroupName { get; private set; } = string.Empty;

    public string GroupType { get; private set; } = "Chat";

    public string InternalObjectId { get; private set; } = string.Empty;

    public string InternalObjectName { get; private set; } = string.Empty;

    public string InternalObjectType { get; private set; } = string.Empty;

    public string ProviderType { get; private set; } = string.Empty;

    public string Remark { get; private set; } = string.Empty;

    public string RobotCode { get; private set; } = string.Empty;

    public string Status { get; private set; } = IntegrationBindingStatus.Pending;

    public Guid? TenantId { get; private set; }

    protected IntegrationGroupBinding()
    {
    }

    public IntegrationGroupBinding(
        Guid id,
        Guid connectorId,
        string connectorCode,
        string providerType,
        string externalGroupId,
        string externalGroupName,
        string internalObjectType,
        string internalObjectId,
        string internalObjectName,
        Guid? tenantId,
        string? groupType = null,
        string? robotCode = null,
        string? status = null,
        string? remark = null) : base(id)
    {
        ConnectorId = connectorId;
        ConnectorCode = NormalizeRequired(connectorCode, nameof(connectorCode), 64);
        ProviderType = NormalizeRequired(providerType, nameof(providerType), 64);
        ExternalGroupId = NormalizeRequired(externalGroupId, nameof(externalGroupId), 256);
        TenantId = tenantId;
        UpdateBinding(externalGroupName, internalObjectType, internalObjectId, internalObjectName, groupType, robotCode, status, remark);
    }

    public void UpdateBinding(
        string? externalGroupName,
        string? internalObjectType,
        string? internalObjectId,
        string? internalObjectName,
        string? groupType,
        string? robotCode,
        string? status,
        string? remark)
    {
        ExternalGroupName = Normalize(externalGroupName, 128);
        InternalObjectType = Normalize(internalObjectType, 64);
        InternalObjectId = Normalize(internalObjectId, 128);
        InternalObjectName = Normalize(internalObjectName, 128);
        GroupType = Normalize(groupType, 32, "Chat");
        RobotCode = Normalize(robotCode, 128);
        Status = Normalize(status, 32, IntegrationBindingStatus.Pending);
        Remark = Normalize(remark, 512);
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
