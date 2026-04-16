using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneProjectIssue : FullAuditedAggregateRoot<Guid>
{
    public string? DeveloperOwnerName { get; private set; }

    public DateTime? DueDate { get; private set; }

    public string Level { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string? ProductOwnerName { get; private set; }

    public string? RequirementTitle { get; private set; }

    public string Status { get; private set; }

    public string? TesterOwnerName { get; private set; }

    public string Title { get; private set; }

    public string Type { get; private set; }

    protected PhaseOneProjectIssue()
    {
        Level = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
        Type = string.Empty;
    }

    public PhaseOneProjectIssue(
        Guid id,
        Guid projectId,
        string type,
        string title,
        string level,
        Guid ownerUserId,
        string status,
        DateTime? dueDate = null,
        string? requirementTitle = null,
        string? productOwnerName = null,
        string? developerOwnerName = null,
        string? testerOwnerName = null) : base(id)
    {
        ProjectId = projectId;
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256);
        Level = Check.NotNullOrWhiteSpace(level, nameof(level), 32);
        OwnerUserId = ownerUserId;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        DueDate = dueDate;
        RequirementTitle = requirementTitle?.Trim();
        ProductOwnerName = productOwnerName?.Trim();
        DeveloperOwnerName = developerOwnerName?.Trim();
        TesterOwnerName = testerOwnerName?.Trim();
    }
}
