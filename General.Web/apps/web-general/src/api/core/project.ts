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
    developerOwnerName?: null | string;
    dueDate?: null | string;
    id: string;
    isOverdue: boolean;
    level: string;
    ownerName: string;
    productOwnerName?: null | string;
    requirementTitle?: null | string;
    status: string;
    testerOwnerName?: null | string;
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

  export interface ProjectDetail {
    budgetTotalAmount?: null | number;
    calendarItems: CalendarItem[];
    contractTotalAmount?: null | number;
    cycles: CycleItem[];
    description: string;
    documents: DocumentItem[];
    highRiskCount: number;
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
    raidItems: RaidItem[];
    receivedAmount?: null | number;
    sponsorName: string;
    status: string;
    taskCompletionRate: number;
    tasks: TaskItem[];
    totalWorklogHours: number;
    worklogs: WorklogItem[];
  }

  export interface ProjectListInput {
    keyword?: string;
    onlyMyRelated?: boolean;
    status?: string;
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
    id: string;
    isOverdue: boolean;
    level: string;
    ownerName: string;
    plannedResolveDate?: null | string;
    projectId: string;
    projectName: string;
    status: string;
    title: string;
    type: string;
  }

  export interface RaidListInput {
    keyword?: string;
    onlyOpen?: boolean;
    type?: string;
  }

  export interface TaskItem {
    actualEndTime?: null | string;
    blockReason?: null | string;
    contractClause?: null | string;
    developerOwnerName?: null | string;
    id: string;
    isBlocked: boolean;
    organizationUnitName: string;
    ownerName: string;
    plannedEndTime?: null | string;
    plannedStartTime?: null | string;
    priority: string;
    productOwnerName?: null | string;
    projectId: string;
    projectName: string;
    status: string;
    taskCode: string;
    testerOwnerName?: null | string;
    title: string;
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
