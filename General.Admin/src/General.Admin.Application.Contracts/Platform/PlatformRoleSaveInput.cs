using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformRoleSaveInput
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;
}
