using Volo.Abp.Domain.Entities;

namespace General.Admin.Project;

/// <summary>
/// 项目验收记录与验收标准的关联。
/// </summary>
public class ProjectAcceptanceRecordCriteria : Entity<Guid>
{
    public Guid AcceptanceCriteriaId { get; private set; }

    public Guid AcceptanceRecordId { get; private set; }

    public Guid ProjectId { get; private set; }

    protected ProjectAcceptanceRecordCriteria()
    {
    }

    public ProjectAcceptanceRecordCriteria(
        Guid id,
        Guid projectId,
        Guid acceptanceRecordId,
        Guid acceptanceCriteriaId) : base(id)
    {
        ProjectId = projectId;
        AcceptanceRecordId = acceptanceRecordId;
        AcceptanceCriteriaId = acceptanceCriteriaId;
    }
}
