namespace General.Admin.Platform;

public class PlatformScheduledJobRecordQueryInput
{
    public int MaxResultCount { get; set; } = 50;

    public DateTime? StartedFrom { get; set; }

    public DateTime? StartedTo { get; set; }

    public string? Status { get; set; }

    public string? TriggerMode { get; set; }
}
