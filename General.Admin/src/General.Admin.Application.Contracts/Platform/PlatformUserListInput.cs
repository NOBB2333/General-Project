namespace General.Admin.Platform;

public class PlatformUserListInput
{
    public string? Keyword { get; set; }

    public Guid? OrganizationUnitId { get; set; }

    public bool? IsActive { get; set; }

    public string? RoleName { get; set; }
}
