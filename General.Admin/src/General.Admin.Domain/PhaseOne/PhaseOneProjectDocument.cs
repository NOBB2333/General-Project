using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneProjectDocument : FullAuditedAggregateRoot<Guid>
{
    public string Category { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Status { get; private set; }

    public string Summary { get; private set; }

    public string Title { get; private set; }

    public string Version { get; private set; }

    protected PhaseOneProjectDocument()
    {
        Category = string.Empty;
        Status = string.Empty;
        Summary = string.Empty;
        Title = string.Empty;
        Version = string.Empty;
    }

    public PhaseOneProjectDocument(
        Guid id,
        Guid projectId,
        string category,
        string title,
        string version,
        Guid ownerUserId,
        string status,
        string? summary = null) : base(id)
    {
        ProjectId = projectId;
        Category = Check.NotNullOrWhiteSpace(category, nameof(category), 32);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Version = Check.NotNullOrWhiteSpace(version, nameof(version), 32);
        OwnerUserId = ownerUserId;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        Summary = summary?.Trim() ?? string.Empty;
    }
}
