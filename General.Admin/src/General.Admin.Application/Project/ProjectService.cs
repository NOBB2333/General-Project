using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.Project;

public class ProjectService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectCharter, Guid> _projectCharterRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<ProjectCycle, Guid> _projectCycleRepository;
    private readonly IRepository<ProjectDocument, Guid> _projectDocumentRepository;
    private readonly IRepository<ProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<ProjectMemberRole, Guid> _projectMemberRoleRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectQualityChecklist, Guid> _projectQualityChecklistRepository;
    private readonly IRepository<ProjectQualityInspection, Guid> _projectQualityInspectionRepository;
    private readonly IRepository<ProjectRaidItem, Guid> _projectRaidItemRepository;
    private readonly IRepository<ProjectTaskDependency, Guid> _projectTaskDependencyRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectService(
        ICurrentUser currentUser,
        CurrentUserDataScopeService dataScopeService,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<ProjectAcceptanceCriteria, Guid> projectAcceptanceCriteriaRepository,
        IRepository<ProjectCharter, Guid> projectCharterRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<ProjectCycle, Guid> projectCycleRepository,
        IRepository<ProjectDocument, Guid> projectDocumentRepository,
        IRepository<ProjectIssue, Guid> projectIssueRepository,
        IRepository<ProjectMember, Guid> projectMemberRepository,
        IRepository<ProjectMemberRole, Guid> projectMemberRoleRepository,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<ProjectQualityChecklist, Guid> projectQualityChecklistRepository,
        IRepository<ProjectQualityInspection, Guid> projectQualityInspectionRepository,
        IRepository<ProjectRaidItem, Guid> projectRaidItemRepository,
        IRepository<ProjectTaskDependency, Guid> projectTaskDependencyRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository,
        IRepository<ProjectWorklog, Guid> projectWorklogRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _dataScopeService = dataScopeService;
        _organizationUnitRepository = organizationUnitRepository;
        _projectAcceptanceCriteriaRepository = projectAcceptanceCriteriaRepository;
        _projectCharterRepository = projectCharterRepository;
        _projectRepository = projectRepository;
        _projectCycleRepository = projectCycleRepository;
        _projectDocumentRepository = projectDocumentRepository;
        _projectIssueRepository = projectIssueRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectMemberRoleRepository = projectMemberRoleRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectQualityChecklistRepository = projectQualityChecklistRepository;
        _projectQualityInspectionRepository = projectQualityInspectionRepository;
        _projectRaidItemRepository = projectRaidItemRepository;
        _projectTaskDependencyRepository = projectTaskDependencyRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _userRepository = userRepository;
    }

    public async Task<ProjectWorkspaceDto> GetWorkspaceAsync()
    {
        var context = await BuildContextAsync();
        var today = DateTime.Today;
        var currentUserId = _currentUser.Id;

        var ongoingProjects = context.Projects
            .Where(project => !IsClosed(project.Status))
            .ToList();
        var myTasks = context.Tasks
            .Where(task => currentUserId.HasValue && task.OwnerUserId == currentUserId.Value)
            .Where(task => !IsClosed(task.Status))
            .OrderBy(task => task.PlannedEndTime ?? DateTime.MaxValue)
            .Take(8)
            .ToList();
        var weeklyMilestones = context.Milestones
            .Where(item => !IsClosed(item.Status))
            .Where(item => item.PlannedCompletionDate.Date >= today && item.PlannedCompletionDate.Date <= today.AddDays(7))
            .OrderBy(item => item.PlannedCompletionDate)
            .Take(8)
            .ToList();
        var raidAlerts = context.RaidItems
            .Where(item => !IsClosed(item.Status))
            .Where(item => item.Level.Equals("高", StringComparison.OrdinalIgnoreCase) || (item.PlannedResolveDate.HasValue && item.PlannedResolveDate.Value.Date < today))
            .OrderByDescending(item => item.Level)
            .ThenBy(item => item.PlannedResolveDate ?? DateTime.MaxValue)
            .Take(8)
            .ToList();

        return new ProjectWorkspaceDto
        {
            BlockedTaskCount = context.Tasks.Count(task => task.IsBlocked && !IsClosed(task.Status)),
            HighRiskCount = context.RaidItems.Count(item => item.Type == "风险" && item.Level == "高" && !IsClosed(item.Status)),
            KeyProjects = context.Projects
                .OrderByDescending(project => project.IsKeyProject)
                .ThenBy(project => project.PlannedEndDate ?? DateTime.MaxValue)
                .Take(6)
                .Select(project => MapProject(project, context))
                .ToList(),
            MyTasks = myTasks.Select(task => MapTask(task, context)).ToList(),
            MyTodoCount = myTasks.Count,
            OngoingProjectCount = ongoingProjects.Count,
            OverdueTaskCount = context.Tasks.Count(task => IsOverdue(task.PlannedEndTime, task.Status)),
            RaidAlerts = raidAlerts.Select(item => MapRaidItem(item, context)).ToList(),
            WeeklyMilestones = weeklyMilestones.Select(item => MapMilestone(item, context)).ToList()
        };
    }

    public async Task<ProjectDetailDto> GetDetailAsync(Guid projectId)
    {
        // 使用精准上下文：仅查询该项目的子实体
        var context = await BuildDetailContextAsync(projectId);
        var project = context.Projects[0];

        var projectTasks = (context.TasksByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.PlannedEndTime ?? DateTime.MaxValue)
            .ThenBy(item => item.TaskCode)
            .ToList();
        var projectMilestones = (context.MilestonesByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.PlannedCompletionDate)
            .ToList();
        var projectCycles = (context.CyclesByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.StartDate ?? DateTime.MaxValue)
            .ThenBy(item => item.EndDate ?? DateTime.MaxValue)
            .ToList();
        var projectRaidItems = (context.RaidItemsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(item => item.Level)
            .ThenBy(item => item.PlannedResolveDate ?? DateTime.MaxValue)
            .ToList();
        var projectIssues = (context.IssuesByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.DueDate ?? DateTime.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();
        var projectDocuments = (context.DocumentsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Title)
            .ToList();
        var projectMembers = (context.MembersByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(item => item.IsActive())
            .ThenBy(item => context.UserNameMap.GetValueOrDefault(item.UserId) ?? string.Empty)
            .ToList();
        var projectWorklogs = (context.WorklogsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(item => item.WorkDate)
            .ThenByDescending(item => item.CreationTime)
            .ToList();
        var taskDependencies = context.TaskDependencies
            .OrderBy(item => context.TaskTitleMap.GetValueOrDefault(item.SuccessorTaskId) ?? string.Empty)
            .ToList();
        var acceptanceCriteria = context.AcceptanceCriteria
            .OrderBy(item => item.Status)
            .ThenBy(item => item.Title)
            .ToList();
        var qualityChecklists = context.QualityChecklists
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Title)
            .ToList();
        var qualityInspections = context.QualityInspections
            .OrderByDescending(item => item.InspectionTime)
            .ToList();

        var completedTaskCount = projectTasks.Count(item => IsClosed(item.Status));
        var taskCompletionRate = projectTasks.Count == 0
            ? 0
            : (int)Math.Round(completedTaskCount * 100d / projectTasks.Count, MidpointRounding.AwayFromZero);
        var healthRiskScore = CalculateHealthRiskScore(projectTasks, projectIssues, projectRaidItems, taskCompletionRate);

        return new ProjectDetailDto
        {
            BudgetTotalAmount = project.BudgetTotalAmount,
            AcceptanceCriteria = acceptanceCriteria.Select(item => MapAcceptanceCriteria(item, context)).ToList(),
            CalendarItems = BuildCalendarItems(projectTasks, projectCycles, context),
            Charter = context.Charters.FirstOrDefault() is { } charter ? MapCharter(charter) : null,
            ContractTotalAmount = project.ContractTotalAmount,
            Cycles = projectCycles.Select(item => MapCycle(item, context)).ToList(),
            Description = project.Description ?? string.Empty,
            Documents = projectDocuments.Select(item => MapDocument(item, context)).ToList(),
            HighRiskCount = projectRaidItems.Count(item => item.Type == ProjectRaidTypes.Risk && item.Level == ProjectPriorities.High && !IsClosed(item.Status)),
            HealthRiskScore = healthRiskScore,
            Id = project.Id,
            IsKeyProject = project.IsKeyProject,
            Issues = projectIssues.Select(item => MapIssue(item, context)).ToList(),
            ManagerName = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            MemberCount = projectMembers.Count(item => item.IsActive()),
            Members = projectMembers.Select(item => MapMember(item, context)).ToList(),
            Milestones = projectMilestones.Select(item => MapMilestone(item, context)).ToList(),
            MyRelation = ResolveMyRelation(project, context),
            Name = project.Name,
            OpenIssueCount = projectIssues.Count(item => !IsClosed(item.Status)),
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(project.OrganizationUnitId) ?? "-",
            PlannedEndDate = project.PlannedEndDate,
            PlannedStartDate = project.PlannedStartDate,
            Priority = project.Priority,
            ProjectCode = project.ProjectCode,
            ProjectSource = project.ProjectSource ?? string.Empty,
            ProjectType = project.ProjectType ?? string.Empty,
            QualityChecklists = qualityChecklists.Select(item => MapQualityChecklist(item, context)).ToList(),
            QualityInspections = qualityInspections.Select(item => MapQualityInspection(item, context)).ToList(),
            RaidItems = projectRaidItems.Select(item => MapRaidItem(item, context)).ToList(),
            ReceivedAmount = project.ReceivedAmount,
            SponsorName = context.UserNameMap.GetValueOrDefault(project.SponsorUserId) ?? "-",
            Status = project.Status,
            TaskCompletionRate = taskCompletionRate,
            TaskDependencies = taskDependencies.Select(item => MapTaskDependency(item, context)).ToList(),
            TaskScheduleImpacts = BuildTaskScheduleImpacts(projectTasks, taskDependencies),
            Tasks = projectTasks.Select(item => MapTask(item, context)).ToList(),
            TotalWorklogHours = Math.Round(projectWorklogs.Sum(item => item.Hours), 1),
            Worklogs = projectWorklogs.Select(item => MapWorklog(item, context)).ToList()
        };
    }

    public async Task<List<ProjectListItemDto>> GetProjectListAsync(ProjectListInput input)
    {
        var context = await BuildContextAsync();
        var keyword = input.Keyword?.Trim();

        return context.Projects
            .Where(project => string.IsNullOrWhiteSpace(input.Status) || project.Status.Equals(input.Status.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(project => !input.OnlyMyRelated || IsMyRelatedProject(project, context))
            .Select(project => MapProject(project, context))
            .Where(item => string.IsNullOrWhiteSpace(keyword)
                || item.ProjectCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.ManagerName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.OrganizationUnitName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.IsKeyProject)
            .ThenBy(item => item.PlannedEndDate ?? DateTime.MaxValue)
            .ToList();
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
            input.Priority.Trim(),
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

    public async Task<Guid> CreateAcceptanceCriteriaAsync(Guid projectId, ProjectAcceptanceCriteriaSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
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

    public async Task<Guid> CreateQualityChecklistAsync(Guid projectId, ProjectQualityChecklistSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
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
        await EnsureProjectAccessAsync(projectId);
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

    public async Task<Guid> CreateTaskDependencyAsync(Guid projectId, ProjectTaskDependencySaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
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

        if (WouldCreateDependencyCycle(input.PredecessorTaskId, input.SuccessorTaskId, existingDependencies))
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

    public async Task UpdateTaskWbsAsync(Guid taskId, ProjectTaskWbsUpdateInput input)
    {
        var task = await _projectTaskRepository.GetAsync(taskId);
        await EnsureProjectAccessAsync(task.ProjectId);
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

            if (WouldCreateWbsCycle(taskId, input.ParentTaskId.Value, projectTasks))
            {
                throw new BusinessException("WBS 父子关系不能形成循环。");
            }
        }

        var wbsCode = input.WbsCode.Trim();
        if (!string.IsNullOrWhiteSpace(wbsCode)
            && projectTasks.Any(item => item.Id != taskId && item.WbsCode.Equals(wbsCode, StringComparison.OrdinalIgnoreCase)))
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
            wbsCode,
            input.SortOrder,
            input.BaselineStartTime,
            input.BaselineEndTime);
        await _projectTaskRepository.UpdateAsync(task, autoSave: true);
    }

    public async Task UpdateMemberRolesAsync(Guid projectMemberId, ProjectMemberRolesUpdateInput input)
    {
        var member = await _projectMemberRepository.GetAsync(projectMemberId);
        await EnsureProjectAccessAsync(member.ProjectId);
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

    public async Task UpdateIssueQualityLinkAsync(Guid issueId, ProjectIssueQualityLinkInput input)
    {
        var issue = await _projectIssueRepository.GetAsync(issueId);
        await EnsureProjectAccessAsync(issue.ProjectId);
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

    public async Task UpdateProjectStatusAsync(Guid projectId, ProjectStatusUpdateInput input)
    {
        var project = await _projectRepository.GetAsync(projectId);
        await EnsureProjectAccessAsync(projectId);
        var status = input.Status.Trim();
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new BusinessException("项目状态不能为空。");
        }

        if (IsExecutionStatus(status))
        {
            var charter = (await _projectCharterRepository.GetListAsync(item => item.ProjectId == projectId)).FirstOrDefault();
            if (charter?.ApprovalStatus != "已批准")
            {
                throw new BusinessException("项目进入执行中前必须先批准项目章程。");
            }
        }

        project.UpdateStatus(status);
        await _projectRepository.UpdateAsync(project, autoSave: true);
    }

    public async Task UpdateMilestoneStatusAsync(Guid milestoneId, ProjectMilestoneStatusUpdateInput input)
    {
        var milestone = await _projectMilestoneRepository.GetAsync(milestoneId);
        await EnsureProjectAccessAsync(milestone.ProjectId);
        var status = input.Status.Trim();
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new BusinessException("里程碑状态不能为空。");
        }

        if (IsClosed(status))
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
        }

        milestone.UpdateStatus(status, input.ActualCompletionDate);
        await _projectMilestoneRepository.UpdateAsync(milestone, autoSave: true);
    }

    public async Task<Guid> SaveCharterAsync(Guid projectId, ProjectCharterSaveInput input)
    {
        await EnsureProjectAccessAsync(projectId);
        var existing = (await _projectCharterRepository.GetListAsync(item => item.ProjectId == projectId)).FirstOrDefault();
        if (existing == null)
        {
            var charter = new ProjectCharter(
                Guid.NewGuid(),
                projectId,
                input.BusinessCase.Trim(),
                input.Objectives.Trim(),
                input.ScopeBoundary.Trim(),
                input.Assumptions.Trim(),
                input.Constraints.Trim(),
                input.SuccessCriteria.Trim(),
                input.ApprovalStatus.Trim());
            await _projectCharterRepository.InsertAsync(charter, autoSave: true);
            return charter.Id;
        }

        existing.Update(
            input.BusinessCase.Trim(),
            input.Objectives.Trim(),
            input.ScopeBoundary.Trim(),
            input.Assumptions.Trim(),
            input.Constraints.Trim(),
            input.SuccessCriteria.Trim(),
            input.ApprovalStatus.Trim());
        await _projectCharterRepository.UpdateAsync(existing, autoSave: true);
        return existing.Id;
    }

    public async Task UpdateRaidRiskAsync(Guid raidItemId, ProjectRaidRiskUpdateInput input)
    {
        var item = await _projectRaidItemRepository.GetAsync(raidItemId);
        await EnsureProjectAccessAsync(item.ProjectId);
        item.UpdateRiskAssessment(
            input.Probability,
            input.Impact,
            input.ResponseStrategy,
            input.TriggerCondition,
            input.ContingencyPlan);
        await _projectRaidItemRepository.UpdateAsync(item, autoSave: true);
    }

    public async Task<ProjectMyRelatedDto> GetMyRelatedAsync()
    {
        var context = await BuildContextAsync();
        if (!_currentUser.Id.HasValue)
        {
            return new ProjectMyRelatedDto();
        }

        var currentUserId = _currentUser.Id.Value;
        var weekStartDate = GetWeekStart(DateTime.Today);
        var myProjects = context.Projects
            .Where(project => IsMyRelatedProject(project, context))
            .OrderByDescending(project => project.IsKeyProject)
            .ThenBy(project => project.PlannedEndDate ?? DateTime.MaxValue)
            .Select(project =>
            {
                var tasks = (context.TasksByProject.GetValueOrDefault(project.Id) ?? [])
                    .Where(item => item.OwnerUserId == currentUserId && !IsClosed(item.Status)).ToList();
                var issues = (context.IssuesByProject.GetValueOrDefault(project.Id) ?? [])
                    .Where(item => item.OwnerUserId == currentUserId && !IsClosed(item.Status)).ToList();
                var weekHours = (context.WorklogsByProject.GetValueOrDefault(project.Id) ?? [])
                    .Where(item => item.UserId == currentUserId && item.WeekStartDate.Date == weekStartDate.Date)
                    .Sum(item => item.Hours);
                var nextCycle = (context.CyclesByProject.GetValueOrDefault(project.Id) ?? [])
                    .Where(item => !IsClosed(item.Status))
                    .OrderBy(item => item.StartDate ?? DateTime.MaxValue)
                    .FirstOrDefault();

                return new ProjectOverviewItemDto
                {
                    ActiveIssueCount = issues.Count,
                    DueSoonTaskCount = tasks.Count(item => IsDueSoon(item.PlannedEndTime)),
                    Id = project.Id,
                    MyRelation = ResolveMyRelation(project, context),
                    Name = project.Name,
                    NextCycleName = nextCycle?.Name ?? "-",
                    ProjectCode = project.ProjectCode,
                    Status = project.Status,
                    WeekHours = Math.Round(weekHours, 1)
                };
            })
            .ToList();

        var myTasks = context.Tasks
            .Where(item => item.OwnerUserId == currentUserId)
            .Where(item => !IsClosed(item.Status))
            .OrderBy(item => item.PlannedEndTime ?? DateTime.MaxValue)
            .Take(12)
            .Select(item => MapTask(item, context))
            .ToList();
        var myIssues = context.Issues
            .Where(item => item.OwnerUserId == currentUserId)
            .Where(item => !IsClosed(item.Status))
            .OrderBy(item => item.DueDate ?? DateTime.MaxValue)
            .Take(10)
            .Select(item => MapIssue(item, context))
            .ToList();
        var recentWorklogs = context.Worklogs
            .Where(item => item.UserId == currentUserId)
            .OrderByDescending(item => item.WorkDate)
            .ThenByDescending(item => item.CreationTime)
            .Take(10)
            .Select(item => MapWorklog(item, context))
            .ToList();

        return new ProjectMyRelatedDto
        {
            MyIssues = myIssues,
            MyProjects = myProjects,
            MyTasks = myTasks,
            RecentWorklogs = recentWorklogs
        };
    }

    public async Task<List<ProjectTaskItemDto>> GetTaskListAsync(ProjectTaskListInput input)
    {
        var context = await BuildContextAsync();
        var keyword = input.Keyword?.Trim();

        var tasks = context.Tasks
            .Where(task => !input.ProjectId.HasValue || task.ProjectId == input.ProjectId.Value)
            .Where(task => string.IsNullOrWhiteSpace(input.Status) || task.Status.Equals(input.Status.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(task => !input.OnlyMine || (_currentUser.Id.HasValue && task.OwnerUserId == _currentUser.Id.Value))
            .Where(task => string.IsNullOrWhiteSpace(keyword)
                || task.TaskCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || task.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || context.ProjectNameMap[task.ProjectId].Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderBy(task => task.PlannedEndTime ?? DateTime.MaxValue)
            .ThenBy(task => task.TaskCode)
            .ToList();

        return tasks.Select(task => MapTask(task, context)).ToList();
    }

    public async Task<List<ProjectRaidItemDto>> GetRaidListAsync(ProjectRaidListInput input)
    {
        var context = await BuildContextAsync();
        var keyword = input.Keyword?.Trim();
        var today = DateTime.Today;

        var raidItems = context.RaidItems
            .Where(item => string.IsNullOrWhiteSpace(input.Type) || item.Type.Equals(input.Type.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(item => !input.OnlyOpen || !IsClosed(item.Status))
            .Where(item => string.IsNullOrWhiteSpace(keyword)
                || item.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || context.ProjectNameMap[item.ProjectId].Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Level)
            .ThenBy(item => item.PlannedResolveDate ?? today.AddYears(20))
            .ToList();

        return raidItems.Select(item => MapRaidItem(item, context)).ToList();
    }

    private async Task<ProjectContext> BuildContextAsync()
    {
        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .ToDictionary(item => item.Id, item => item.DisplayName);
        var users = (await _userRepository.GetListAsync())
            .ToDictionary(item => item.Id, ResolveDisplayName);
        var allProjects = (await _projectRepository.GetListAsync())
            .OrderBy(project => project.ProjectCode)
            .ToList();
        var allMembers = (await _projectMemberRepository.GetListAsync()).ToList();

        var visibleProjectIds = new HashSet<Guid>();
        var canSeeAllProjects = _currentUser.IsInRole(PlatformRoleNames.Admin)
            || _currentUser.IsInRole(PlatformRoleNames.Pmo);

        if (canSeeAllProjects)
        {
            visibleProjectIds.UnionWith(allProjects.Select(item => item.Id));
        }
        else
        {
            visibleProjectIds.UnionWith(
                allProjects
                    .Where(project => accessibleOrganizationUnitIds.Contains(project.OrganizationUnitId))
                    .Select(project => project.Id));

            if (_currentUser.Id.HasValue)
            {
                var currentUserId = _currentUser.Id.Value;
                visibleProjectIds.UnionWith(
                    allProjects
                        .Where(project =>
                            project.ManagerUserId == currentUserId ||
                            project.SponsorUserId == currentUserId)
                        .Select(project => project.Id));
                visibleProjectIds.UnionWith(
                    allMembers
                        .Where(item => item.UserId == currentUserId)
                        .Where(item => item.IsActive() || item.AllowHistoricalRead)
                        .Select(item => item.ProjectId));
            }
        }

        var projects = allProjects
            .Where(project => visibleProjectIds.Contains(project.Id))
            .ToList();
        var projectIds = projects.Select(project => project.Id).ToHashSet();

        var milestones = (await _projectMilestoneRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var cycles = (await _projectCycleRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var tasks = (await _projectTaskRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var memberRoles = (await _projectMemberRoleRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var raidItems = (await _projectRaidItemRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var issues = (await _projectIssueRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var documents = (await _projectDocumentRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var members = allMembers
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var worklogs = (await _projectWorklogRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var myMemberMap = _currentUser.Id.HasValue
            ? members
                .Where(item => item.UserId == _currentUser.Id.Value)
                .GroupBy(item => item.ProjectId)
                .ToDictionary(item => item.Key, item => item.OrderByDescending(x => x.JoinDate).First())
            : new Dictionary<Guid, ProjectMember>();

        return new ProjectContext(
            organizationUnits,
            projects,
            projects.ToDictionary(item => item.Id, item => item.Name),
            milestones,
            cycles,
            [],
            [],
            [],
            [],
            [],
            memberRoles,
            raidItems,
            tasks,
            issues,
            documents,
            members,
            worklogs,
            users,
            myMemberMap,
            _currentUser.Id,
            _currentUser.IsInRole(PlatformRoleNames.Admin),
            _currentUser.IsInRole(PlatformRoleNames.Pmo));
    }

    /// <summary>
    /// 针对单项目详情视图的精准上下文：只加载目标项目的子实体，避免全量扫描。
    /// </summary>
    private async Task<ProjectContext> BuildDetailContextAsync(Guid projectId)
    {
        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .ToDictionary(item => item.Id, item => item.DisplayName);
        var users = (await _userRepository.GetListAsync())
            .ToDictionary(item => item.Id, ResolveDisplayName);

        var project = await _projectRepository.GetAsync(projectId);
        var allMembers = (await _projectMemberRepository.GetListAsync()).ToList();

        // 权限检查：当前用户必须能看到该项目
        var canAccess = _currentUser.IsInRole(PlatformRoleNames.Admin)
            || _currentUser.IsInRole(PlatformRoleNames.Pmo)
            || accessibleOrganizationUnitIds.Contains(project.OrganizationUnitId)
            || (_currentUser.Id.HasValue &&
                (project.ManagerUserId == _currentUser.Id.Value || project.SponsorUserId == _currentUser.Id.Value))
            || (_currentUser.Id.HasValue && allMembers.Any(m =>
                m.ProjectId == projectId
                && m.UserId == _currentUser.Id.Value
                && (m.IsActive() || m.AllowHistoricalRead)));

        if (!canAccess)
        {
            throw new EntityNotFoundException(typeof(Project), projectId);
        }

        var projects = new List<Project> { project };
        var members = allMembers.Where(m => m.ProjectId == projectId).ToList();

        // 仅查询该项目的子实体（精准查询，不做全表扫描）
        var milestones = await _projectMilestoneRepository.GetListAsync(x => x.ProjectId == projectId);
        var cycles     = await _projectCycleRepository.GetListAsync(x => x.ProjectId == projectId);
        var charters   = await _projectCharterRepository.GetListAsync(x => x.ProjectId == projectId);
        var tasks      = await _projectTaskRepository.GetListAsync(x => x.ProjectId == projectId);
        var taskDependencies = await _projectTaskDependencyRepository.GetListAsync(x => x.ProjectId == projectId);
        var memberRoles = await _projectMemberRoleRepository.GetListAsync(x => x.ProjectId == projectId);
        var raidItems  = await _projectRaidItemRepository.GetListAsync(x => x.ProjectId == projectId);
        var acceptanceCriteria = await _projectAcceptanceCriteriaRepository.GetListAsync(x => x.ProjectId == projectId);
        var qualityChecklists = await _projectQualityChecklistRepository.GetListAsync(x => x.ProjectId == projectId);
        var qualityInspections = await _projectQualityInspectionRepository.GetListAsync(x => x.ProjectId == projectId);
        var issues     = await _projectIssueRepository.GetListAsync(x => x.ProjectId == projectId);
        var documents  = await _projectDocumentRepository.GetListAsync(x => x.ProjectId == projectId);
        var worklogs   = await _projectWorklogRepository.GetListAsync(x => x.ProjectId == projectId);

        var myMemberMap = _currentUser.Id.HasValue
            ? members
                .Where(item => item.UserId == _currentUser.Id.Value)
                .GroupBy(item => item.ProjectId)
                .ToDictionary(item => item.Key, item => item.OrderByDescending(x => x.JoinDate).First())
            : new Dictionary<Guid, ProjectMember>();

        return new ProjectContext(
            organizationUnits,
            projects,
            projects.ToDictionary(item => item.Id, item => item.Name),
            milestones,
            cycles,
            acceptanceCriteria,
            charters,
            qualityChecklists,
            qualityInspections,
            taskDependencies,
            memberRoles,
            raidItems,
            tasks,
            issues,
            documents,
            members,
            worklogs,
            users,
            myMemberMap,
            _currentUser.Id,
            _currentUser.IsInRole(PlatformRoleNames.Admin),
            _currentUser.IsInRole(PlatformRoleNames.Pmo));
    }

    private async Task EnsureProjectAccessAsync(Guid projectId)
    {
        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        var project = await _projectRepository.GetAsync(projectId);
        if (_currentUser.IsInRole(PlatformRoleNames.Admin)
            || _currentUser.IsInRole(PlatformRoleNames.Pmo)
            || accessibleOrganizationUnitIds.Contains(project.OrganizationUnitId)
            || (_currentUser.Id.HasValue &&
                (project.ManagerUserId == _currentUser.Id.Value || project.SponsorUserId == _currentUser.Id.Value))
            || (_currentUser.Id.HasValue && await _projectMemberRepository.AnyAsync(member =>
                member.ProjectId == projectId
                && member.UserId == _currentUser.Id.Value
                && (!member.LeaveDate.HasValue || member.LeaveDate.Value.Date >= DateTime.Today || member.AllowHistoricalRead))))
        {
            return;
        }

        throw new EntityNotFoundException(typeof(Project), projectId);
    }

    private static List<ProjectCalendarItemDto> BuildCalendarItems(
        IReadOnlyCollection<ProjectTask> tasks,
        IReadOnlyCollection<ProjectCycle> cycles,
        ProjectContext context)
    {
        var items = tasks
            .Where(item => item.PlannedStartTime.HasValue || item.PlannedEndTime.HasValue)
            .Select(item => new ProjectCalendarItemDto
            {
                ColorToken = ResolveCalendarColor(item.Priority, item.Status),
                EndDate = item.PlannedEndTime,
                Id = item.Id,
                OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
                StartDate = item.PlannedStartTime ?? item.PlannedEndTime,
                Status = item.Status,
                Title = item.Title,
                Type = "任务"
            })
            .Concat(cycles.Select(item => new ProjectCalendarItemDto
            {
                ColorToken = item.Type == "冲刺" ? "var(--ant-color-primary)" : "var(--ant-color-success)",
                EndDate = item.EndDate,
                Id = item.Id,
                OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
                StartDate = item.StartDate,
                Status = item.Status,
                Title = item.Name,
                Type = item.Type
            }))
            .OrderBy(item => item.StartDate ?? DateTime.MaxValue)
            .ToList();

        return items;
    }

    private static List<ProjectTaskScheduleImpactDto> BuildTaskScheduleImpacts(
        IReadOnlyCollection<ProjectTask> tasks,
        IReadOnlyCollection<ProjectTaskDependency> dependencies)
    {
        var taskMap = tasks.ToDictionary(item => item.Id);
        return dependencies
            .Where(item => item.DependencyType == "FinishToStart")
            .Select(item =>
            {
                taskMap.TryGetValue(item.PredecessorTaskId, out var predecessor);
                taskMap.TryGetValue(item.SuccessorTaskId, out var successor);
                return new
                {
                    Predecessor = predecessor,
                    Successor = successor
                };
            })
            .Where(item => item.Predecessor != null && item.Successor != null)
            .Where(item =>
                IsOverdue(item.Predecessor!.PlannedEndTime, item.Predecessor.Status)
                || (item.Predecessor.PlannedEndTime.HasValue
                    && item.Successor!.PlannedStartTime.HasValue
                    && item.Successor.PlannedStartTime.Value.Date < item.Predecessor.PlannedEndTime.Value.Date))
            .Select(item => new ProjectTaskScheduleImpactDto
            {
                ImpactReason = IsOverdue(item.Predecessor!.PlannedEndTime, item.Predecessor.Status)
                    ? "前置任务已延期，可能影响后续任务启动。"
                    : "后续任务计划开始早于前置任务计划完成，存在排期冲突。",
                PredecessorPlannedEndTime = item.Predecessor.PlannedEndTime,
                PredecessorTaskId = item.Predecessor.Id,
                PredecessorTaskTitle = item.Predecessor.Title,
                SuccessorPlannedStartTime = item.Successor!.PlannedStartTime,
                SuccessorTaskId = item.Successor.Id,
                SuccessorTaskTitle = item.Successor.Title
            })
            .OrderBy(item => item.SuccessorPlannedStartTime ?? DateTime.MaxValue)
            .ThenBy(item => item.SuccessorTaskTitle)
            .ToList();
    }

    private static ProjectCycleItemDto MapCycle(ProjectCycle item, ProjectContext context)
    {
        return new ProjectCycleItemDto
        {
            EndDate = item.EndDate,
            Id = item.Id,
            Name = item.Name,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            Progress = item.Progress,
            StartDate = item.StartDate,
            Status = item.Status,
            Summary = item.Summary,
            Type = item.Type
        };
    }

    private static ProjectAcceptanceCriteriaDto MapAcceptanceCriteria(ProjectAcceptanceCriteria item, ProjectContext context)
    {
        return new ProjectAcceptanceCriteriaDto
        {
            Description = item.Description,
            Id = item.Id,
            MilestoneId = item.MilestoneId,
            MilestoneName = item.MilestoneId.HasValue
                ? context.MilestoneNameMap.GetValueOrDefault(item.MilestoneId.Value) ?? "-"
                : "-",
            ProjectId = item.ProjectId,
            Status = item.Status,
            TaskId = item.TaskId,
            TaskTitle = item.TaskId.HasValue
                ? context.TaskTitleMap.GetValueOrDefault(item.TaskId.Value) ?? "-"
                : "-",
            Title = item.Title
        };
    }

    private static ProjectCharterDto MapCharter(ProjectCharter item)
    {
        return new ProjectCharterDto
        {
            ApprovalStatus = item.ApprovalStatus,
            Assumptions = item.Assumptions,
            BusinessCase = item.BusinessCase,
            Constraints = item.Constraints,
            Id = item.Id,
            Objectives = item.Objectives,
            ProjectId = item.ProjectId,
            ScopeBoundary = item.ScopeBoundary,
            SuccessCriteria = item.SuccessCriteria
        };
    }

    private static ProjectDocumentItemDto MapDocument(ProjectDocument item, ProjectContext context)
    {
        return new ProjectDocumentItemDto
        {
            Category = item.Category,
            Id = item.Id,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            Status = item.Status,
            Summary = item.Summary,
            Title = item.Title,
            Version = item.Version
        };
    }

    private static ProjectIssueItemDto MapIssue(ProjectIssue item, ProjectContext context)
    {
        return new ProjectIssueItemDto
        {
            AcceptanceCriteriaId = item.AcceptanceCriteriaId,
            AcceptanceCriteriaTitle = item.AcceptanceCriteriaId.HasValue
                ? context.AcceptanceCriteriaTitleMap.GetValueOrDefault(item.AcceptanceCriteriaId.Value) ?? "-"
                : "-",
            DeveloperOwnerName = item.DeveloperOwnerName,
            DeveloperOwnerUserId = item.DeveloperOwnerUserId,
            DueDate = item.DueDate,
            Id = item.Id,
            IsOverdue = item.DueDate.HasValue && item.DueDate.Value.Date < DateTime.Today && !IsClosed(item.Status),
            Level = item.Level,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            ProductOwnerName = item.ProductOwnerName,
            ProductOwnerUserId = item.ProductOwnerUserId,
            QualityChecklistId = item.QualityChecklistId,
            QualityChecklistTitle = item.QualityChecklistId.HasValue
                ? context.QualityChecklistTitleMap.GetValueOrDefault(item.QualityChecklistId.Value) ?? "-"
                : "-",
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            RequirementTitle = item.RequirementTitle,
            Status = item.Status,
            TesterOwnerName = item.TesterOwnerName,
            TesterOwnerUserId = item.TesterOwnerUserId,
            Title = item.Title,
            Type = item.Type
        };
    }

    private static ProjectMemberItemDto MapMember(ProjectMember item, ProjectContext context)
    {
        var roleNames = ResolveMemberRoles(item, context);
        return new ProjectMemberItemDto
        {
            AccessLevel = item.IsActive() ? "编辑" : item.AllowHistoricalRead ? "历史只读" : "已移除",
            Id = item.Id,
            IsActive = item.IsActive(),
            JoinDate = item.JoinDate,
            LeaveDate = item.LeaveDate,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(item.OrganizationUnitId) ?? "-",
            RoleNames = roleNames,
            UserId = item.UserId,
            UserName = context.UserNameMap.GetValueOrDefault(item.UserId) ?? "-"
        };
    }

    private static ProjectQualityChecklistDto MapQualityChecklist(ProjectQualityChecklist item, ProjectContext context)
    {
        return new ProjectQualityChecklistDto
        {
            Category = item.Category,
            Description = item.Description,
            Id = item.Id,
            InspectionCount = context.QualityInspections.Count(inspection => inspection.QualityChecklistId == item.Id),
            ProjectId = item.ProjectId,
            Title = item.Title
        };
    }

    private static ProjectQualityInspectionDto MapQualityInspection(ProjectQualityInspection item, ProjectContext context)
    {
        return new ProjectQualityInspectionDto
        {
            FindingSummary = item.FindingSummary,
            Id = item.Id,
            InspectionResult = item.InspectionResult,
            InspectionTime = item.InspectionTime,
            InspectorName = context.UserNameMap.GetValueOrDefault(item.InspectorUserId) ?? "-",
            ProjectId = item.ProjectId,
            QualityChecklistId = item.QualityChecklistId,
            QualityChecklistTitle = context.QualityChecklistTitleMap.GetValueOrDefault(item.QualityChecklistId) ?? "-",
            RectificationRequirement = item.RectificationRequirement
        };
    }

    private static ProjectListItemDto MapProject(Project project, ProjectContext context)
    {
        var projectTasks      = context.TasksByProject.GetValueOrDefault(project.Id)      ?? [];
        var projectMilestones = context.MilestonesByProject.GetValueOrDefault(project.Id) ?? [];
        var raidItems         = context.RaidItemsByProject.GetValueOrDefault(project.Id)  ?? [];
        var members           = context.MembersByProject.GetValueOrDefault(project.Id)    ?? [];

        return new ProjectListItemDto
        {
            BlockedTaskCount     = projectTasks.Count(item => item.IsBlocked && !IsClosed(item.Status)),
            CompletedTaskCount   = projectTasks.Count(item => item.Status == ProjectStatuses.Completed || item.Status == ProjectStatuses.Closed),
            HighRiskCount        = raidItems.Count(item => item.Type == ProjectRaidTypes.Risk && item.Level == ProjectPriorities.High && !IsClosed(item.Status)),
            Id                   = project.Id,
            IsKeyProject         = project.IsKeyProject,
            ManagerName          = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            MemberCount          = members.Count(item => item.IsActive()),
            MilestoneTotalCount  = projectMilestones.Count,
            MyRelation           = ResolveMyRelation(project, context),
            Name                 = project.Name,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(project.OrganizationUnitId) ?? "-",
            OverdueMilestoneCount = projectMilestones.Count(item => item.PlannedCompletionDate.Date < DateTime.Today && !IsClosed(item.Status)),
            OverdueTaskCount     = projectTasks.Count(item => IsOverdue(item.PlannedEndTime, item.Status)),
            PlannedEndDate       = project.PlannedEndDate,
            PlannedStartDate     = project.PlannedStartDate,
            Priority             = project.Priority,
            ProjectCode          = project.ProjectCode,
            Status               = project.Status,
            TaskTotalCount       = projectTasks.Count
        };
    }

    private static ProjectMilestoneItemDto MapMilestone(ProjectMilestone item, ProjectContext context)
    {
        return new ProjectMilestoneItemDto
        {
            ActualCompletionDate = item.ActualCompletionDate,
            Id = item.Id,
            Name = item.Name,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            PlannedCompletionDate = item.PlannedCompletionDate,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            Status = item.Status
        };
    }

    private static ProjectTaskItemDto MapTask(ProjectTask item, ProjectContext context)
    {
        return new ProjectTaskItemDto
        {
            ActualEndTime = item.ActualEndTime,
            BlockReason = item.BlockReason,
            ContractClause = item.ContractClause,
            DeveloperOwnerName = item.DeveloperOwnerName,
            DeveloperOwnerUserId = item.DeveloperOwnerUserId,
            Id = item.Id,
            IsBlocked = item.IsBlocked,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(item.OrganizationUnitId) ?? "-",
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            ParentTaskId = item.ParentTaskId,
            BaselineEndTime = item.BaselineEndTime,
            BaselineStartTime = item.BaselineStartTime,
            PlannedEndTime = item.PlannedEndTime,
            PlannedStartTime = item.PlannedStartTime,
            Priority = item.Priority,
            ProductOwnerName = item.ProductOwnerName,
            ProductOwnerUserId = item.ProductOwnerUserId,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            Status = item.Status,
            TaskCode = item.TaskCode,
            TesterOwnerName = item.TesterOwnerName,
            TesterOwnerUserId = item.TesterOwnerUserId,
            Title = item.Title,
            SortOrder = item.SortOrder,
            WbsCode = item.WbsCode
        };
    }

    private static ProjectTaskDependencyDto MapTaskDependency(ProjectTaskDependency item, ProjectContext context)
    {
        return new ProjectTaskDependencyDto
        {
            DependencyType = item.DependencyType,
            Id = item.Id,
            PredecessorTaskId = item.PredecessorTaskId,
            PredecessorTaskTitle = context.TaskTitleMap.GetValueOrDefault(item.PredecessorTaskId) ?? "-",
            ProjectId = item.ProjectId,
            SuccessorTaskId = item.SuccessorTaskId,
            SuccessorTaskTitle = context.TaskTitleMap.GetValueOrDefault(item.SuccessorTaskId) ?? "-"
        };
    }

    private static ProjectRaidItemDto MapRaidItem(ProjectRaidItem item, ProjectContext context)
    {
        return new ProjectRaidItemDto
        {
            ContingencyPlan = item.ContingencyPlan,
            Id = item.Id,
            Impact = item.Impact,
            IsOverdue = item.PlannedResolveDate.HasValue && item.PlannedResolveDate.Value.Date < DateTime.Today && !IsClosed(item.Status),
            Level = item.Level,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            PlannedResolveDate = item.PlannedResolveDate,
            Probability = item.Probability,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            ResponseStrategy = item.ResponseStrategy,
            RiskScore = item.RiskScore,
            Status = item.Status,
            Title = item.Title,
            TriggerCondition = item.TriggerCondition,
            Type = item.Type
        };
    }

    private static ProjectWorklogItemDto MapWorklog(ProjectWorklog item, ProjectContext context)
    {
        return new ProjectWorklogItemDto
        {
            Hours = item.Hours,
            Id = item.Id,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            Summary = item.Summary,
            TaskTitle = item.TaskId.HasValue
                ? context.TaskTitleMap.GetValueOrDefault(item.TaskId.Value) ?? "-"
                : "-",
            UserId = item.UserId,
            UserName = context.UserNameMap.GetValueOrDefault(item.UserId) ?? "-",
            WeekStartDate = item.WeekStartDate,
            WorkDate = item.WorkDate
        };
    }

    private static bool IsClosed(string status) => ProjectStatuses.IsClosed(status);

    private static bool IsExecutionStatus(string status) => status is "进行中" or "执行中";

    private static int CalculateHealthRiskScore(
        IReadOnlyCollection<ProjectTask> tasks,
        IReadOnlyCollection<ProjectIssue> issues,
        IReadOnlyCollection<ProjectRaidItem> raidItems,
        int taskCompletionRate)
    {
        var openRisks = raidItems
            .Where(item => item.Type == ProjectRaidTypes.Risk && !IsClosed(item.Status))
            .ToList();
        var quantifiedRiskScore = openRisks.Sum(item =>
            item.RiskScore > 0
                ? item.RiskScore
                : item.Level == ProjectPriorities.High ? 15 : item.Level == ProjectPriorities.Medium ? 8 : 3);
        var overdueTaskPenalty = tasks.Count(item => IsOverdue(item.PlannedEndTime, item.Status)) * 8;
        var openIssuePenalty = issues.Count(item => !IsClosed(item.Status)) * 10;
        var schedulePenalty = Math.Max(0, 100 - taskCompletionRate);

        return Math.Min(100, quantifiedRiskScore + overdueTaskPenalty + openIssuePenalty + schedulePenalty);
    }

    private static bool WouldCreateDependencyCycle(
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

    private static bool WouldCreateWbsCycle(
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

    private static bool IsDueSoon(DateTime? plannedEndTime)
    {
        if (!plannedEndTime.HasValue)
        {
            return false;
        }

        var today = DateTime.Today;
        return plannedEndTime.Value.Date >= today && plannedEndTime.Value.Date <= today.AddDays(7);
    }

    private static bool IsMyRelatedProject(Project project, ProjectContext context)
    {
        if (!context.CurrentUserId.HasValue)
        {
            return false;
        }

        var currentUserId = context.CurrentUserId.Value;
        return project.ManagerUserId == currentUserId
            || project.SponsorUserId == currentUserId
            || context.MyMemberMap.ContainsKey(project.Id)
            || context.MyTaskProjectIds.Contains(project.Id)
            || context.MyIssueProjectIds.Contains(project.Id)
            || context.MyWorklogProjectIds.Contains(project.Id);
    }

    private static bool IsOverdue(DateTime? plannedEndTime, string status)
    {
        return plannedEndTime.HasValue && plannedEndTime.Value < DateTime.Now && !IsClosed(status);
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static string ResolveCalendarColor(string priority, string status)
    {
        if (IsClosed(status))
        {
            return "var(--ant-color-success)";
        }

        return priority == ProjectPriorities.High
            ? "var(--ant-color-error)"
            : "var(--ant-color-primary)";
    }

    private static string ResolveDisplayName(IdentityUser user)
    {
        var name = $"{user.Name}{user.Surname}".Trim();
        return string.IsNullOrWhiteSpace(name)
            ? user.UserName ?? string.Empty
            : name;
    }

    private static string ResolveMyRelation(Project project, ProjectContext context)
    {
        if (context.CurrentUserId.HasValue)
        {
            if (project.ManagerUserId == context.CurrentUserId.Value)
            {
                return "项目经理";
            }

            if (project.SponsorUserId == context.CurrentUserId.Value)
            {
                return "项目发起人";
            }
        }

        if (context.MyMemberMap.TryGetValue(project.Id, out var member))
        {
            var roleLabel = string.Join(" / ", ResolveMemberRoles(member, context));
            if (!member.IsActive())
            {
                return string.IsNullOrWhiteSpace(roleLabel)
                    ? "历史只读"
                    : $"{roleLabel} / 历史只读";
            }

            return string.IsNullOrWhiteSpace(roleLabel) ? "项目成员" : roleLabel;
        }

        if (context.CurrentUserIsAdmin)
        {
            return "管理员";
        }

        if (context.CurrentUserIsPmo)
        {
            return "PMO";
        }

        return "组织可见";
    }

    private static List<string> ResolveMemberRoles(ProjectMember member, ProjectContext context)
    {
        var structuredRoles = context.MemberRoles
            .Where(item => item.ProjectMemberId == member.Id)
            .Select(item => item.RoleName)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return structuredRoles.Count > 0
            ? structuredRoles
            : member.GetRoleList().ToList();
    }

    private sealed record ProjectContext(
        Dictionary<Guid, string> OrganizationUnitNameMap,
        List<Project> Projects,
        Dictionary<Guid, string> ProjectNameMap,
        List<ProjectMilestone> Milestones,
        List<ProjectCycle> Cycles,
        List<ProjectAcceptanceCriteria> AcceptanceCriteria,
        List<ProjectCharter> Charters,
        List<ProjectQualityChecklist> QualityChecklists,
        List<ProjectQualityInspection> QualityInspections,
        List<ProjectTaskDependency> TaskDependencies,
        List<ProjectMemberRole> MemberRoles,
        List<ProjectRaidItem> RaidItems,
        List<ProjectTask> Tasks,
        List<ProjectIssue> Issues,
        List<ProjectDocument> Documents,
        List<ProjectMember> Members,
        List<ProjectWorklog> Worklogs,
        Dictionary<Guid, string> UserNameMap,
        Dictionary<Guid, ProjectMember> MyMemberMap,
        Guid? CurrentUserId,
        bool CurrentUserIsAdmin,
        bool CurrentUserIsPmo)
    {
        // 单条查找
        public Dictionary<Guid, string> MilestoneNameMap { get; } =
            Milestones.ToDictionary(item => item.Id, item => item.Name);

        public Dictionary<Guid, string> AcceptanceCriteriaTitleMap { get; } =
            AcceptanceCriteria.ToDictionary(item => item.Id, item => item.Title);

        public Dictionary<Guid, string> QualityChecklistTitleMap { get; } =
            QualityChecklists.ToDictionary(item => item.Id, item => item.Title);

        public Dictionary<Guid, string> TaskTitleMap { get; } =
            Tasks.ToDictionary(item => item.Id, item => item.Title);

        // 预分组字典：O(1) 按项目取子集，避免 MapProject/GetDetail 里的 O(N×M) 扫描
        public Dictionary<Guid, List<ProjectTask>>      TasksByProject      { get; } = Tasks.GroupBy(t => t.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<ProjectMilestone>> MilestonesByProject { get; } = Milestones.GroupBy(m => m.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<ProjectRaidItem>>  RaidItemsByProject  { get; } = RaidItems.GroupBy(r => r.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<ProjectMember>>    MembersByProject    { get; } = Members.GroupBy(m => m.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<ProjectIssue>>     IssuesByProject     { get; } = Issues.GroupBy(i => i.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<ProjectWorklog>>   WorklogsByProject   { get; } = Worklogs.GroupBy(w => w.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<ProjectCycle>>     CyclesByProject     { get; } = Cycles.GroupBy(c => c.ProjectId).ToDictionary(g => g.Key, g => g.ToList());
        public Dictionary<Guid, List<ProjectDocument>>  DocumentsByProject  { get; } = Documents.GroupBy(d => d.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

        // 预计算当前用户在各项目中的存在性：O(1) 支持 IsMyRelatedProject 判断
        public HashSet<Guid> MyTaskProjectIds    { get; } = CurrentUserId.HasValue ? Tasks.Where(t => t.OwnerUserId == CurrentUserId.Value).Select(t => t.ProjectId).ToHashSet() : [];
        public HashSet<Guid> MyIssueProjectIds   { get; } = CurrentUserId.HasValue ? Issues.Where(i => i.OwnerUserId == CurrentUserId.Value).Select(i => i.ProjectId).ToHashSet() : [];
        public HashSet<Guid> MyWorklogProjectIds { get; } = CurrentUserId.HasValue ? Worklogs.Where(w => w.UserId == CurrentUserId.Value).Select(w => w.ProjectId).ToHashSet() : [];
    }
}
