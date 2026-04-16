using System.ComponentModel.DataAnnotations;

namespace General.Admin.PhaseOne;

public class OrganizationUnitSaveInput
{
    [Required]
    [MaxLength(64)]
    public string DisplayName { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }
}
