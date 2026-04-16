namespace General.Admin.PhaseOne;

public class BackendRouteDto
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string? Component { get; set; }

    public string? Redirect { get; set; }

    public BackendRouteMetaDto Meta { get; set; } = new();

    public List<BackendRouteDto> Children { get; set; } = [];
}
