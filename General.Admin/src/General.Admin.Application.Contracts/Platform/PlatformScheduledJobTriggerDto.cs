namespace General.Admin.Platform;

public class PlatformScheduledJobTriggerDto
{
    public string CronExpression { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public DateTime? LastRunTime { get; set; }

    public string LastRunResult { get; set; } = string.Empty;

    public DateTime? NextRunTime { get; set; }

    public string TriggerKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
