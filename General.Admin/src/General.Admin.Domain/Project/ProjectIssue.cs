using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 缺陷与问题：支持关联验收标准和质量检查单，责任人按产品/开发/测试结构化（UserId）。
/// </summary>
public class ProjectIssue : FullAuditedAggregateRoot<Guid>
{
    public string? DeveloperOwnerName { get; private set; }

    public Guid? DeveloperOwnerUserId { get; private set; }

    public Guid? AcceptanceCriteriaId { get; private set; }

    public DateTime? DueDate { get; private set; }

    public string Level { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string? ProductOwnerName { get; private set; }

    public Guid? ProductOwnerUserId { get; private set; }

    public Guid? QualityChecklistId { get; private set; }

    public string? RequirementTitle { get; private set; }

    public string Status { get; private set; }

    public string? TesterOwnerName { get; private set; }

    public Guid? TesterOwnerUserId { get; private set; }

    public string Title { get; private set; }

    public string Type { get; private set; }

    protected ProjectIssue()
    {
        Level = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
        Type = string.Empty;
    }

    public ProjectIssue(
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
        string? testerOwnerName = null,
        Guid? productOwnerUserId = null,
        Guid? developerOwnerUserId = null,
        Guid? testerOwnerUserId = null,
        Guid? acceptanceCriteriaId = null,
        Guid? qualityChecklistId = null) : base(id)
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
        ProductOwnerUserId = productOwnerUserId;
        DeveloperOwnerUserId = developerOwnerUserId;
        TesterOwnerUserId = testerOwnerUserId;
        AcceptanceCriteriaId = acceptanceCriteriaId;
        QualityChecklistId = qualityChecklistId;
    }

    public void LinkQualityObjects(Guid? acceptanceCriteriaId, Guid? qualityChecklistId)
    {
        AcceptanceCriteriaId = acceptanceCriteriaId;
        QualityChecklistId = qualityChecklistId;
    }

    public void Update(
        string type,
        string title,
        string level,
        Guid ownerUserId,
        string status,
        DateTime? dueDate = null,
        string? requirementTitle = null,
        string? productOwnerName = null,
        string? developerOwnerName = null,
        string? testerOwnerName = null,
        Guid? productOwnerUserId = null,
        Guid? developerOwnerUserId = null,
        Guid? testerOwnerUserId = null,
        Guid? acceptanceCriteriaId = null,
        Guid? qualityChecklistId = null)
    {
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
        ProductOwnerUserId = productOwnerUserId;
        DeveloperOwnerUserId = developerOwnerUserId;
        TesterOwnerUserId = testerOwnerUserId;
        AcceptanceCriteriaId = acceptanceCriteriaId;
        QualityChecklistId = qualityChecklistId;
    }
}
