using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformAdminResetPasswordInput
{
    [Required]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
