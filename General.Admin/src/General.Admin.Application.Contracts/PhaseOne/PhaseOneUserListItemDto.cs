namespace General.Admin.PhaseOne;

public class PhaseOneUserListItemDto
{
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<PhaseOneExternalAccountMappingDto> ExternalAccounts { get; set; } = [];

    public string? ExternalSource { get; set; }

    public string? ExternalUserId { get; set; }

    public string? EmployeeNo { get; set; }

    public Guid Id { get; set; }

    public bool IsActive { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public List<string> OrganizationUnitNames { get; set; } = [];

    public string? PhoneNumber { get; set; }

    public List<string> Roles { get; set; } = [];

    public string? TenantName { get; set; }

    public string Username { get; set; } = string.Empty;
}
