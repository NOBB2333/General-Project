namespace General.Admin.Platform;

public class LoginResultDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string HomePath { get; set; } = "/platform/workspace";

    public bool IsHostTenantOperation { get; set; }

    public Guid? OperationTenantId { get; set; }

    public string? OperationTenantName { get; set; }

    public string RealName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];

    public string Username { get; set; } = string.Empty;
}
