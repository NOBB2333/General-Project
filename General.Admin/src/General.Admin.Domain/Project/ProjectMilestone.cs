using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

public class ProjectMilestone : FullAuditedAggregateRoot<Guid>
{
    public DateTime? ActualCompletionDate { get; private set; }

    public string Name { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public DateTime PlannedCompletionDate { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Status { get; private set; }

    protected ProjectMilestone()
    {
        Name = string.Empty;
        Status = string.Empty;
    }

    public ProjectMilestone(
        Guid id,
        Guid projectId,
        string name,
        Guid ownerUserId,
        DateTime plannedCompletionDate,
        string status,
        DateTime? actualCompletionDate = null) : base(id)
    {
        ProjectId = projectId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
        OwnerUserId = ownerUserId;
        PlannedCompletionDate = plannedCompletionDate;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        ActualCompletionDate = actualCompletionDate;
    }
}
