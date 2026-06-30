using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

/// <summary>
/// 外部系统同步执行记录。承接同步统计、冲突、错误、重试和人工跳过。
/// </summary>
public class IntegrationSyncRecord : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ConflictJson { get; private set; }

    public int ConflictCount { get; private set; }

    public string ConnectorCode { get; private set; }

    public Guid ConnectorId { get; private set; }

    public int CreatedCount { get; private set; }

    public int DurationMilliseconds { get; private set; }

    public string ErrorMessage { get; private set; }

    public int FailedCount { get; private set; }

    public DateTime? FinishedTime { get; private set; }

    public string Mode { get; private set; }

    public string ProviderType { get; private set; }

    public string RequestJson { get; private set; }

    public string ResultJson { get; private set; }

    public int RetryCount { get; private set; }

    public int SkippedCount { get; private set; }

    public DateTime? StartedTime { get; private set; }

    public string Status { get; private set; }

    public string Summary { get; private set; }

    public Guid SyncJobId { get; private set; }

    public string SyncType { get; private set; }

    public Guid? TenantId { get; private set; }

    public int TotalCount { get; private set; }

    public int UpdatedCount { get; private set; }

    protected IntegrationSyncRecord()
    {
        ConflictJson = "{}";
        ConnectorCode = string.Empty;
        ErrorMessage = string.Empty;
        Mode = IntegrationSyncModes.Incremental;
        ProviderType = string.Empty;
        RequestJson = "{}";
        ResultJson = "{}";
        Status = IntegrationSyncRecordStatus.Pending;
        Summary = string.Empty;
        SyncType = IntegrationSyncTypes.User;
    }

    public IntegrationSyncRecord(
        Guid id,
        Guid syncJobId,
        Guid connectorId,
        string connectorCode,
        string providerType,
        string syncType,
        string mode,
        string requestJson,
        Guid? tenantId) : base(id)
    {
        SyncJobId = syncJobId;
        ConnectorId = connectorId;
        ConnectorCode = Normalize(connectorCode, 64);
        ProviderType = Normalize(providerType, 64);
        SyncType = Normalize(syncType, 32, IntegrationSyncTypes.User);
        Mode = Normalize(mode, 32, IntegrationSyncModes.Incremental);
        RequestJson = NormalizeJson(requestJson, 16_000);
        TenantId = tenantId;
        ConflictJson = "{}";
        ResultJson = "{}";
        Status = IntegrationSyncRecordStatus.Pending;
        ErrorMessage = string.Empty;
        Summary = string.Empty;
    }

    public void MarkRunning()
    {
        StartedTime = DateTime.UtcNow;
        FinishedTime = null;
        Status = IntegrationSyncRecordStatus.Running;
        ErrorMessage = string.Empty;
    }

    public void MarkSucceeded(
        int totalCount,
        int createdCount,
        int updatedCount,
        int skippedCount,
        int conflictCount,
        int failedCount,
        string? resultJson,
        string? conflictJson,
        string? summary,
        int durationMilliseconds)
    {
        TotalCount = Math.Max(0, totalCount);
        CreatedCount = Math.Max(0, createdCount);
        UpdatedCount = Math.Max(0, updatedCount);
        SkippedCount = Math.Max(0, skippedCount);
        ConflictCount = Math.Max(0, conflictCount);
        FailedCount = Math.Max(0, failedCount);
        ResultJson = NormalizeJson(resultJson, 32_000);
        ConflictJson = NormalizeJson(conflictJson, 32_000);
        Summary = Normalize(summary, 2000);
        DurationMilliseconds = Math.Max(0, durationMilliseconds);
        ErrorMessage = string.Empty;
        FinishedTime = DateTime.UtcNow;
        Status = failedCount > 0 ? IntegrationSyncRecordStatus.Failed : IntegrationSyncRecordStatus.Succeeded;
    }

    public void MarkFailed(string errorMessage, string? resultJson, int durationMilliseconds)
    {
        FailedCount = Math.Max(1, FailedCount);
        ErrorMessage = Normalize(errorMessage, 4000);
        ResultJson = NormalizeJson(resultJson, 32_000);
        Summary = ErrorMessage;
        DurationMilliseconds = Math.Max(0, durationMilliseconds);
        FinishedTime = DateTime.UtcNow;
        Status = IntegrationSyncRecordStatus.Failed;
    }

    public void MarkSkipped(string? reason)
    {
        ErrorMessage = Normalize(reason, 2000);
        Summary = string.IsNullOrWhiteSpace(ErrorMessage) ? "同步记录已人工跳过。" : ErrorMessage;
        FinishedTime = DateTime.UtcNow;
        Status = IntegrationSyncRecordStatus.Skipped;
    }

    public void ResetForRetry()
    {
        RetryCount++;
        StartedTime = null;
        FinishedTime = null;
        DurationMilliseconds = 0;
        ErrorMessage = string.Empty;
        Status = IntegrationSyncRecordStatus.Pending;
    }

    public bool CanRetry()
    {
        return Status == IntegrationSyncRecordStatus.Failed;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
