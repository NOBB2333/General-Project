namespace General.Admin.Platform;

public class PlatformRoleDto
{
    public List<string> ApiBlacklist { get; set; } = [];

    public List<Guid> AccountUserIds { get; set; } = [];

    public string AccountScopeMode { get; set; } = string.Empty;

    public List<Guid> CustomOrganizationUnitIds { get; set; } = [];

    public string DataScopeMode { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string HomePath { get; set; } = "/platform/workspace";

    public Guid Id { get; set; }

    public int MenuAuthorizationCount { get; set; }

    public int MenuCount { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool Status { get; set; } = true;

    public int UserCount { get; set; }
}
