using System.ComponentModel.DataAnnotations;

namespace General.Admin.Project;

public class ProjectSaveInput
{
    public decimal? BudgetTotalAmount { get; set; }

    public decimal? ContractTotalAmount { get; set; }

    [MaxLength(512)]
    public string? Description { get; set; }

    public bool IsKeyProject { get; set; }

    [Required]
    public Guid ManagerUserId { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid OrganizationUnitId { get; set; }

    public DateTime? PlannedEndDate { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    [Required]
    [MaxLength(32)]
    public string Priority { get; set; } = "中";

    [Required]
    [MaxLength(64)]
    public string ProjectCode { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? ProjectSource { get; set; }

    [MaxLength(64)]
    public string? ProjectType { get; set; }

    public decimal? ReceivedAmount { get; set; }

    [MaxLength(64)]
    public string? ShortName { get; set; }

    [Required]
    public Guid SponsorUserId { get; set; }

    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = "待规划";
}
