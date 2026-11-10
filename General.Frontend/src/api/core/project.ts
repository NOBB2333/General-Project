import { apiClient } from '@/api/request';

export namespace ProjectApi {
  export interface ProjectSaveInput {
    budgetTotalAmount?: null | number;
    contractTotalAmount?: null | number;
    description?: null | string;
    isKeyProject: boolean;
    managerUserId: string;
    name: string;
    organizationUnitId: string;
    plannedEndDate?: null | string;
    plannedStartDate?: null | string;
    priority: string;
    projectCode: string;
    projectSource?: null | string;
    projectType?: null | string;
    receivedAmount?: null | number;
    shortName?: null | string;
    sponsorUserId: string;
    status: string;
  }

  export interface CharterSaveInput {
    approvalStatus: string;
    assumptions: string;
    businessCase: string;
    constraints: string;
    objectives: string;
    scopeBoundary: string;
    successCriteria: string;
  }

  export interface WorkflowActionInput {
    comment: string;
  }

  export interface ProjectChangeImpactSaveInput {
    analysis: string;
    estimatedCostDelta?: null | number;
    estimatedScheduleDeltaDays?: null | number;
    impactType: string;
    relatedBusinessCode?: string;
    relatedBudgetCode?: string;
    relatedContractCode?: string;
    relatedMilestoneId?: null | string;
    relatedTaskId?: null | string;
    severity: string;
  }

  export interface ChangeRequestSaveInput {
    applicantUserId?: null | string;
    changeType: string;
    expectedResult: string;
    impactScope: string;
    impacts: ProjectChangeImpactSaveInput[];
    reason: string;
    requestCode?: string;
    title: string;
  }

  export interface StatusUpdateInput {
    comment?: string;
    status: string;
  }

  export interface ApprovalRecord {
    action: string;
    actorUserId: null | string;
    actorUserName: string;
    approvalInstanceId: string;
    comment: string;
    creationTime: string;
    fromStatus: string;
    id: string;
    nodeName: string;
    toStatus: string;
  }

  export interface WorkflowInstance {
    applicantUserId: null | string;
    applicantUserName: string;
    approvalRecords: ApprovalRecord[];
    businessCode: string;
    businessId: string;
    businessTitle: string;
    businessType: string;
    completedTime: null | string;
    creationTime: string;
    currentNodeName: string;
    elsaWorkflowInstanceId: string;
    id: string;
    status: string;
    workflowDefinitionKey: string;
    workflowDefinitionVersion: number;
  }

  export interface Charter {
    approvalStatus: string;
    approvalWorkflow: null | WorkflowInstance;
    assumptions: string;
    businessCase: string;
    constraints: string;
    id: string;
    objectives: string;
    projectId: string;
    scopeBoundary: string;
    successCriteria: string;
  }

  export interface ChangeImpact {
    analysis: string;
    estimatedCostDelta: null | number;
    estimatedScheduleDeltaDays: null | number;
    id: string;
    impactType: string;
    relatedBusinessCode: string;
    relatedBudgetCode: string;
    relatedContractCode: string;
    relatedMilestoneId: null | string;
    relatedMilestoneName: string;
    relatedTaskId: null | string;
    relatedTaskTitle: string;
    severity: string;
  }

  export interface ChangeApproval {
    action: string;
    approverName: string;
    approverUserId: string;
    comment: string;
    decisionTime: string;
    id: string;
  }

  export interface ChangeRequest {
    applicantName: string;
    applicantUserId: string;
    approvalWorkflow: null | WorkflowInstance;
    approvals: ChangeApproval[];
    changeType: string;
    expectedResult: string;
    id: string;
    impactScope: string;
    impacts: ChangeImpact[];
    projectId: string;
    reason: string;
    requestCode: string;
    status: string;
    title: string;
  }

  export interface ListInput {
    keyword?: string;
    onlyMyRelated?: boolean;
    status?: string;
  }

  export interface PagedListInput extends ListInput {
    maxResultCount: number;
    skipCount: number;
  }

  export interface ListItem {
    blockedTaskCount: number;
    completedTaskCount: number;
    highRiskCount: number;
    id: string;
    isKeyProject: boolean;
    managerName: string;
    memberCount: number;
    milestoneTotalCount: number;
    myRelation: string;
    name: string;
    organizationUnitName: string;
    overdueMilestoneCount: number;
    overdueTaskCount: number;
    plannedEndDate: null | string;
    plannedStartDate: null | string;
    priority: string;
    projectCode: string;
    status: string;
    taskTotalCount: number;
  }

  export interface PagedListResult {
    abnormalProjectCount: number;
    items: ListItem[];
    keyProjectCount: number;
    myRelatedProjectCount: number;
    totalCount: number;
  }

  export interface TaskItem {
    actualEndTime: null | string;
    actualStartTime: null | string;
    actualWorkHours: null | number;
    baselineEndTime: null | string;
    baselineStartTime: null | string;
    blockReason: null | string;
    contractClause: null | string;
    sprintId: null | string;
    sprintName: string;
    developerOwnerName: null | string;
    developerOwnerUserId: null | string;
    developerOwnerUserIds: string[];
    developerOwnerNames: string[];
    description: string;
    estimatedWorkHours: null | number;
    id: string;
    isBlocked: boolean;
    isOutOfContract: boolean;
    organizationUnitName: string;
    ownerName: string;
    ownerUserId: string;
    parentTaskId: null | string;
    plannedEndTime: null | string;
    plannedStartTime: null | string;
    priority: string;
    productOwnerName: null | string;
    productOwnerUserId: null | string;
    productOwnerUserIds: string[];
    productOwnerNames: string[];
    projectId: string;
    projectName: string;
    requirementId: null | string;
    requirementTitle: string;
    status: string;
    taskCode: string;
    testerOwnerName: null | string;
    testerOwnerUserId: null | string;
    testerOwnerUserIds: string[];
    testerOwnerNames: string[];
    title: string;
    sortOrder: number;
    wbsCode: string;
    wbsNodeId: null | string;
    wbsNodeName: string;
  }

  export interface TaskListInput {
    keyword?: string;
    onlyMine?: boolean;
    projectId?: string;
    status?: string;
  }

  export interface PagedTaskListInput extends TaskListInput {
    maxResultCount: number;
    skipCount: number;
  }

  export interface PagedTaskListResult {
    items: TaskItem[];
    totalCount: number;
  }

  export interface TaskDependency {
    dependencyType: string;
    id: string;
    predecessorTaskId: string;
    predecessorTaskTitle: string;
    projectId: string;
    successorTaskId: string;
    successorTaskTitle: string;
  }

  export interface TaskDependencySaveInput {
    dependencyType: string;
    predecessorTaskId: string;
    successorTaskId: string;
  }

  export interface TaskProgressInput {
    blockReason: string;
    hours: number;
    isBlocked: boolean;
    status: string;
    summary: string;
    workDate: null | string;
  }

  export interface IssueItem {
    acceptanceCriteriaId: null | string;
    acceptanceCriteriaTitle: string;
    creationTime: string;
    developerOwnerName: null | string;
    developerOwnerUserId: null | string;
    dueDate: null | string;
    id: string;
    isOverdue: boolean;
    level: string;
    ownerName: string;
    ownerUserId: string;
    productOwnerName: null | string;
    productOwnerUserId: null | string;
    qualityChecklistId: null | string;
    qualityChecklistTitle: string;
    projectId: string;
    projectName: string;
    requirementTitle: null | string;
    status: string;
    testerOwnerName: null | string;
    testerOwnerUserId: null | string;
    title: string;
    type: string;
  }

  export interface RaidItem {
    contingencyPlan: string;
    id: string;
    impact: number;
    isOverdue: boolean;
    level: string;
    ownerName: string;
    plannedResolveDate: null | string;
    probability: number;
    projectId: string;
    projectName: string;
    responseStrategy: string;
    riskScore: number;
    status: string;
    title: string;
    triggerCondition: string;
    type: string;
  }

  export interface RequirementItem {
    acceptanceCriteria: string;
    businessOwnerName: string;
    businessOwnerUserId: null | string;
    changeReason: string;
    description: string;
    id: string;
    impactSummary: string;
    isCurrent: boolean;
    milestoneId: null | string;
    milestoneName: string;
    priority: string;
    productOwnerName: string;
    productOwnerUserId: null | string;
    projectId: string;
    replacesRequirementId: null | string;
    replacesRequirementTitle: string;
    requirementCode: string;
    sizeEstimate: null | number;
    source: string;
    status: string;
    title: string;
    type: string;
    versionNo: number;
    wbsNodeId: null | string;
    wbsNodeName: string;
  }

  export interface IssueSaveInput {
    acceptanceCriteriaId: null | string;
    developerOwnerUserId: null | string;
    dueDate: null | string;
    level: string;
    ownerUserId: string;
    productOwnerUserId: null | string;
    qualityChecklistId: null | string;
    requirementTitle: string;
    status: string;
    testerOwnerUserId: null | string;
    title: string;
    type: string;
  }

  export interface RequirementSaveInput {
    acceptanceCriteria: string;
    businessOwnerUserId: null | string;
    changeReason: string;
    description: string;
    impactSummary: string;
    isCurrent: boolean;
    milestoneId: null | string;
    priority: string;
    productOwnerUserId: null | string;
    replacesRequirementId: null | string;
    requirementCode: string;
    sizeEstimate: null | number;
    source: string;
    status: string;
    title: string;
    type: string;
    versionNo: number;
    wbsNodeId: null | string;
  }

  export interface RaidRiskUpdateInput {
    contingencyPlan: string;
    impact: number;
    probability: number;
    responseStrategy: string;
    triggerCondition: string;
  }

  export interface MilestoneItem {
    actualCompletionDate: null | string;
    id: string;
    name: string;
    ownerName: string;
    ownerUserId: string;
    plannedCompletionDate: string;
    projectId: string;
    projectName: string;
    sprintId: null | string;
    sprintName: string;
    status: string;
  }

  export interface WbsNode {
    actualWorkHours: number;
    completionRate: number;
    deliveryDescription: string;
    estimateSummary: string;
    estimatedWorkHours: number;
    id: string;
    milestoneId: null | string;
    milestoneName: string;
    name: string;
    ownerName: string;
    ownerUserId: null | string;
    parentId: null | string;
    projectId: string;
    requirementCount: number;
    scopeDescription: string;
    sortOrder: number;
    status: string;
    taskCount: number;
    type: string;
    wbsCode: string;
    sprintId: null | string;
    sprintName: string;
  }

  export interface SprintItem {
    closedMilestoneCount: number;
    closedTaskCount: number;
    endDate: null | string;
    highRiskCount: number;
    id: string;
    milestoneCount: number;
    name: string;
    openIssueCount: number;
    overdueMilestoneCount: number;
    overdueTaskCount: number;
    ownerName: string;
    ownerUserId: string;
    progress: number;
    reportCount: number;
    riskCount: number;
    sprintCode: string;
    startDate: null | string;
    status: string;
    summary: string;
    taskCount: number;
    latestReportStatus: string;
  }

  export interface Baseline {
    budgetAtCompletion: null | number;
    costBaselineSummary: string;
    createdFromChangeRequestCode: string;
    createdFromChangeRequestId: null | string;
    effectiveDate: string;
    id: string;
    isActive: boolean;
    projectId: string;
    scheduleBaselineSummary: string;
    scopeBaselineSummary: string;
    versionNo: number;
  }

  export interface WbsNodeSaveInput {
    deliveryDescription: string;
    estimateSummary: string;
    milestoneId: null | string;
    name: string;
    ownerUserId: null | string;
    parentId: null | string;
    scopeDescription: string;
    sortOrder: number;
    sprintId: null | string;
    status: string;
    type: string;
    wbsCode: string;
  }

  export interface SprintSaveInput {
    endDate: null | string;
    name: string;
    ownerUserId: string;
    progress: number;
    startDate: null | string;
    status: string;
    summary: string;
  }

  export interface MilestoneSaveInput {
    actualCompletionDate: null | string;
    name: string;
    ownerUserId: string;
    plannedCompletionDate: string;
    sprintId: null | string;
    status: string;
  }

  export interface BaselineSaveInput {
    budgetAtCompletion: number;
    costBaselineSummary: string;
    createdFromChangeRequestId: null | string;
    effectiveDate: string;
    isActive: boolean;
    scheduleBaselineSummary: string;
    scopeBaselineSummary: string;
  }

  export interface WorklogItem {
    hours: number;
    id: string;
    projectId: string;
    projectName: string;
    summary: string;
    taskId: null | string;
    taskTitle: string;
    userId: string;
    userName: string;
    weekStartDate: string;
    workDate: string;
  }

  export interface Workspace {
    blockedTaskCount: number;
    highRiskCount: number;
    keyProjects: ListItem[];
    myTasks: TaskItem[];
    myTodoCount: number;
    ongoingProjectCount: number;
    overdueTaskCount: number;
    raidAlerts: RaidItem[];
    weeklyMilestones: MilestoneItem[];
  }

  export interface DashboardMetric {
    label: string;
    trend: string;
    unit: string;
    value: number;
  }

  export interface DashboardActionItem {
    actionKey: string;
    dueDate: null | string;
    level: string;
    projectId: string;
    projectName: string;
    reason: string;
    sourceId: string;
    sourceType: string;
    status: string;
    title: string;
  }

  export interface OverviewItem {
    activeIssueCount: number;
    dueSoonTaskCount: number;
    id: string;
    myRelation: string;
    name: string;
    nextSprintName: string;
    projectCode: string;
    status: string;
    weekHours: number;
  }

  export interface DashboardProjectCard {
    blockedTaskCount: number;
    costAmount: null | number;
    contractAmount: null | number;
    costPerformanceIndex: null | number;
    earnedValue: null | number;
    completedTaskCount: number;
    healthRiskScore: number;
    healthStatus: string;
    highRiskCount: number;
    id: string;
    isKeyProject: boolean;
    managerName: string;
    name: string;
    openIssueCount: number;
    organizationUnitName: string;
    overdueMilestoneCount: number;
    overdueTaskCount: number;
    plannedEndDate: null | string;
    plannedStartDate: null | string;
    plannedValue: null | number;
    priority: string;
    progress: number;
    projectCode: string;
    receivedAmount: null | number;
    schedulePerformanceIndex: null | number;
    status: string;
    taskTotalCount: number;
  }

  export interface DashboardBusinessRisk {
    businessCloseStatus: string;
    contractAmount: null | number;
    costAmount: null | number;
    grossProfitRate: null | number;
    projectId: string;
    projectName: string;
    receivedAmount: null | number;
    riskLabel: string;
    yearEndReceivableForecast: null | number;
  }

  export interface EarnedValueSummary {
    actualCost: number;
    costPerformanceIndex: number;
    earnedValue: number;
    plannedValue: number;
    schedulePerformanceIndex: number;
    snapshotDate: null | string;
  }

  export interface HealthBucket {
    count: number;
    healthStatus: string;
  }

  export interface PerformancePoint {
    actualCost: number;
    costPerformanceIndex: number;
    earnedValue: number;
    plannedValue: number;
    projectId: string;
    projectName: string;
    schedulePerformanceIndex: number;
    snapshotDate: string;
  }

  export interface DepartmentPressure {
    projectCount: number;
    name: string;
    riskScore: number;
  }

  export interface MyRelatedDashboard {
    actionItems: DashboardActionItem[];
    metrics: DashboardMetric[];
    myIssues: IssueItem[];
    myProjects: OverviewItem[];
    myTasks: TaskItem[];
    recentWorklogs: WorklogItem[];
  }

  export interface PmDashboard {
    actionItems: DashboardActionItem[];
    businessRisks: DashboardBusinessRisk[];
    earnedValueSummary: EarnedValueSummary;
    healthBuckets: HealthBucket[];
    metrics: DashboardMetric[];
    performancePoints: PerformancePoint[];
    projects: DashboardProjectCard[];
  }

  export interface PmoDashboard extends PmDashboard {
    departmentPressures: DepartmentPressure[];
    exceptionProjects: DashboardProjectCard[];
  }

  export type DetailSection = 'control' | 'delivery' | 'docs' | 'issues' | 'overview' | 'plan' | 'resources' | 'tasks';

  export interface HealthBreakdown {
    actionKey: string;
    actionLabel: string;
    code: string;
    dataSource: string;
    label: string;
    reason: string;
    ruleVersion: string;
    score: number;
  }

  export interface DetailRecord {
    id: string;
    name?: string;
    status?: string;
    summary?: string;
    title?: string;
  }

  export interface Report {
    attachmentFileId: null | string;
    attachmentFileName: string;
    authorName: string;
    authorUserId: string;
    businessSummary: string;
    completedSummary: string;
    hoursSummary: string;
    id: string;
    milestoneSummary: string;
    nextPlan: string;
    periodEnd: string;
    periodStart: string;
    projectId: string;
    reportType: string;
    riskIssueSummary: string;
    sprintId: null | string;
    sprintName: string;
    status: string;
    title: string;
  }

  export interface MeetingActionItem {
    actionType: string;
    assigneeName: string;
    assigneeUserId: null | string;
    dueDate: null | string;
    id: string;
    linkedIssueId: null | string;
    linkedIssueTitle: string;
    linkedRaidItemId: null | string;
    linkedRaidItemTitle: string;
    linkedRequirementId: null | string;
    linkedRequirementTitle: string;
    linkedTaskId: null | string;
    linkedTaskTitle: string;
    status: string;
    title: string;
  }

  export interface MeetingRecord {
    actionItems: MeetingActionItem[];
    attachmentFileId: null | string;
    attachmentFileName: string;
    decisions: string;
    facilitatorName: string;
    facilitatorUserId: string;
    id: string;
    meetingTime: string;
    minutes: string;
    participantNames: string[];
    participantUserIds: string[];
    projectId: string;
    status: string;
    topic: string;
  }

  export interface MeetingRecordSaveInput {
    actionItems: MeetingActionItemSaveInput[];
    attachmentFileId?: null | string;
    decisions: string;
    facilitatorUserId: string;
    meetingTime?: null | string;
    minutes: string;
    participantUserIds: string[];
    status: string;
    topic: string;
  }

  export interface MeetingActionItemSaveInput {
    actionType: string;
    assigneeUserId?: null | string;
    dueDate?: null | string;
    estimatedWorkHours?: null | number;
    generatedIssueType?: string;
    generatedRiskResponseStrategy?: string;
    linkedIssueId?: null | string;
    linkedRaidItemId?: null | string;
    linkedRequirementId?: null | string;
    linkedTaskId?: null | string;
    priority?: string;
    status: string;
    taskCode?: string;
    title: string;
  }

  export interface QualityChecklist {
    category: string;
    description: string;
    id: string;
    inspectionCount: number;
    projectId: string;
    title: string;
  }

  export interface QualityInspection {
    findingSummary: string;
    id: string;
    inspectionResult: string;
    inspectionTime: string;
    inspectorName: string;
    projectId: string;
    qualityChecklistId: string;
    qualityChecklistTitle: string;
    rectificationRequirement: string;
  }

  export interface QualityChecklistSaveInput {
    category: string;
    description: string;
    title: string;
  }

  export interface QualityInspectionSaveInput {
    findingSummary: string;
    inspectionResult: string;
    inspectionTime: null | string;
    inspectorUserId: string;
    qualityChecklistId: string;
    rectificationRequirement: string;
  }

  export interface DetailDocument {
    category: string;
    id: string;
    ownerName: string;
    status: string;
    summary: string;
    title: string;
    version: string;
  }

  export interface DetailMember {
    accessLevel: string;
    id: string;
    isActive: boolean;
    joinDate: string;
    leaveDate: null | string;
    organizationUnitName: string;
    roleNames: string[];
    userId: string;
    userName: string;
  }

  export interface MemberRolesUpdateInput {
    roleNames: string[];
  }

  export interface ProjectCalendarItem {
    colorToken: string;
    endDate: null | string;
    id: string;
    ownerName: string;
    startDate: null | string;
    status: string;
    title: string;
    type: string;
  }

  export interface Detail {
    acceptanceCriteria: DetailRecord[];
    acceptanceRecords: DetailRecord[];
    activeBaseline: Baseline | null;
    budgetTotalAmount: null | number;
    calendarItems: ProjectCalendarItem[];
    changeRequests: ChangeRequest[];
    charter: Charter | null;
    contractTotalAmount: null | number;
    description: string;
    documents: DetailDocument[];
    healthBreakdowns: HealthBreakdown[];
    healthCalculatedTime: string;
    healthDataSource: string;
    healthReasons: string[];
    healthRiskScore: number;
    healthStatus: string;
    id: string;
    isKeyProject: boolean;
    managerName: string;
    meetingRecords: MeetingRecord[];
    memberCount: number;
    members: DetailMember[];
    milestones: MilestoneItem[];
    myRelation: string;
    name: string;
    openIssueCount: number;
    organizationUnitName: string;
    performanceSnapshot: null | PerformanceSnapshot;
    performanceSnapshots: PerformanceSnapshot[];
    plannedEndDate: null | string;
    plannedStartDate: null | string;
    priority: string;
    projectCode: string;
    projectSource: string;
    projectType: string;
    qualityChecklists: QualityChecklist[];
    qualityInspections: QualityInspection[];
    raidItems: RaidItem[];
    receivedAmount: null | number;
    reports: Report[];
    requirements: RequirementItem[];
    sponsorName: string;
    sprints: SprintItem[];
    status: string;
    taskCompletionRate: number;
    taskDependencies: TaskDependency[];
    taskScheduleImpacts: DetailRecord[];
    tasks: TaskItem[];
    totalWorklogHours: number;
    wbsNodes: WbsNode[];
    worklogs: WorklogItem[];
    issues: IssueItem[];
  }

  export interface PerformanceSnapshot {
    actualCost: number;
    costPerformanceIndex: number;
    earnedValue: number;
    id: string;
    plannedValue: number;
    projectBaselineId: null | string;
    projectId: string;
    schedulePerformanceIndex: number;
    snapshotDate: string;
    snapshotPeriod: string;
  }
}

export function getProjectWorkspaceApi(signal?: AbortSignal): Promise<ProjectApi.Workspace> {
  return apiClient.request('/app/project/workspace', { signal });
}

export function getProjectMyRelatedDashboardApi(signal?: AbortSignal): Promise<ProjectApi.MyRelatedDashboard> {
  return apiClient.request('/app/project/dashboard/my-related', { signal });
}

export function getProjectPmDashboardApi(signal?: AbortSignal): Promise<ProjectApi.PmDashboard> {
  return apiClient.request('/app/project/dashboard/pm', { signal });
}

export function getProjectPmoDashboardApi(signal?: AbortSignal): Promise<ProjectApi.PmoDashboard> {
  return apiClient.request('/app/project/dashboard/pmo', { signal });
}

export function getProjectListApi(input: ProjectApi.ListInput, signal?: AbortSignal): Promise<ProjectApi.ListItem[]> {
  return apiClient.request('/app/project/list', { query: { ...input }, signal });
}

export function getProjectPagedListApi(
  input: ProjectApi.PagedListInput,
  signal?: AbortSignal,
): Promise<ProjectApi.PagedListResult> {
  return apiClient.request('/app/project/list/paged', { query: { ...input }, signal });
}

export function getProjectDetailApi(projectId: string, signal?: AbortSignal): Promise<ProjectApi.Detail> {
  return apiClient.request(`/app/project/detail/${encodeURIComponent(projectId)}`, { signal });
}

export function getProjectDetailSectionApi(
  section: ProjectApi.DetailSection,
  projectId: string,
  signal?: AbortSignal,
): Promise<ProjectApi.Detail> {
  return apiClient.request(`/app/project/detail/${section}/${encodeURIComponent(projectId)}`, { signal });
}

export function updateProjectMemberRolesApi(
  projectMemberId: string,
  input: ProjectApi.MemberRolesUpdateInput,
): Promise<boolean> {
  return apiClient.request(`/app/project/members/${encodeURIComponent(projectMemberId)}/roles`, {
    body: input,
    method: 'PUT',
  });
}

export function createProjectMeetingRecordApi(
  projectId: string,
  input: ProjectApi.MeetingRecordSaveInput,
): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/meeting-records`, {
    body: input,
    method: 'POST',
  });
}

export function createProjectIssueApi(projectId: string, input: ProjectApi.IssueSaveInput): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/issues`, {
    body: input,
    method: 'POST',
  });
}

export function updateProjectIssueApi(issueId: string, input: ProjectApi.IssueSaveInput): Promise<boolean> {
  return apiClient.request(`/app/project/issues/${encodeURIComponent(issueId)}`, {
    body: input,
    method: 'PUT',
  });
}

export function deleteProjectIssueApi(issueId: string): Promise<boolean> {
  return apiClient.request(`/app/project/issues/${encodeURIComponent(issueId)}`, { method: 'DELETE' });
}

export function createProjectQualityChecklistApi(
  projectId: string,
  input: ProjectApi.QualityChecklistSaveInput,
): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/quality-checklists`, {
    body: input,
    method: 'POST',
  });
}

export function createProjectQualityInspectionApi(
  projectId: string,
  input: ProjectApi.QualityInspectionSaveInput,
): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/quality-inspections`, {
    body: input,
    method: 'POST',
  });
}

export function createProjectRequirementApi(
  projectId: string,
  input: ProjectApi.RequirementSaveInput,
): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/requirements`, {
    body: input,
    method: 'POST',
  });
}

export function updateProjectRequirementApi(
  requirementId: string,
  input: ProjectApi.RequirementSaveInput,
): Promise<boolean> {
  return apiClient.request(`/app/project/requirements/${encodeURIComponent(requirementId)}`, {
    body: input,
    method: 'PUT',
  });
}

export function updateProjectRaidRiskApi(raidItemId: string, input: ProjectApi.RaidRiskUpdateInput): Promise<boolean> {
  return apiClient.request(`/app/project/raid/${encodeURIComponent(raidItemId)}/risk-assessment`, {
    body: input,
    method: 'PUT',
  });
}

export function getProjectTaskPagedListApi(
  input: ProjectApi.PagedTaskListInput,
  signal?: AbortSignal,
): Promise<ProjectApi.PagedTaskListResult> {
  return apiClient.request('/app/project/task/list/paged', { query: { ...input }, signal });
}

export function createProjectTaskDependencyApi(
  projectId: string,
  input: ProjectApi.TaskDependencySaveInput,
): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/task-dependencies`, {
    body: input,
    method: 'POST',
  });
}

export function recordProjectTaskProgressApi(taskId: string, input: ProjectApi.TaskProgressInput): Promise<string> {
  return apiClient.request(`/app/project/tasks/${encodeURIComponent(taskId)}/progress`, {
    body: input,
    method: 'POST',
  });
}

export function createProjectApi(input: ProjectApi.ProjectSaveInput): Promise<string> {
  return apiClient.request('/app/project', { body: input, method: 'POST' });
}

export function saveProjectCharterApi(projectId: string, input: ProjectApi.CharterSaveInput): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/charter`, { body: input, method: 'POST' });
}

export function submitProjectCharterApprovalApi(
  projectId: string,
  input: ProjectApi.WorkflowActionInput,
): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/charter/approval/submit`, {
    body: input,
    method: 'POST',
  });
}

export function approveProjectCharterApi(projectId: string, input: ProjectApi.WorkflowActionInput): Promise<boolean> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/charter/approval/approve`, {
    body: input,
    method: 'POST',
  });
}

export function rejectProjectCharterApi(projectId: string, input: ProjectApi.WorkflowActionInput): Promise<boolean> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/charter/approval/reject`, {
    body: input,
    method: 'POST',
  });
}

export function withdrawProjectCharterApprovalApi(
  projectId: string,
  input: ProjectApi.WorkflowActionInput,
): Promise<boolean> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/charter/approval/withdraw`, {
    body: input,
    method: 'POST',
  });
}

export function createProjectChangeRequestApi(
  projectId: string,
  input: ProjectApi.ChangeRequestSaveInput,
): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/change-requests`, {
    body: input,
    method: 'POST',
  });
}

export function submitProjectChangeApprovalApi(
  changeRequestId: string,
  input: ProjectApi.WorkflowActionInput,
): Promise<string> {
  return apiClient.request(`/app/project/change-requests/${encodeURIComponent(changeRequestId)}/approval/submit`, {
    body: input,
    method: 'POST',
  });
}

export function approveProjectChangeApi(
  changeRequestId: string,
  input: ProjectApi.WorkflowActionInput,
): Promise<boolean> {
  return apiClient.request(`/app/project/change-requests/${encodeURIComponent(changeRequestId)}/approval/approve`, {
    body: input,
    method: 'POST',
  });
}

export function rejectProjectChangeApi(
  changeRequestId: string,
  input: ProjectApi.WorkflowActionInput,
): Promise<boolean> {
  return apiClient.request(`/app/project/change-requests/${encodeURIComponent(changeRequestId)}/approval/reject`, {
    body: input,
    method: 'POST',
  });
}

export function withdrawProjectChangeApprovalApi(
  changeRequestId: string,
  input: ProjectApi.WorkflowActionInput,
): Promise<boolean> {
  return apiClient.request(`/app/project/change-requests/${encodeURIComponent(changeRequestId)}/approval/withdraw`, {
    body: input,
    method: 'POST',
  });
}

export function updateProjectStatusApi(projectId: string, input: ProjectApi.StatusUpdateInput): Promise<boolean> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/status`, { body: input, method: 'PUT' });
}

export function createProjectWbsNodeApi(projectId: string, input: ProjectApi.WbsNodeSaveInput): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/wbs-nodes`, { body: input, method: 'POST' });
}

export function updateProjectWbsNodeApi(wbsNodeId: string, input: ProjectApi.WbsNodeSaveInput): Promise<boolean> {
  return apiClient.request(`/app/project/wbs-nodes/${encodeURIComponent(wbsNodeId)}`, {
    body: input,
    method: 'PUT',
  });
}

export function deleteProjectWbsNodeApi(wbsNodeId: string): Promise<boolean> {
  return apiClient.request(`/app/project/wbs-nodes/${encodeURIComponent(wbsNodeId)}`, { method: 'DELETE' });
}

export function createProjectSprintApi(projectId: string, input: ProjectApi.SprintSaveInput): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/sprints`, { body: input, method: 'POST' });
}

export function updateProjectSprintApi(sprintId: string, input: ProjectApi.SprintSaveInput): Promise<boolean> {
  return apiClient.request(`/app/project/sprints/${encodeURIComponent(sprintId)}`, { body: input, method: 'PUT' });
}

export function deleteProjectSprintApi(sprintId: string): Promise<boolean> {
  return apiClient.request(`/app/project/sprints/${encodeURIComponent(sprintId)}`, { method: 'DELETE' });
}

export function createProjectMilestoneApi(projectId: string, input: ProjectApi.MilestoneSaveInput): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/milestones`, {
    body: input,
    method: 'POST',
  });
}

export function updateProjectMilestoneApi(milestoneId: string, input: ProjectApi.MilestoneSaveInput): Promise<boolean> {
  return apiClient.request(`/app/project/milestones/${encodeURIComponent(milestoneId)}`, {
    body: input,
    method: 'PUT',
  });
}

export function deleteProjectMilestoneApi(milestoneId: string): Promise<boolean> {
  return apiClient.request(`/app/project/milestones/${encodeURIComponent(milestoneId)}`, { method: 'DELETE' });
}

export function createProjectBaselineApi(projectId: string, input: ProjectApi.BaselineSaveInput): Promise<string> {
  return apiClient.request(`/app/project/${encodeURIComponent(projectId)}/baselines`, { body: input, method: 'POST' });
}
