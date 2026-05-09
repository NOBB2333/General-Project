namespace General.Admin.Platform;

public class PlatformUserListInput
{
    public string? Keyword { get; set; }

    public Guid? OrganizationUnitId { get; set; }

    public bool? IsActive { get; set; }

    public string? RoleName { get; set; }

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }
}
