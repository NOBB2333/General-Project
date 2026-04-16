using System.ComponentModel.DataAnnotations;

namespace General.Admin.PhaseOne;

public class PhaseOneRoleSaveInput
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;
}
