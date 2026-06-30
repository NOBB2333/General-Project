using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.Project;

public class ProjectContextLoader : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecord, Guid> _projectAcceptanceRecordRepository;
    private readonly IRepository<ProjectAcceptanceRecordCriteria, Guid> _projectAcceptanceRecordCriteriaRepository;
    private readonly IRepository<ProjectAcceptanceRecordRequirement, Guid> _projectAcceptanceRecordRequirementRepository;
    private readonly IRepository<ProjectBaseline, Guid> _projectBaselineRepository;
    private readonly IRepository<ProjectChangeApproval, Guid> _projectChangeApprovalRepository;
    private readonly IRepository<ProjectChangeImpact, Guid> _projectChangeImpactRepository;
    private readonly IRepository<ProjectChangeRequest, Guid> _projectChangeRequestRepository;
    private readonly IRepository<ProjectCharter, Guid> _projectCharterRepository;
    private readonly IRepository<Project, Guid> _projectRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectDocument, Guid> _projectDocumentRepository;
    private readonly IRepository<ProjectIssue, Guid> _projectIssueRepository;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<ProjectMemberRole, Guid> _projectMemberRoleRepository;
    private readonly IRepository<ProjectMilestone, Guid> _projectMilestoneRepository;
    private readonly IRepository<ProjectPerformanceSnapshot, Guid> _projectPerformanceSnapshotRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectReport, Guid> _projectReportRepository;
    private readonly IRepository<ProjectMeetingRecord, Guid> _projectMeetingRecordRepository;
    private readonly IRepository<ProjectMeetingActionItem, Guid> _projectMeetingActionItemRepository;
    private readonly IRepository<PlatformApprovalRecord, Guid> _platformApprovalRecordRepository;
    private readonly IRepository<PlatformWorkflowInstance, Guid> _platformWorkflowInstanceRepository;
    private readonly IRepository<ProjectQualityChecklist, Guid> _projectQualityChecklistRepository;
    private readonly IRepository<ProjectQualityInspection, Guid> _projectQualityInspectionRepository;
    private readonly IRepository<ProjectRaidItem, Guid> _projectRaidItemRepository;
    private readonly IRepository<ProjectTaskDependency, Guid> _projectTaskDependencyRepository;
    private readonly IRepository<ProjectTaskParticipant, Guid> _projectTaskParticipantRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;
    private readonly IRepository<ProjectWbsNode, Guid> _projectWbsNodeRepository;
    private readonly IRepository<ProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public ProjectContextLoader(
        ICurrentUser currentUser,
        CurrentUserDataScopeService dataScopeService,
        IRepository<AppPlatformFile, Guid> fileRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<ProjectAcceptanceCriteria, Guid> projectAcceptanceCriteriaRepository,
        IRepository<ProjectAcceptanceRecord, Guid> projectAcceptanceRecordRepository,
        IRepository<ProjectAcceptanceRecordCriteria, Guid> projectAcceptanceRecordCriteriaRepository,
        IRepository<ProjectAcceptanceRecordRequirement, Guid> projectAcceptanceRecordRequirementRepository,
        IRepository<ProjectBaseline, Guid> projectBaselineRepository,
        IRepository<ProjectChangeApproval, Guid> projectChangeApprovalRepository,
        IRepository<ProjectChangeImpact, Guid> projectChangeImpactRepository,
        IRepository<ProjectChangeRequest, Guid> projectChangeRequestRepository,
        IRepository<ProjectCharter, Guid> projectCharterRepository,
        IRepository<Project, Guid> projectRepository,
        IRepository<ProjectSprint, Guid> projectSprintRepository,
        IRepository<ProjectDocument, Guid> projectDocumentRepository,
        IRepository<ProjectIssue, Guid> projectIssueRepository,
        IRepository<ProjectMember, Guid> projectMemberRepository,
        IRepository<ProjectMemberRole, Guid> projectMemberRoleRepository,
        IRepository<ProjectMilestone, Guid> projectMilestoneRepository,
        IRepository<ProjectPerformanceSnapshot, Guid> projectPerformanceSnapshotRepository,
        IRepository<ProjectRequirement, Guid> projectRequirementRepository,
        IRepository<ProjectReport, Guid> projectReportRepository,
        IRepository<ProjectMeetingRecord, Guid> projectMeetingRecordRepository,
        IRepository<ProjectMeetingActionItem, Guid> projectMeetingActionItemRepository,
        IRepository<PlatformApprovalRecord, Guid> platformApprovalRecordRepository,
        IRepository<PlatformWorkflowInstance, Guid> platformWorkflowInstanceRepository,
        IRepository<ProjectQualityChecklist, Guid> projectQualityChecklistRepository,
        IRepository<ProjectQualityInspection, Guid> projectQualityInspectionRepository,
        IRepository<ProjectRaidItem, Guid> projectRaidItemRepository,
        IRepository<ProjectTaskDependency, Guid> projectTaskDependencyRepository,
        IRepository<ProjectTaskParticipant, Guid> projectTaskParticipantRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository,
        IRepository<ProjectWbsNode, Guid> projectWbsNodeRepository,
        IRepository<ProjectWorklog, Guid> projectWorklogRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _dataScopeService = dataScopeService;
        _fileRepository = fileRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _projectAcceptanceCriteriaRepository = projectAcceptanceCriteriaRepository;
        _projectAcceptanceRecordRepository = projectAcceptanceRecordRepository;
        _projectAcceptanceRecordCriteriaRepository = projectAcceptanceRecordCriteriaRepository;
        _projectAcceptanceRecordRequirementRepository = projectAcceptanceRecordRequirementRepository;
        _projectBaselineRepository = projectBaselineRepository;
        _projectChangeApprovalRepository = projectChangeApprovalRepository;
        _projectChangeImpactRepository = projectChangeImpactRepository;
        _projectChangeRequestRepository = projectChangeRequestRepository;
        _projectCharterRepository = projectCharterRepository;
        _projectRepository = projectRepository;
        _projectSprintRepository = projectSprintRepository;
        _projectDocumentRepository = projectDocumentRepository;
        _projectIssueRepository = projectIssueRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectMemberRoleRepository = projectMemberRoleRepository;
        _projectMilestoneRepository = projectMilestoneRepository;
        _projectPerformanceSnapshotRepository = projectPerformanceSnapshotRepository;
        _projectRequirementRepository = projectRequirementRepository;
        _projectReportRepository = projectReportRepository;
        _projectMeetingRecordRepository = projectMeetingRecordRepository;
        _projectMeetingActionItemRepository = projectMeetingActionItemRepository;
        _platformApprovalRecordRepository = platformApprovalRecordRepository;
        _platformWorkflowInstanceRepository = platformWorkflowInstanceRepository;
        _projectQualityChecklistRepository = projectQualityChecklistRepository;
        _projectQualityInspectionRepository = projectQualityInspectionRepository;
        _projectRaidItemRepository = projectRaidItemRepository;
        _projectTaskDependencyRepository = projectTaskDependencyRepository;
        _projectTaskParticipantRepository = projectTaskParticipantRepository;
        _projectTaskRepository = projectTaskRepository;
        _projectWbsNodeRepository = projectWbsNodeRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _userRepository = userRepository;
    }

    public async Task<ProjectContext> LoadAsync()
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
        var sprints = (await _projectSprintRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var tasks = (await _projectTaskRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var taskParticipants = (await _projectTaskParticipantRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var wbsNodes = (await _projectWbsNodeRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var requirements = (await _projectRequirementRepository.GetListAsync())
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
        var performanceSnapshots = (await _projectPerformanceSnapshotRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var reports = (await _projectReportRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var meetingRecords = (await _projectMeetingRecordRepository.GetListAsync())
            .Where(item => projectIds.Contains(item.ProjectId))
            .ToList();
        var meetingRecordIds = meetingRecords.Select(item => item.Id).ToHashSet();
        var meetingActionItems = (await _projectMeetingActionItemRepository.GetListAsync())
            .Where(item => meetingRecordIds.Contains(item.MeetingRecordId))
            .ToList();
        var changeRequestIds = new HashSet<Guid>();
        var workflowInstances = (await _platformWorkflowInstanceRepository.GetListAsync())
            .Where(item => IsProjectWorkflowInstance(item, projectIds, changeRequestIds))
            .ToList();
        var approvalInstanceIds = workflowInstances.Select(item => item.Id).ToHashSet();
        var approvalRecords = approvalInstanceIds.Count == 0
            ? []
            : (await _platformApprovalRecordRepository.GetListAsync())
                .Where(item => approvalInstanceIds.Contains(item.ApprovalInstanceId))
                .ToList();
        var sprintIds = sprints.Select(item => item.Id).ToHashSet();
        var requirementIds = requirements.Select(item => item.Id).ToHashSet();
        var taskIds = tasks.Select(item => item.Id).ToHashSet();
        var reportIds = reports.Select(item => item.Id).ToHashSet();
        var meetingIds = meetingRecords.Select(item => item.Id).ToHashSet();
        var files = (await _fileRepository.GetListAsync())
            .Where(item =>
                item.BusinessType == "Project" ||
                item.BusinessType == "ProjectSprint" ||
                item.BusinessType == "ProjectRequirement" ||
                item.BusinessType == "ProjectTask" ||
                item.BusinessType == "ProjectReport" ||
                item.BusinessType == "ProjectMeetingRecord")
            .Where(item =>
                Guid.TryParse(item.BusinessId, out var businessId) &&
                (projectIds.Contains(businessId) ||
                 sprintIds.Contains(businessId) ||
                 requirementIds.Contains(businessId) ||
                 taskIds.Contains(businessId) ||
                 reportIds.Contains(businessId) ||
                 meetingIds.Contains(businessId)))
            .ToList();
        var myMemberMap = _currentUser.Id.HasValue
            ? members
                .Where(item => item.UserId == _currentUser.Id.Value)
                .GroupBy(item => item.ProjectId)
                .ToDictionary(item => item.Key, item => item.OrderByDescending(x => x.JoinDate).First())
            : new Dictionary<Guid, ProjectMember>();

        return new ProjectContext(
            organizationUnits,
            files,
            projects,
            projects.ToDictionary(item => item.Id, item => item.Name),
            milestones,
            sprints,
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            approvalRecords,
            workflowInstances,
            performanceSnapshots,
            requirements,
            reports,
            meetingRecords,
            meetingActionItems,
            [],
            [],
            [],
            taskParticipants,
            memberRoles,
            raidItems,
            tasks,
            wbsNodes,
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

    public async Task<ProjectContext> LoadDetailAsync(Guid projectId)
    {
        return await LoadDetailAsync(projectId, null);
    }

    public async Task<ProjectContext> LoadDetailAsync(Guid projectId, string? section)
    {
        var normalizedSection = NormalizeSection(section);
        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        var organizationUnits = (await _organizationUnitRepository.GetListAsync())
            .ToDictionary(item => item.Id, item => item.DisplayName);
        var users = (await _userRepository.GetListAsync())
            .ToDictionary(item => item.Id, ResolveDisplayName);

        var project = await _projectRepository.GetAsync(projectId);
        var allMembers = (await _projectMemberRepository.GetListAsync()).ToList();
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
        var milestones = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Plan, ProjectDetailSections.Tasks, ProjectDetailSections.Control, ProjectDetailSections.Delivery, ProjectDetailSections.Docs)
            ? await _projectMilestoneRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var sprints = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Plan, ProjectDetailSections.Tasks, ProjectDetailSections.Delivery, ProjectDetailSections.Docs, ProjectDetailSections.Resources)
            ? await _projectSprintRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var charters = ShouldLoad(normalizedSection, ProjectDetailSections.Overview)
            ? await _projectCharterRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var baselines = ShouldLoad(normalizedSection, ProjectDetailSections.Control, ProjectDetailSections.Overview)
            ? await _projectBaselineRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var changeRequests = ShouldLoad(normalizedSection, ProjectDetailSections.Control)
            ? await _projectChangeRequestRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var changeRequestIds = changeRequests.Select(item => item.Id).ToHashSet();
        var changeApprovals = ShouldLoad(normalizedSection, ProjectDetailSections.Control)
            ? await _projectChangeApprovalRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var changeImpacts = ShouldLoad(normalizedSection, ProjectDetailSections.Control)
            ? await _projectChangeImpactRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var performanceSnapshots = ShouldLoad(normalizedSection, ProjectDetailSections.Control, ProjectDetailSections.Overview)
            ? await _projectPerformanceSnapshotRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var requirements = ShouldLoad(normalizedSection, ProjectDetailSections.Tasks, ProjectDetailSections.Delivery, ProjectDetailSections.Docs, ProjectDetailSections.Issues)
            ? await _projectRequirementRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var tasks = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Plan, ProjectDetailSections.Tasks, ProjectDetailSections.Control, ProjectDetailSections.Delivery, ProjectDetailSections.Docs, ProjectDetailSections.Resources, ProjectDetailSections.Issues)
            ? await _projectTaskRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var taskParticipants = tasks.Count == 0
            ? []
            : await _projectTaskParticipantRepository.GetListAsync(x => x.ProjectId == projectId);
        var wbsNodes = ShouldLoad(normalizedSection, ProjectDetailSections.Plan, ProjectDetailSections.Tasks)
            ? await _projectWbsNodeRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var taskDependencies = ShouldLoad(normalizedSection, ProjectDetailSections.Tasks, ProjectDetailSections.Control)
            ? await _projectTaskDependencyRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var memberRoles = await _projectMemberRoleRepository.GetListAsync(x => x.ProjectId == projectId);
        var raidItems = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Tasks, ProjectDetailSections.Docs)
            ? await _projectRaidItemRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var acceptanceCriteria = ShouldLoad(normalizedSection, ProjectDetailSections.Plan, ProjectDetailSections.Delivery, ProjectDetailSections.Issues)
            ? await _projectAcceptanceCriteriaRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var qualityChecklists = ShouldLoad(normalizedSection, ProjectDetailSections.Delivery, ProjectDetailSections.Issues)
            ? await _projectQualityChecklistRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var qualityInspections = ShouldLoad(normalizedSection, ProjectDetailSections.Plan, ProjectDetailSections.Delivery)
            ? await _projectQualityInspectionRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var acceptanceRecords = ShouldLoad(normalizedSection, ProjectDetailSections.Plan, ProjectDetailSections.Delivery)
            ? await _projectAcceptanceRecordRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var acceptanceRecordIds = acceptanceRecords.Select(item => item.Id).ToHashSet();
        var acceptanceRecordCriteria = acceptanceRecordIds.Count == 0
            ? []
            : (await _projectAcceptanceRecordCriteriaRepository.GetListAsync())
                .Where(item => acceptanceRecordIds.Contains(item.AcceptanceRecordId))
                .ToList();
        var acceptanceRecordRequirements = acceptanceRecordIds.Count == 0
            ? []
            : (await _projectAcceptanceRecordRequirementRepository.GetListAsync())
                .Where(item => acceptanceRecordIds.Contains(item.AcceptanceRecordId))
                .ToList();
        var issues = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Issues, ProjectDetailSections.Tasks, ProjectDetailSections.Docs)
            ? await _projectIssueRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var documents = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Plan, ProjectDetailSections.Docs)
            ? await _projectDocumentRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var worklogs = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Tasks, ProjectDetailSections.Resources)
            ? await _projectWorklogRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var reports = ShouldLoad(normalizedSection, ProjectDetailSections.Plan, ProjectDetailSections.Docs)
            ? await _projectReportRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var meetingRecords = ShouldLoad(normalizedSection, ProjectDetailSections.Plan, ProjectDetailSections.Docs)
            ? await _projectMeetingRecordRepository.GetListAsync(x => x.ProjectId == projectId)
            : [];
        var meetingRecordIds = meetingRecords.Select(item => item.Id).ToHashSet();
        var meetingActionItems = meetingRecordIds.Count == 0
            ? []
            : (await _projectMeetingActionItemRepository.GetListAsync())
                .Where(item => meetingRecordIds.Contains(item.MeetingRecordId))
                .ToList();
        var workflowInstances = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Control)
            ? (await _platformWorkflowInstanceRepository.GetListAsync())
                .Where(item => IsProjectWorkflowInstance(item, [projectId], changeRequestIds))
                .ToList()
            : [];
        var approvalInstanceIds = workflowInstances.Select(item => item.Id).ToHashSet();
        var approvalRecords = approvalInstanceIds.Count == 0
            ? []
            : (await _platformApprovalRecordRepository.GetListAsync())
                .Where(item => approvalInstanceIds.Contains(item.ApprovalInstanceId))
                .ToList();
        var sprintIds = sprints.Select(item => item.Id).ToHashSet();
        var requirementIds = requirements.Select(item => item.Id).ToHashSet();
        var taskIds = tasks.Select(item => item.Id).ToHashSet();
        var reportIds = reports.Select(item => item.Id).ToHashSet();
        var meetingIds = meetingRecords.Select(item => item.Id).ToHashSet();
        var files = ShouldLoad(normalizedSection, ProjectDetailSections.Overview, ProjectDetailSections.Delivery, ProjectDetailSections.Docs, ProjectDetailSections.Tasks)
            ? (await _fileRepository.GetListAsync())
                .Where(item =>
                    item.BusinessType == "Project" ||
                    item.BusinessType == "ProjectSprint" ||
                    item.BusinessType == "ProjectRequirement" ||
                    item.BusinessType == "ProjectTask" ||
                    item.BusinessType == "ProjectReport" ||
                    item.BusinessType == "ProjectMeetingRecord")
                .Where(item =>
                    Guid.TryParse(item.BusinessId, out var businessId) &&
                    (businessId == projectId ||
                     sprintIds.Contains(businessId) ||
                     requirementIds.Contains(businessId) ||
                     taskIds.Contains(businessId) ||
                     reportIds.Contains(businessId) ||
                     meetingIds.Contains(businessId)))
                .ToList()
            : [];

        var myMemberMap = _currentUser.Id.HasValue
            ? members
                .Where(item => item.UserId == _currentUser.Id.Value)
                .GroupBy(item => item.ProjectId)
                .ToDictionary(item => item.Key, item => item.OrderByDescending(x => x.JoinDate).First())
            : new Dictionary<Guid, ProjectMember>();

        return new ProjectContext(
            organizationUnits,
            files,
            projects,
            projects.ToDictionary(item => item.Id, item => item.Name),
            milestones,
            sprints,
            acceptanceCriteria,
            acceptanceRecords,
            acceptanceRecordCriteria,
            acceptanceRecordRequirements,
            baselines,
            changeApprovals,
            changeImpacts,
            changeRequests,
            charters,
            approvalRecords,
            workflowInstances,
            performanceSnapshots,
            requirements,
            reports,
            meetingRecords,
            meetingActionItems,
            qualityChecklists,
            qualityInspections,
            taskDependencies,
            taskParticipants,
            memberRoles,
            raidItems,
            tasks,
            wbsNodes,
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

    private static string? NormalizeSection(string? section)
    {
        if (string.IsNullOrWhiteSpace(section))
        {
            return null;
        }

        var normalized = section.Trim();
        if (!ProjectDetailSections.All.Contains(normalized))
        {
            throw new BusinessException($"未知的项目详情分区：{section}");
        }

        return normalized;
    }

    private static bool ShouldLoad(string? section, params string[] sections)
    {
        return section is null || sections.Contains(section, StringComparer.OrdinalIgnoreCase);
    }

    private static string ResolveDisplayName(IdentityUser user)
    {
        var name = $"{user.Name}{user.Surname}".Trim();
        return string.IsNullOrWhiteSpace(name)
            ? user.UserName ?? string.Empty
            : name;
    }

    private static bool IsProjectWorkflowInstance(
        PlatformWorkflowInstance instance,
        HashSet<Guid> projectIds,
        HashSet<Guid> changeRequestIds)
    {
        if (!Guid.TryParse(instance.BusinessId, out var businessId))
        {
            return false;
        }

        return instance.WorkflowDefinitionKey == PlatformWorkflowDefinitionKeys.ProjectCharterApproval
            && instance.BusinessType == "ProjectCharter"
            && projectIds.Contains(businessId)
            || instance.WorkflowDefinitionKey == PlatformWorkflowDefinitionKeys.ProjectChangeApproval
            && instance.BusinessType == "ProjectChangeRequest"
            && changeRequestIds.Contains(businessId);
    }
}
