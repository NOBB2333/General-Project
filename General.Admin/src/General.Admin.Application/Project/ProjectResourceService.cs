using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Project;

public class ProjectResourceService : ITransientDependency
{
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<ProjectMemberRole, Guid> _projectMemberRoleRepository;

    public ProjectResourceService(
        ProjectAccessGuard projectAccessGuard,
        IRepository<ProjectMember, Guid> projectMemberRepository,
        IRepository<ProjectMemberRole, Guid> projectMemberRoleRepository)
    {
        _projectAccessGuard = projectAccessGuard;
        _projectMemberRepository = projectMemberRepository;
        _projectMemberRoleRepository = projectMemberRoleRepository;
    }

    public async Task UpdateMemberRolesAsync(Guid projectMemberId, ProjectMemberRolesUpdateInput input)
    {
        var member = await _projectMemberRepository.GetAsync(projectMemberId);
        await _projectAccessGuard.EnsureProjectAccessAsync(member.ProjectId);
        var roleNames = input.RoleNames
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (roleNames.Count == 0)
        {
            throw new BusinessException("项目成员至少需要一个角色。");
        }

        var existingRoles = await _projectMemberRoleRepository.GetListAsync(item => item.ProjectMemberId == projectMemberId);
        if (existingRoles.Count > 0)
        {
            await _projectMemberRoleRepository.DeleteManyAsync(existingRoles);
        }

        var roles = roleNames
            .Select(roleName => new ProjectMemberRole(Guid.NewGuid(), member.ProjectId, member.Id, member.UserId, roleName))
            .ToList();
        await _projectMemberRoleRepository.InsertManyAsync(roles, autoSave: true);
    }
}
