namespace General.Admin.Project;

public sealed record ProjectContext(
    Dictionary<Guid, string> OrganizationUnitNameMap,
    List<AppPlatformFile> Files,
    List<Project> Projects,
    Dictionary<Guid, string> ProjectNameMap,
    List<ProjectMilestone> Milestones,
    List<ProjectSprint> Sprints,
    List<ProjectAcceptanceCriteria> AcceptanceCriteria,
    List<ProjectAcceptanceRecord> AcceptanceRecords,
    List<ProjectAcceptanceRecordCriteria> AcceptanceRecordCriteria,
    List<ProjectAcceptanceRecordRequirement> AcceptanceRecordRequirements,
    List<ProjectBaseline> Baselines,
    List<ProjectChangeApproval> ChangeApprovals,
    List<ProjectChangeImpact> ChangeImpacts,
    List<ProjectChangeRequest> ChangeRequests,
    List<ProjectCharter> Charters,
    List<PlatformApprovalRecord> ApprovalRecords,
    List<PlatformWorkflowInstance> WorkflowInstances,
    List<ProjectPerformanceSnapshot> PerformanceSnapshots,
    List<ProjectRequirement> Requirements,
    List<ProjectReport> Reports,
    List<ProjectMeetingRecord> MeetingRecords,
    List<ProjectMeetingActionItem> MeetingActionItems,
    List<ProjectQualityChecklist> QualityChecklists,
    List<ProjectQualityInspection> QualityInspections,
    List<ProjectTaskDependency> TaskDependencies,
    List<ProjectTaskParticipant> TaskParticipants,
    List<ProjectMemberRole> MemberRoles,
    List<ProjectRaidItem> RaidItems,
    List<ProjectTask> Tasks,
    List<ProjectWbsNode> WbsNodes,
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
    public Dictionary<Guid, string> MilestoneNameMap { get; } =
        Milestones.ToDictionary(item => item.Id, item => item.Name);

    public Dictionary<Guid, string> SprintNameMap { get; } =
        Sprints.ToDictionary(item => item.Id, item => item.Name);

    public Dictionary<Guid, string> AcceptanceCriteriaTitleMap { get; } =
        AcceptanceCriteria.ToDictionary(item => item.Id, item => item.Title);

    public Dictionary<Guid, AppPlatformFile> FileMap { get; } =
        Files.ToDictionary(item => item.Id, item => item);

    public Dictionary<Guid, List<ProjectAcceptanceRecordCriteria>> AcceptanceRecordCriteriaMap { get; } =
        AcceptanceRecordCriteria
            .GroupBy(item => item.AcceptanceRecordId)
            .ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, List<ProjectAcceptanceRecordRequirement>> AcceptanceRecordRequirementMap { get; } =
        AcceptanceRecordRequirements
            .GroupBy(item => item.AcceptanceRecordId)
            .ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, string> QualityChecklistTitleMap { get; } =
        QualityChecklists.ToDictionary(item => item.Id, item => item.Title);

    public Dictionary<Guid, string> TaskTitleMap { get; } =
        Tasks.ToDictionary(item => item.Id, item => item.Title);

    public Dictionary<Guid, string> ChangeRequestCodeMap { get; } =
        ChangeRequests.ToDictionary(item => item.Id, item => item.RequestCode);

    public Dictionary<Guid, string> RequirementTitleMap { get; } =
        Requirements.ToDictionary(item => item.Id, item => item.Title);

    public Dictionary<Guid, List<PlatformApprovalRecord>> ApprovalRecordsByInstance { get; } =
        ApprovalRecords.GroupBy(item => item.ApprovalInstanceId).ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, List<PlatformWorkflowInstance>> CharterWorkflowInstancesByProject { get; } =
        WorkflowInstances
            .Where(item => item.WorkflowDefinitionKey == PlatformWorkflowDefinitionKeys.ProjectCharterApproval
                && item.BusinessType == "ProjectCharter"
                && Guid.TryParse(item.BusinessId, out _))
            .Select(item => new { ProjectId = Guid.Parse(item.BusinessId), Instance = item })
            .GroupBy(item => item.ProjectId)
            .ToDictionary(
                item => item.Key,
                item => item.Select(x => x.Instance).OrderByDescending(x => x.CreationTime).ToList());

    public Dictionary<Guid, List<PlatformWorkflowInstance>> ChangeWorkflowInstancesByChangeRequest { get; } =
        WorkflowInstances
            .Where(item => item.WorkflowDefinitionKey == PlatformWorkflowDefinitionKeys.ProjectChangeApproval
                && item.BusinessType == "ProjectChangeRequest"
                && Guid.TryParse(item.BusinessId, out _))
            .Select(item => new { ChangeRequestId = Guid.Parse(item.BusinessId), Instance = item })
            .GroupBy(item => item.ChangeRequestId)
            .ToDictionary(
                item => item.Key,
                item => item.Select(x => x.Instance).OrderByDescending(x => x.CreationTime).ToList());

    public Dictionary<Guid, List<ProjectTask>> TasksByProject { get; } =
        Tasks.GroupBy(t => t.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<ProjectTaskParticipant>> TaskParticipantsByTask { get; } =
        TaskParticipants.GroupBy(item => item.TaskId).ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, List<ProjectWbsNode>> WbsNodesByProject { get; } =
        WbsNodes.GroupBy(item => item.ProjectId).ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, List<ProjectRequirement>> RequirementsByProject { get; } =
        Requirements.GroupBy(item => item.ProjectId).ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, List<ProjectReport>> ReportsByProject { get; } =
        Reports.GroupBy(item => item.ProjectId).ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, List<ProjectMeetingRecord>> MeetingRecordsByProject { get; } =
        MeetingRecords.GroupBy(item => item.ProjectId).ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, List<ProjectMeetingActionItem>> MeetingActionItemsByRecord { get; } =
        MeetingActionItems.GroupBy(item => item.MeetingRecordId).ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, string> WbsNodeNameMap { get; } =
        WbsNodes.ToDictionary(item => item.Id, item => $"{item.WbsCode} · {item.Name}");

    public Dictionary<Guid, List<ProjectMilestone>> MilestonesByProject { get; } =
        Milestones.GroupBy(m => m.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<ProjectRaidItem>> RaidItemsByProject { get; } =
        RaidItems.GroupBy(r => r.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<ProjectMember>> MembersByProject { get; } =
        Members.GroupBy(m => m.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<ProjectIssue>> IssuesByProject { get; } =
        Issues.GroupBy(i => i.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<ProjectWorklog>> WorklogsByProject { get; } =
        Worklogs.GroupBy(w => w.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<ProjectPerformanceSnapshot>> PerformanceSnapshotsByProject { get; } =
        PerformanceSnapshots.GroupBy(item => item.ProjectId).ToDictionary(item => item.Key, item => item.ToList());

    public Dictionary<Guid, List<ProjectSprint>> SprintsByProject { get; } =
        Sprints.GroupBy(c => c.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<ProjectDocument>> DocumentsByProject { get; } =
        Documents.GroupBy(d => d.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public HashSet<Guid> MyTaskProjectIds { get; } =
        CurrentUserId.HasValue
            ? Tasks
                .Where(t =>
                    t.OwnerUserId == CurrentUserId.Value ||
                    TaskParticipants.Any(p => p.TaskId == t.Id && p.UserId == CurrentUserId.Value))
                .Select(t => t.ProjectId)
                .ToHashSet()
            : [];

    public HashSet<Guid> MyIssueProjectIds { get; } =
        CurrentUserId.HasValue ? Issues.Where(i => i.OwnerUserId == CurrentUserId.Value).Select(i => i.ProjectId).ToHashSet() : [];

    public HashSet<Guid> MyWorklogProjectIds { get; } =
        CurrentUserId.HasValue ? Worklogs.Where(w => w.UserId == CurrentUserId.Value).Select(w => w.ProjectId).ToHashSet() : [];
}
