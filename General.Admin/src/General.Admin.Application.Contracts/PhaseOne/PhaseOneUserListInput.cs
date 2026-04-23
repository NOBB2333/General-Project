namespace General.Admin.PhaseOne;

public class PhaseOneUserListInput
{
    public string? Keyword { get; set; }

    public Guid? OrganizationUnitId { get; set; }

    public bool? IsActive { get; set; }

    public string? RoleName { get; set; }
}
