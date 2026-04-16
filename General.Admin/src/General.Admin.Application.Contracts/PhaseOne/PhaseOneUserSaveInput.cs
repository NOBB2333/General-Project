using System.ComponentModel.DataAnnotations;

namespace General.Admin.PhaseOne;

public class PhaseOneUserSaveInput
{
    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? OrganizationUnitId { get; set; }

    public List<string> RoleNames { get; set; } = [];
}
