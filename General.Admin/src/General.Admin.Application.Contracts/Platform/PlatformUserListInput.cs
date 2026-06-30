namespace General.Admin.Platform;

public class PlatformUserListInput
{
    public string? Keyword { get; set; }

    public string? UserName { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? EmployeeNo { get; set; }

    public string? PhoneNumber { get; set; }

    public Guid? OrganizationUnitId { get; set; }

    public bool? IsActive { get; set; }

    public string? RoleName { get; set; }

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }
}
