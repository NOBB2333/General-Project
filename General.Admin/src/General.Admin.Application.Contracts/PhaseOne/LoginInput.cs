using System.ComponentModel.DataAnnotations;

namespace General.Admin.PhaseOne;

public class LoginInput
{
    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
