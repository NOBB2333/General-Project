using System.ComponentModel.DataAnnotations;

namespace General.Admin.PhaseOne;

public class PhaseOneMenuSaveInput
{
    [Required]
    [MaxLength(32)]
    public string AppCode { get; set; } = PhaseOneAppCodes.Platform;

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
    public PhaseOneMenuType Type { get; set; } = PhaseOneMenuType.Menu;

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
