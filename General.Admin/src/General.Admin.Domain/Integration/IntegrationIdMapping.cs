using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 内外部对象 ID 映射。用于同步任务、消息回流和后续 Skill/AI 查询时的稳定定位。
/// </summary>
public class IntegrationIdMapping : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid ConnectorId { get; private set; }

    public string ConnectorCode { get; private set; }

    public string ExternalObjectId { get; private set; }

    public string ExternalObjectType { get; private set; }

    public string InternalObjectId { get; private set; }

    public string InternalObjectType { get; private set; }

    public string ProviderType { get; private set; }

    public string Remark { get; private set; }

    public string Source { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    protected IntegrationIdMapping()
    {
        ConnectorCode = string.Empty;
        ExternalObjectId = string.Empty;
        ExternalObjectType = string.Empty;
        InternalObjectId = string.Empty;
        InternalObjectType = string.Empty;
        ProviderType = string.Empty;
        Remark = string.Empty;
        Source = "Manual";
        Status = IntegrationBindingStatus.Bound;
    }

    public IntegrationIdMapping(
        Guid id,
        Guid connectorId,
        string connectorCode,
        string providerType,
        string internalObjectType,
        string internalObjectId,
        string externalObjectType,
        string externalObjectId,
        Guid? tenantId,
        string? source = null,
        string? status = null,
        string? remark = null) : base(id)
    {
        ConnectorId = connectorId;
        ConnectorCode = NormalizeRequired(connectorCode, nameof(connectorCode), 64);
        ProviderType = NormalizeRequired(providerType, nameof(providerType), 64);
        InternalObjectType = NormalizeRequired(internalObjectType, nameof(internalObjectType), 64);
        InternalObjectId = NormalizeRequired(internalObjectId, nameof(internalObjectId), 128);
        ExternalObjectType = NormalizeRequired(externalObjectType, nameof(externalObjectType), 64);
        ExternalObjectId = NormalizeRequired(externalObjectId, nameof(externalObjectId), 256);
        TenantId = tenantId;
        Source = Normalize(source, 32, "Manual");
        Status = Normalize(status, 32, IntegrationBindingStatus.Bound);
        Remark = Normalize(remark, 512);
    }

    public void Update(string? source, string? status, string? remark)
    {
        Source = Normalize(source, 32, Source);
        Status = Normalize(status, 32, Status);
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
