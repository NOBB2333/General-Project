namespace General.Admin.PhaseOne;

public class PlatformScheduledJobDto
{
    public string CronExpression { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public bool IsRunning { get; set; }

    public string JobKey { get; set; } = string.Empty;

    public DateTime? LastRunTime { get; set; }

    public string LastRunResult { get; set; } = string.Empty;

    public DateTime? NextRunTime { get; set; }

    public string Title { get; set; } = string.Empty;
}
