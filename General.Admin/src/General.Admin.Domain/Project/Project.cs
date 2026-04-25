using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

public class Project : FullAuditedAggregateRoot<Guid>
{
    public decimal? BudgetTotalAmount { get; private set; }

    public decimal? ContractTotalAmount { get; private set; }

    public string? Description { get; private set; }

    public bool IsKeyProject { get; private set; }

    public Guid ManagerUserId { get; private set; }

    public string Name { get; private set; }

    public Guid OrganizationUnitId { get; private set; }

    public DateTime? PlannedEndDate { get; private set; }

    public DateTime? PlannedStartDate { get; private set; }

    public string Priority { get; private set; }

    public string ProjectCode { get; private set; }

    public string? ProjectSource { get; private set; }

    public string? ProjectType { get; private set; }

    public decimal? ReceivedAmount { get; private set; }

    public string? ShortName { get; private set; }

    public Guid SponsorUserId { get; private set; }

    public string Status { get; private set; }

    protected Project()
    {
        Name = string.Empty;
        Priority = string.Empty;
        ProjectCode = string.Empty;
        Status = string.Empty;
    }

    public Project(
        Guid id,
        string projectCode,
        string name,
        string? shortName,
        string? projectType,
        string? projectSource,
        Guid organizationUnitId,
        Guid managerUserId,
        Guid sponsorUserId,
        string priority,
        string status,
        DateTime? plannedStartDate,
        DateTime? plannedEndDate,
        bool isKeyProject,
        string? description,
        decimal? budgetTotalAmount = null,
        decimal? contractTotalAmount = null,
        decimal? receivedAmount = null) : base(id)
    {
        ProjectCode = Check.NotNullOrWhiteSpace(projectCode, nameof(projectCode), 64);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
        ShortName = shortName?.Trim();
        ProjectType = projectType?.Trim();
        ProjectSource = projectSource?.Trim();
        OrganizationUnitId = organizationUnitId;
        ManagerUserId = managerUserId;
        SponsorUserId = sponsorUserId;
        Priority = Check.NotNullOrWhiteSpace(priority, nameof(priority), 32);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        PlannedStartDate = plannedStartDate;
        PlannedEndDate = plannedEndDate;
        IsKeyProject = isKeyProject;
        Description = description?.Trim();
        BudgetTotalAmount = budgetTotalAmount;
        ContractTotalAmount = contractTotalAmount;
        ReceivedAmount = receivedAmount;
    }
}
