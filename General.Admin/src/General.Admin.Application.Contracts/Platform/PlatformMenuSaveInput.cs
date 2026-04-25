using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformMenuSaveInput
{
    [Required]
    [MaxLength(32)]
    public string AppCode { get; set; } = PlatformAppCodes.Platform;

    public Guid? ParentId { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Path { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Component { get; set; }

    [Required]
    public PlatformMenuType Type { get; set; } = PlatformMenuType.Menu;

    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Icon { get; set; }

    [MaxLength(128)]
    public string? PermissionCode { get; set; }

    public int Order { get; set; } = 10;

    public bool IsEnabled { get; set; } = true;
}
