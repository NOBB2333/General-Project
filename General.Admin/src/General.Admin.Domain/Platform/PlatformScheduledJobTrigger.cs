using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class PlatformScheduledJobTrigger : FullAuditedAggregateRoot<Guid>
{
    public string CronExpression { get; private set; }

    public string Description { get; private set; }

    public bool IsEnabled { get; private set; }

    public Guid JobId { get; private set; }

    public DateTime? LastRunTime { get; private set; }

    public string LastRunResult { get; private set; }

    public DateTime? NextRunTime { get; private set; }

    public string TriggerKey { get; private set; }

    public string Title { get; private set; }

    protected PlatformScheduledJobTrigger()
    {
        CronExpression = string.Empty;
        Description = string.Empty;
        LastRunResult = string.Empty;
        Title = string.Empty;
        TriggerKey = string.Empty;
    }

    public PlatformScheduledJobTrigger(
        Guid id,
        Guid jobId,
        string triggerKey,
        string title,
        string cronExpression,
        string description,
        bool isEnabled,
        DateTime? nextRunTime = null,
        DateTime? lastRunTime = null,
        string? lastRunResult = null) : base(id)
    {
        JobId = jobId;
        TriggerKey = Check.NotNullOrWhiteSpace(triggerKey, nameof(triggerKey), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        CronExpression = Check.NotNullOrWhiteSpace(cronExpression, nameof(cronExpression), 64);
        Description = description?.Trim() ?? string.Empty;
        IsEnabled = isEnabled;
        NextRunTime = nextRunTime;
        LastRunTime = lastRunTime;
        LastRunResult = NormalizeResult(lastRunResult);
    }

    public void Update(
        string title,
        string cronExpression,
        string description,
        bool isEnabled,
        DateTime? nextRunTime)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        CronExpression = Check.NotNullOrWhiteSpace(cronExpression, nameof(cronExpression), 64);
        Description = description?.Trim() ?? string.Empty;
        IsEnabled = isEnabled;
        NextRunTime = nextRunTime;
    }

    public void Toggle(bool isEnabled, DateTime? nextRunTime)
    {
        IsEnabled = isEnabled;
        NextRunTime = nextRunTime;
    }

    public void MarkRun(DateTime runTime, string result, DateTime? nextRunTime)
    {
        LastRunTime = runTime;
        LastRunResult = NormalizeResult(result);
        NextRunTime = nextRunTime;
    }

    private static string NormalizeResult(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= 256 ? normalized : normalized[..256];
    }
}
