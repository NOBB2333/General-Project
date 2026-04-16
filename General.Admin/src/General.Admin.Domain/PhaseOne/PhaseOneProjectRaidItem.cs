using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneProjectRaidItem : FullAuditedAggregateRoot<Guid>
{
    public string Level { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public DateTime? PlannedResolveDate { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    public string Type { get; private set; }

    protected PhaseOneProjectRaidItem()
    {
        Level = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
        Type = string.Empty;
    }

    public PhaseOneProjectRaidItem(
        Guid id,
        Guid projectId,
        string type,
        string title,
        string level,
        Guid ownerUserId,
        string status,
        DateTime? plannedResolveDate = null) : base(id)
    {
        ProjectId = projectId;
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256);
        Level = Check.NotNullOrWhiteSpace(level, nameof(level), 32);
        OwnerUserId = ownerUserId;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        PlannedResolveDate = plannedResolveDate;
    }
}
