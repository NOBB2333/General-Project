namespace General.Admin.PhaseOne;

public class OrganizationUnitTreeDto
{
    public List<OrganizationUnitTreeDto> Children { get; set; } = [];

    public string Code { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public int MemberCount { get; set; }

    public Guid? ParentId { get; set; }
}
