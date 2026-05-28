using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 验收标准：定义项目、里程碑或任务级的验收条件，关闭/完成前需确认通过。
/// </summary>
public class ProjectAcceptanceCriteria : FullAuditedAggregateRoot<Guid>
{
    public string Description { get; private set; }

    public Guid? MilestoneId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Status { get; private set; }

    public Guid? TaskId { get; private set; }

    public string Title { get; private set; }

    protected ProjectAcceptanceCriteria()
    {
        Description = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
    }

    public ProjectAcceptanceCriteria(
        Guid id,
        Guid projectId,
        string title,
        string description,
        string status,
        Guid? milestoneId = null,
        Guid? taskId = null) : base(id)
    {
        ProjectId = projectId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Description = Check.NotNullOrWhiteSpace(description, nameof(description), 1024);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        MilestoneId = milestoneId;
        TaskId = taskId;
    }
}
