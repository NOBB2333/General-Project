using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 自动化任务运行记录。用于保存输入快照、输出草稿和失败信息。
/// </summary>
public class AiJobRun : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid AiJobId { get; private set; }

    public int DraftItemCount { get; private set; }

    public long ElapsedMilliseconds { get; private set; }

    public string ErrorMessage { get; private set; }

    public DateTime? FinishedTime { get; private set; }

    public string InputSnapshotJson { get; private set; }

    public string JobCode { get; private set; }

    public string JobName { get; private set; }

    public string JobType { get; private set; }

    public string OutputDraftJson { get; private set; }

    public string RunMode { get; private set; }

    public DateTime StartedTime { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiJobRun()
    {
        ErrorMessage = string.Empty;
        InputSnapshotJson = "{}";
        JobCode = string.Empty;
        JobName = string.Empty;
        JobType = string.Empty;
        OutputDraftJson = "{}";
        RunMode = "Manual";
        Status = "Running";
    }

    public AiJobRun(
        Guid id,
        Guid aiJobId,
        string jobCode,
        string jobName,
        string jobType,
        string runMode,
        DateTime startedTime,
        Guid? tenantId) : base(id)
    {
        AiJobId = aiJobId;
        JobCode = Check.NotNullOrWhiteSpace(jobCode, nameof(jobCode), 64).Trim();
        JobName = Check.NotNullOrWhiteSpace(jobName, nameof(jobName), 128).Trim();
        JobType = Check.NotNullOrWhiteSpace(jobType, nameof(jobType), 64).Trim();
        RunMode = NormalizeRunMode(runMode);
        StartedTime = startedTime;
        TenantId = tenantId;
        Status = "Running";
        ErrorMessage = string.Empty;
        InputSnapshotJson = "{}";
        OutputDraftJson = "{}";
    }

    public void Complete(
        string inputSnapshotJson,
        string outputDraftJson,
        int draftItemCount,
        long elapsedMilliseconds,
        DateTime finishedTime)
    {
        InputSnapshotJson = NormalizeJsonObject(inputSnapshotJson);
        OutputDraftJson = NormalizeJsonObject(outputDraftJson);
        DraftItemCount = Math.Max(0, draftItemCount);
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        FinishedTime = finishedTime;
        ErrorMessage = string.Empty;
        Status = "Success";
    }

    public void Fail(
        string inputSnapshotJson,
        string errorMessage,
        long elapsedMilliseconds,
        DateTime finishedTime)
    {
        InputSnapshotJson = NormalizeJsonObject(inputSnapshotJson);
        ErrorMessage = Normalize(errorMessage, 4000);
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        FinishedTime = finishedTime;
        Status = "Failed";
    }

    public void Cancel(
        string inputSnapshotJson,
        string message,
        long elapsedMilliseconds,
        DateTime finishedTime)
    {
        InputSnapshotJson = NormalizeJsonObject(inputSnapshotJson);
        ErrorMessage = Normalize(message, 4000);
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        FinishedTime = finishedTime;
        Status = "Cancelled";
    }

    private static string NormalizeRunMode(string? value)
    {
        var normalized = Normalize(value, 32, "Manual");
        return normalized.Equals("manual", StringComparison.OrdinalIgnoreCase) ? "Manual" :
            normalized.Equals("scheduled", StringComparison.OrdinalIgnoreCase) ? "Scheduled" :
            normalized;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJsonObject(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 64_000 ? normalized : normalized[..64_000];
    }
}
