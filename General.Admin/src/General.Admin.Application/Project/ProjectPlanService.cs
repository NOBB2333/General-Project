using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.Project;

public class ProjectPlanService : ITransientDependency
{
    private static readonly HashSet<string> SupportedTaskStatuses =
    [
        "未开始",
        ProjectStatuses.InProgress,
        "阻塞",
        ProjectStatuses.Testing,
        ProjectStatuses.Accepting,
        ProjectStatuses.Completed,
        ProjectStatuses.Closed
    ];

    private readonly ICurrentUser _currentUser;
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecord, Guid> _projectAcceptanceRecordRepository;
    private readonly IRepository<ProjectChangeImpact, Guid> _projectChangeImpactRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<ProjectReport, Guid> _projectReportRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectTaskDependency, Guid> _projectTaskDependencyRepository;
    private readonly IRepository<ProjectTaskParticipant, Guid> _projectTaskParticipantRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<ProjectWbsNode, Guid> _projectWbsNodeRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectPlanService(
        ICurrentUser currentUser,
        ProjectAccessGuard projectAccessGuard,
        IRepository<ProjectAcceptanceCriteria, Guid> projectAcceptanceCriteriaRepository,
        IRepository<ProjectAcceptanceRecord, Guid> projectAcceptanceRecordRepository,
        IRepository<ProjectChangeImpact, Guid> projectChangeImpactRepository,
        IRepository<ProjectSprint, Guid> projectSprintRepository,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<ProjectReport, Guid> projectReportRepository,
        IRepository<ProjectRequirement, Guid> projectRequirementRepository,
        IRepository<ProjectTaskDependency, Guid> projectTaskDependencyRepository,
        IRepository<ProjectTaskParticipant, Guid> projectTaskParticipantRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository,
        IRepository<ProjectWorklog, Guid> projectWorklogRepository,
        IRepository<ProjectWbsNode, Guid> projectWbsNodeRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _projectAccessGuard = projectAccessGuard;
        _projectAcceptanceCriteriaRepository = projectAcceptanceCriteriaRepository;
        _projectAcceptanceRecordRepository = projectAcceptanceRecordRepository;
        _projectChangeImpactRepository = projectChangeImpactRepository;
        _projectSprintRepository = projectSprintRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectRepository = projectRepository;
        _projectReportRepository = projectReportRepository;
        _projectRequirementRepository = projectRequirementRepository;
        _projectTaskDependencyRepository = projectTaskDependencyRepository;
        _projectTaskParticipantRepository = projectTaskParticipantRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _projectWbsNodeRepository = projectWbsNodeRepository;
        _userRepository = userRepository;
    }

    public async Task<Guid> CreateWbsNodeAsync(Guid projectId, ProjectWbsNodeSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        await ValidateWbsNodeReferencesAsync(projectId, input);

        var projectWbsNodes = await _projectWbsNodeRepository.GetListAsync(item => item.ProjectId == projectId);
        if (input.ParentId.HasValue && projectWbsNodes.All(item => item.Id != input.ParentId.Value))
        {
            throw new BusinessException("WBS 父节点必须属于当前项目。");
        }

        var wbsCode = input.WbsCode.Trim();
        if (projectWbsNodes.Any(item => item.WbsCode.Equals(wbsCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("WBS 编码在当前项目中已存在。");
        }

        var item = new ProjectWbsNode(
            Guid.NewGuid(),
            projectId,
            wbsCode,
            input.Name.Trim(),
            input.Type.Trim(),
            input.Status.Trim(),
            input.ParentId,
            input.MilestoneId,
            input.SprintId,
            input.OwnerUserId,
            input.SortOrder,
            input.ScopeDescription,
            input.DeliveryDescription,
            input.EstimateSummary);

        await _projectWbsNodeRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public async Task<Guid> CreateSprintAsync(Guid projectId, ProjectSprintSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        await ValidateSprintInputAsync(projectId, input);
        var projectSprints = await _projectSprintRepository.GetListAsync(item => item.ProjectId == projectId);

        var item = new ProjectSprint(
            Guid.NewGuid(),
            projectId,
            ResolveNextSprintCode(projectSprints),
            input.Name.Trim(),
            input.OwnerUserId,
            input.StartDate,
            input.EndDate,
            input.Status.Trim(),
            input.Progress,
            input.Summary);

        await _projectSprintRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public async Task<Guid> CreateMilestoneAsync(Guid projectId, ProjectMilestoneSaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        await ValidateMilestoneInputAsync(projectId, input);

        var status = input.Status.Trim();
        if (ProjectStatuses.IsClosed(status))
        {
            throw new BusinessException("里程碑创建后请通过状态更新入口完成关闭校验。");
        }

        var item = new ProjectMilestone(
            Guid.NewGuid(),
            projectId,
            input.Name.Trim(),
            input.OwnerUserId,
            input.PlannedCompletionDate,
            status,
            input.SprintId,
            input.ActualCompletionDate);

        await _projectMilestoneRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public async Task<Guid> CreateTaskDependencyAsync(Guid projectId, ProjectTaskDependencySaveInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        if (input.PredecessorTaskId == input.SuccessorTaskId)
        {
            throw new BusinessException("前置任务和后续任务不能相同。");
        }

        var projectTasks = await _projectTaskRepository.GetListAsync(item => item.ProjectId == projectId);
        if (!projectTasks.Any(item => item.Id == input.PredecessorTaskId)
            || !projectTasks.Any(item => item.Id == input.SuccessorTaskId))
        {
            throw new BusinessException("任务依赖必须属于当前项目。");
        }

        var existingDependencies = await _projectTaskDependencyRepository.GetListAsync(item => item.ProjectId == projectId);
        if (existingDependencies.Any(item => item.PredecessorTaskId == input.PredecessorTaskId && item.SuccessorTaskId == input.SuccessorTaskId))
        {
            throw new BusinessException("任务依赖已存在。");
        }

        if (WouldCreateDependencyLoop(input.PredecessorTaskId, input.SuccessorTaskId, existingDependencies))
        {
            throw new BusinessException("任务依赖不能形成循环。");
        }

        var item = new ProjectTaskDependency(
            Guid.NewGuid(),
            projectId,
            input.PredecessorTaskId,
            input.SuccessorTaskId,
            input.DependencyType.Trim());

        await _projectTaskDependencyRepository.InsertAsync(item, autoSave: true);
        return item.Id;
    }

    public async Task<Guid> CreateTaskFromRequirementAsync(
        Guid projectId,
        Guid requirementId,
        ProjectTaskFromRequirementInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var project = await _projectRepository.GetAsync(projectId);
        var requirement = await _projectRequirementRepository.FindAsync(requirementId);
        if (requirement == null || requirement.ProjectId != projectId)
        {
            throw new BusinessException("需求必须属于当前项目。");
        }

        var projectTasks = await _projectTaskRepository.GetListAsync(item => item.ProjectId == projectId);
        var taskCode = await ValidateTaskCreationInputAsync(projectTasks, input);
        var productOwnerUserIds = NormalizeTaskParticipantIds(input.ProductOwnerUserIds, input.ProductOwnerUserId ?? requirement.ProductOwnerUserId);
        var developerOwnerUserIds = NormalizeTaskParticipantIds(input.DeveloperOwnerUserIds, input.DeveloperOwnerUserId);
        var testerOwnerUserIds = NormalizeTaskParticipantIds(input.TesterOwnerUserIds, input.TesterOwnerUserId);
        var productOwnerUserId = FirstOrDefaultParticipantId(productOwnerUserIds);
        var developerOwnerUserId = FirstOrDefaultParticipantId(developerOwnerUserIds);
        var testerOwnerUserId = FirstOrDefaultParticipantId(testerOwnerUserIds);
        var ownerNames = await ResolveTaskOwnerNamesAsync(
            productOwnerUserId,
            developerOwnerUserId,
            testerOwnerUserId);

        var wbsNode = requirement.WbsNodeId.HasValue
            ? await _projectWbsNodeRepository.FindAsync(requirement.WbsNodeId.Value)
            : null;
        if (requirement.WbsNodeId.HasValue && (wbsNode == null || wbsNode.ProjectId != projectId))
        {
            throw new BusinessException("需求关联的 WBS 节点必须属于当前项目。");
        }

        var task = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            project.OrganizationUnitId,
            taskCode,
            string.IsNullOrWhiteSpace(input.Title) ? requirement.Title : input.Title.Trim(),
            input.OwnerUserId,
            ProjectStatuses.InProgress,
            string.IsNullOrWhiteSpace(input.Priority) ? requirement.Priority : input.Priority.Trim(),
            input.PlannedStartTime,
            input.PlannedEndTime,
            estimatedWorkHours: input.EstimatedWorkHours,
            productOwnerName: ownerNames.ProductOwnerName,
            developerOwnerName: ownerNames.DeveloperOwnerName,
            testerOwnerName: ownerNames.TesterOwnerName,
            productOwnerUserId: productOwnerUserId,
            developerOwnerUserId: developerOwnerUserId,
            testerOwnerUserId: testerOwnerUserId,
            wbsCode: wbsNode?.WbsCode ?? string.Empty,
            sortOrder: projectTasks.Count == 0 ? 0 : projectTasks.Max(item => item.SortOrder) + 10,
            wbsNodeId: requirement.WbsNodeId,
            requirementId: requirement.Id,
            description: string.IsNullOrWhiteSpace(input.Description)
                ? requirement.Description
                : input.Description,
            isOutOfContract: input.IsOutOfContract);

        await _projectTaskRepository.InsertAsync(task, autoSave: true);
        await SyncTaskParticipantsAsync(task.ProjectId, task.Id, productOwnerUserIds, developerOwnerUserIds, testerOwnerUserIds);
        return task.Id;
    }

    public async Task<Guid> CreateRequirementFromWbsAsync(
        Guid projectId,
        Guid wbsNodeId,
        ProjectRequirementFromWbsInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var wbsNode = await _projectWbsNodeRepository.FindAsync(wbsNodeId);
        if (wbsNode == null || wbsNode.ProjectId != projectId)
        {
            throw new BusinessException("WBS 节点必须属于当前项目。");
        }

        if (!string.Equals(wbsNode.Type, "工作包", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("只有工作包节点才能生成需求。");
        }

        if (string.IsNullOrWhiteSpace(input.Title) && string.IsNullOrWhiteSpace(wbsNode.Name))
        {
            throw new BusinessException("需求标题不能为空。");
        }

        if (input.SizeEstimate.HasValue && input.SizeEstimate.Value < 0)
        {
            throw new BusinessException("规模估算不能为负数。");
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

        var projectRequirements = await _projectRequirementRepository.GetListAsync(item => item.ProjectId == projectId);
        var requirementCode = string.IsNullOrWhiteSpace(input.RequirementCode)
            ? ResolveNextRequirementCode(projectRequirements)
            : input.RequirementCode.Trim();
        if (projectRequirements.Any(item => item.RequirementCode.Equals(requirementCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("需求编号在当前项目中已存在。");
        }

        var requirement = new ProjectRequirement(
            Guid.NewGuid(),
            projectId,
            requirementCode,
            string.IsNullOrWhiteSpace(input.Title) ? wbsNode.Name : input.Title.Trim(),
            string.IsNullOrWhiteSpace(input.Description) ? wbsNode.ScopeDescription : input.Description,
            string.IsNullOrWhiteSpace(input.Status) ? "草稿" : input.Status.Trim(),
            NormalizeVersionNo(input.VersionNo),
            NormalizeTaskPriority(input.Priority),
            string.IsNullOrWhiteSpace(input.Source) ? "内部" : input.Source.Trim(),
            string.IsNullOrWhiteSpace(input.Type) ? "功能" : input.Type.Trim(),
            wbsNodeId: wbsNode.Id,
            milestoneId: wbsNode.MilestoneId,
            acceptanceCriteria: string.IsNullOrWhiteSpace(input.AcceptanceCriteria)
                ? wbsNode.DeliveryDescription
                : input.AcceptanceCriteria,
            sizeEstimate: input.SizeEstimate,
            businessOwnerUserId: input.BusinessOwnerUserId,
            productOwnerUserId: input.ProductOwnerUserId ?? wbsNode.OwnerUserId,
            isCurrent: true,
            impactSummary: input.ImpactSummary);

        await _projectRequirementRepository.InsertAsync(requirement, autoSave: true);
        return requirement.Id;
    }

    public async Task<Guid> CreateTaskFromWbsAsync(
        Guid projectId,
        Guid wbsNodeId,
        ProjectTaskFromWbsInput input)
    {
        await _projectAccessGuard.EnsureProjectAccessAsync(projectId);
        var project = await _projectRepository.GetAsync(projectId);
        var wbsNode = await _projectWbsNodeRepository.FindAsync(wbsNodeId);
        if (wbsNode == null || wbsNode.ProjectId != projectId)
        {
            throw new BusinessException("WBS 节点必须属于当前项目。");
        }

        if (!string.Equals(wbsNode.Type, "工作包", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("只有工作包节点才能生成任务。");
        }

        if (input.SprintId.HasValue
            && !await _projectSprintRepository.AnyAsync(item => item.Id == input.SprintId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("任务所属 Sprint 必须属于当前项目。");
        }

        var projectTasks = await _projectTaskRepository.GetListAsync(item => item.ProjectId == projectId);
        var taskCode = await ValidateTaskCreationInputAsync(projectTasks, input);
        var productOwnerUserIds = NormalizeTaskParticipantIds(input.ProductOwnerUserIds, input.ProductOwnerUserId ?? wbsNode.OwnerUserId);
        var developerOwnerUserIds = NormalizeTaskParticipantIds(input.DeveloperOwnerUserIds, input.DeveloperOwnerUserId);
        var testerOwnerUserIds = NormalizeTaskParticipantIds(input.TesterOwnerUserIds, input.TesterOwnerUserId);
        var productOwnerUserId = FirstOrDefaultParticipantId(productOwnerUserIds);
        var developerOwnerUserId = FirstOrDefaultParticipantId(developerOwnerUserIds);
        var testerOwnerUserId = FirstOrDefaultParticipantId(testerOwnerUserIds);
        var ownerNames = await ResolveTaskOwnerNamesAsync(
            productOwnerUserId,
            developerOwnerUserId,
            testerOwnerUserId);

        var task = new ProjectTask(
            Guid.NewGuid(),
            projectId,
            project.OrganizationUnitId,
            taskCode,
            string.IsNullOrWhiteSpace(input.Title) ? wbsNode.Name : input.Title.Trim(),
            input.OwnerUserId,
            ProjectStatuses.InProgress,
            NormalizeTaskPriority(input.Priority),
            input.PlannedStartTime,
            input.PlannedEndTime,
            estimatedWorkHours: input.EstimatedWorkHours,
            productOwnerName: ownerNames.ProductOwnerName,
            developerOwnerName: ownerNames.DeveloperOwnerName,
            testerOwnerName: ownerNames.TesterOwnerName,
            productOwnerUserId: productOwnerUserId,
            developerOwnerUserId: developerOwnerUserId,
            testerOwnerUserId: testerOwnerUserId,
            sprintId: input.SprintId ?? wbsNode.SprintId,
            wbsCode: wbsNode.WbsCode,
            sortOrder: projectTasks.Count == 0 ? 0 : projectTasks.Max(item => item.SortOrder) + 10,
            wbsNodeId: wbsNode.Id,
            description: string.IsNullOrWhiteSpace(input.Description)
                ? wbsNode.ScopeDescription
                : input.Description,
            isOutOfContract: input.IsOutOfContract);

        await _projectTaskRepository.InsertAsync(task, autoSave: true);
        await SyncTaskParticipantsAsync(task.ProjectId, task.Id, productOwnerUserIds, developerOwnerUserIds, testerOwnerUserIds);
        return task.Id;
    }

    public async Task DeleteWbsNodeAsync(Guid wbsNodeId)
    {
        var node = await _projectWbsNodeRepository.GetAsync(wbsNodeId);
        await _projectAccessGuard.EnsureProjectAccessAsync(node.ProjectId);

        if (await _projectWbsNodeRepository.AnyAsync(item => item.ParentId == wbsNodeId))
        {
            throw new BusinessException("WBS 节点存在子节点，不能直接删除。");
        }

        if (await _projectTaskRepository.AnyAsync(item => item.ProjectId == node.ProjectId && item.WbsNodeId == wbsNodeId))
        {
            throw new BusinessException("WBS 节点已关联任务，不能直接删除。");
        }

        if (!string.IsNullOrWhiteSpace(node.WbsCode)
            && await _projectTaskRepository.AnyAsync(item =>
                item.ProjectId == node.ProjectId &&
                !item.WbsNodeId.HasValue &&
                item.WbsCode == node.WbsCode))
        {
            throw new BusinessException("WBS 节点已关联任务，不能直接删除。");
        }

        if (await _projectRequirementRepository.AnyAsync(item => item.ProjectId == node.ProjectId && item.WbsNodeId == wbsNodeId))
        {
            throw new BusinessException("WBS 节点已关联需求，不能直接删除。");
        }

        await _projectWbsNodeRepository.DeleteAsync(node, autoSave: true);
    }

    public async Task DeleteSprintAsync(Guid sprintId)
    {
        var sprint = await _projectSprintRepository.GetAsync(sprintId);
        await _projectAccessGuard.EnsureProjectAccessAsync(sprint.ProjectId);

        if (await _projectMilestoneRepository.AnyAsync(item => item.ProjectId == sprint.ProjectId && item.SprintId == sprintId))
        {
            throw new BusinessException("Sprint 已关联里程碑，不能直接删除。");
        }

        if (await _projectTaskRepository.AnyAsync(item => item.ProjectId == sprint.ProjectId && item.SprintId == sprintId))
        {
            throw new BusinessException("Sprint 已关联任务，不能直接删除。");
        }

        if (await _projectWbsNodeRepository.AnyAsync(item => item.ProjectId == sprint.ProjectId && item.SprintId == sprintId))
        {
            throw new BusinessException("Sprint 已关联 WBS 节点，不能直接删除。");
        }

        if (await _projectReportRepository.AnyAsync(item => item.ProjectId == sprint.ProjectId && item.SprintId == sprintId))
        {
            throw new BusinessException("Sprint 已关联项目报告，不能直接删除。");
        }

        if (await _projectAcceptanceRecordRepository.AnyAsync(item => item.ProjectId == sprint.ProjectId && item.SprintId == sprintId))
        {
            throw new BusinessException("Sprint 已关联验收记录，不能直接删除。");
        }

        await _projectSprintRepository.DeleteAsync(sprint, autoSave: true);
    }

    public async Task DeleteMilestoneAsync(Guid milestoneId)
    {
        var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
        await _projectAccessGuard.EnsureProjectAccessAsync(milestone.ProjectId);

        if (await _projectWbsNodeRepository.AnyAsync(item => item.ProjectId == milestone.ProjectId && item.MilestoneId == milestoneId))
        {
            throw new BusinessException("里程碑已关联 WBS 节点，不能直接删除。");
        }

        if (await _projectRequirementRepository.AnyAsync(item => item.ProjectId == milestone.ProjectId && item.MilestoneId == milestoneId))
        {
            throw new BusinessException("里程碑已关联需求，不能直接删除。");
        }

        if (await _projectAcceptanceCriteriaRepository.AnyAsync(item => item.ProjectId == milestone.ProjectId && item.MilestoneId == milestoneId))
        {
            throw new BusinessException("里程碑已关联验收标准，不能直接删除。");
        }

        if (await _projectAcceptanceRecordRepository.AnyAsync(item => item.ProjectId == milestone.ProjectId && item.MilestoneId == milestoneId))
        {
            throw new BusinessException("里程碑已关联验收记录，不能直接删除。");
        }

        if (await _projectChangeImpactRepository.AnyAsync(item => item.ProjectId == milestone.ProjectId && item.RelatedMilestoneId == milestoneId))
        {
            throw new BusinessException("里程碑已关联变更影响分析，不能直接删除。");
        }

        await _projectMilestoneRepository.DeleteAsync(milestone, autoSave: true);
    }

    public async Task UpdateWbsNodeAsync(Guid wbsNodeId, ProjectWbsNodeSaveInput input)
    {
        var node = await _projectWbsNodeRepository.GetAsync(wbsNodeId);
        await _projectAccessGuard.EnsureProjectAccessAsync(node.ProjectId);
        await ValidateWbsNodeReferencesAsync(node.ProjectId, input);

        if (input.ParentId == wbsNodeId)
        {
            throw new BusinessException("WBS 节点不能把自己设为父节点。");
        }

        var projectWbsNodes = await _projectWbsNodeRepository.GetListAsync(item => item.ProjectId == node.ProjectId);
        if (input.ParentId.HasValue)
        {
            if (projectWbsNodes.All(item => item.Id != input.ParentId.Value))
            {
                throw new BusinessException("WBS 父节点必须属于当前项目。");
            }

            if (WouldCreateWbsNodeLoop(wbsNodeId, input.ParentId.Value, projectWbsNodes))
            {
                throw new BusinessException("WBS 父子关系不能形成循环。");
            }
        }

        var wbsCode = input.WbsCode.Trim();
        if (projectWbsNodes.Any(item => item.Id != wbsNodeId && item.WbsCode.Equals(wbsCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("WBS 编码在当前项目中已存在。");
        }

        node.Update(
            wbsCode,
            input.Name.Trim(),
            input.Type.Trim(),
            input.Status.Trim(),
            input.ParentId,
            input.MilestoneId,
            input.SprintId,
            input.OwnerUserId,
            input.SortOrder,
            input.ScopeDescription,
            input.DeliveryDescription,
            input.EstimateSummary);
        await _projectWbsNodeRepository.UpdateAsync(node, autoSave: true);
    }

    public async Task UpdateSprintAsync(Guid sprintId, ProjectSprintSaveInput input)
    {
        var sprint = await _projectSprintRepository.GetAsync(sprintId);
        await _projectAccessGuard.EnsureProjectAccessAsync(sprint.ProjectId);
        await ValidateSprintInputAsync(sprint.ProjectId, input, sprintId);

        sprint.Update(
            input.Name.Trim(),
            input.OwnerUserId,
            input.StartDate,
            input.EndDate,
            input.Status.Trim(),
            input.Progress,
            input.Summary);
        await _projectSprintRepository.UpdateAsync(sprint, autoSave: true);
    }

    public async Task UpdateMilestoneAsync(Guid milestoneId, ProjectMilestoneSaveInput input)
    {
        var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
        await _projectAccessGuard.EnsureProjectAccessAsync(milestone.ProjectId);
        await ValidateMilestoneInputAsync(milestone.ProjectId, input);

        var status = input.Status.Trim();
        if (!ProjectStatuses.IsClosed(milestone.Status) && ProjectStatuses.IsClosed(status))
        {
            throw new BusinessException("里程碑关闭请使用状态更新入口，以完成验收和质量校验。");
        }

        milestone.Update(
            input.Name.Trim(),
            input.OwnerUserId,
            input.PlannedCompletionDate,
            status,
            input.SprintId,
            input.ActualCompletionDate);
        await _projectMilestoneRepository.UpdateAsync(milestone, autoSave: true);
    }

    public async Task UpdateTaskWbsAsync(Guid taskId, ProjectTaskWbsUpdateInput input)
    {
        var task = await _projectTaskRepository.GetAsync(taskId);
        await _projectAccessGuard.EnsureProjectAccessAsync(task.ProjectId);
        if (input.ParentTaskId == taskId)
        {
            throw new BusinessException("任务不能把自己设为父任务。");
        }

        var projectTasks = await _projectTaskRepository.GetListAsync(item => item.ProjectId == task.ProjectId);
        if (input.ParentTaskId.HasValue)
        {
            var parentTask = projectTasks.FirstOrDefault(item => item.Id == input.ParentTaskId.Value);
            if (parentTask == null)
            {
                throw new BusinessException("父任务必须属于当前项目。");
            }

            if (WouldCreateWbsLoop(taskId, input.ParentTaskId.Value, projectTasks))
            {
                throw new BusinessException("WBS 父子关系不能形成循环。");
            }
        }

        if (input.SprintId.HasValue
            && !await _projectSprintRepository.AnyAsync(item => item.Id == input.SprintId.Value && item.ProjectId == task.ProjectId))
        {
            throw new BusinessException("任务关联的 Sprint 必须属于当前项目。");
        }

        var requirement = input.RequirementId.HasValue
            ? await _projectRequirementRepository.FindAsync(input.RequirementId.Value)
            : null;
        if (input.RequirementId.HasValue && (requirement == null || requirement.ProjectId != task.ProjectId))
        {
            throw new BusinessException("任务关联的需求必须属于当前项目。");
        }

        var effectiveWbsNodeId = input.WbsNodeId ?? requirement?.WbsNodeId;
        if (input.WbsNodeId.HasValue
            && requirement?.WbsNodeId.HasValue == true
            && input.WbsNodeId.Value != requirement.WbsNodeId.Value)
        {
            throw new BusinessException("任务关联的需求与 WBS 节点不一致。");
        }

        var wbsNode = effectiveWbsNodeId.HasValue
            ? await _projectWbsNodeRepository.FindAsync(effectiveWbsNodeId.Value)
            : null;
        if (effectiveWbsNodeId.HasValue && (wbsNode == null || wbsNode.ProjectId != task.ProjectId))
        {
            throw new BusinessException("任务关联的 WBS 节点必须属于当前项目。");
        }

        var wbsCode = wbsNode?.WbsCode ?? input.WbsCode.Trim();
        if (!string.IsNullOrWhiteSpace(wbsCode)
            && projectTasks.Any(item =>
                item.Id != taskId &&
                !effectiveWbsNodeId.HasValue &&
                item.WbsCode.Equals(wbsCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("WBS 编码在当前项目中已存在。");
        }

        if (input.BaselineStartTime.HasValue
            && input.BaselineEndTime.HasValue
            && input.BaselineStartTime.Value > input.BaselineEndTime.Value)
        {
            throw new BusinessException("基线开始时间不能晚于基线结束时间。");
        }

        task.UpdateWbs(
            input.ParentTaskId,
            input.SprintId,
            wbsCode,
            input.SortOrder,
            input.BaselineStartTime,
            input.BaselineEndTime,
            effectiveWbsNodeId,
            input.RequirementId);
        await _projectTaskRepository.UpdateAsync(task, autoSave: true);
    }

    public async Task UpdateTaskAsync(Guid taskId, ProjectTaskSaveInput input)
    {
        var task = await _projectTaskRepository.GetAsync(taskId);
        await _projectAccessGuard.EnsureProjectAccessAsync(task.ProjectId);
        var projectTasks = await _projectTaskRepository.GetListAsync(item => item.ProjectId == task.ProjectId);

        if (string.IsNullOrWhiteSpace(input.TaskCode))
        {
            throw new BusinessException("任务编号不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            throw new BusinessException("任务标题不能为空。");
        }

        var taskCode = input.TaskCode.Trim();
        if (projectTasks.Any(item =>
                item.Id != taskId &&
                item.TaskCode.Equals(taskCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("任务编号在当前项目中已存在。");
        }

        await ValidateTaskCoreInputAsync(
            input.OwnerUserId,
            input.PlannedStartTime,
            input.PlannedEndTime,
            input.EstimatedWorkHours);

        var status = string.IsNullOrWhiteSpace(input.Status)
            ? ProjectStatuses.InProgress
            : input.Status.Trim();
        if (!SupportedTaskStatuses.Contains(status))
        {
            throw new BusinessException("任务状态不支持。");
        }

        if (input.SprintId.HasValue
            && !await _projectSprintRepository.AnyAsync(item => item.Id == input.SprintId.Value && item.ProjectId == task.ProjectId))
        {
            throw new BusinessException("任务所属 Sprint 必须属于当前项目。");
        }

        var wbsNode = input.WbsNodeId.HasValue
            ? await _projectWbsNodeRepository.FindAsync(input.WbsNodeId.Value)
            : null;
        if (input.WbsNodeId.HasValue && (wbsNode == null || wbsNode.ProjectId != task.ProjectId))
        {
            throw new BusinessException("任务关联的 WBS 节点必须属于当前项目。");
        }

        var productOwnerUserIds = NormalizeTaskParticipantIds(input.ProductOwnerUserIds, input.ProductOwnerUserId);
        var developerOwnerUserIds = NormalizeTaskParticipantIds(input.DeveloperOwnerUserIds, input.DeveloperOwnerUserId);
        var testerOwnerUserIds = NormalizeTaskParticipantIds(input.TesterOwnerUserIds, input.TesterOwnerUserId);
        var productOwnerUserId = FirstOrDefaultParticipantId(productOwnerUserIds);
        var developerOwnerUserId = FirstOrDefaultParticipantId(developerOwnerUserIds);
        var testerOwnerUserId = FirstOrDefaultParticipantId(testerOwnerUserIds);
        var ownerNames = await ResolveTaskOwnerNamesAsync(
            productOwnerUserId,
            developerOwnerUserId,
            testerOwnerUserId);

        task.UpdateBasicInfo(
            taskCode,
            input.Title.Trim(),
            input.OwnerUserId,
            status,
            NormalizeTaskPriority(input.Priority),
            input.PlannedStartTime,
            input.PlannedEndTime,
            input.EstimatedWorkHours,
            input.SprintId,
            input.WbsNodeId,
            wbsNode?.WbsCode ?? string.Empty,
            ownerNames.ProductOwnerName,
            ownerNames.DeveloperOwnerName,
            ownerNames.TesterOwnerName,
            productOwnerUserId,
            developerOwnerUserId,
            testerOwnerUserId,
            input.Description,
            input.IsOutOfContract);
        await _projectTaskRepository.UpdateAsync(task, autoSave: true);
        await SyncTaskParticipantsAsync(task.ProjectId, task.Id, productOwnerUserIds, developerOwnerUserIds, testerOwnerUserIds);
    }

    public async Task<Guid> RecordTaskProgressAsync(Guid taskId, ProjectTaskProgressInput input)
    {
        var task = await _projectTaskRepository.GetAsync(taskId);
        await _projectAccessGuard.EnsureProjectAccessAsync(task.ProjectId);

        if (!_currentUser.Id.HasValue)
        {
            throw new BusinessException("当前用户信息缺失，无法记录任务进展。");
        }

        if (input.Hours <= 0 || input.Hours > 24)
        {
            throw new BusinessException("本次工时必须大于 0 且不超过 24 小时。");
        }

        if (string.IsNullOrWhiteSpace(input.Summary))
        {
            throw new BusinessException("请填写本次进展说明。");
        }

        var status = string.IsNullOrWhiteSpace(input.Status)
            ? ProjectStatuses.InProgress
            : input.Status.Trim();
        if (!SupportedTaskStatuses.Contains(status))
        {
            throw new BusinessException("任务状态不支持。");
        }

        var isBlocked = !ProjectStatuses.IsClosed(status) && (input.IsBlocked || status == "阻塞");
        if (isBlocked && string.IsNullOrWhiteSpace(input.BlockReason))
        {
            throw new BusinessException("任务标记为阻塞时必须填写阻塞原因。");
        }

        var workDate = (input.WorkDate ?? DateTime.Today).Date;
        var summary = input.Summary.Trim();
        var currentUserId = _currentUser.Id.Value;
        var worklog = new ProjectWorklog(
            Guid.NewGuid(),
            task.ProjectId,
            currentUserId,
            ResolveWeekStartDate(workDate),
            workDate,
            Math.Round(input.Hours, 1),
            summary,
            task.Id);

        await _projectWorklogRepository.InsertAsync(worklog, autoSave: true);

        var taskWorklogs = await _projectWorklogRepository.GetListAsync(item => item.TaskId == task.Id);
        var actualWorkHours = taskWorklogs.Sum(item => item.Hours);
        task.RecordProgress(
            status,
            isBlocked,
            input.BlockReason,
            actualWorkHours,
            workDate);
        await _projectTaskRepository.UpdateAsync(task, autoSave: true);

        return worklog.Id;
    }

    private async Task ValidateWbsNodeReferencesAsync(Guid projectId, ProjectWbsNodeSaveInput input)
    {
        if (input.SprintId.HasValue
            && !await _projectSprintRepository.AnyAsync(item => item.Id == input.SprintId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("WBS 节点关联的 Sprint 必须属于当前项目。");
        }

        if (input.MilestoneId.HasValue
            && !await _projectMilestoneRepository.AnyAsync(item => item.Id == input.MilestoneId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("WBS 节点关联的里程碑必须属于当前项目。");
        }

        if (input.OwnerUserId.HasValue
            && !await _userRepository.AnyAsync(item => item.Id == input.OwnerUserId.Value))
        {
            throw new BusinessException("WBS 节点负责人不存在。");
        }

        if (string.IsNullOrWhiteSpace(input.WbsCode))
        {
            throw new BusinessException("WBS 编码不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new BusinessException("WBS 节点名称不能为空。");
        }
    }

    private async Task ValidateSprintInputAsync(Guid projectId, ProjectSprintSaveInput input, Guid? currentSprintId = null)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new BusinessException("Sprint 名称不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Status))
        {
            throw new BusinessException("Sprint 状态不能为空。");
        }

        if (input.OwnerUserId == Guid.Empty || !await _userRepository.AnyAsync(item => item.Id == input.OwnerUserId))
        {
            throw new BusinessException("Sprint 负责人不存在。");
        }

        if (input.StartDate.HasValue
            && input.EndDate.HasValue
            && input.StartDate.Value > input.EndDate.Value)
        {
            throw new BusinessException("开始时间不能晚于结束时间。");
        }

        await Task.CompletedTask;
    }

    private async Task ValidateMilestoneInputAsync(Guid projectId, ProjectMilestoneSaveInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new BusinessException("里程碑名称不能为空。");
        }

        if (string.IsNullOrWhiteSpace(input.Status))
        {
            throw new BusinessException("里程碑状态不能为空。");
        }

        if (input.PlannedCompletionDate == default)
        {
            throw new BusinessException("里程碑计划完成日期不能为空。");
        }

        if (input.OwnerUserId == Guid.Empty || !await _userRepository.AnyAsync(item => item.Id == input.OwnerUserId))
        {
            throw new BusinessException("里程碑负责人不存在。");
        }

        if (input.SprintId.HasValue
            && !await _projectSprintRepository.AnyAsync(item => item.Id == input.SprintId.Value && item.ProjectId == projectId))
        {
            throw new BusinessException("里程碑关联的 Sprint 必须属于当前项目。");
        }
    }

    private static bool WouldCreateDependencyLoop(
        Guid predecessorTaskId,
        Guid successorTaskId,
        IReadOnlyCollection<ProjectTaskDependency> existingDependencies)
    {
        var dependenciesByPredecessor = existingDependencies
            .GroupBy(item => item.PredecessorTaskId)
            .ToDictionary(item => item.Key, item => item.Select(x => x.SuccessorTaskId).ToList());
        var visited = new HashSet<Guid>();
        var stack = new Stack<Guid>();
        stack.Push(successorTaskId);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            if (current == predecessorTaskId)
            {
                return true;
            }

            if (dependenciesByPredecessor.TryGetValue(current, out var successors))
            {
                foreach (var successor in successors)
                {
                    stack.Push(successor);
                }
            }
        }

        return false;
    }

    private static string ResolveNextTaskCode(IReadOnlyCollection<ProjectTask> tasks)
    {
        var maxNumber = tasks
            .Select(item => item.TaskCode)
            .Select(code => code.StartsWith("TASK-", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(code[5..], out var number)
                    ? number
                    : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"TASK-{maxNumber + 1:000}";
    }

    private static string ResolveNextRequirementCode(IReadOnlyCollection<ProjectRequirement> requirements)
    {
        var maxNumber = requirements
            .Select(item => item.RequirementCode)
            .Select(code => code.StartsWith("REQ-", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(code[4..], out var number)
                    ? number
                    : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"REQ-{maxNumber + 1:000}";
    }

    private static string ResolveNextSprintCode(IReadOnlyCollection<ProjectSprint> sprints)
    {
        var maxNumber = sprints
            .Select(item => item.SprintCode)
            .Select(code => code.StartsWith("SPR-", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(code[4..], out var number)
                    ? number
                    : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"SPR-{maxNumber + 1:000}";
    }

    private static DateTime ResolveWeekStartDate(DateTime workDate)
    {
        return workDate.Date.AddDays(-((int)workDate.DayOfWeek + 6) % 7);
    }

    private async Task<string> ValidateTaskCreationInputAsync(
        IReadOnlyCollection<ProjectTask> projectTasks,
        ProjectTaskFromRequirementInput input)
    {
        return await ValidateTaskCreationInputAsync(
            projectTasks,
            input.OwnerUserId,
            input.PlannedStartTime,
            input.PlannedEndTime,
            input.EstimatedWorkHours,
            input.TaskCode);
    }

    private async Task<string> ValidateTaskCreationInputAsync(
        IReadOnlyCollection<ProjectTask> projectTasks,
        ProjectTaskFromWbsInput input)
    {
        return await ValidateTaskCreationInputAsync(
            projectTasks,
            input.OwnerUserId,
            input.PlannedStartTime,
            input.PlannedEndTime,
            input.EstimatedWorkHours,
            input.TaskCode);
    }

    private async Task<string> ValidateTaskCreationInputAsync(
        IReadOnlyCollection<ProjectTask> projectTasks,
        Guid ownerUserId,
        DateTime? plannedStartTime,
        DateTime? plannedEndTime,
        double? estimatedWorkHours,
        string taskCodeInput)
    {
        await ValidateTaskCoreInputAsync(
            ownerUserId,
            plannedStartTime,
            plannedEndTime,
            estimatedWorkHours);

        var taskCode = string.IsNullOrWhiteSpace(taskCodeInput)
            ? ResolveNextTaskCode(projectTasks)
            : taskCodeInput.Trim();
        if (projectTasks.Any(item => item.TaskCode.Equals(taskCode, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("任务编号在当前项目中已存在。");
        }

        return taskCode;
    }

    private async Task ValidateTaskCoreInputAsync(
        Guid ownerUserId,
        DateTime? plannedStartTime,
        DateTime? plannedEndTime,
        double? estimatedWorkHours)
    {
        if (ownerUserId == Guid.Empty || !await _userRepository.AnyAsync(item => item.Id == ownerUserId))
        {
            throw new BusinessException("任务负责人不存在。");
        }

        if (plannedStartTime.HasValue
            && plannedEndTime.HasValue
            && plannedStartTime.Value > plannedEndTime.Value)
        {
            throw new BusinessException("计划开始时间不能晚于计划完成时间。");
        }

        if (estimatedWorkHours.HasValue && estimatedWorkHours.Value < 0)
        {
            throw new BusinessException("预计工时不能为负数。");
        }
    }

    private async Task<(
        string? ProductOwnerName,
        string? DeveloperOwnerName,
        string? TesterOwnerName)> ResolveTaskOwnerNamesAsync(
        Guid? productOwnerUserId,
        Guid? developerOwnerUserId,
        Guid? testerOwnerUserId)
    {
        await ValidateOptionalUserAsync(productOwnerUserId, "产品负责人不存在。");
        await ValidateOptionalUserAsync(developerOwnerUserId, "开发负责人不存在。");
        await ValidateOptionalUserAsync(testerOwnerUserId, "测试负责人不存在。");

        return (
            await ResolveUserNameAsync(productOwnerUserId),
            await ResolveUserNameAsync(developerOwnerUserId),
            await ResolveUserNameAsync(testerOwnerUserId));
    }

    private async Task ValidateOptionalUserAsync(Guid? userId, string errorMessage)
    {
        if (userId.HasValue && !await _userRepository.AnyAsync(item => item.Id == userId.Value))
        {
            throw new BusinessException(errorMessage);
        }
    }

    private async Task SyncTaskParticipantsAsync(
        Guid projectId,
        Guid taskId,
        IReadOnlyCollection<Guid> productOwnerUserIds,
        IReadOnlyCollection<Guid> developerOwnerUserIds,
        IReadOnlyCollection<Guid> testerOwnerUserIds)
    {
        await SyncTaskParticipantsByRoleAsync(projectId, taskId, "产品", productOwnerUserIds);
        await SyncTaskParticipantsByRoleAsync(projectId, taskId, "开发", developerOwnerUserIds);
        await SyncTaskParticipantsByRoleAsync(projectId, taskId, "测试", testerOwnerUserIds);
    }

    private async Task SyncTaskParticipantsByRoleAsync(
        Guid projectId,
        Guid taskId,
        string roleName,
        IReadOnlyCollection<Guid> userIds)
    {
        foreach (var userId in userIds)
        {
            await ValidateOptionalUserAsync(userId, $"{roleName}参与人不存在。");
        }

        var existing = await _projectTaskParticipantRepository.GetListAsync(item =>
            item.ProjectId == projectId &&
            item.TaskId == taskId &&
            item.RoleName == roleName);
        foreach (var participant in existing)
        {
            await _projectTaskParticipantRepository.DeleteAsync(participant);
        }

        foreach (var userId in userIds)
        {
            await _projectTaskParticipantRepository.InsertAsync(
                new ProjectTaskParticipant(Guid.NewGuid(), projectId, taskId, userId, roleName));
        }
    }

    private static List<Guid> NormalizeTaskParticipantIds(
        IReadOnlyCollection<Guid>? userIds,
        Guid? primaryUserId)
    {
        var values = new List<Guid>();
        if (primaryUserId.HasValue && primaryUserId.Value != Guid.Empty)
        {
            values.Add(primaryUserId.Value);
        }

        values.AddRange((userIds ?? []).Where(userId => userId != Guid.Empty));
        return values.Distinct().ToList();
    }

    private static Guid? FirstOrDefaultParticipantId(IReadOnlyCollection<Guid> userIds)
    {
        var userId = userIds.FirstOrDefault();
        return userId == Guid.Empty ? null : userId;
    }

    private async Task<string?> ResolveUserNameAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        var user = await _userRepository.FindAsync(userId.Value);
        return user == null ? null : string.IsNullOrWhiteSpace(user.Name) ? user.UserName : user.Name;
    }

    private static int NormalizeVersionNo(int versionNo)
    {
        return versionNo <= 0 ? 1 : versionNo;
    }

    private static string NormalizeTaskPriority(string? priority)
    {
        var value = priority?.Trim();
        return value switch
        {
            ProjectPriorities.P0 or ProjectPriorities.P1 or ProjectPriorities.P2 or ProjectPriorities.P3 => value,
            _ => ProjectPriorities.P1
        };
    }

    private static bool WouldCreateWbsLoop(
        Guid taskId,
        Guid parentTaskId,
        IReadOnlyCollection<ProjectTask> projectTasks)
    {
        var parentMap = projectTasks
            .Where(item => item.ParentTaskId.HasValue)
            .ToDictionary(item => item.Id, item => item.ParentTaskId!.Value);
        var current = parentTaskId;
        var visited = new HashSet<Guid>();

        while (visited.Add(current))
        {
            if (current == taskId)
            {
                return true;
            }

            if (!parentMap.TryGetValue(current, out current))
            {
                return false;
            }
        }

        return true;
    }

    private static bool WouldCreateWbsNodeLoop(
        Guid wbsNodeId,
        Guid parentId,
        IReadOnlyCollection<ProjectWbsNode> projectWbsNodes)
    {
        var parentMap = projectWbsNodes
            .Where(item => item.ParentId.HasValue)
            .ToDictionary(item => item.Id, item => item.ParentId!.Value);
        var current = parentId;
        var visited = new HashSet<Guid>();

        while (visited.Add(current))
        {
            if (current == wbsNodeId)
            {
                return true;
            }

            if (!parentMap.TryGetValue(current, out current))
            {
                return false;
            }
        }

        return true;
    }

}
