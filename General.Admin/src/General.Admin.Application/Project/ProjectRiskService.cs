using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Project;

public class ProjectRiskService : ITransientDependency
{
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<ProjectRaidItem, Guid> _projectRaidItemRepository;

    public ProjectRiskService(
        ProjectAccessGuard projectAccessGuard,
        IRepository<ProjectRaidItem, Guid> projectRaidItemRepository)
    {
        _projectAccessGuard = projectAccessGuard;
        _projectRaidItemRepository = projectRaidItemRepository;
    }

    public async Task UpdateRaidRiskAsync(Guid raidItemId, ProjectRaidRiskUpdateInput input)
    {
        var item = await _projectRaidItemRepository.GetAsync(raidItemId);
        await _projectAccessGuard.EnsureProjectAccessAsync(item.ProjectId);
        item.UpdateRiskAssessment(
            input.Probability,
            input.Impact,
            input.ResponseStrategy,
            input.TriggerCondition,
            input.ContingencyPlan);
        await _projectRaidItemRepository.UpdateAsync(item, autoSave: true);
    }
}
