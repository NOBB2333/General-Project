namespace General.Admin.Platform;

public class PlatformScheduledJobSaveInput
{
    public string CronExpression { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string HandlerKey { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public string JobKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
}
