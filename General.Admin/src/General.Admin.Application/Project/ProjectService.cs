using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Project;

public class ProjectService : ITransientDependency
{
    private readonly IRepository<Project, Guid> _projectRepository;

    public ProjectService(IRepository<Project, Guid> projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Guid> CreateAsync(ProjectSaveInput input)
    {
        if (await _projectRepository.AnyAsync(x => x.ProjectCode == input.ProjectCode.Trim()))
        {
            throw new BusinessException("项目编号已存在。");
        }

        var project = new Project(
            Guid.NewGuid(),
            input.ProjectCode.Trim(),
            input.Name.Trim(),
            input.ShortName,
            input.ProjectType,
            input.ProjectSource,
            input.OrganizationUnitId,
            input.ManagerUserId,
            input.SponsorUserId,
            ProjectPriorities.Normalize(input.Priority),
            input.Status.Trim(),
            input.PlannedStartDate,
            input.PlannedEndDate,
            input.IsKeyProject,
            input.Description,
            input.BudgetTotalAmount,
            input.ContractTotalAmount,
            input.ReceivedAmount);

        await _projectRepository.InsertAsync(project, autoSave: true);
        return project.Id;
    }
}
