using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class OrganizationUnitSaveInput
{
    [Required]
    [MaxLength(64)]
    public string DisplayName { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }
}
