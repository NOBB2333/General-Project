using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

public class ProjectSprint : FullAuditedAggregateRoot<Guid>
{
    public DateTime? EndDate { get; private set; }

    public string Name { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public int Progress { get; private set; }

    public Guid ProjectId { get; private set; }

    public string SprintCode { get; private set; }

    public DateTime? StartDate { get; private set; }

    public string Status { get; private set; }

    public string Summary { get; private set; }

    protected ProjectSprint()
    {
        Name = string.Empty;
        SprintCode = string.Empty;
        Status = string.Empty;
        Summary = string.Empty;
    }

    public ProjectSprint(
        Guid id,
        Guid projectId,
        string sprintCode,
        string name,
        Guid ownerUserId,
        DateTime? startDate,
        DateTime? endDate,
        string status,
        int progress,
        string? summary = null) : base(id)
    {
        ProjectId = projectId;
        SprintCode = Check.NotNullOrWhiteSpace(sprintCode, nameof(sprintCode), 64);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
        OwnerUserId = ownerUserId;
        StartDate = startDate;
        EndDate = endDate;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        Progress = progress < 0 ? 0 : Math.Min(progress, 100);
        Summary = summary?.Trim() ?? string.Empty;
    }

    public void Update(
        string name,
        Guid ownerUserId,
        DateTime? startDate,
        DateTime? endDate,
        string status,
        int progress,
        string? summary = null)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
        OwnerUserId = ownerUserId;
        StartDate = startDate;
        EndDate = endDate;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        Progress = progress < 0 ? 0 : Math.Min(progress, 100);
        Summary = summary?.Trim() ?? string.Empty;
    }
}
