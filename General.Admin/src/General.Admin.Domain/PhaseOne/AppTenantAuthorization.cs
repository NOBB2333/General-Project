using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class AppTenantAuthorization : FullAuditedAggregateRoot<Guid>
{
    public Guid? AdminUserId { get; private set; }

    public string ApiBlacklist { get; private set; }

    public bool IsActive { get; private set; }

    public string MenuIds { get; private set; }

    public string? Remark { get; private set; }

    public Guid TenantId { get; private set; }

    protected AppTenantAuthorization()
    {
        ApiBlacklist = "[]";
        IsActive = true;
        MenuIds = "[]";
    }

    public AppTenantAuthorization(
        Guid id,
        Guid tenantId,
        string menuIds,
        string apiBlacklist,
        bool isActive,
        Guid? adminUserId = null,
        string? remark = null) : base(id)
    {
        TenantId = tenantId;
        MenuIds = NormalizeJson(menuIds);
        ApiBlacklist = NormalizeJson(apiBlacklist);
        IsActive = isActive;
        AdminUserId = adminUserId;
        Remark = NormalizeRemark(remark);
    }

    public void Update(string menuIds, string apiBlacklist, bool isActive, Guid? adminUserId = null, string? remark = null)
    {
        MenuIds = NormalizeJson(menuIds);
        ApiBlacklist = NormalizeJson(apiBlacklist);
        IsActive = isActive;
        AdminUserId = adminUserId;
        Remark = NormalizeRemark(remark);
    }

    public void UpdateStatus(bool isActive)
    {
        IsActive = isActive;
    }

    private static string NormalizeJson(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
    }

    private static string? NormalizeRemark(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 256 ? trimmed : trimmed[..256];
    }
}
