using System.ComponentModel.DataAnnotations;

namespace General.Admin.PhaseOne;

public class PhaseOneTenantSaveInput
{
    public Guid? AdminUserId { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? DefaultConnectionString { get; set; }

    [MaxLength(256)]
    public string? Remark { get; set; }
}
