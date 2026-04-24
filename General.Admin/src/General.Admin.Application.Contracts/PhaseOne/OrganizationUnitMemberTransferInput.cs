using System.ComponentModel.DataAnnotations;

namespace General.Admin.PhaseOne;

public class OrganizationUnitMemberTransferInput
{
    [MinLength(1)]
    public List<Guid> UserIds { get; set; } = [];

    [Required]
    public Guid TargetOrganizationUnitId { get; set; }
}
