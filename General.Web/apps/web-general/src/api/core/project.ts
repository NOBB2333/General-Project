import { requestClient } from '#/api/request';

export namespace ProjectApi {
  export interface CalendarItem {
    colorToken: string;
    endDate?: null | string;
    id: string;
    ownerName: string;
    startDate?: null | string;
    status: string;
    title: string;
    type: string;
  }

  export interface SprintItem {
    closedMilestoneCount: number;
    closedTaskCount: number;
    endDate?: null | string;
    highRiskCount: number;
    id: string;
    latestReportStatus: string;
    milestoneCount: number;
    name: string;
    openIssueCount: number;
    ownerName: string;
    ownerUserId: string;
    overdueMilestoneCount: number;
    overdueTaskCount: number;
    progress: number;
    reportCount: number;
    riskCount: number;
    sprintCode: string;
    startDate?: null | string;
    status: string;
    summary: string;
    taskCount: number;
  }

  export interface SprintSaveInput {
    endDate?: string;
    name: string;
    ownerUserId: string;
    progress: number;
    startDate?: string;
    status: string;
    summary: string;
  }

  export interface DocumentItem {
    category: string;
    id: string;
    ownerName: string;
    status: string;
    summary: string;
    title: string;
    version: string;
  }

  export interface IssueItem {
    acceptanceCriteriaId?: null | string;
    acceptanceCriteriaTitle: string;
    creationTime: string;
    developerOwnerName?: null | string;
    developerOwnerUserId?: null | string;
    dueDate?: null | string;
    id: string;
    isOverdue: boolean;
    level: string;
    ownerName: string;
    ownerUserId: string;
    productOwnerName?: null | string;
    productOwnerUserId?: null | string;
    qualityChecklistId?: null | string;
    qualityChecklistTitle: string;
    requirementTitle?: null | string;
    status: string;
    testerOwnerName?: null | string;
    testerOwnerUserId?: null | string;
    title: string;
    type: string;
  }

  export interface MemberItem {
    accessLevel: string;
    id: string;
    isActive: boolean;
    joinDate: string;
    leaveDate?: null | string;
    organizationUnitName: string;
    roleNames: string[];
    userId: string;
    userName: string;
  }

  export interface MemberRolesUpdateInput {
    roleNames: string[];
  }

  export interface IssueQualityLinkInput {
    acceptanceCriteriaId?: string;
    qualityChecklistId?: string;
  }

  export interface IssueSaveInput {
    acceptanceCriteriaId?: string;
    developerOwnerUserId?: string;
    dueDate?: string;
    level: string;
    ownerUserId: string;
    productOwnerUserId?: string;
    qualityChecklistId?: string;
    requirementTitle: string;
    status: string;
    testerOwnerUserId?: string;
    title: string;
    type: string;
  }

  export interface MyRelatedItem {
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

  export interface MyRelatedPayload {
    myIssues: IssueItem[];
    myProjects: MyRelatedItem[];
    myTasks: TaskItem[];
    recentWorklogs: WorklogItem[];
  }

  export interface DashboardMetric {
    label: string;
    trend: string;
    unit: string;
    value: number;
  }

  export interface DashboardHealthBucket {
    count: number;
    healthStatus: string;
  }

  export interface DashboardProjectCard {
    blockedTaskCount: number;
    completedTaskCount: number;
    contractAmount?: null | number;
    costAmount?: null | number;
    costPerformanceIndex?: null | number;
    earnedValue?: null | number;
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
    plannedEndDate?: null | string;
    plannedStartDate?: null | string;
    plannedValue?: null | number;
    priority: string;
    progress: number;
    projectCode: string;
    receivedAmount?: null | number;
    schedulePerformanceIndex?: null | number;
    status: string;
    taskTotalCount: number;
  }

  export interface DashboardActionItem {
    actionKey: string;
    dueDate?: null | string;
    level: string;
    projectId: string;
    projectName: string;
    reason: string;
    sourceId: string;
    sourceType: string;
    status: string;
    title: string;
  }

  export interface DashboardPerformancePoint {
    actualCost: number;
    costPerformanceIndex: number;
    earnedValue: number;
    plannedValue: number;
    projectId: string;
    projectName: string;
    schedulePerformanceIndex: number;
    snapshotDate: string;
  }

  export interface DashboardBusinessRisk {
    businessCloseStatus: string;
    contractAmount?: null | number;
    costAmount?: null | number;
    grossProfitRate?: null | number;
    projectId: string;
    projectName: string;
    receivedAmount?: null | number;
    riskLabel: string;
    yearEndReceivableForecast?: null | number;
  }

  export interface DashboardEarnedValueSummary {
    actualCost: number;
    costPerformanceIndex: number;
    earnedValue: number;
    plannedValue: number;
    schedulePerformanceIndex: number;
    snapshotDate?: null | string;
  }

  export interface DashboardDepartmentPressure {
    name: string;
    projectCount: number;
    riskScore: number;
  }

  export interface MyExecutionDashboard {
    actionItems: DashboardActionItem[];
    metrics: DashboardMetric[];
    myIssues: IssueItem[];
    myProjects: MyRelatedItem[];
    myTasks: TaskItem[];
    recentWorklogs: WorklogItem[];
  }

  export interface PmDashboard {
    actionItems: DashboardActionItem[];
    businessRisks: DashboardBusinessRisk[];
    earnedValueSummary: DashboardEarnedValueSummary;
    healthBuckets: DashboardHealthBucket[];
    metrics: DashboardMetric[];
    performancePoints: DashboardPerformancePoint[];
    projects: DashboardProjectCard[];
  }

  export interface PmoDashboard {
    actionItems: DashboardActionItem[];
    businessRisks: DashboardBusinessRisk[];
    departmentPressures: DashboardDepartmentPressure[];
    earnedValueSummary: DashboardEarnedValueSummary;
    exceptionProjects: DashboardProjectCard[];
    healthBuckets: DashboardHealthBucket[];
    metrics: DashboardMetric[];
    performancePoints: DashboardPerformancePoint[];
    projects: DashboardProjectCard[];
  }

  export interface AcceptanceCriteriaItem {
    description: string;
    id: string;
    milestoneId?: null | string;
    milestoneName: string;
    projectId: string;
    status: string;
    taskId?: null | string;
    taskTitle: string;
    title: string;
  }

  export interface AcceptanceCriteriaSaveInput {
    description: string;
    milestoneId?: string;
    status: string;
    taskId?: string;
    title: string;
  }

  export interface AcceptanceRecordItem {
    acceptanceCriteriaIds: string[];
    acceptanceCriteriaTitles: string[];
    acceptanceFileId?: null | string;
    acceptanceFileName: string;
    acceptanceResult: string;
    acceptanceTime: string;
    acceptorName: string;
    acceptorUserId: string;
    id: string;
    milestoneId?: null | string;
    milestoneName: string;
    projectId: string;
    rectificationRequirement: string;
    sprintId?: null | string;
    sprintName: string;
    requirementIds: string[];
    requirementTitles: string[];
    remark: string;
    title: string;
  }

  export interface AcceptanceRecordSaveInput {
    acceptanceCriteriaIds: string[];
    acceptanceFileId?: string;
    acceptanceResult: string;
    acceptanceTime?: string;
    acceptorUserId: string;
    createIssueWhenRejected: boolean;
    issueDueDate?: string;
    issueLevel: string;
    issueOwnerUserId?: string;
    milestoneId?: string;
    rectificationRequirement: string;
    sprintId?: string;
    requirementIds: string[];
    remark: string;
    title: string;
  }

  export interface Charter {
    approvalStatus: string;
    approvalWorkflow?: null | WorkflowInstance;
    assumptions: string;
    businessCase: string;
    constraints: string;
    id: string;
    objectives: string;
    projectId: string;
    scopeBoundary: string;
    successCriteria: string;
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

  export interface CharterApprovalActionInput {
    comment: string;
  }

  export interface ProjectWorkflowActionInput {
    comment: string;
  }

  export interface ApprovalRecord {
    action: string;
    actorUserId?: null | string;
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
    applicantUserId?: null | string;
    applicantUserName: string;
    approvalRecords: ApprovalRecord[];
    businessCode: string;
    businessId: string;
    businessTitle: string;
    businessType: string;
    completedTime?: null | string;
    creationTime: string;
    currentNodeName: string;
    elsaWorkflowInstanceId: string;
    id: string;
    status: string;
    workflowDefinitionKey: string;
    workflowDefinitionVersion: number;
  }

  export interface ProjectBaseline {
    budgetAtCompletion: number;
    costBaselineSummary: string;
    createdFromChangeRequestCode: string;
    createdFromChangeRequestId?: null | string;
    effectiveDate: string;
    id: string;
    isActive: boolean;
    projectId: string;
    scheduleBaselineSummary: string;
    scopeBaselineSummary: string;
    versionNo: number;
  }

  export interface ProjectBaselineSaveInput {
    budgetAtCompletion: number;
    costBaselineSummary: string;
    createdFromChangeRequestId?: string;
    effectiveDate?: string;
    isActive: boolean;
    scheduleBaselineSummary: string;
    scopeBaselineSummary: string;
  }

  export interface ProjectChangeApproval {
    action: string;
    approverName: string;
    approverUserId: string;
    comment: string;
    decisionTime: string;
    id: string;
  }

  export interface ProjectChangeImpact {
    analysis: string;
    estimatedCostDelta?: null | number;
    estimatedScheduleDeltaDays?: null | number;
    id: string;
    impactType: string;
    relatedBusinessCode: string;
    relatedBudgetCode: string;
    relatedContractCode: string;
    relatedMilestoneId?: null | string;
    relatedMilestoneName: string;
    relatedTaskId?: null | string;
    relatedTaskTitle: string;
    severity: string;
  }

  export interface ProjectChangeImpactSaveInput {
    analysis: string;
    estimatedCostDelta?: number;
    estimatedScheduleDeltaDays?: number;
    impactType: string;
    relatedBusinessCode: string;
    relatedBudgetCode: string;
    relatedContractCode: string;
    relatedMilestoneId?: string;
    relatedTaskId?: string;
    severity: string;
  }

  export interface ProjectChangeRequest {
    applicantName: string;
    applicantUserId: string;
    approvalWorkflow?: null | WorkflowInstance;
    approvals: ProjectChangeApproval[];
    changeType: string;
    expectedResult: string;
    id: string;
    impactScope: string;
    impacts: ProjectChangeImpact[];
    projectId: string;
    reason: string;
    requestCode: string;
    status: string;
    title: string;
  }

  export interface ProjectChangeRequestSaveInput {
    applicantUserId?: string;
    changeType: string;
    expectedResult: string;
    impactScope: string;
    impacts: ProjectChangeImpactSaveInput[];
    reason: string;
    requestCode: string;
    title: string;
  }

  export interface ProjectPerformanceSnapshot {
    actualCost: number;
    costPerformanceIndex: number;
    earnedValue: number;
    id: string;
    plannedValue: number;
    projectBaselineId?: null | string;
    projectId: string;
    schedulePerformanceIndex: number;
    snapshotDate: string;
    snapshotPeriod: string;
  }

  export interface ProjectPerformanceSnapshotSaveInput {
    actualCost?: number;
    earnedValue?: number;
    plannedValue?: number;
    projectBaselineId?: string;
    snapshotDate?: string;
    snapshotPeriod: string;
  }

  export interface ProjectReport {
    attachmentFileId?: null | string;
    attachmentFileName: string;
    authorName: string;
    authorUserId: string;
    businessSummary: string;
    completedSummary: string;
    sprintId?: null | string;
    sprintName: string;
    hoursSummary: string;
    id: string;
    milestoneSummary: string;
    nextPlan: string;
    periodEnd: string;
    periodStart: string;
    projectId: string;
    reportType: string;
    riskIssueSummary: string;
    status: string;
    title: string;
  }

  export interface ProjectReportDraft {
    businessSummary: string;
    completedSummary: string;
    hoursSummary: string;
    milestoneSummary: string;
    nextPlan: string;
    periodEnd: string;
    periodStart: string;
    reportType: string;
    riskIssueSummary: string;
    title: string;
  }

  export interface ProjectReportDraftInput {
    periodEnd?: string;
    periodStart?: string;
    reportType: string;
  }

  export interface ProjectReportSaveInput {
    attachmentFileId?: string;
    authorUserId: string;
    businessSummary: string;
    completedSummary: string;
    sprintId?: string;
    hoursSummary: string;
    milestoneSummary: string;
    nextPlan: string;
    periodEnd?: string;
    periodStart?: string;
    reportType: string;
    riskIssueSummary: string;
    status: string;
    title: string;
  }

  export interface ProjectMeetingActionItem {
    actionType: string;
    assigneeName: string;
    assigneeUserId?: null | string;
    dueDate?: null | string;
    id: string;
    linkedIssueId?: null | string;
    linkedIssueTitle: string;
    linkedRaidItemId?: null | string;
    linkedRaidItemTitle: string;
    linkedRequirementId?: null | string;
    linkedRequirementTitle: string;
    linkedTaskId?: null | string;
    linkedTaskTitle: string;
    status: string;
    title: string;
  }

  export interface ProjectMeetingRecord {
    actionItems: ProjectMeetingActionItem[];
    attachmentFileId?: null | string;
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

  export interface ProjectMeetingActionItemSaveInput {
    actionType: string;
    assigneeUserId?: string;
    dueDate?: string;
    estimatedWorkHours?: number;
    generatedIssueType: string;
    generatedRiskResponseStrategy: string;
    linkedIssueId?: string;
    linkedRaidItemId?: string;
    linkedRequirementId?: string;
    linkedTaskId?: string;
    priority: string;
    status: string;
    taskCode: string;
    title: string;
  }

  export interface ProjectMeetingRecordSaveInput {
    actionItems: ProjectMeetingActionItemSaveInput[];
    attachmentFileId?: string;
    decisions: string;
    facilitatorUserId: string;
    meetingTime?: string;
    minutes: string;
    participantUserIds: string[];
    status: string;
    topic: string;
  }

  export interface ProjectHealthBreakdown {
    actionKey: string;
    actionLabel: string;
    code: string;
    dataSource: string;
    label: string;
    reason: string;
    ruleVersion: string;
    score: number;
  }

  export interface ProjectRequirement {
    acceptanceCriteria: string;
    businessOwnerName: string;
    businessOwnerUserId?: null | string;
    changeReason: string;
    description: string;
    id: string;
    impactSummary: string;
    isCurrent: boolean;
    milestoneId?: null | string;
    milestoneName: string;
    priority: string;
    productOwnerName: string;
    productOwnerUserId?: null | string;
    projectId: string;
    replacesRequirementId?: null | string;
    replacesRequirementTitle: string;
    requirementCode: string;
    sizeEstimate?: null | number;
    source: string;
    status: string;
    title: string;
    type: string;
    versionNo: number;
    wbsNodeId?: null | string;
    wbsNodeName: string;
  }

  export interface ProjectRequirementSaveInput {
    acceptanceCriteria: string;
    businessOwnerUserId?: string;
    changeReason: string;
    description: string;
    impactSummary: string;
    isCurrent: boolean;
    milestoneId?: string;
    priority: string;
    productOwnerUserId?: string;
    replacesRequirementId?: string;
    requirementCode: string;
    sizeEstimate?: number;
    source: string;
    status: string;
    title: string;
    type: string;
    versionNo: number;
    wbsNodeId?: string;
  }

  export interface RequirementFromWbsSaveInput {
    acceptanceCriteria: string;
    businessOwnerUserId?: string;
    description: string;
    impactSummary: string;
    priority: string;
    productOwnerUserId?: string;
    requirementCode: string;
    sizeEstimate?: number;
    source: string;
    status: string;
    title: string;
    type: string;
    versionNo: number;
  }

  export interface TaskFromRequirementSaveInput {
    developerOwnerUserId?: string;
    developerOwnerUserIds: string[];
    description: string;
    estimatedWorkHours?: number;
    isOutOfContract: boolean;
    ownerUserId: string;
    plannedEndTime?: string;
    plannedStartTime?: string;
    priority: string;
    productOwnerUserId?: string;
    productOwnerUserIds: string[];
    taskCode: string;
    testerOwnerUserId?: string;
    testerOwnerUserIds: string[];
    title: string;
  }

  export interface TaskFromWbsSaveInput {
    sprintId?: string;
    developerOwnerUserId?: string;
    developerOwnerUserIds: string[];
    description: string;
    estimatedWorkHours?: number;
    isOutOfContract: boolean;
    ownerUserId: string;
    plannedEndTime?: string;
    plannedStartTime?: string;
    priority: string;
    productOwnerUserId?: string;
    productOwnerUserIds: string[];
    taskCode: string;
    testerOwnerUserId?: string;
    testerOwnerUserIds: string[];
    title: string;
  }

  export interface TaskSaveInput {
    sprintId?: string;
    developerOwnerUserId?: string;
    developerOwnerUserIds: string[];
    description: string;
    estimatedWorkHours?: number;
    isOutOfContract: boolean;
    ownerUserId: string;
    plannedEndTime?: string;
    plannedStartTime?: string;
    priority: string;
    productOwnerUserId?: string;
    productOwnerUserIds: string[];
    status: string;
    taskCode: string;
    testerOwnerUserId?: string;
    testerOwnerUserIds: string[];
    title: string;
    wbsNodeId?: string;
  }

  export interface ProjectDetail {
    acceptanceCriteria: AcceptanceCriteriaItem[];
    acceptanceRecords: AcceptanceRecordItem[];
    activeBaseline?: null | ProjectBaseline;
    budgetTotalAmount?: null | number;
    calendarItems: CalendarItem[];
    charter?: Charter | null;
    changeRequests: ProjectChangeRequest[];
    contractTotalAmount?: null | number;
    sprints: SprintItem[];
    description: string;
    documents: DocumentItem[];
    highRiskCount: number;
    healthBreakdowns: ProjectHealthBreakdown[];
    healthCalculatedTime: string;
    healthDataSource: string;
    healthReasons: string[];
    healthRiskScore: number;
    healthStatus: string;
    id: string;
    isKeyProject: boolean;
    issues: IssueItem[];
    managerName: string;
    memberCount: number;
    members: MemberItem[];
    meetingRecords: ProjectMeetingRecord[];
    milestones: WorkspaceMilestoneItem[];
    myRelation: string;
    name: string;
    openIssueCount: number;
    organizationUnitName: string;
    performanceSnapshot?: null | ProjectPerformanceSnapshot;
    performanceSnapshots: ProjectPerformanceSnapshot[];
    plannedEndDate?: null | string;
    plannedStartDate?: null | string;
    priority: string;
    projectCode: string;
    projectSource: string;
    projectType: string;
    requirements: ProjectRequirement[];
    reports: ProjectReport[];
    qualityChecklists: QualityChecklistItem[];
    qualityInspections: QualityInspectionItem[];
    raidItems: RaidItem[];
    receivedAmount?: null | number;
    sponsorName: string;
    status: string;
    taskCompletionRate: number;
    taskDependencies: TaskDependencyItem[];
    taskScheduleImpacts: TaskScheduleImpactItem[];
    tasks: TaskItem[];
    totalWorklogHours: number;
    wbsNodes: WbsNode[];
    worklogs: WorklogItem[];
  }

  export type ProjectDetailSection =
    | 'control'
    | 'delivery'
    | 'docs'
    | 'issues'
    | 'overview'
    | 'plan'
    | 'resources'
    | 'tasks';

  export interface ProjectListInput {
    keyword?: string;
    onlyMyRelated?: boolean;
    status?: string;
  }

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

  export interface ProjectListItem {
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
    plannedEndDate?: null | string;
    plannedStartDate?: null | string;
    priority: string;
    projectCode: string;
    status: string;
    taskTotalCount: number;
  }

  export interface ProjectStatusUpdateInput {
    status: string;
  }

  export interface ProjectMilestoneStatusUpdateInput {
    actualCompletionDate?: string;
    sprintId?: string;
    status: string;
  }

  export interface ProjectMilestoneSaveInput {
    actualCompletionDate?: string;
    sprintId?: string;
    name: string;
    ownerUserId: string;
    plannedCompletionDate: string;
    status: string;
  }

  export interface RaidItem {
    contingencyPlan: string;
    id: string;
    impact: number;
    isOverdue: boolean;
    level: string;
    ownerName: string;
    plannedResolveDate?: null | string;
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

  export interface RaidListInput {
    keyword?: string;
    onlyOpen?: boolean;
    type?: string;
  }

  export interface RaidRiskUpdateInput {
    contingencyPlan: string;
    impact: number;
    probability: number;
    responseStrategy: string;
    triggerCondition: string;
  }

  export interface QualityChecklistItem {
    category: string;
    description: string;
    id: string;
    inspectionCount: number;
    projectId: string;
    title: string;
  }

  export interface QualityChecklistSaveInput {
    category: string;
    description: string;
    title: string;
  }

  export interface QualityInspectionSaveInput {
    findingSummary: string;
    inspectionResult: string;
    inspectionTime?: string;
    inspectorUserId: string;
    qualityChecklistId: string;
    rectificationRequirement: string;
  }

  export interface QualityInspectionItem {
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

  export interface TaskDependencyItem {
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

  export interface TaskScheduleImpactItem {
    impactReason: string;
    predecessorPlannedEndTime?: null | string;
    predecessorTaskId: string;
    predecessorTaskTitle: string;
    successorPlannedStartTime?: null | string;
    successorTaskId: string;
    successorTaskTitle: string;
  }

  export interface TaskWbsUpdateInput {
    baselineEndTime?: string;
    baselineStartTime?: string;
    sprintId?: string;
    parentTaskId?: string;
    requirementId?: string;
    sortOrder: number;
    wbsCode: string;
    wbsNodeId?: string;
  }

  export interface TaskProgressInput {
    blockReason: string;
    hours: number;
    isBlocked: boolean;
    status: string;
    summary: string;
    workDate?: string;
  }

  export interface WbsNode {
    actualWorkHours: number;
    completionRate: number;
    sprintId?: null | string;
    sprintName: string;
    deliveryDescription: string;
    estimateSummary: string;
    estimatedWorkHours: number;
    id: string;
    milestoneId?: null | string;
    milestoneName: string;
    name: string;
    ownerName: string;
    ownerUserId?: null | string;
    parentId?: null | string;
    projectId: string;
    requirementCount: number;
    scopeDescription: string;
    sortOrder: number;
    status: string;
    taskCount: number;
    type: string;
    wbsCode: string;
  }

  export interface WbsNodeSaveInput {
    sprintId?: string;
    deliveryDescription: string;
    estimateSummary: string;
    milestoneId?: string;
    name: string;
    ownerUserId?: string;
    parentId?: string;
    scopeDescription: string;
    sortOrder: number;
    status: string;
    type: string;
    wbsCode: string;
  }

  export interface TaskItem {
    actualEndTime?: null | string;
    actualStartTime?: null | string;
    actualWorkHours?: null | number;
    baselineEndTime?: null | string;
    baselineStartTime?: null | string;
    blockReason?: null | string;
    contractClause?: null | string;
    sprintId?: null | string;
    sprintName: string;
    developerOwnerName?: null | string;
    developerOwnerNames: string[];
    developerOwnerUserId?: null | string;
    developerOwnerUserIds: string[];
    description: string;
    estimatedWorkHours?: null | number;
    id: string;
    isBlocked: boolean;
    isOutOfContract: boolean;
    organizationUnitName: string;
    ownerName: string;
    ownerUserId: string;
    parentTaskId?: null | string;
    plannedEndTime?: null | string;
    plannedStartTime?: null | string;
    priority: string;
    productOwnerName?: null | string;
    productOwnerNames: string[];
    productOwnerUserId?: null | string;
    productOwnerUserIds: string[];
    projectId: string;
    projectName: string;
    requirementId?: null | string;
    requirementTitle: string;
    status: string;
    taskCode: string;
    testerOwnerName?: null | string;
    testerOwnerNames: string[];
    testerOwnerUserId?: null | string;
    testerOwnerUserIds: string[];
    title: string;
    sortOrder: number;
    wbsCode: string;
    wbsNodeId?: null | string;
    wbsNodeName: string;
  }

  export interface TaskListInput {
    keyword?: string;
    onlyMine?: boolean;
    projectId?: string;
    status?: string;
  }

  export interface WorklogItem {
    hours: number;
    id: string;
    projectId?: string;
    projectName?: string;
    summary: string;
    taskId?: null | string;
    taskTitle: string;
    userId: string;
    userName: string;
    weekStartDate: string;
    workDate: string;
  }

  export interface WorkspaceItem {
    blockedTaskCount: number;
    highRiskCount: number;
    keyProjects: ProjectListItem[];
    myTasks: TaskItem[];
    myTodoCount: number;
    ongoingProjectCount: number;
    overdueTaskCount: number;
    raidAlerts: RaidItem[];
    weeklyMilestones: WorkspaceMilestoneItem[];
  }

  export interface WorkspaceMilestoneItem {
    actualCompletionDate?: null | string;
    sprintId?: null | string;
    sprintName: string;
    id: string;
    name: string;
    ownerName: string;
    ownerUserId: string;
    plannedCompletionDate: string;
    projectId: string;
    projectName: string;
    status: string;
  }
}

export async function getProjectWorkspaceApi() {
  return requestClient.get<ProjectApi.WorkspaceItem>('/app/project/workspace');
}

export async function getProjectMyRelatedDashboardApi() {
  return requestClient.get<ProjectApi.MyExecutionDashboard>('/app/project/dashboard/my-related');
}

export async function getProjectPmDashboardApi() {
  return requestClient.get<ProjectApi.PmDashboard>('/app/project/dashboard/pm');
}

export async function getProjectPmoDashboardApi() {
  return requestClient.get<ProjectApi.PmoDashboard>('/app/project/dashboard/pmo');
}

export async function getProjectListApi(params?: ProjectApi.ProjectListInput) {
  return requestClient.get<ProjectApi.ProjectListItem[]>('/app/project/list', {
    params,
  });
}

export async function getProjectDetailApi(projectId: string) {
  return requestClient.get<ProjectApi.ProjectDetail>(`/app/project/detail/${projectId}`);
}

export async function getProjectDetailSectionApi(
  section: ProjectApi.ProjectDetailSection,
  projectId: string,
) {
  return requestClient.get<ProjectApi.ProjectDetail>(
    `/app/project/detail/${section}/${projectId}`,
  );
}

export async function createProjectApi(data: ProjectApi.ProjectSaveInput) {
  return requestClient.post<string>('/app/project', data);
}

export async function createProjectAcceptanceCriteriaApi(
  projectId: string,
  data: ProjectApi.AcceptanceCriteriaSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/acceptance-criteria`, data);
}

export async function createProjectAcceptanceRecordApi(
  projectId: string,
  data: ProjectApi.AcceptanceRecordSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/acceptance-records`, data);
}

export async function createProjectBaselineApi(
  projectId: string,
  data: ProjectApi.ProjectBaselineSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/baselines`, data);
}

export async function submitProjectChangeApprovalApi(
  changeRequestId: string,
  data: ProjectApi.ProjectWorkflowActionInput
) {
  return requestClient.post<string>(
    `/app/project/change-requests/${changeRequestId}/approval/submit`,
    data
  );
}

export async function approveProjectChangeApi(
  changeRequestId: string,
  data: ProjectApi.ProjectWorkflowActionInput
) {
  return requestClient.post<boolean>(
    `/app/project/change-requests/${changeRequestId}/approval/approve`,
    data
  );
}

export async function rejectProjectChangeApi(
  changeRequestId: string,
  data: ProjectApi.ProjectWorkflowActionInput
) {
  return requestClient.post<boolean>(
    `/app/project/change-requests/${changeRequestId}/approval/reject`,
    data
  );
}

export async function withdrawProjectChangeApprovalApi(
  changeRequestId: string,
  data: ProjectApi.ProjectWorkflowActionInput
) {
  return requestClient.post<boolean>(
    `/app/project/change-requests/${changeRequestId}/approval/withdraw`,
    data
  );
}

export async function createProjectChangeRequestApi(
  projectId: string,
  data: ProjectApi.ProjectChangeRequestSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/change-requests`, data);
}

export async function createProjectSprintApi(projectId: string, data: ProjectApi.SprintSaveInput) {
  return requestClient.post<string>(`/app/project/${projectId}/sprints`, data);
}

export async function createProjectMilestoneApi(
  projectId: string,
  data: ProjectApi.ProjectMilestoneSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/milestones`, data);
}

export async function createProjectPerformanceSnapshotApi(
  projectId: string,
  data: ProjectApi.ProjectPerformanceSnapshotSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/performance-snapshots`, data);
}

export async function getProjectReportDraftApi(
  projectId: string,
  params: ProjectApi.ProjectReportDraftInput
) {
  return requestClient.get<ProjectApi.ProjectReportDraft>(
    `/app/project/${projectId}/reports/draft`,
    { params }
  );
}

export async function createProjectReportApi(
  projectId: string,
  data: ProjectApi.ProjectReportSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/reports`, data);
}

export async function createProjectMeetingRecordApi(
  projectId: string,
  data: ProjectApi.ProjectMeetingRecordSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/meeting-records`, data);
}

export async function createProjectQualityChecklistApi(
  projectId: string,
  data: ProjectApi.QualityChecklistSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/quality-checklists`, data);
}

export async function createProjectQualityInspectionApi(
  projectId: string,
  data: ProjectApi.QualityInspectionSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/quality-inspections`, data);
}

export async function createProjectRequirementApi(
  projectId: string,
  data: ProjectApi.ProjectRequirementSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/requirements`, data);
}

export async function createProjectTaskFromRequirementApi(
  projectId: string,
  requirementId: string,
  data: ProjectApi.TaskFromRequirementSaveInput
) {
  return requestClient.post<string>(
    `/app/project/${projectId}/requirements/${requirementId}/tasks`,
    data
  );
}

export async function createProjectRequirementFromWbsApi(
  projectId: string,
  wbsNodeId: string,
  data: ProjectApi.RequirementFromWbsSaveInput
) {
  return requestClient.post<string>(
    `/app/project/${projectId}/wbs-nodes/${wbsNodeId}/requirements`,
    data
  );
}

export async function createProjectTaskFromWbsApi(
  projectId: string,
  wbsNodeId: string,
  data: ProjectApi.TaskFromWbsSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/wbs-nodes/${wbsNodeId}/tasks`, data);
}

export async function createProjectTaskDependencyApi(
  projectId: string,
  data: ProjectApi.TaskDependencySaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/task-dependencies`, data);
}

export async function createProjectWbsNodeApi(
  projectId: string,
  data: ProjectApi.WbsNodeSaveInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/wbs-nodes`, data);
}

export async function updateProjectWbsNodeApi(
  wbsNodeId: string,
  data: ProjectApi.WbsNodeSaveInput
) {
  return requestClient.put<boolean>(`/app/project/wbs-nodes/${wbsNodeId}`, data);
}

export async function updateProjectSprintApi(sprintId: string, data: ProjectApi.SprintSaveInput) {
  return requestClient.put<boolean>(`/app/project/sprints/${sprintId}`, data);
}

export async function deleteProjectSprintApi(sprintId: string) {
  return requestClient.delete<boolean>(`/app/project/sprints/${sprintId}`);
}

export async function deleteProjectMilestoneApi(milestoneId: string) {
  return requestClient.delete<boolean>(`/app/project/milestones/${milestoneId}`);
}

export async function deleteProjectWbsNodeApi(wbsNodeId: string) {
  return requestClient.delete<boolean>(`/app/project/wbs-nodes/${wbsNodeId}`);
}

export async function getProjectMyRelatedApi() {
  return requestClient.get<ProjectApi.MyRelatedPayload>('/app/project/my-related');
}

export async function getProjectTaskListApi(params?: ProjectApi.TaskListInput) {
  return requestClient.get<ProjectApi.TaskItem[]>('/app/project/task/list', {
    params,
  });
}

export async function getProjectRaidListApi(params?: ProjectApi.RaidListInput) {
  return requestClient.get<ProjectApi.RaidItem[]>('/app/project/raid/list', {
    params,
  });
}

export async function saveProjectCharterApi(projectId: string, data: ProjectApi.CharterSaveInput) {
  return requestClient.post<string>(`/app/project/${projectId}/charter`, data);
}

export async function submitProjectCharterApprovalApi(
  projectId: string,
  data: ProjectApi.CharterApprovalActionInput
) {
  return requestClient.post<string>(`/app/project/${projectId}/charter/approval/submit`, data);
}

export async function approveProjectCharterApi(
  projectId: string,
  data: ProjectApi.CharterApprovalActionInput
) {
  return requestClient.post<boolean>(`/app/project/${projectId}/charter/approval/approve`, data);
}

export async function rejectProjectCharterApi(
  projectId: string,
  data: ProjectApi.CharterApprovalActionInput
) {
  return requestClient.post<boolean>(`/app/project/${projectId}/charter/approval/reject`, data);
}

export async function withdrawProjectCharterApprovalApi(
  projectId: string,
  data: ProjectApi.CharterApprovalActionInput
) {
  return requestClient.post<boolean>(`/app/project/${projectId}/charter/approval/withdraw`, data);
}

export async function updateProjectStatusApi(
  projectId: string,
  data: ProjectApi.ProjectStatusUpdateInput
) {
  return requestClient.put<boolean>(`/app/project/${projectId}/status`, data);
}

export async function updateProjectMilestoneStatusApi(
  milestoneId: string,
  data: ProjectApi.ProjectMilestoneStatusUpdateInput
) {
  return requestClient.put<boolean>(`/app/project/milestones/${milestoneId}/status`, data);
}

export async function updateProjectMilestoneApi(
  milestoneId: string,
  data: ProjectApi.ProjectMilestoneSaveInput
) {
  return requestClient.put<boolean>(`/app/project/milestones/${milestoneId}`, data);
}

export async function updateProjectRaidRiskApi(
  raidItemId: string,
  data: ProjectApi.RaidRiskUpdateInput
) {
  return requestClient.put<boolean>(`/app/project/raid/${raidItemId}/risk-assessment`, data);
}

export async function updateProjectRequirementApi(
  requirementId: string,
  data: ProjectApi.ProjectRequirementSaveInput
) {
  return requestClient.put<boolean>(`/app/project/requirements/${requirementId}`, data);
}

export async function updateProjectMemberRolesApi(
  projectMemberId: string,
  data: ProjectApi.MemberRolesUpdateInput
) {
  return requestClient.put<boolean>(`/app/project/members/${projectMemberId}/roles`, data);
}

export async function updateProjectIssueQualityLinkApi(
  issueId: string,
  data: ProjectApi.IssueQualityLinkInput
) {
  return requestClient.put<boolean>(`/app/project/issues/${issueId}/quality-link`, data);
}

export async function createProjectIssueApi(projectId: string, data: ProjectApi.IssueSaveInput) {
  return requestClient.post<string>(`/app/project/${projectId}/issues`, data);
}

export async function updateProjectIssueApi(issueId: string, data: ProjectApi.IssueSaveInput) {
  return requestClient.put<boolean>(`/app/project/issues/${issueId}`, data);
}

export async function deleteProjectIssueApi(issueId: string) {
  return requestClient.delete<boolean>(`/app/project/issues/${issueId}`);
}

export async function updateProjectTaskWbsApi(taskId: string, data: ProjectApi.TaskWbsUpdateInput) {
  return requestClient.put<boolean>(`/app/project/tasks/${taskId}/wbs`, data);
}

export async function updateProjectTaskApi(taskId: string, data: ProjectApi.TaskSaveInput) {
  return requestClient.put<boolean>(`/app/project/tasks/${taskId}`, data);
}

export async function recordProjectTaskProgressApi(
  taskId: string,
  data: ProjectApi.TaskProgressInput
) {
  return requestClient.post<string>(`/app/project/tasks/${taskId}/progress`, data);
}
