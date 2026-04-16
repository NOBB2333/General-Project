namespace General.Admin.PhaseOne;

public class PhaseOneUserListItemDto
{
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public List<string> OrganizationUnitNames { get; set; } = [];

    public List<string> Roles { get; set; } = [];

    public string Username { get; set; } = string.Empty;
}
