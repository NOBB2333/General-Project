using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 质量检查单：定义检查项模板，用于质量门禁和验收流程中的逐项核查。
/// </summary>
public class ProjectQualityChecklist : FullAuditedAggregateRoot<Guid>
{
    public string Category { get; private set; }

    public string Description { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; }

    protected ProjectQualityChecklist()
    {
        Category = string.Empty;
        Description = string.Empty;
        Title = string.Empty;
    }

    public ProjectQualityChecklist(
        Guid id,
        Guid projectId,
        string title,
        string category,
        string description) : base(id)
    {
        ProjectId = projectId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Category = Check.NotNullOrWhiteSpace(category, nameof(category), 64);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 1024);
    }
}
