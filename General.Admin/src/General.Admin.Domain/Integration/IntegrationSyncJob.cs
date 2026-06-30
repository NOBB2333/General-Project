using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 外部系统同步任务定义。描述同步对象、方向、模式、Cron 和最近执行结果。
/// </summary>
public class IntegrationSyncJob : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ConfigJson { get; private set; }

    public string ConnectorCode { get; private set; }

    public Guid ConnectorId { get; private set; }

    public string CronExpression { get; private set; }

    public string Direction { get; private set; }

    public bool IsEnabled { get; private set; }

    public string JobCode { get; private set; }

    public string LastRunError { get; private set; }

    public string LastRunStatus { get; private set; }

    public string LastRunSummary { get; private set; }

    public DateTime? LastRunTime { get; private set; }

    public string Mode { get; private set; }

    public string Name { get; private set; }

    public string ProviderType { get; private set; }

    public string Remark { get; private set; }

    public string SyncType { get; private set; }

    public Guid? TenantId { get; private set; }

    protected IntegrationSyncJob()
    {
        ConfigJson = "{}";
        ConnectorCode = string.Empty;
        CronExpression = string.Empty;
        Direction = IntegrationSyncDirections.Inbound;
        JobCode = string.Empty;
        LastRunError = string.Empty;
        LastRunStatus = string.Empty;
        LastRunSummary = string.Empty;
        Mode = IntegrationSyncModes.Incremental;
        Name = string.Empty;
        ProviderType = string.Empty;
        Remark = string.Empty;
        SyncType = IntegrationSyncTypes.User;
    }

    public IntegrationSyncJob(
        Guid id,
        Guid connectorId,
        string connectorCode,
        string providerType,
        string jobCode,
        string name,
        string syncType,
        string direction,
        string mode,
        string configJson,
        string cronExpression,
        bool isEnabled,
        Guid? tenantId,
        string? remark = null) : base(id)
    {
        ConnectorId = connectorId;
        ConnectorCode = Normalize(connectorCode, 64);
        ProviderType = Normalize(providerType, 64);
        JobCode = Check.NotNullOrWhiteSpace(jobCode, nameof(jobCode), 64).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        SyncType = NormalizeOption(syncType, IntegrationSyncTypes.All, IntegrationSyncTypes.User, 32);
        Direction = NormalizeOption(direction, IntegrationSyncDirections.All, IntegrationSyncDirections.Inbound, 32);
        Mode = NormalizeOption(mode, IntegrationSyncModes.All, IntegrationSyncModes.Incremental, 32);
        ConfigJson = NormalizeJson(configJson, 16_000);
        CronExpression = Normalize(cronExpression, 128);
        IsEnabled = isEnabled;
        TenantId = tenantId;
        Remark = Normalize(remark, 512);
        LastRunStatus = string.Empty;
        LastRunSummary = string.Empty;
        LastRunError = string.Empty;
    }

    public void Update(
        Guid connectorId,
        string connectorCode,
        string providerType,
        string name,
        string syncType,
        string direction,
        string mode,
        string configJson,
        string cronExpression,
        bool isEnabled,
        string? remark)
    {
        ConnectorId = connectorId;
        ConnectorCode = Normalize(connectorCode, 64);
        ProviderType = Normalize(providerType, 64);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        SyncType = NormalizeOption(syncType, IntegrationSyncTypes.All, IntegrationSyncTypes.User, 32);
        Direction = NormalizeOption(direction, IntegrationSyncDirections.All, IntegrationSyncDirections.Inbound, 32);
        Mode = NormalizeOption(mode, IntegrationSyncModes.All, IntegrationSyncModes.Incremental, 32);
        ConfigJson = NormalizeJson(configJson, 16_000);
        CronExpression = Normalize(cronExpression, 128);
        IsEnabled = isEnabled;
        Remark = Normalize(remark, 512);
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public void MarkRunResult(string status, string? summary, string? error)
    {
        LastRunTime = DateTime.UtcNow;
        LastRunStatus = Normalize(status, 32);
        LastRunSummary = Normalize(summary, 2000);
        LastRunError = Normalize(error, 2000);
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeOption(string? value, IReadOnlySet<string> allowedValues, string defaultValue, int maxLength)
    {
        var normalized = Normalize(value, maxLength);
        return allowedValues.Contains(normalized) ? normalized : defaultValue;
    }
}
