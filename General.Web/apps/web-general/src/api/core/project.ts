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

  export interface CycleItem {
    endDate?: null | string;
    id: string;
    name: string;
    ownerName: string;
    progress: number;
    startDate?: null | string;
    status: string;
    summary: string;
    type: string;
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
    developerOwnerName?: null | string;
    developerOwnerUserId?: null | string;
    dueDate?: null | string;
    id: string;
    isOverdue: boolean;
    level: string;
    ownerName: string;
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

  export interface MyRelatedItem {
    activeIssueCount: number;
    dueSoonTaskCount: number;
    id: string;
    myRelation: string;
    name: string;
    nextCycleName: string;
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

  export interface Charter {
    approvalStatus: string;
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

  export interface ProjectDetail {
    acceptanceCriteria: AcceptanceCriteriaItem[];
    budgetTotalAmount?: null | number;
    calendarItems: CalendarItem[];
    charter?: Charter | null;
    contractTotalAmount?: null | number;
    cycles: CycleItem[];
    description: string;
    documents: DocumentItem[];
    highRiskCount: number;
    healthRiskScore: number;
    id: string;
    isKeyProject: boolean;
    issues: IssueItem[];
    managerName: string;
    memberCount: number;
    members: MemberItem[];
    milestones: WorkspaceMilestoneItem[];
    myRelation: string;
    name: string;
    openIssueCount: number;
    organizationUnitName: string;
    plannedEndDate?: null | string;
    plannedStartDate?: null | string;
    priority: string;
    projectCode: string;
    projectSource: string;
    projectType: string;
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
    worklogs: WorklogItem[];
  }

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
    parentTaskId?: string;
    sortOrder: number;
    wbsCode: string;
  }

  export interface TaskItem {
    actualEndTime?: null | string;
    baselineEndTime?: null | string;
    baselineStartTime?: null | string;
    blockReason?: null | string;
    contractClause?: null | string;
    developerOwnerName?: null | string;
    developerOwnerUserId?: null | string;
    id: string;
    isBlocked: boolean;
    organizationUnitName: string;
    ownerName: string;
    parentTaskId?: null | string;
    plannedEndTime?: null | string;
    plannedStartTime?: null | string;
    priority: string;
    productOwnerName?: null | string;
    productOwnerUserId?: null | string;
    projectId: string;
    projectName: string;
    status: string;
    taskCode: string;
    testerOwnerName?: null | string;
    testerOwnerUserId?: null | string;
    title: string;
    sortOrder: number;
    wbsCode: string;
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
    id: string;
    name: string;
    ownerName: string;
    plannedCompletionDate: string;
    projectId: string;
    projectName: string;
    status: string;
  }
}

export async function getProjectWorkspaceApi() {
  return requestClient.get<ProjectApi.WorkspaceItem>('/app/project/workspace');
}

export async function getProjectListApi(params?: ProjectApi.ProjectListInput) {
  return requestClient.get<ProjectApi.ProjectListItem[]>('/app/project/list', {
    params,
  });
}

export async function getProjectDetailApi(projectId: string) {
  return requestClient.get<ProjectApi.ProjectDetail>(`/app/project/detail/${projectId}`);
}

export async function createProjectApi(data: ProjectApi.ProjectSaveInput) {
  return requestClient.post<string>('/app/project', data);
}

export async function createProjectAcceptanceCriteriaApi(
  projectId: string,
  data: ProjectApi.AcceptanceCriteriaSaveInput,
) {
  return requestClient.post<string>(`/app/project/${projectId}/acceptance-criteria`, data);
}

export async function createProjectQualityChecklistApi(
  projectId: string,
  data: ProjectApi.QualityChecklistSaveInput,
) {
  return requestClient.post<string>(`/app/project/${projectId}/quality-checklists`, data);
}

export async function createProjectQualityInspectionApi(
  projectId: string,
  data: ProjectApi.QualityInspectionSaveInput,
) {
  return requestClient.post<string>(`/app/project/${projectId}/quality-inspections`, data);
}

export async function createProjectTaskDependencyApi(
  projectId: string,
  data: ProjectApi.TaskDependencySaveInput,
) {
  return requestClient.post<string>(`/app/project/${projectId}/task-dependencies`, data);
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

export async function updateProjectRaidRiskApi(raidItemId: string, data: ProjectApi.RaidRiskUpdateInput) {
  return requestClient.put<boolean>(`/app/project/raid/${raidItemId}/risk-assessment`, data);
}

export async function updateProjectMemberRolesApi(
  projectMemberId: string,
  data: ProjectApi.MemberRolesUpdateInput,
) {
  return requestClient.put<boolean>(`/app/project/members/${projectMemberId}/roles`, data);
}

export async function updateProjectIssueQualityLinkApi(
  issueId: string,
  data: ProjectApi.IssueQualityLinkInput,
) {
  return requestClient.put<boolean>(`/app/project/issues/${issueId}/quality-link`, data);
}

export async function updateProjectTaskWbsApi(taskId: string, data: ProjectApi.TaskWbsUpdateInput) {
  return requestClient.put<boolean>(`/app/project/tasks/${taskId}/wbs`, data);
}
