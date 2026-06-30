using Volo.Abp.Domain.Entities;

namespace General.Admin.Project;

/// <summary>
/// 项目验收记录与需求的关联。
/// </summary>
public class ProjectAcceptanceRecordRequirement : Entity<Guid>
{
    public Guid AcceptanceRecordId { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid RequirementId { get; private set; }

    protected ProjectAcceptanceRecordRequirement()
    {
    }

    public ProjectAcceptanceRecordRequirement(
        Guid id,
        Guid projectId,
        Guid acceptanceRecordId,
        Guid requirementId) : base(id)
    {
        ProjectId = projectId;
        AcceptanceRecordId = acceptanceRecordId;
        RequirementId = requirementId;
    }
}
