using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目需求参与人：支持产品、开发、测试等角色多人参与，主负责人字段仍保留用于兼容列表和旧查询。
/// </summary>
public class ProjectTaskParticipant : FullAuditedAggregateRoot<Guid>
{
    public Guid ProjectId { get; private set; }

    public string RoleName { get; private set; }

    public Guid TaskId { get; private set; }

    public Guid UserId { get; private set; }

    protected ProjectTaskParticipant()
    {
        RoleName = string.Empty;
    }

    public ProjectTaskParticipant(
        Guid id,
        Guid projectId,
        Guid taskId,
        Guid userId,
        string roleName) : base(id)
    {
        ProjectId = projectId;
        TaskId = taskId;
        UserId = userId;
        RoleName = Check.NotNullOrWhiteSpace(roleName, nameof(roleName), 32);
    }
}
