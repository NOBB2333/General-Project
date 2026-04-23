using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class AppExternalAccountMapping : FullAuditedAggregateRoot<Guid>
{
    public DateTime BoundAt { get; private set; }

    public string ExternalSource { get; private set; }

    public string ExternalUserId { get; private set; }

    public DateTime? LastSyncedAt { get; private set; }

    public string? Remark { get; private set; }

    public string Status { get; private set; }

    public Guid UserId { get; private set; }

    protected AppExternalAccountMapping()
    {
        ExternalSource = string.Empty;
        ExternalUserId = string.Empty;
        Status = "active";
    }

    public AppExternalAccountMapping(
        Guid id,
        Guid userId,
        string externalSource,
        string externalUserId,
        string status,
        DateTime boundAt,
        DateTime? lastSyncedAt,
        string? remark) : base(id)
    {
        UserId = userId;
        ExternalSource = Check.NotNullOrWhiteSpace(externalSource, nameof(externalSource), 64).Trim();
        ExternalUserId = Check.NotNullOrWhiteSpace(externalUserId, nameof(externalUserId), 128).Trim();
        Status = NormalizeStatus(status);
        BoundAt = boundAt;
        LastSyncedAt = lastSyncedAt;
        Remark = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim();
    }

    public void Update(string status, DateTime? lastSyncedAt, string? remark)
    {
        Status = NormalizeStatus(status);
        LastSyncedAt = lastSyncedAt;
        Remark = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim();
    }

    private static string NormalizeStatus(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "active" : value.Trim().ToLowerInvariant();
    }
}
