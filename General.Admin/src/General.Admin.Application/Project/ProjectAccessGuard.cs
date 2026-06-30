using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace General.Admin.Project;

public class ProjectAccessGuard : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<Project, Guid> _projectRepository;

    public ProjectAccessGuard(
        ICurrentUser currentUser,
        CurrentUserDataScopeService dataScopeService,
        IRepository<ProjectMember, Guid> projectMemberRepository,
        IRepository<Project, Guid> projectRepository)
    {
        _currentUser = currentUser;
        _dataScopeService = dataScopeService;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
    }

    public async Task EnsureProjectAccessAsync(Guid projectId)
    {
        var project = await _projectRepository.GetAsync(projectId);
        if (_currentUser.IsInRole(PlatformRoleNames.Admin) || _currentUser.IsInRole(PlatformRoleNames.Pmo))
        {
            return;
        }

        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        if (accessibleOrganizationUnitIds.Contains(project.OrganizationUnitId))
        {
            return;
        }

        if (_currentUser.Id.HasValue)
        {
            var currentUserId = _currentUser.Id.Value;
            if (project.ManagerUserId == currentUserId || project.SponsorUserId == currentUserId)
            {
                return;
            }

            if (await _projectMemberRepository.AnyAsync(member =>
                member.ProjectId == projectId
                && member.UserId == currentUserId
                && (!member.LeaveDate.HasValue || member.LeaveDate.Value.Date >= DateTime.Today || member.AllowHistoricalRead)))
            {
                return;
            }
        }

        throw new EntityNotFoundException(typeof(Project), projectId);
    }
}
