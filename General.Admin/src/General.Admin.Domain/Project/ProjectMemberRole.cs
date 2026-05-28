using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目成员角色：将成员的项目角色从逗号分隔字符串升级为结构化记录，支撑权限计算。
/// </summary>
public class ProjectMemberRole : FullAuditedAggregateRoot<Guid>
{
    public Guid ProjectId { get; private set; }

    public Guid ProjectMemberId { get; private set; }

    public string RoleName { get; private set; }

    public Guid UserId { get; private set; }

    protected ProjectMemberRole()
    {
        RoleName = string.Empty;
    }

    public ProjectMemberRole(
        Guid id,
        Guid projectId,
        Guid projectMemberId,
        Guid userId,
        string roleName) : base(id)
    {
        ProjectId = projectId;
        ProjectMemberId = projectMemberId;
        UserId = userId;
        RoleName = Check.NotNullOrWhiteSpace(roleName, nameof(roleName), 64);
    }
}
