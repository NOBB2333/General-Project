namespace General.Admin.Platform;

public class BackendRouteMetaDto
{
    public string Title { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public int? Order { get; set; }

    public bool? AffixTab { get; set; }

    public bool? KeepAlive { get; set; }

    public bool? HideInBreadcrumb { get; set; }

    public bool? HideInMenu { get; set; }

    public bool? HideInTab { get; set; }

    public string? Link { get; set; }

    public bool? MenuVisibleWithForbidden { get; set; }
}
