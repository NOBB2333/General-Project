using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PlatformScheduledJob : FullAuditedAggregateRoot<Guid>
{
    public string CronExpression { get; private set; }

    public string Description { get; private set; }

    public bool IsEnabled { get; private set; }

    public string JobKey { get; private set; }

    public DateTime? LastRunTime { get; private set; }

    public string LastRunResult { get; private set; }

    public DateTime? NextRunTime { get; private set; }

    public string Title { get; private set; }

    protected PlatformScheduledJob()
    {
        CronExpression = string.Empty;
        Description = string.Empty;
        JobKey = string.Empty;
        LastRunResult = string.Empty;
        Title = string.Empty;
    }

    public PlatformScheduledJob(
        Guid id,
        string jobKey,
        string title,
        string cronExpression,
        string description,
        bool isEnabled,
        DateTime? nextRunTime = null,
        DateTime? lastRunTime = null,
        string? lastRunResult = null) : base(id)
    {
        JobKey = Check.NotNullOrWhiteSpace(jobKey, nameof(jobKey), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        CronExpression = Check.NotNullOrWhiteSpace(cronExpression, nameof(cronExpression), 64);
        Description = description?.Trim() ?? string.Empty;
        IsEnabled = isEnabled;
        NextRunTime = nextRunTime;
        LastRunTime = lastRunTime;
        LastRunResult = lastRunResult?.Trim() ?? string.Empty;
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public void MarkRun(DateTime runTime, string result, DateTime? nextRunTime)
    {
        LastRunTime = runTime;
        LastRunResult = result.Trim();
        NextRunTime = nextRunTime;
    }
}
