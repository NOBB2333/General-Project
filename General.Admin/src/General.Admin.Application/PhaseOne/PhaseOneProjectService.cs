using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.PhaseOne;

public class PhaseOneProjectService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<PhaseOneProject, Guid> _projectRepository;
    private readonly IRepository<PhaseOneProjectCycle, Guid> _projectCycleRepository;
    private readonly IRepository<PhaseOneProjectDocument, Guid> _projectDocumentRepository;
    private readonly IRepository<PhaseOneProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<PhaseOneProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<PhaseOneProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<PhaseOneProjectRaidItem, Guid> _projectRaidItemRepository;
    private readonly IRepository<PhaseOneProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<PhaseOneProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PhaseOneProjectService(
        ICurrentUser currentUser,
        CurrentUserDataScopeService dataScopeService,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<PhaseOneProject, Guid> projectRepository,
        IRepository<PhaseOneProjectCycle, Guid> projectCycleRepository,
        IRepository<PhaseOneProjectDocument, Guid> projectDocumentRepository,
        IRepository<PhaseOneProjectIssue, Guid> projectIssueRepository,
        IRepository<PhaseOneProjectMember, Guid> projectMemberRepository,
        IRepository<PhaseOneProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<PhaseOneProjectRaidItem, Guid> projectRaidItemRepository,
        IRepository<PhaseOneProjectTask, Guid> projectTaskRepository,
        IRepository<PhaseOneProjectWorklog, Guid> projectWorklogRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _dataScopeService = dataScopeService;
        _organizationUnitRepository = organizationUnitRepository;
        _projectRepository = projectRepository;
        _projectCycleRepository = projectCycleRepository;
        _projectDocumentRepository = projectDocumentRepository;
        _projectIssueRepository = projectIssueRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectRaidItemRepository = projectRaidItemRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _userRepository = userRepository;
    }

    public async Task<PhaseOneProjectWorkspaceDto> GetWorkspaceAsync()
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

        return new PhaseOneProjectWorkspaceDto
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

    public async Task<PhaseOneProjectDetailDto> GetDetailAsync(Guid projectId)
    {
        var context = await BuildContextAsync();
        var project = context.Projects.FirstOrDefault(x => x.Id == projectId);
        if (project == null)
        {
            throw new AbpAuthorizationException("项目不存在或当前用户无权查看。");
        }

        var projectTasks = context.Tasks
            .Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.PlannedEndTime ?? DateTime.MaxValue)
            .ThenBy(item => item.TaskCode)
            .ToList();
        var projectMilestones = context.Milestones
            .Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.PlannedCompletionDate)
            .ToList();
        var projectCycles = context.Cycles
            .Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.StartDate ?? DateTime.MaxValue)
            .ThenBy(item => item.EndDate ?? DateTime.MaxValue)
            .ToList();
        var projectRaidItems = context.RaidItems
            .Where(item => item.ProjectId == projectId)
            .OrderByDescending(item => item.Level)
            .ThenBy(item => item.PlannedResolveDate ?? DateTime.MaxValue)
            .ToList();
        var projectIssues = context.Issues
            .Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.DueDate ?? DateTime.MaxValue)
            .ThenBy(item => item.Title)
            .ToList();
        var projectDocuments = context.Documents
            .Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Title)
            .ToList();
        var projectMembers = context.Members
            .Where(item => item.ProjectId == projectId)
            .OrderByDescending(item => item.IsActive())
            .ThenBy(item => context.UserNameMap.GetValueOrDefault(item.UserId) ?? string.Empty)
            .ToList();
        var projectWorklogs = context.Worklogs
            .Where(item => item.ProjectId == projectId)
            .OrderByDescending(item => item.WorkDate)
            .ThenByDescending(item => item.CreationTime)
            .ToList();

        var completedTaskCount = projectTasks.Count(item => IsClosed(item.Status));
        var taskCompletionRate = projectTasks.Count == 0
            ? 0
            : (int)Math.Round(completedTaskCount * 100d / projectTasks.Count, MidpointRounding.AwayFromZero);

        return new PhaseOneProjectDetailDto
        {
            BudgetTotalAmount = project.BudgetTotalAmount,
            CalendarItems = BuildCalendarItems(projectTasks, projectCycles, context),
            ContractTotalAmount = project.ContractTotalAmount,
            Cycles = projectCycles.Select(item => MapCycle(item, context)).ToList(),
            Description = project.Description ?? string.Empty,
            Documents = projectDocuments.Select(item => MapDocument(item, context)).ToList(),
            HighRiskCount = projectRaidItems.Count(item => item.Type == "风险" && item.Level == "高" && !IsClosed(item.Status)),
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
            RaidItems = projectRaidItems.Select(item => MapRaidItem(item, context)).ToList(),
            ReceivedAmount = project.ReceivedAmount,
            SponsorName = context.UserNameMap.GetValueOrDefault(project.SponsorUserId) ?? "-",
            Status = project.Status,
            TaskCompletionRate = taskCompletionRate,
            Tasks = projectTasks.Select(item => MapTask(item, context)).ToList(),
            TotalWorklogHours = Math.Round(projectWorklogs.Sum(item => item.Hours), 1),
            Worklogs = projectWorklogs.Select(item => MapWorklog(item, context)).ToList()
        };
    }

    public async Task<List<PhaseOneProjectListItemDto>> GetProjectListAsync(PhaseOneProjectListInput input)
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

    public async Task<PhaseOneProjectMyRelatedDto> GetMyRelatedAsync()
    {
        var context = await BuildContextAsync();
        if (!_currentUser.Id.HasValue)
        {
            return new PhaseOneProjectMyRelatedDto();
        }

        var currentUserId = _currentUser.Id.Value;
        var weekStartDate = GetWeekStart(DateTime.Today);
        var myProjects = context.Projects
            .Where(project => IsMyRelatedProject(project, context))
            .OrderByDescending(project => project.IsKeyProject)
            .ThenBy(project => project.PlannedEndDate ?? DateTime.MaxValue)
            .Select(project =>
            {
                var tasks = context.Tasks.Where(item => item.ProjectId == project.Id && item.OwnerUserId == currentUserId && !IsClosed(item.Status)).ToList();
                var issues = context.Issues.Where(item => item.ProjectId == project.Id && item.OwnerUserId == currentUserId && !IsClosed(item.Status)).ToList();
                var weekHours = context.Worklogs
                    .Where(item => item.ProjectId == project.Id && item.UserId == currentUserId && item.WeekStartDate.Date == weekStartDate.Date)
                    .Sum(item => item.Hours);
                var nextCycle = context.Cycles
                    .Where(item => item.ProjectId == project.Id && !IsClosed(item.Status))
                    .OrderBy(item => item.StartDate ?? DateTime.MaxValue)
                    .FirstOrDefault();

                return new PhaseOneProjectOverviewItemDto
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

        return new PhaseOneProjectMyRelatedDto
        {
            MyIssues = myIssues,
            MyProjects = myProjects,
            MyTasks = myTasks,
            RecentWorklogs = recentWorklogs
        };
    }

    public async Task<List<PhaseOneProjectTaskItemDto>> GetTaskListAsync(PhaseOneProjectTaskListInput input)
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

    public async Task<List<PhaseOneProjectRaidItemDto>> GetRaidListAsync(PhaseOneProjectRaidListInput input)
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
        if (_currentUser.IsInRole(PhaseOneRoleNames.Admin))
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
                visibleProjectIds.UnionWith(
                    allMembers
                        .Where(item => item.UserId == _currentUser.Id.Value)
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
            : new Dictionary<Guid, PhaseOneProjectMember>();

        return new ProjectContext(
            organizationUnits,
            projects,
            projects.ToDictionary(item => item.Id, item => item.Name),
            milestones,
            cycles,
            raidItems,
            tasks,
            issues,
            documents,
            members,
            worklogs,
            users,
            myMemberMap,
            _currentUser.Id,
            _currentUser.IsInRole(PhaseOneRoleNames.Admin));
    }

    private static List<PhaseOneProjectCalendarItemDto> BuildCalendarItems(
        IReadOnlyCollection<PhaseOneProjectTask> tasks,
        IReadOnlyCollection<PhaseOneProjectCycle> cycles,
        ProjectContext context)
    {
        var items = tasks
            .Where(item => item.PlannedStartTime.HasValue || item.PlannedEndTime.HasValue)
            .Select(item => new PhaseOneProjectCalendarItemDto
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
            .Concat(cycles.Select(item => new PhaseOneProjectCalendarItemDto
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

    private static PhaseOneProjectCycleItemDto MapCycle(PhaseOneProjectCycle item, ProjectContext context)
    {
        return new PhaseOneProjectCycleItemDto
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

    private static PhaseOneProjectDocumentItemDto MapDocument(PhaseOneProjectDocument item, ProjectContext context)
    {
        return new PhaseOneProjectDocumentItemDto
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

    private static PhaseOneProjectIssueItemDto MapIssue(PhaseOneProjectIssue item, ProjectContext context)
    {
        return new PhaseOneProjectIssueItemDto
        {
            DeveloperOwnerName = item.DeveloperOwnerName,
            DueDate = item.DueDate,
            Id = item.Id,
            IsOverdue = item.DueDate.HasValue && item.DueDate.Value.Date < DateTime.Today && !IsClosed(item.Status),
            Level = item.Level,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            ProductOwnerName = item.ProductOwnerName,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            RequirementTitle = item.RequirementTitle,
            Status = item.Status,
            TesterOwnerName = item.TesterOwnerName,
            Title = item.Title,
            Type = item.Type
        };
    }

    private static PhaseOneProjectMemberItemDto MapMember(PhaseOneProjectMember item, ProjectContext context)
    {
        return new PhaseOneProjectMemberItemDto
        {
            AccessLevel = item.IsActive() ? "编辑" : item.AllowHistoricalRead ? "历史只读" : "已移除",
            Id = item.Id,
            IsActive = item.IsActive(),
            JoinDate = item.JoinDate,
            LeaveDate = item.LeaveDate,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(item.OrganizationUnitId) ?? "-",
            RoleNames = item.GetRoleList().ToList(),
            UserId = item.UserId,
            UserName = context.UserNameMap.GetValueOrDefault(item.UserId) ?? "-"
        };
    }

    private static PhaseOneProjectListItemDto MapProject(PhaseOneProject project, ProjectContext context)
    {
        var projectTasks = context.Tasks.Where(item => item.ProjectId == project.Id).ToList();
        var projectMilestones = context.Milestones.Where(item => item.ProjectId == project.Id).ToList();
        var raidItems = context.RaidItems.Where(item => item.ProjectId == project.Id).ToList();
        var members = context.Members.Where(item => item.ProjectId == project.Id).ToList();

        return new PhaseOneProjectListItemDto
        {
            BlockedTaskCount = projectTasks.Count(item => item.IsBlocked && !IsClosed(item.Status)),
            CompletedTaskCount = projectTasks.Count(item => item.Status == "已完成" || item.Status == "已关闭"),
            HighRiskCount = raidItems.Count(item => item.Type == "风险" && item.Level == "高" && !IsClosed(item.Status)),
            Id = project.Id,
            IsKeyProject = project.IsKeyProject,
            ManagerName = context.UserNameMap.GetValueOrDefault(project.ManagerUserId) ?? "-",
            MemberCount = members.Count(item => item.IsActive()),
            MilestoneTotalCount = projectMilestones.Count,
            MyRelation = ResolveMyRelation(project, context),
            Name = project.Name,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(project.OrganizationUnitId) ?? "-",
            OverdueMilestoneCount = projectMilestones.Count(item => item.PlannedCompletionDate.Date < DateTime.Today && !IsClosed(item.Status)),
            OverdueTaskCount = projectTasks.Count(item => IsOverdue(item.PlannedEndTime, item.Status)),
            PlannedEndDate = project.PlannedEndDate,
            PlannedStartDate = project.PlannedStartDate,
            Priority = project.Priority,
            ProjectCode = project.ProjectCode,
            Status = project.Status,
            TaskTotalCount = projectTasks.Count
        };
    }

    private static PhaseOneProjectMilestoneItemDto MapMilestone(PhaseOneProjectMilestone item, ProjectContext context)
    {
        return new PhaseOneProjectMilestoneItemDto
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

    private static PhaseOneProjectTaskItemDto MapTask(PhaseOneProjectTask item, ProjectContext context)
    {
        return new PhaseOneProjectTaskItemDto
        {
            ActualEndTime = item.ActualEndTime,
            BlockReason = item.BlockReason,
            ContractClause = item.ContractClause,
            DeveloperOwnerName = item.DeveloperOwnerName,
            Id = item.Id,
            IsBlocked = item.IsBlocked,
            OrganizationUnitName = context.OrganizationUnitNameMap.GetValueOrDefault(item.OrganizationUnitId) ?? "-",
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            PlannedEndTime = item.PlannedEndTime,
            PlannedStartTime = item.PlannedStartTime,
            Priority = item.Priority,
            ProductOwnerName = item.ProductOwnerName,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            Status = item.Status,
            TaskCode = item.TaskCode,
            TesterOwnerName = item.TesterOwnerName,
            Title = item.Title
        };
    }

    private static PhaseOneProjectRaidItemDto MapRaidItem(PhaseOneProjectRaidItem item, ProjectContext context)
    {
        return new PhaseOneProjectRaidItemDto
        {
            Id = item.Id,
            IsOverdue = item.PlannedResolveDate.HasValue && item.PlannedResolveDate.Value.Date < DateTime.Today && !IsClosed(item.Status),
            Level = item.Level,
            OwnerName = context.UserNameMap.GetValueOrDefault(item.OwnerUserId) ?? "-",
            PlannedResolveDate = item.PlannedResolveDate,
            ProjectId = item.ProjectId,
            ProjectName = context.ProjectNameMap.GetValueOrDefault(item.ProjectId) ?? "-",
            Status = item.Status,
            Title = item.Title,
            Type = item.Type
        };
    }

    private static PhaseOneProjectWorklogItemDto MapWorklog(PhaseOneProjectWorklog item, ProjectContext context)
    {
        return new PhaseOneProjectWorklogItemDto
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

    private static bool IsClosed(string status)
    {
        return status == "已完成" || status == "已关闭" || status == "已解除";
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

    private static bool IsMyRelatedProject(PhaseOneProject project, ProjectContext context)
    {
        if (!context.CurrentUserId.HasValue)
        {
            return false;
        }

        var currentUserId = context.CurrentUserId.Value;
        return project.ManagerUserId == currentUserId
            || project.SponsorUserId == currentUserId
            || context.MyMemberMap.ContainsKey(project.Id)
            || context.Tasks.Any(item => item.ProjectId == project.Id && item.OwnerUserId == currentUserId)
            || context.Issues.Any(item => item.ProjectId == project.Id && item.OwnerUserId == currentUserId)
            || context.Worklogs.Any(item => item.ProjectId == project.Id && item.UserId == currentUserId);
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

        return priority == "高"
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

    private static string ResolveMyRelation(PhaseOneProject project, ProjectContext context)
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
            var roleLabel = string.Join(" / ", member.GetRoleList());
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

        return "组织可见";
    }

    private sealed record ProjectContext(
        Dictionary<Guid, string> OrganizationUnitNameMap,
        List<PhaseOneProject> Projects,
        Dictionary<Guid, string> ProjectNameMap,
        List<PhaseOneProjectMilestone> Milestones,
        List<PhaseOneProjectCycle> Cycles,
        List<PhaseOneProjectRaidItem> RaidItems,
        List<PhaseOneProjectTask> Tasks,
        List<PhaseOneProjectIssue> Issues,
        List<PhaseOneProjectDocument> Documents,
        List<PhaseOneProjectMember> Members,
        List<PhaseOneProjectWorklog> Worklogs,
        Dictionary<Guid, string> UserNameMap,
        Dictionary<Guid, PhaseOneProjectMember> MyMemberMap,
        Guid? CurrentUserId,
        bool CurrentUserIsAdmin)
    {
        public Dictionary<Guid, string> TaskTitleMap { get; } = Tasks.ToDictionary(item => item.Id, item => item.Title);
    }
}
