using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 内部组织单元与外部部门的绑定。
/// </summary>
public class IntegrationDepartmentBinding : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid ConnectorId { get; private set; }

    public string ConnectorCode { get; private set; } = string.Empty;

    public string ConflictReason { get; private set; } = string.Empty;

    public string ExternalDepartmentId { get; private set; } = string.Empty;

    public string ExternalDepartmentName { get; private set; } = string.Empty;

    public Guid? InternalOrganizationUnitId { get; private set; }

    public string InternalOrganizationUnitName { get; private set; } = string.Empty;

    public string MatchType { get; private set; } = IntegrationBindingMatchTypes.Manual;

    public string ParentExternalDepartmentId { get; private set; } = string.Empty;

    public string ProviderType { get; private set; } = string.Empty;

    public string Remark { get; private set; } = string.Empty;

    public string Status { get; private set; } = IntegrationBindingStatus.Pending;

    public Guid? TenantId { get; private set; }

    protected IntegrationDepartmentBinding()
    {
    }

    public IntegrationDepartmentBinding(
        Guid id,
        Guid connectorId,
        string connectorCode,
        string providerType,
        string externalDepartmentId,
        string externalDepartmentName,
        Guid? tenantId,
        Guid? internalOrganizationUnitId = null,
        string? internalOrganizationUnitName = null,
        string? parentExternalDepartmentId = null,
        string? matchType = null,
        string? status = null,
        string? conflictReason = null,
        string? remark = null) : base(id)
    {
        ConnectorId = connectorId;
        ConnectorCode = NormalizeRequired(connectorCode, nameof(connectorCode), 64);
        ProviderType = NormalizeRequired(providerType, nameof(providerType), 64);
        ExternalDepartmentId = NormalizeRequired(externalDepartmentId, nameof(externalDepartmentId), 256);
        TenantId = tenantId;
        UpdateBinding(internalOrganizationUnitId, internalOrganizationUnitName, externalDepartmentName, parentExternalDepartmentId, matchType, status, conflictReason, remark);
    }

    public void UpdateBinding(
        Guid? internalOrganizationUnitId,
        string? internalOrganizationUnitName,
        string? externalDepartmentName,
        string? parentExternalDepartmentId,
        string? matchType,
        string? status,
        string? conflictReason,
        string? remark)
    {
        InternalOrganizationUnitId = internalOrganizationUnitId;
        InternalOrganizationUnitName = Normalize(internalOrganizationUnitName, 128);
        ExternalDepartmentName = Normalize(externalDepartmentName, 128);
        ParentExternalDepartmentId = Normalize(parentExternalDepartmentId, 256);
        MatchType = Normalize(matchType, 32, IntegrationBindingMatchTypes.Manual);
        Status = Normalize(status, 32, IntegrationBindingStatus.Pending);
        ConflictReason = Normalize(conflictReason, 1000);
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
