namespace General.Admin.Platform;

public class MenuPermissionTreeDto
{
    public string AppCode { get; set; } = string.Empty;

    public List<MenuPermissionTreeDto> Children { get; set; } = [];

    public string? Component { get; set; }

    public string? Icon { get; set; }

    public Guid Id { get; set; }

    public bool IsEnabled { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public string Path { get; set; } = string.Empty;

    public string? PermissionCode { get; set; }

    public int Order { get; set; }

    public string Title { get; set; } = string.Empty;

    public PlatformMenuType Type { get; set; }
}
