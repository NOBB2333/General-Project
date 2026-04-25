namespace General.Admin.Platform;

public class PlatformTenantListItemDto
{
    public List<string> ApiBlacklist { get; set; } = [];

    public string? AdminEmail { get; set; }

    public string? AdminUserName { get; set; }

    public Guid? AdminUserId { get; set; }

    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }

    public bool HasDefaultConnectionString { get; set; }

    public bool IsActive { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Remark { get; set; }

    public string? DefaultConnectionStringDisplay { get; set; }

    public bool HasExplicitAuthorization { get; set; }
}
