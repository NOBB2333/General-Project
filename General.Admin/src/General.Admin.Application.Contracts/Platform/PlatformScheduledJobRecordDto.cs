namespace General.Admin.Platform;

public class PlatformScheduledJobRecordDto
{
    public long? DurationMilliseconds { get; set; }

    public DateTime? EndTime { get; set; }

    public string? ErrorMessage { get; set; }

    public string InstanceId { get; set; } = string.Empty;

    public string JobKey { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string? Result { get; set; }

    public DateTime StartTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public string TriggerKey { get; set; } = string.Empty;

    public string TriggerMode { get; set; } = string.Empty;
}
