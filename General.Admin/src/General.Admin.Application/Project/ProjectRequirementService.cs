using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace General.Admin.Project;

public class ProjectRequirementService : ITransientDependency
{
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectWbsNode, Guid> _projectWbsNodeRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectRequirementService(
        ProjectAccessGuard projectAccessGuard,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<ProjectRequirement, Guid> projectRequirementRepository,
        IRepository<ProjectWbsNode, Guid> projectWbsNodeRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _projectAccessGuard = projectAccessGuard;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectRequirementRepository = projectRequirementRepository;
        _projectWbsNodeRepository = projectWbsNodeRepository;
        _userRepository = userRepository;
    }

    public async Task<Guid> CreateRequirementAsync(Guid projectId, ProjectRequirementSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        await ValidateRequirementInputAsync(projectId, input);

        var requirementCode = input.RequirementCode.Trim();
        if (await _projectRequirementRepository.AnyAsync(item =>
            item.ProjectId == projectId && item.RequirementCode == requirementCode))
        {
            throw new BusinessException("需求编号在当前项目中已存在。");
        }

        var item = new ProjectRequirement(
            Guid.NewGuid(),
            projectId,
            requirementCode,
            input.Title.Trim(),
            input.Description,
            input.Status.Trim(),
            NormalizeVersionNo(input.VersionNo),
            NormalizeRequirementPriority(input.Priority),
            input.Source.Trim(),
            input.Type.Trim(),
            input.WbsNodeId,
            input.MilestoneId,
            input.AcceptanceCriteria,
            input.ReplacesRequirementId,
            input.SizeEstimate,
            input.BusinessOwnerUserId,
            input.ProductOwnerUserId,
            input.IsCurrent,
            input.ChangeReason,
            input.ImpactSummary);

        await _projectRequirementRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public async Task UpdateRequirementAsync(Guid requirementId, ProjectRequirementSaveInput input)
    {
        var requirement = await _projectRequirementRepository.GetAsync(requirementId);
        await _projectAccessGuard.EnsureProjectAccessAsync(requirement.ProjectId);
        await ValidateRequirementInputAsync(requirement.ProjectId, input, requirementId);

        var requirementCode = input.RequirementCode.Trim();
        if (await _projectRequirementRepository.AnyAsync(item =>
            item.ProjectId == requirement.ProjectId
            && item.Id != requirementId
            && item.RequirementCode == requirementCode))
        {
            throw new BusinessException("需求编号在当前项目中已存在。");
        }

        requirement.Update(
            requirementCode,
            input.Title.Trim(),
            input.Description,
            input.Status.Trim(),
            NormalizeVersionNo(input.VersionNo),
            NormalizeRequirementPriority(input.Priority),
            input.Source.Trim(),
            input.Type.Trim(),
            input.WbsNodeId,
            input.MilestoneId,
            input.AcceptanceCriteria,
            input.ReplacesRequirementId,
            input.SizeEstimate,
            input.BusinessOwnerUserId,
            input.ProductOwnerUserId,
            input.IsCurrent,
            input.ChangeReason,
            input.ImpactSummary);
        await _projectRequirementRepository.UpdateAsync(requirement, autoSave: true);
    }

    private async Task ValidateRequirementInputAsync(
        Guid projectId,
        ProjectRequirementSaveInput input,
        Guid? currentRequirementId = null)
    {
        if (string.IsNullOrWhiteSpace(input.RequirementCode))
        {
            throw new BusinessException("需求编号不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new BusinessException("需求标题不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Status))
        {
            throw new BusinessException("需求状态不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Priority))
        {
            throw new BusinessException("需求优先级不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Source))
        {
            throw new BusinessException("需求来源不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Type))
        {
            throw new BusinessException("需求类型不能为空。");
        }

        if (input.WbsNodeId.HasValue
            && !await _projectWbsNodeRepository.AnyAsync(item => item.Id == input.WbsNodeId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("需求关联的 WBS 节点必须属于当前项目。");
        }

        if (input.MilestoneId.HasValue
            && !await _projectMilestoneRepository.AnyAsync(item => item.Id == input.MilestoneId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("需求关联的里程碑必须属于当前项目。");
        }

        if (input.ReplacesRequirementId.HasValue)
        {
            if (currentRequirementId.HasValue && input.ReplacesRequirementId.Value == currentRequirementId.Value)
            {
                throw new BusinessException("需求不能替代自身。");
            }

            if (!await _projectRequirementRepository.AnyAsync(item =>
                item.Id == input.ReplacesRequirementId.Value && item.ProjectId == projectId))
            {
                throw new BusinessException("被替代需求必须属于当前项目。");
            }
        }

        if (input.BusinessOwnerUserId.HasValue
            && !await _userRepository.AnyAsync(item => item.Id == input.BusinessOwnerUserId.Value))
        {
            throw new BusinessException("业务负责人不存在。");
        }

        if (input.ProductOwnerUserId.HasValue
            && !await _userRepository.AnyAsync(item => item.Id == input.ProductOwnerUserId.Value))
        {
            throw new BusinessException("产品负责人不存在。");
        }
    }

    private static int NormalizeVersionNo(int versionNo)
    {
        return versionNo <= 0 ? 1 : versionNo;
    }

    private static string NormalizeRequirementPriority(string? priority)
    {
        var value = priority?.Trim();
        return value switch
        {
            ProjectPriorities.P0 or ProjectPriorities.P1 or ProjectPriorities.P2 or ProjectPriorities.P3 => value,
            _ => ProjectPriorities.P1
        };
    }
}
