using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

public class ProjectCycle : FullAuditedAggregateRoot<Guid>
{
    public DateTime? EndDate { get; private set; }

    public string Name { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public int Progress { get; private set; }

    public Guid ProjectId { get; private set; }

    public DateTime? StartDate { get; private set; }

    public string Status { get; private set; }

    public string Summary { get; private set; }

    public string Type { get; private set; }

    protected ProjectCycle()
    {
        Name = string.Empty;
        Status = string.Empty;
        Summary = string.Empty;
        Type = string.Empty;
    }

    public ProjectCycle(
        Guid id,
        Guid projectId,
        string type,
        string name,
        Guid ownerUserId,
        DateTime? startDate,
        DateTime? endDate,
        string status,
        int progress,
        string? summary = null) : base(id)
    {
        ProjectId = projectId;
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
        OwnerUserId = ownerUserId;
        StartDate = startDate;
        EndDate = endDate;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        Progress = progress < 0 ? 0 : Math.Min(progress, 100);
        Summary = summary?.Trim() ?? string.Empty;
    }
}
