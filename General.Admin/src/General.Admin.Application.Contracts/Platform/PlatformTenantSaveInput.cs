using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformTenantSaveInput
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
