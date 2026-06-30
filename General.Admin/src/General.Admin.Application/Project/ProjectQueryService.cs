using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;
using static General.Admin.Project.ProjectProjectionMapper;

namespace General.Admin.Project;

public class ProjectQueryService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly ProjectContextLoader _contextLoader;

    public ProjectQueryService(
        ICurrentUser currentUser,
        ProjectContextLoader contextLoader)
    {
        _currentUser = currentUser;
        _contextLoader = contextLoader;
    }

    public async Task<ProjectWorkspaceDto> GetWorkspaceAsync()
    {
        var context = await _contextLoader.LoadAsync();
        var today = DateTime.Today;
        var currentUserId = _currentUser.Id;

        var ongoingProjects = context.Projects
            .Where(project => !IsClosed(project.Status))
            .ToList();
        var myTasks = context.Tasks
            .Where(task => currentUserId.HasValue && IsCurrentUserTask(task, currentUserId.Value, context))
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
            .Where(item => item.Level == ProjectPriorities.P0 || (item.PlannedResolveDate.HasValue && item.PlannedResolveDate.Value.Date < today))
            .OrderByDescending(item => ProjectPriorities.ResolveWeight(item.Level))
            .ThenBy(item => item.PlannedResolveDate ?? DateTime.MaxValue)
            .Take(8)
            .ToList();

        return new ProjectWorkspaceDto
        {
            BlockedTaskCount = context.Tasks.Count(task => task.IsBlocked && !IsClosed(task.Status)),
            HighRiskCount = context.RaidItems.Count(item => item.Type == ProjectRaidTypes.Risk && item.Level == ProjectPriorities.P0 && !IsClosed(item.Status)),
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
        var context = await _contextLoader.LoadDetailAsync(projectId);
        return BuildProjectDetail(projectId, context);
    }

    public async Task<ProjectDetailDto> GetDetailSectionAsync(Guid projectId, string section)
    {
        var context = await _contextLoader.LoadDetailAsync(projectId, section);
        return BuildProjectDetail(projectId, context);
    }

    private static ProjectDetailDto BuildProjectDetail(Guid projectId, ProjectContext context)
    {
        var project = context.Projects[0];

        var projectTasks = (context.TasksByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.PlannedEndTime ?? DateTime.MaxValue)
            .ThenBy(item => item.TaskCode)
            .ToList();
        var projectMilestones = (context.MilestonesByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.PlannedCompletionDate)
            .ToList();
        var projectSprints = (context.SprintsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.StartDate ?? DateTime.MaxValue)
            .ThenBy(item => item.EndDate ?? DateTime.MaxValue)
            .ThenBy(item => item.SprintCode)
            .ToList();
        var projectRaidItems = (context.RaidItemsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(item => ProjectPriorities.ResolveWeight(item.Level))
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
        var projectReports = (context.ReportsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(item => item.PeriodEnd)
            .ThenByDescending(item => item.CreationTime)
            .ToList();
        var projectMeetingRecords = (context.MeetingRecordsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(item => item.MeetingTime)
            .ThenByDescending(item => item.CreationTime)
            .ToList();
        var projectMembers = (context.MembersByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(item => item.IsActive())
            .ThenBy(item => context.UserNameMap.GetValueOrDefault(item.UserId) ?? string.Empty)
            .ToList();
        var projectWorklogs = (context.WorklogsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderByDescending(item => item.WorkDate)
            .ThenByDescending(item => item.CreationTime)
            .ToList();
        var canViewTeamResourceLoad = CanViewTeamResourceLoad(project, context);
        var visibleResourceMembers = ResolveVisibleResourceMembers(projectMembers, context, canViewTeamResourceLoad);
        var visibleResourceTasks = ResolveVisibleResourceTasks(projectTasks, context, canViewTeamResourceLoad);
        var visibleResourceWorklogs = ResolveVisibleResourceWorklogs(projectWorklogs, context, canViewTeamResourceLoad);
        var visibleTaskIds = visibleResourceTasks.Select(item => item.Id).ToHashSet();
        var taskDependencies = context.TaskDependencies
            .Where(item => canViewTeamResourceLoad ||
                (visibleTaskIds.Contains(item.PredecessorTaskId) && visibleTaskIds.Contains(item.SuccessorTaskId)))
            .OrderBy(item => context.TaskTitleMap.GetValueOrDefault(item.SuccessorTaskId) ?? string.Empty)
            .ToList();
        var acceptanceCriteria = context.AcceptanceCriteria
            .OrderBy(item => item.Status)
            .ThenBy(item => item.Title)
            .ToList();
        var acceptanceRecords = context.AcceptanceRecords
            .OrderByDescending(item => item.AcceptanceTime)
            .ThenBy(item => item.Title)
            .ToList();
        var baselines = context.Baselines
            .OrderByDescending(item => item.IsActive)
            .ThenByDescending(item => item.VersionNo)
            .ToList();
        var changeRequests = context.ChangeRequests
            .OrderByDescending(item => item.CreationTime)
            .ThenByDescending(item => item.RequestCode)
            .ToList();
        var performanceSnapshots = context.PerformanceSnapshots
            .OrderByDescending(item => item.SnapshotDate)
            .ToList();
        var qualityChecklists = context.QualityChecklists
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Title)
            .ToList();
        var qualityInspections = context.QualityInspections
            .OrderByDescending(item => item.InspectionTime)
            .ToList();
        var requirements = (context.RequirementsByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.WbsNodeId.HasValue)
            .ThenBy(item => item.WbsNodeId)
            .ThenBy(item => item.RequirementCode)
            .ThenByDescending(item => item.VersionNo)
            .ToList();
        var wbsNodes = (context.WbsNodesByProject.GetValueOrDefault(projectId) ?? [])
            .OrderBy(item => item.ParentId.HasValue)
            .ThenBy(item => item.ParentId)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.WbsCode)
            .ToList();

        var completedTaskCount = projectTasks.Count(item => IsClosed(item.Status));
        var taskCompletionRate = projectTasks.Count == 0
            ? 0
            : (int)Math.Round(completedTaskCount * 100d / projectTasks.Count, MidpointRounding.AwayFromZero);
        var healthBreakdowns = BuildHealthBreakdowns(projectTasks, projectIssues, projectRaidItems, taskCompletionRate);
        var healthRiskScore = healthBreakdowns.Sum(item => item.Score);

        return new ProjectDetailDto
        {
            ActiveBaseline = baselines.FirstOrDefault(item => item.IsActive) is { } activeBaseline
                ? MapBaseline(activeBaseline, context)
                : null,
            BudgetTotalAmount = project.BudgetTotalAmount,
            AcceptanceCriteria = acceptanceCriteria.Select(item => MapAcceptanceCriteria(item, context)).ToList(),
            AcceptanceRecords = acceptanceRecords.Select(item => MapAcceptanceRecord(item, context)).ToList(),
            CalendarItems = BuildCalendarItems(visibleResourceTasks, projectSprints, context),
            ChangeRequests = changeRequests.Select(item => MapChangeRequest(item, context)).ToList(),
            Charter = context.Charters.FirstOrDefault() is { } charter ? MapCharter(charter, context) : null,
            ContractTotalAmount = project.ContractTotalAmount,
            Sprints = projectSprints.Select(item => MapSprint(item, context)).ToList(),
            Description = project.Description ?? string.Empty,
            Documents = projectDocuments.Select(item => MapDocument(item, context)).ToList(),
            HighRiskCount = projectRaidItems.Count(item => item.Type == ProjectRaidTypes.Risk && item.Level == ProjectPriorities.P0 && !IsClosed(item.Status)),
            HealthBreakdowns = healthBreakdowns,
            HealthCalculatedTime = DateTime.Now,
            HealthDataSource = HealthDataSource,
            HealthReasons = BuildHealthReasons(healthBreakdowns),
            HealthRiskScore = healthRiskScore,
            HealthStatus = ResolveHealthStatus(healthRiskScore),
            Id = project.Id,
            IsKeyProject = project.IsKeyProject,
            Issues = projectIssues.Select(item => MapIssue(item, context)).ToList(),
            ManagerName = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            MemberCount = canViewTeamResourceLoad
                ? projectMembers.Count(item => item.IsActive())
                : visibleResourceMembers.Count(item => item.IsActive()),
            Members = visibleResourceMembers.Select(item => MapMember(item, context)).ToList(),
            Milestones = projectMilestones.Select(item => MapMilestone(item, context)).ToList(),
            MyRelation = ResolveMyRelation(project, context),
            Name = project.Name,
            OpenIssueCount = projectIssues.Count(item => !IsClosed(item.Status)),
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(project.OrganizationUnitId) ?? "-",
            PerformanceSnapshot = performanceSnapshots.FirstOrDefault() is { } performanceSnapshot
                ? MapPerformanceSnapshot(performanceSnapshot)
                : null,
            PerformanceSnapshots = performanceSnapshots.Select(MapPerformanceSnapshot).ToList(),
            PlannedEndDate = project.PlannedEndDate,
            PlannedStartDate = project.PlannedStartDate,
            Priority = project.Priority,
            ProjectCode = project.ProjectCode,
            ProjectSource = project.ProjectSource ?? string.Empty,
            ProjectType = project.ProjectType ?? string.Empty,
            Requirements = requirements.Select(item => MapRequirement(item, context)).ToList(),
            Reports = projectReports.Select(item => MapReport(item, context)).ToList(),
            MeetingRecords = projectMeetingRecords.Select(item => MapMeetingRecord(item, context)).ToList(),
            QualityChecklists = qualityChecklists.Select(item => MapQualityChecklist(item, context)).ToList(),
            QualityInspections = qualityInspections.Select(item => MapQualityInspection(item, context)).ToList(),
            RaidItems = projectRaidItems.Select(item => MapRaidItem(item, context)).ToList(),
            ReceivedAmount = project.ReceivedAmount,
            SponsorName = context.UserNameMap.GetValueOrDefault(project.SponsorUserId) ?? "-",
            Status = project.Status,
            TaskCompletionRate = taskCompletionRate,
            TaskDependencies = taskDependencies.Select(item => MapTaskDependency(item, context)).ToList(),
            TaskScheduleImpacts = BuildTaskScheduleImpacts(visibleResourceTasks, taskDependencies),
            Tasks = visibleResourceTasks.Select(item => MapTask(item, context)).ToList(),
            TotalWorklogHours = Math.Round(visibleResourceWorklogs.Sum(item => item.Hours), 1),
            WbsNodes = wbsNodes.Select(item => MapWbsNode(item, context)).ToList(),
            Worklogs = visibleResourceWorklogs.Select(item => MapWorklog(item, context)).ToList()
        };
    }

    public async Task<List<ProjectListItemDto>> GetProjectListAsync(ProjectListInput input)
    {
        var context = await _contextLoader.LoadAsync();
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

    public async Task<ProjectMyRelatedDto> GetMyRelatedAsync()
    {
        var context = await _contextLoader.LoadAsync();
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
                    .Where(item => IsCurrentUserTask(item, currentUserId, context) && !IsClosed(item.Status)).ToList();
                var issues = (context.IssuesByProject.GetValueOrDefault(project.Id) ?? [])
                    .Where(item => item.OwnerUserId == currentUserId && !IsClosed(item.Status)).ToList();
                var weekHours = (context.WorklogsByProject.GetValueOrDefault(project.Id) ?? [])
                    .Where(item => item.UserId == currentUserId && item.WeekStartDate.Date == weekStartDate.Date)
                    .Sum(item => item.Hours);
                var nextSprint = (context.SprintsByProject.GetValueOrDefault(project.Id) ?? [])
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
                    NextSprintName = nextSprint?.Name ?? "-",
                    ProjectCode = project.ProjectCode,
                    Status = project.Status,
                    WeekHours = Math.Round(weekHours, 1)
                };
            })
            .ToList();

        var myTasks = context.Tasks
            .Where(item => IsCurrentUserTask(item, currentUserId, context))
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
        var context = await _contextLoader.LoadAsync();
        var keyword = input.Keyword?.Trim();

        var tasks = context.Tasks
            .Where(task => !input.ProjectId.HasValue || task.ProjectId == input.ProjectId.Value)
            .Where(task => string.IsNullOrWhiteSpace(input.Status) || task.Status.Equals(input.Status.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(task => !input.OnlyMine || (_currentUser.Id.HasValue && IsCurrentUserTask(task, _currentUser.Id.Value, context)))
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
        var context = await _contextLoader.LoadAsync();
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

    private static bool CanViewTeamResourceLoad(Project project, ProjectContext context)
    {
        if (context.CurrentUserIsAdmin || context.CurrentUserIsPmo)
        {
            return true;
        }

        if (!context.CurrentUserId.HasValue)
        {
            return false;
        }

        var currentUserId = context.CurrentUserId.Value;
        if (project.ManagerUserId == currentUserId || project.SponsorUserId == currentUserId)
        {
            return true;
        }

        if (!context.MyMemberMap.TryGetValue(project.Id, out var member))
        {
            return false;
        }

        var roleNames = ResolveMemberRoleNames(member, context);
        return roleNames.Any(role =>
            role.Equals(PlatformRoleNames.Pm, StringComparison.OrdinalIgnoreCase) ||
            role.Equals(PlatformRoleNames.Pmo, StringComparison.OrdinalIgnoreCase) ||
            role.Equals("PM", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("PMO", StringComparison.OrdinalIgnoreCase) ||
            role.Equals("项目经理", StringComparison.OrdinalIgnoreCase));
    }

    private static List<ProjectMember> ResolveVisibleResourceMembers(
        IReadOnlyCollection<ProjectMember> projectMembers,
        ProjectContext context,
        bool canViewTeamResourceLoad)
    {
        if (canViewTeamResourceLoad)
        {
            return projectMembers.ToList();
        }

        return context.CurrentUserId.HasValue
            ? projectMembers.Where(item => item.UserId == context.CurrentUserId.Value).ToList()
            : [];
    }

    private static List<ProjectTask> ResolveVisibleResourceTasks(
        IReadOnlyCollection<ProjectTask> projectTasks,
        ProjectContext context,
        bool canViewTeamResourceLoad)
    {
        if (canViewTeamResourceLoad)
        {
            return projectTasks.ToList();
        }

        return context.CurrentUserId.HasValue
            ? projectTasks.Where(item => IsCurrentUserTask(item, context.CurrentUserId.Value, context)).ToList()
            : [];
    }

    private static List<ProjectWorklog> ResolveVisibleResourceWorklogs(
        IReadOnlyCollection<ProjectWorklog> projectWorklogs,
        ProjectContext context,
        bool canViewTeamResourceLoad)
    {
        if (canViewTeamResourceLoad)
        {
            return projectWorklogs.ToList();
        }

        return context.CurrentUserId.HasValue
            ? projectWorklogs.Where(item => item.UserId == context.CurrentUserId.Value).ToList()
            : [];
    }

    private static bool IsCurrentUserTask(ProjectTask task, Guid currentUserId, ProjectContext context)
    {
        return task.OwnerUserId == currentUserId ||
            task.ProductOwnerUserId == currentUserId ||
            task.DeveloperOwnerUserId == currentUserId ||
            task.TesterOwnerUserId == currentUserId ||
            context.TaskParticipantsByTask.GetValueOrDefault(task.Id)?.Any(item => item.UserId == currentUserId) == true;
    }

    private static List<string> ResolveMemberRoleNames(ProjectMember member, ProjectContext context)
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
}
