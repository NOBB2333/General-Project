using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class AppUpdateLog : FullAuditedAggregateRoot<Guid>
{
    public Guid AuthorUserId { get; private set; }

    public string? ImpactScope { get; private set; }

    public DateTime PublishedAt { get; private set; }

    public string Summary { get; private set; }

    public string Title { get; private set; }

    public string Version { get; private set; }

    protected AppUpdateLog()
    {
        Summary = string.Empty;
        Title = string.Empty;
        Version = string.Empty;
    }

    public AppUpdateLog(
        Guid id,
        Guid authorUserId,
        string version,
        string title,
        string summary,
        DateTime publishedAt,
        string? impactScope) : base(id)
    {
        AuthorUserId = authorUserId;
        Version = Check.NotNullOrWhiteSpace(version, nameof(version), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Summary = Check.NotNullOrWhiteSpace(summary, nameof(summary), 2048);
        PublishedAt = publishedAt;
        ImpactScope = string.IsNullOrWhiteSpace(impactScope) ? null : impactScope.Trim();
    }

    public void Update(string version, string title, string summary, DateTime publishedAt, string? impactScope)
    {
        Version = Check.NotNullOrWhiteSpace(version, nameof(version), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Summary = Check.NotNullOrWhiteSpace(summary, nameof(summary), 2048);
        PublishedAt = publishedAt;
        ImpactScope = string.IsNullOrWhiteSpace(impactScope) ? null : impactScope.Trim();
    }
}
