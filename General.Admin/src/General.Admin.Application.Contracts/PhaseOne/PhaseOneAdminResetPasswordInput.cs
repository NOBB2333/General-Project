using System.ComponentModel.DataAnnotations;

namespace General.Admin.PhaseOne;

public class PhaseOneAdminResetPasswordInput
{
    [Required]
    [MaxLength(128)]
    public string NewPassword { get; set; } = string.Empty;
}
