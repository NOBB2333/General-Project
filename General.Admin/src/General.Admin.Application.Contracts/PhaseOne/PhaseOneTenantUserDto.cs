namespace General.Admin.PhaseOne;

public class PhaseOneTenantUserDto
{
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public string Username { get; set; } = string.Empty;
}
