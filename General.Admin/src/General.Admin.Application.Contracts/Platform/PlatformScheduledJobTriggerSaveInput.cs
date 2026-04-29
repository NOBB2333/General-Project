namespace General.Admin.Platform;

public class PlatformScheduledJobTriggerSaveInput
{
    public string CronExpression { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public string Title { get; set; } = string.Empty;

    public string TriggerKey { get; set; } = string.Empty;
}
