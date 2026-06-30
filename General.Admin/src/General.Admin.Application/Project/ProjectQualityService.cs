using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace General.Admin.Project;

public class ProjectQualityService : ITransientDependency
{
    private const string ProjectBusinessType = "Project";
    private const string AcceptanceRecordCategory = "Project/AcceptanceRecord";
    private const string DeliveryCategory = "Project/Delivery";

    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecord, Guid> _projectAcceptanceRecordRepository;
    private readonly IRepository<ProjectAcceptanceRecordCriteria, Guid> _projectAcceptanceRecordCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecordRequirement, Guid> _projectAcceptanceRecordRequirementRepository;
    private readonly IRepository<ProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectQualityChecklist, Guid> _projectQualityChecklistRepository;
    private readonly IRepository<ProjectQualityInspection, Guid> _projectQualityInspectionRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectQualityService(
        IRepository<AppPlatformFile, Guid> fileRepository,
        ProjectAccessGuard projectAccessGuard,
        IRepository<ProjectAcceptanceCriteria, Guid> projectAcceptanceCriteriaRepository,
        IRepository<ProjectAcceptanceRecord, Guid> projectAcceptanceRecordRepository,
        IRepository<ProjectAcceptanceRecordCriteria, Guid> projectAcceptanceRecordCriteriaRepository,
        IRepository<ProjectAcceptanceRecordRequirement, Guid> projectAcceptanceRecordRequirementRepository,
        IRepository<ProjectIssue, Guid> projectIssueRepository,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<ProjectSprint, Guid> projectSprintRepository,
        IRepository<ProjectQualityChecklist, Guid> projectQualityChecklistRepository,
        IRepository<ProjectQualityInspection, Guid> projectQualityInspectionRepository,
        IRepository<ProjectRequirement, Guid> projectRequirementRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _fileRepository = fileRepository;
        _projectAccessGuard = projectAccessGuard;
        _projectAcceptanceCriteriaRepository = projectAcceptanceCriteriaRepository;
        _projectAcceptanceRecordRepository = projectAcceptanceRecordRepository;
        _projectAcceptanceRecordCriteriaRepository = projectAcceptanceRecordCriteriaRepository;
        _projectAcceptanceRecordRequirementRepository = projectAcceptanceRecordRequirementRepository;
        _projectIssueRepository = projectIssueRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectSprintRepository = projectSprintRepository;
        _projectQualityChecklistRepository = projectQualityChecklistRepository;
        _projectQualityInspectionRepository = projectQualityInspectionRepository;
        _projectRequirementRepository = projectRequirementRepository;
        _projectTaskRepository = projectTaskRepository;
        _userRepository = userRepository;
    }

    public async Task<Guid> CreateAcceptanceCriteriaAsync(Guid projectId, ProjectAcceptanceCriteriaSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        if (!input.TaskId.HasValue && !input.MilestoneId.HasValue)
        {
            throw new BusinessException("验收标准必须关联任务或里程碑。");
        }

        if (input.TaskId.HasValue
            && !await _projectTaskRepository.AnyAsync(item => item.Id == input.TaskId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("验收标准关联的任务必须属于当前项目。");
        }

        if (input.MilestoneId.HasValue
            && !await _projectMilestoneRepository.AnyAsync(item => item.Id == input.MilestoneId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("验收标准关联的里程碑必须属于当前项目。");
        }

        var item = new ProjectAcceptanceCriteria(
            Guid.NewGuid(),
            projectId,
            input.Title.Trim(),
            input.Description.Trim(),
            input.Status.Trim(),
            input.MilestoneId,
            input.TaskId);

        await _projectAcceptanceCriteriaRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public async Task<Guid> CreateAcceptanceRecordAsync(Guid projectId, ProjectAcceptanceRecordSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);

        if (input.AcceptanceCriteriaIds.Count == 0)
        {
            throw new BusinessException("验收记录必须至少关联一条验收标准。");
        }

        var criteriaIds = input.AcceptanceCriteriaIds.Distinct().ToList();
        var criteriaCount = await _projectAcceptanceCriteriaRepository.CountAsync(item =>
            item.ProjectId == projectId && criteriaIds.Contains(item.Id));
        if (criteriaCount != criteriaIds.Count)
        {
            throw new BusinessException("验收记录关联的验收标准必须属于当前项目。");
        }

        var requirementIds = input.RequirementIds.Distinct().ToList();
        if (requirementIds.Count > 0)
        {
            var requirementCount = await _projectRequirementRepository.CountAsync(item =>
                item.ProjectId == projectId && requirementIds.Contains(item.Id));
            if (requirementCount != requirementIds.Count)
            {
                throw new BusinessException("验收记录关联的需求必须属于当前项目。");
            }
        }

        if (input.MilestoneId.HasValue
            && !await _projectMilestoneRepository.AnyAsync(item => item.Id == input.MilestoneId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("验收记录关联的里程碑必须属于当前项目。");
        }

        if (input.SprintId.HasValue
            && !await _projectSprintRepository.AnyAsync(item => item.Id == input.SprintId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("验收记录关联的 Sprint 必须属于当前项目。");
        }

        if (!await _userRepository.AnyAsync(item => item.Id == input.AcceptorUserId))
        {
            throw new BusinessException("验收人不存在。");
        }

        if (input.AcceptanceFileId.HasValue)
        {
            var projectIdText = projectId.ToString();
            var sprintIdText = input.SprintId?.ToString();
            var file = await _fileRepository.GetAsync(input.AcceptanceFileId.Value);
            var isProjectFile = file.BusinessType == ProjectBusinessType && file.BusinessId == projectIdText;
            var isSprintFile = input.SprintId.HasValue
                && file.BusinessType == "ProjectSprint"
                && file.BusinessId == sprintIdText;
            if ((!isProjectFile && !isSprintFile)
                || (file.Category != AcceptanceRecordCategory && file.Category != DeliveryCategory))
            {
                throw new BusinessException("验收记录关联的文件必须是当前项目或 Sprint 的验收记录或交付物文件。");
            }
        }

        var result = input.AcceptanceResult.Trim();
        if (result == "不通过" && string.IsNullOrWhiteSpace(input.RectificationRequirement))
        {
            throw new BusinessException("验收不通过时必须填写整改要求。");
        }

        if (input.CreateIssueWhenRejected && result != "不通过")
        {
            throw new BusinessException("只有验收不通过时才能生成问题记录。");
        }

        if (input.CreateIssueWhenRejected
            && input.IssueOwnerUserId.HasValue
            && !await _userRepository.AnyAsync(item => item.Id == input.IssueOwnerUserId.Value))
        {
            throw new BusinessException("问题责任人不存在。");
        }

        var record = new ProjectAcceptanceRecord(
            Guid.NewGuid(),
            projectId,
            input.Title.Trim(),
            result,
            input.AcceptorUserId,
            input.AcceptanceTime ?? DateTime.Now,
            input.MilestoneId,
            input.SprintId,
            input.AcceptanceFileId,
            input.RectificationRequirement,
            input.Remark);

        await _projectAcceptanceRecordRepository.InsertAsync(record, autoSave: true);
        await _projectAcceptanceRecordCriteriaRepository.InsertManyAsync(
            criteriaIds
                .Select(criteriaId => new ProjectAcceptanceRecordCriteria(
                    Guid.NewGuid(),
                    projectId,
                    record.Id,
                    criteriaId))
                .ToList(),
            autoSave: true);

        if (requirementIds.Count > 0)
        {
            await _projectAcceptanceRecordRequirementRepository.InsertManyAsync(
                requirementIds
                    .Select(requirementId => new ProjectAcceptanceRecordRequirement(
                        Guid.NewGuid(),
                        projectId,
                        record.Id,
                        requirementId))
                    .ToList(),
                autoSave: true);
        }

        if (input.CreateIssueWhenRejected)
        {
            await _projectIssueRepository.InsertAsync(
                new ProjectIssue(
                    Guid.NewGuid(),
                    projectId,
                    "验收问题",
                    $"验收不通过：{record.Title}",
                    ProjectPriorities.Normalize(input.IssueLevel, ProjectPriorities.P0),
                    input.IssueOwnerUserId ?? input.AcceptorUserId,
                    "待处理",
                    input.IssueDueDate,
                    requirementTitle: $"来源验收记录：{record.Title}",
                    testerOwnerUserId: input.AcceptorUserId,
                    acceptanceCriteriaId: criteriaIds.First()),
                autoSave: true);
        }

        return record.Id;
    }

    public async Task<Guid> CreateQualityChecklistAsync(Guid projectId, ProjectQualityChecklistSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var title = input.Title.Trim();
        var category = input.Category.Trim();
        if (await _projectQualityChecklistRepository.AnyAsync(item => item.ProjectId == projectId && item.Title == title))
        {
            throw new BusinessException("质量检查单标题已存在。");
        }

        var item = new ProjectQualityChecklist(
            Guid.NewGuid(),
            projectId,
            title,
            category,
            input.Description.Trim());

        await _projectQualityChecklistRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public async Task<Guid> CreateQualityInspectionAsync(Guid projectId, ProjectQualityInspectionSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var checklist = await _projectQualityChecklistRepository.GetAsync(input.QualityChecklistId);
        if (checklist.ProjectId != projectId)
        {
            throw new BusinessException("质量检查记录必须关联当前项目的检查单。");
        }

        if (!await _userRepository.AnyAsync(item => item.Id == input.InspectorUserId))
        {
            throw new BusinessException("检查人不存在。");
        }

        var item = new ProjectQualityInspection(
            Guid.NewGuid(),
            projectId,
            input.QualityChecklistId,
            input.InspectorUserId,
            input.InspectionTime ?? DateTime.Now,
            input.InspectionResult.Trim(),
            input.FindingSummary.Trim(),
            input.RectificationRequirement.Trim());

        await _projectQualityInspectionRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public async Task<Guid> CreateIssueAsync(Guid projectId, ProjectIssueSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        await ValidateIssueInputAsync(projectId, input);

        var ownerNames = await ResolveIssueOwnerNamesAsync(input);
        var issue = new ProjectIssue(
            Guid.NewGuid(),
            projectId,
            input.Type.Trim(),
            input.Title.Trim(),
            ProjectPriorities.Normalize(input.Level),
            input.OwnerUserId,
            input.Status.Trim(),
            input.DueDate,
            NormalizeOptionalText(input.RequirementTitle),
            ownerNames.ProductOwnerName,
            ownerNames.DeveloperOwnerName,
            ownerNames.TesterOwnerName,
            input.ProductOwnerUserId,
            input.DeveloperOwnerUserId,
            input.TesterOwnerUserId,
            input.AcceptanceCriteriaId,
            input.QualityChecklistId);

        await _projectIssueRepository.InsertAsync(issue, autoSave: true);
        return issue.Id;
    }

    public async Task UpdateIssueAsync(Guid issueId, ProjectIssueSaveInput input)
    {
        var issue = await _projectIssueRepository.GetAsync(issueId);
        await _projectAccessGuard.EnsureProjectAccessAsync(issue.ProjectId);
        await ValidateIssueInputAsync(issue.ProjectId, input);

        var ownerNames = await ResolveIssueOwnerNamesAsync(input);
        issue.Update(
            input.Type.Trim(),
            input.Title.Trim(),
            ProjectPriorities.Normalize(input.Level),
            input.OwnerUserId,
            input.Status.Trim(),
            input.DueDate,
            NormalizeOptionalText(input.RequirementTitle),
            ownerNames.ProductOwnerName,
            ownerNames.DeveloperOwnerName,
            ownerNames.TesterOwnerName,
            input.ProductOwnerUserId,
            input.DeveloperOwnerUserId,
            input.TesterOwnerUserId,
            input.AcceptanceCriteriaId,
            input.QualityChecklistId);

        await _projectIssueRepository.UpdateAsync(issue, autoSave: true);
    }

    public async Task DeleteIssueAsync(Guid issueId)
    {
        var issue = await _projectIssueRepository.GetAsync(issueId);
        await _projectAccessGuard.EnsureProjectAccessAsync(issue.ProjectId);
        await _projectIssueRepository.DeleteAsync(issue, autoSave: true);
    }

    public async Task UpdateIssueQualityLinkAsync(Guid issueId, ProjectIssueQualityLinkInput input)
    {
        var issue = await _projectIssueRepository.GetAsync(issueId);
        await _projectAccessGuard.EnsureProjectAccessAsync(issue.ProjectId);
        if (input.AcceptanceCriteriaId.HasValue
            && !await _projectAcceptanceCriteriaRepository.AnyAsync(item => item.Id == input.AcceptanceCriteriaId.Value && item.ProjectId == issue.ProjectId))
        {
            throw new BusinessException("问题关联的验收标准必须属于当前项目。");
        }

        if (input.QualityChecklistId.HasValue
            && !await _projectQualityChecklistRepository.AnyAsync(item => item.Id == input.QualityChecklistId.Value && item.ProjectId == issue.ProjectId))
        {
            throw new BusinessException("问题关联的质量检查单必须属于当前项目。");
        }

        issue.LinkQualityObjects(input.AcceptanceCriteriaId, input.QualityChecklistId);
        await _projectIssueRepository.UpdateAsync(issue, autoSave: true);
    }

    private async Task ValidateIssueInputAsync(Guid projectId, ProjectIssueSaveInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new BusinessException("问题标题不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Type))
        {
            throw new BusinessException("问题类型不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Level))
        {
            throw new BusinessException("问题等级不能为空。");
        }

        if (!ProjectPriorities.IsSupported(input.Level))
        {
            throw new BusinessException("问题等级只支持 P0、P1、P2、P3。");
        }

        if (string.IsNullOrWhiteSpace(input.Status))
        {
            throw new BusinessException("问题状态不能为空。");
        }

        await EnsureUserExistsAsync(input.OwnerUserId, "负责人不存在。");
        if (input.ProductOwnerUserId.HasValue)
        {
            await EnsureUserExistsAsync(input.ProductOwnerUserId.Value, "产品负责人不存在。");
        }

        if (input.DeveloperOwnerUserId.HasValue)
        {
            await EnsureUserExistsAsync(input.DeveloperOwnerUserId.Value, "开发负责人不存在。");
        }

        if (input.TesterOwnerUserId.HasValue)
        {
            await EnsureUserExistsAsync(input.TesterOwnerUserId.Value, "测试负责人不存在。");
        }

        if (input.AcceptanceCriteriaId.HasValue
            && !await _projectAcceptanceCriteriaRepository.AnyAsync(item => item.Id == input.AcceptanceCriteriaId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("问题关联的验收标准必须属于当前项目。");
        }

        if (input.QualityChecklistId.HasValue
            && !await _projectQualityChecklistRepository.AnyAsync(item => item.Id == input.QualityChecklistId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("问题关联的质量检查单必须属于当前项目。");
        }
    }

    private async Task EnsureUserExistsAsync(Guid userId, string message)
    {
        if (!await _userRepository.AnyAsync(item => item.Id == userId))
        {
            throw new BusinessException(message);
        }
    }

    private async Task<IssueOwnerNames> ResolveIssueOwnerNamesAsync(ProjectIssueSaveInput input)
    {
        return new IssueOwnerNames(
            await ResolveUserNameAsync(input.ProductOwnerUserId),
            await ResolveUserNameAsync(input.DeveloperOwnerUserId),
            await ResolveUserNameAsync(input.TesterOwnerUserId));
    }

    private async Task<string?> ResolveUserNameAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        var user = await _userRepository.FindAsync(userId.Value);
        return user?.Name ?? user?.UserName;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record IssueOwnerNames(
        string? ProductOwnerName,
        string? DeveloperOwnerName,
        string? TesterOwnerName);
}
