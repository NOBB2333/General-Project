namespace General.Admin.PhaseOne;

public class PhaseOneRoleDto
{
    public string Description { get; set; } = string.Empty;

    public string HomePath { get; set; } = "/platform/workspace";

    public Guid Id { get; set; }

    public int MenuCount { get; set; }

    public string Name { get; set; } = string.Empty;

    public int UserCount { get; set; }
}
