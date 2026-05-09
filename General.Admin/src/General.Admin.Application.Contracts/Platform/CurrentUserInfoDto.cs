namespace General.Admin.Platform;

public class CurrentUserInfoDto
{
    public string? Avatar { get; set; }

    public string Desc { get; set; } = string.Empty;

    public string HomePath { get; set; } = "/platform/workspace";

    public bool IsHostTenantOperation { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public Guid? OperationTenantId { get; set; }

    public string? OperationTenantName { get; set; }

    public List<string> OrganizationUnitNames { get; set; } = [];

    public string RealName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];

    public string Token { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
}
