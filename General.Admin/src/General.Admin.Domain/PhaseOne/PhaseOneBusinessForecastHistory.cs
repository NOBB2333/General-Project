using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneBusinessForecastHistory : FullAuditedAggregateRoot<Guid>
{
    public Guid ChangedByUserId { get; private set; }

    public string ChangeType { get; private set; }

    public string Metric { get; private set; }

    public string NewValue { get; private set; }

    public string OldValue { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Reason { get; private set; }

    public string RelatedCode { get; private set; }

    protected PhaseOneBusinessForecastHistory()
    {
        ChangeType = string.Empty;
        Metric = string.Empty;
        NewValue = string.Empty;
        OldValue = string.Empty;
        Reason = string.Empty;
        RelatedCode = string.Empty;
    }

    public PhaseOneBusinessForecastHistory(
        Guid id,
        Guid projectId,
        string metric,
        string oldValue,
        string newValue,
        Guid changedByUserId,
        string reason,
        string changeType,
        string? relatedCode = null) : base(id)
    {
        ProjectId = projectId;
        Metric = Check.NotNullOrWhiteSpace(metric, nameof(metric), 64);
        OldValue = Check.NotNullOrWhiteSpace(oldValue, nameof(oldValue), 64);
        NewValue = Check.NotNullOrWhiteSpace(newValue, nameof(newValue), 64);
        ChangedByUserId = changedByUserId;
        Reason = Check.NotNullOrWhiteSpace(reason, nameof(reason), 256);
        ChangeType = Check.NotNullOrWhiteSpace(changeType, nameof(changeType), 32);
        RelatedCode = relatedCode?.Trim() ?? string.Empty;
    }
}
