namespace General.Admin.Platform;

public class CurrentUserInfoDto
{
    public string? Avatar { get; set; }

    public string Desc { get; set; } = string.Empty;

    public string HomePath { get; set; } = "/platform/workspace";

    public DateTime? LastLoginTime { get; set; }

    public List<string> OrganizationUnitNames { get; set; } = [];

    public string RealName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];

    public string Token { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
}
