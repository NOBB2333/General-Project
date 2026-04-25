namespace General.Admin.Platform;

public class OrganizationUnitTreeDto
{
    public List<OrganizationUnitTreeDto> Children { get; set; } = [];

    public string Code { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public int DirectMemberCount { get; set; }

    public Guid Id { get; set; }

    public int MemberCount { get; set; }

    public Guid? ParentId { get; set; }
}
