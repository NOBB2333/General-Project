using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppScheduledJobRecord : CreationAuditedEntity<Guid>
{
    public long? DurationMilliseconds { get; private set; }

    public DateTime? EndTime { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string InstanceId { get; private set; }

    public string JobKey { get; private set; }

    public string JobTitle { get; private set; }

    public string? Result { get; private set; }

    public DateTime StartTime { get; private set; }

    public string Status { get; private set; }

    public string TriggerKey { get; private set; }

    public string TriggerMode { get; private set; }

    protected AppScheduledJobRecord()
    {
        InstanceId = string.Empty;
        JobKey = string.Empty;
        JobTitle = string.Empty;
        Status = PlatformScheduledJobRecordStatus.Running;
        TriggerKey = string.Empty;
        TriggerMode = string.Empty;
    }

    public AppScheduledJobRecord(
        Guid id,
        string jobKey,
        string jobTitle,
        string triggerKey,
        string triggerMode,
        DateTime startTime,
        string instanceId) : base(id)
    {
        JobKey = Check.NotNullOrWhiteSpace(jobKey, nameof(jobKey), 64);
        JobTitle = Check.NotNullOrWhiteSpace(jobTitle, nameof(jobTitle), 128);
        TriggerKey = triggerKey?.Trim() ?? string.Empty;
        if (TriggerKey.Length > 64)
        {
            TriggerKey = TriggerKey[..64];
        }
        TriggerMode = Check.NotNullOrWhiteSpace(triggerMode, nameof(triggerMode), 16);
        StartTime = startTime;
        InstanceId = Check.NotNullOrWhiteSpace(instanceId, nameof(instanceId), 128);
        Status = PlatformScheduledJobRecordStatus.Running;
    }

    public void MarkSuccess(DateTime endTime, string result)
    {
        EndTime = endTime;
        DurationMilliseconds = CalculateDurationMilliseconds(endTime);
        Status = PlatformScheduledJobRecordStatus.Success;
        Result = Normalize(result, 512);
        ErrorMessage = null;
    }

    public void MarkFailed(DateTime endTime, string result, string errorMessage)
    {
        EndTime = endTime;
        DurationMilliseconds = CalculateDurationMilliseconds(endTime);
        Status = PlatformScheduledJobRecordStatus.Failed;
        Result = Normalize(result, 512);
        ErrorMessage = Normalize(errorMessage, 2048);
    }

    public void MarkSkipped(DateTime endTime, string result)
    {
        EndTime = endTime;
        DurationMilliseconds = CalculateDurationMilliseconds(endTime);
        Status = PlatformScheduledJobRecordStatus.Skipped;
        Result = Normalize(result, 512);
        ErrorMessage = null;
    }

    public void MarkCancelled(DateTime endTime, string result)
    {
        EndTime = endTime;
        DurationMilliseconds = CalculateDurationMilliseconds(endTime);
        Status = PlatformScheduledJobRecordStatus.Cancelled;
        Result = Normalize(result, 512);
        ErrorMessage = null;
    }

    private long CalculateDurationMilliseconds(DateTime endTime)
    {
        return Math.Max(0, (long)(endTime - StartTime).TotalMilliseconds);
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
