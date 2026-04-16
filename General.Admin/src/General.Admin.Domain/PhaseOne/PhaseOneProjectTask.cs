using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneProjectTask : FullAuditedAggregateRoot<Guid>
{
    public DateTime? ActualEndTime { get; private set; }

    public DateTime? ActualStartTime { get; private set; }

    public double? ActualWorkHours { get; private set; }

    public string? BlockReason { get; private set; }

    public string? ContractClause { get; private set; }

    public string? DeveloperOwnerName { get; private set; }

    public double? EstimatedWorkHours { get; private set; }

    public bool IsBlocked { get; private set; }

    public Guid OrganizationUnitId { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public DateTime? PlannedEndTime { get; private set; }

    public DateTime? PlannedStartTime { get; private set; }

    public string Priority { get; private set; }

    public string? ProductOwnerName { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Status { get; private set; }

    public string TaskCode { get; private set; }

    public string? TesterOwnerName { get; private set; }

    public string Title { get; private set; }

    protected PhaseOneProjectTask()
    {
        Priority = string.Empty;
        Status = string.Empty;
        TaskCode = string.Empty;
        Title = string.Empty;
    }

    public PhaseOneProjectTask(
        Guid id,
        Guid projectId,
        Guid organizationUnitId,
        string taskCode,
        string title,
        Guid ownerUserId,
        string status,
        string priority,
        DateTime? plannedStartTime,
        DateTime? plannedEndTime,
        bool isBlocked = false,
        string? blockReason = null,
        double? estimatedWorkHours = null,
        double? actualWorkHours = null,
        DateTime? actualStartTime = null,
        DateTime? actualEndTime = null,
        string? contractClause = null,
        string? productOwnerName = null,
        string? developerOwnerName = null,
        string? testerOwnerName = null) : base(id)
    {
        ProjectId = projectId;
        OrganizationUnitId = organizationUnitId;
        TaskCode = Check.NotNullOrWhiteSpace(taskCode, nameof(taskCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256);
        OwnerUserId = ownerUserId;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        Priority = Check.NotNullOrWhiteSpace(priority, nameof(priority), 32);
        PlannedStartTime = plannedStartTime;
        PlannedEndTime = plannedEndTime;
        IsBlocked = isBlocked;
        BlockReason = blockReason?.Trim();
        EstimatedWorkHours = estimatedWorkHours;
        ActualWorkHours = actualWorkHours;
        ActualStartTime = actualStartTime;
        ActualEndTime = actualEndTime;
        ContractClause = contractClause?.Trim();
        ProductOwnerName = productOwnerName?.Trim();
        DeveloperOwnerName = developerOwnerName?.Trim();
        TesterOwnerName = testerOwnerName?.Trim();
    }
}
