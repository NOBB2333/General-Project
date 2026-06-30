using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Project;

public class ProjectLifecycleService : ITransientDependency
{
    private const string ProjectBusinessType = "Project";
    private const string AcceptanceRecordCategory = "Project/AcceptanceRecord";
    private const string DeliveryCategory = "Project/Delivery";

    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecord, Guid> _projectAcceptanceRecordRepository;
    private readonly IRepository<ProjectAcceptanceRecordCriteria, Guid> _projectAcceptanceRecordCriteriaRepository;
    private readonly IRepository<ProjectCharter, Guid> _projectCharterRepository;
    private readonly IRepository<PlatformWorkflowInstance, Guid> _platformWorkflowInstanceRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<ProjectQualityInspection, Guid> _projectQualityInspectionRepository;

    public ProjectLifecycleService(
        IRepository<AppPlatformFile, Guid> fileRepository,
        ProjectAccessGuard projectAccessGuard,
        IRepository<ProjectAcceptanceCriteria, Guid> projectAcceptanceCriteriaRepository,
        IRepository<ProjectAcceptanceRecord, Guid> projectAcceptanceRecordRepository,
        IRepository<ProjectAcceptanceRecordCriteria, Guid> projectAcceptanceRecordCriteriaRepository,
        IRepository<ProjectCharter, Guid> projectCharterRepository,
        IRepository<PlatformWorkflowInstance, Guid> platformWorkflowInstanceRepository,
        IRepository<ProjectSprint, Guid> projectSprintRepository,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<ProjectQualityInspection, Guid> projectQualityInspectionRepository)
    {
        _fileRepository = fileRepository;
        _projectAccessGuard = projectAccessGuard;
        _projectAcceptanceCriteriaRepository = projectAcceptanceCriteriaRepository;
        _projectAcceptanceRecordRepository = projectAcceptanceRecordRepository;
        _projectAcceptanceRecordCriteriaRepository = projectAcceptanceRecordCriteriaRepository;
        _projectCharterRepository = projectCharterRepository;
        _platformWorkflowInstanceRepository = platformWorkflowInstanceRepository;
        _projectSprintRepository = projectSprintRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectRepository = projectRepository;
        _projectQualityInspectionRepository = projectQualityInspectionRepository;
    }

    public async Task UpdateMilestoneStatusAsync(Guid milestoneId, ProjectMilestoneStatusUpdateInput input)
    {
        var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
        await _projectAccessGuard.EnsureProjectAccessAsync(milestone.ProjectId);
        var status = input.Status.Trim();
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new BusinessException("里程碑状态不能为空。");
        }

        if (ProjectStatuses.IsClosed(status))
        {
            var hasAcceptanceCriteria = await _projectAcceptanceCriteriaRepository.AnyAsync(item =>
                item.ProjectId == milestone.ProjectId
                && item.MilestoneId == milestoneId
                && (item.Status == "已确认" || item.Status == "已验收" || item.Status == "不适用"));
            if (!hasAcceptanceCriteria)
            {
                throw new BusinessException("关键里程碑关闭前必须维护并确认验收标准。");
            }

            var hasQualityInspection = await _projectQualityInspectionRepository.AnyAsync(item => item.ProjectId == milestone.ProjectId);
            if (!hasQualityInspection)
            {
                throw new BusinessException("关键里程碑关闭前必须完成至少一次质量检查。");
            }

            var projectIdText = milestone.ProjectId.ToString();
            var hasAcceptanceEvidenceFile = await _fileRepository.AnyAsync(item =>
                item.BusinessType == ProjectBusinessType
                && item.BusinessId == projectIdText
                && (item.Category == AcceptanceRecordCategory || item.Category == DeliveryCategory));
            if (!hasAcceptanceEvidenceFile)
            {
                throw new BusinessException("关键里程碑关闭前必须归档验收记录或交付物文件。");
            }
        }

        if (input.SprintId.HasValue
            && !await _projectSprintRepository.AnyAsync(item => item.Id == input.SprintId.Value && item.ProjectId == milestone.ProjectId))
        {
            throw new BusinessException("里程碑关联的 Sprint 必须属于当前项目。");
        }

        milestone.UpdateSprint(input.SprintId);
        milestone.UpdateStatus(status, input.ActualCompletionDate);
        await _projectMilestoneRepository.UpdateAsync(milestone, autoSave: true);
    }

    public async Task UpdateProjectStatusAsync(Guid projectId, ProjectStatusUpdateInput input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var status = input.Status.Trim();
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new BusinessException("项目状态不能为空。");
        }

        if (status is "进行中" or "执行中")
        {
            var charter = (await _projectCharterRepository.GetListAsync(item => item.ProjectId == projectId)).FirstOrDefault();
            var approvedCharterWorkflow = await _platformWorkflowInstanceRepository.AnyAsync(item =>
                item.WorkflowDefinitionKey == PlatformWorkflowDefinitionKeys.ProjectCharterApproval
                && item.BusinessType == "ProjectCharter"
                && item.BusinessId == projectId.ToString()
                && item.Status == PlatformWorkflowStatuses.Approved);
            if (charter?.ApprovalStatus != "已批准" || !approvedCharterWorkflow)
            {
                throw new BusinessException("项目进入执行中前必须先完成并通过项目章程审批。");
            }
        }

        if (ProjectStatuses.IsClosed(status))
        {
            await EnsureProjectAcceptanceReadyForCloseAsync(projectId);
        }

        project.UpdateStatus(status);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    private async Task EnsureProjectAcceptanceReadyForCloseAsync(Guid projectId)
    {
        var milestones = await _projectMilestoneRepository.GetListAsync(item => item.ProjectId == projectId);
        if (milestones.Count == 0)
        {
            throw new BusinessException("项目收尾或归档前必须维护关键里程碑。");
        }

        var openMilestone = milestones.FirstOrDefault(item => !ProjectStatuses.IsClosed(item.Status));
        if (openMilestone is not null)
        {
            throw new BusinessException($"项目收尾或归档前必须先关闭关键里程碑：{openMilestone.Name}。");
        }

        var requiredCriteria = await _projectAcceptanceCriteriaRepository.GetListAsync(item =>
            item.ProjectId == projectId
            && (item.Status == "已确认" || item.Status == "已验收"));
        if (requiredCriteria.Count == 0)
        {
            throw new BusinessException("项目收尾或归档前必须维护并确认验收标准。");
        }

        var requiredCriteriaIds = requiredCriteria.Select(item => item.Id).ToList();
        var criteriaLinks = await _projectAcceptanceRecordCriteriaRepository.GetListAsync(item =>
            item.ProjectId == projectId && requiredCriteriaIds.Contains(item.AcceptanceCriteriaId));
        if (criteriaLinks.Count == 0)
        {
            throw new BusinessException("项目收尾或归档前必须为已确认验收标准补齐通过的验收记录和验收文件。");
        }

        var recordIds = criteriaLinks.Select(item => item.AcceptanceRecordId).Distinct().ToList();
        var passedRecords = await _projectAcceptanceRecordRepository.GetListAsync(item =>
            item.ProjectId == projectId
            && recordIds.Contains(item.Id)
            && item.AcceptanceResult == "通过"
            && item.AcceptanceFileId.HasValue);
        var projectIdText = projectId.ToString();
        var acceptanceFileIds = passedRecords.Select(item => item.AcceptanceFileId!.Value).Distinct().ToList();
        var validAcceptanceFileIds = (await _fileRepository.GetListAsync(item =>
                acceptanceFileIds.Contains(item.Id)
                && item.BusinessType == ProjectBusinessType
                && item.BusinessId == projectIdText
                && (item.Category == AcceptanceRecordCategory || item.Category == DeliveryCategory)))
            .Select(item => item.Id)
            .ToHashSet();
        var passedRecordIds = passedRecords
            .Where(item => validAcceptanceFileIds.Contains(item.AcceptanceFileId!.Value))
            .Select(item => item.Id)
            .ToHashSet();
        var acceptedCriteriaIds = criteriaLinks
            .Where(item => passedRecordIds.Contains(item.AcceptanceRecordId))
            .Select(item => item.AcceptanceCriteriaId)
            .Distinct()
            .ToHashSet();
        var missingCriteria = requiredCriteria.FirstOrDefault(item => !acceptedCriteriaIds.Contains(item.Id));
        if (missingCriteria is not null)
        {
            throw new BusinessException($"项目收尾或归档前必须为验收标准“{missingCriteria.Title}”补齐通过的验收记录和验收文件。");
        }
    }
}
