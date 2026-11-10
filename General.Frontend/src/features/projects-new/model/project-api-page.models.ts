import type { ListPageModel, ListPageRow, PageAction, PageModel, PageStat } from '@/components/page';
import type { ProjectApi } from '@/api/core/project';

export interface ProjectListRow extends ListPageRow {
  blockedTaskCount: number;
  completedTaskCount: number;
  highRiskCount: number;
  isKeyProject: boolean;
  managerName: string;
  memberCount: number;
  myRelation: string;
  name: string;
  organizationUnitName: string;
  overdueTaskCount: number;
  plannedEndDate: string;
  priority: string;
  projectCode: string;
  status: string;
  taskTotalCount: number;
}

export function buildProjectListPageModel(
  result: ProjectApi.PagedListResult | null,
  statuses: readonly string[],
  loading: boolean,
  error: string | null,
  canCreate = false,
  mode: 'center' | 'close' | 'init' = 'center',
): ListPageModel<ProjectListRow> {
  const pageDetails = {
    center: {
      label: '项目中心',
      description: '按当前账号的数据范围查看项目及任务、风险和计划信息。',
      path: '/projects-new/project-list',
      routeName: 'page.projects-new.project-list',
    },
    close: {
      label: '项目收尾',
      description: '从项目详情维护项目状态；收尾条件由后端验收规则校验。',
      path: '/projects-new/project-close',
      routeName: 'page.projects-new.project-close',
    },
    init: {
      label: '项目立项',
      description: '创建真实项目主档案并从待规划状态开始。',
      path: '/projects-new/project-init',
      routeName: 'page.projects-new.project-init',
    },
  }[mode];
  const rows = (result?.items ?? []).map((item): ProjectListRow => ({
    blockedTaskCount: item.blockedTaskCount,
    completedTaskCount: item.completedTaskCount,
    highRiskCount: item.highRiskCount,
    id: item.id,
    isKeyProject: item.isKeyProject,
    managerName: item.managerName,
    memberCount: item.memberCount,
    myRelation: item.myRelation,
    name: item.name,
    organizationUnitName: item.organizationUnitName,
    overdueTaskCount: item.overdueTaskCount,
    plannedEndDate: formatDate(item.plannedEndDate),
    priority: item.priority,
    projectCode: item.projectCode,
    status: item.status,
    taskTotalCount: item.taskTotalCount,
  }));

  return {
    key: '项目管理|项目列表',
    module: '项目执行',
    moduleSlug: 'projects-new',
    label: pageDetails.label,
    path: pageDetails.path,
    routeName: pageDetails.routeName,
    pageSlug: mode === 'center' ? 'project-list' : mode === 'init' ? 'project-init' : 'project-close',
    demoPath: '/pmx/projects',
    surface: 'list',
    description: pageDetails.description,
    searchPlaceholder: '搜索项目名称、编号、负责人或所属组织',
    tabs: ['全部项目'],
    tools: [],
    filters: [
      { key: 'status', label: '项目状态', options: ['全部状态', ...statuses] },
      { key: 'scope', label: '项目范围', options: ['全部项目', '我的相关项目'] },
    ],
    columns: [
      { key: 'projectCode', title: '项目编号', width: 150 },
      { key: 'name', title: '项目名称', width: 220 },
      { key: 'organizationUnitName', title: '所属组织', width: 150 },
      { key: 'managerName', title: '项目经理', width: 120 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
      { key: 'priority', title: '优先级', width: 90 },
      { key: 'taskTotalCount', title: '任务数', width: 90 },
      { key: 'completedTaskCount', title: '已完成', width: 90 },
      { key: 'blockedTaskCount', title: '阻塞任务', width: 100 },
      { key: 'highRiskCount', title: '高风险', width: 90 },
      { key: 'overdueTaskCount', title: '逾期任务', width: 100 },
      { key: 'plannedEndDate', title: '计划结束', width: 120 },
      { key: 'myRelation', title: '我的关系', width: 110 },
    ],
    stats: [
      { key: 'total', label: '项目总数', value: String(result?.totalCount ?? 0), tone: 'primary' },
      { key: 'key', label: '重点项目', value: String(result?.keyProjectCount ?? 0), tone: 'success' },
      { key: 'abnormal', label: '异常项目', value: String(result?.abnormalProjectCount ?? 0), tone: 'warning' },
      { key: 'related', label: '我的相关项目', value: String(result?.myRelatedProjectCount ?? 0), tone: 'neutral' },
    ],
    panels: [],
    rows,
    primaryAction: canCreate ? '新建项目' : '',
    pageSize: 20,
    emptyHint: loading ? '正在读取项目列表…' : error ? '项目列表读取失败，可重试。' : '当前条件下没有可见项目。',
    rowActions: ['view'],
    actions: [
      ...(canCreate && mode !== 'close'
        ? ([{ id: 'create', label: '新建项目', kind: 'create', tone: 'primary' }] satisfies PageAction[])
        : []),
      { id: 'refresh', label: '刷新', kind: 'refresh' },
      { id: 'export', label: '导出当前页', kind: 'export' },
    ],
    replication: { status: 'implemented', referenceUrl: 'https://demo.risemap.cn/pmx/projects' },
    listScopes: [{ id: 'all', label: '项目' }],
    listViews: [{ id: 'list', label: '列表' }],
    listTabs: [{ id: 'all', label: '全部项目' }],
  };
}

interface BoardRow {
  code: string;
  id: string;
  name: string;
  status: string;
  title: string;
  updatedAt: string;
  owner?: string;
  [key: string]: string | number | boolean | null | undefined;
}

const boardColumns = [
  { key: 'code', title: '编号 / 来源', width: 150 },
  { key: 'name', title: '项目 / 事项', width: 230 },
  { key: 'projectStatus', title: '项目状态', width: 100 },
  { key: 'owner', title: '负责人 / 责任人', width: 130 },
  { key: 'organizationUnitName', title: '所属组织', width: 140 },
  { key: 'healthStatus', title: '健康状态', width: 100 },
  { key: 'progress', title: '项目进度', width: 90 },
  { key: 'reason', title: '说明', width: 240 },
  { key: 'updatedAt', title: '日期', width: 120 },
];

function formatDate(value: null | string): string {
  return value ? value.slice(0, 10) : '—';
}

function metricStats(metrics: readonly ProjectApi.DashboardMetric[]): PageStat[] {
  return metrics.map((metric, index) => ({
    key: `metric-${index}`,
    label: metric.label,
    value: `${metric.value.toLocaleString('zh-CN')}${metric.unit ? ` ${metric.unit}` : ''}`,
    tone: index === 0 ? 'primary' : 'neutral',
    hint: metric.trend || undefined,
  }));
}

function countStat(key: string, label: string, value: number, tone: PageStat['tone'] = 'neutral'): PageStat {
  return { key, label, value: String(value), tone };
}

function boardPage(label: string, description: string, stats: PageStat[], rows: BoardRow[]): PageModel {
  return {
    key: '项目管理-新|项目看板',
    module: '项目执行',
    moduleSlug: 'projects-new',
    label,
    path: '/projects-new/project-board',
    routeName: 'page.projects-new.project-board',
    pageSlug: 'project-board',
    demoPath: '/pmx',
    surface: 'board',
    description,
    searchPlaceholder: '搜索项目或事项',
    tabs: ['总览'],
    tools: [],
    filters: [],
    columns: boardColumns,
    stats,
    panels: [],
    rows,
    primaryAction: '',
    rowActions: ['view'],
    emptyHint: '当前没有可显示的项目数据。',
    replication: { status: 'implemented', referenceUrl: 'https://demo.risemap.cn/pmx' },
  };
}

function projectBoardRow(
  item: Pick<ProjectApi.ListItem, 'id' | 'name' | 'projectCode' | 'status' | 'managerName' | 'organizationUnitName'> &
    Partial<Pick<ProjectApi.DashboardProjectCard, 'healthStatus' | 'progress' | 'overdueTaskCount' | 'highRiskCount'>>,
): BoardRow {
  return {
    code: item.projectCode,
    healthStatus: item.healthStatus ?? '—',
    id: `project-${item.id}`,
    name: item.name,
    organizationUnitName: item.organizationUnitName,
    owner: item.managerName,
    progress: item.progress === undefined ? '—' : `${item.progress}%`,
    projectStatus: item.status,
    status: '项目',
    title: item.name,
    updatedAt: '—',
  };
}

function actionBoardRow(item: ProjectApi.DashboardActionItem): BoardRow {
  return {
    code: item.sourceType,
    dueDate: formatDate(item.dueDate),
    id: `action-${item.sourceId}`,
    name: item.title,
    owner: item.projectName,
    projectStatus: item.status,
    reason: item.reason,
    status: '待处理',
    title: item.title,
    updatedAt: formatDate(item.dueDate),
  };
}

export function buildWorkspacePageModel(workspace: ProjectApi.Workspace): PageModel {
  const rows: BoardRow[] = [
    ...workspace.keyProjects.map(projectBoardRow),
    ...workspace.myTasks.map((item) => ({
      code: item.taskCode,
      id: `task-${item.id}`,
      name: item.title,
      owner: item.ownerName,
      projectStatus: item.status,
      reason: item.blockReason ?? '',
      status: '我的任务',
      title: item.title,
      updatedAt: formatDate(item.plannedEndTime),
    })),
    ...workspace.raidAlerts.map((item) => ({
      code: item.type,
      id: `raid-${item.id}`,
      name: item.title,
      owner: item.ownerName,
      projectStatus: item.status,
      reason: `${item.level} · ${item.projectName}`,
      status: '风险提醒',
      title: item.title,
      updatedAt: formatDate(item.plannedResolveDate),
    })),
    ...workspace.weeklyMilestones.map((item) => ({
      code: item.projectName,
      id: `milestone-${item.id}`,
      name: item.name,
      owner: item.ownerName,
      projectStatus: item.status,
      reason: item.projectName,
      status: '本周里程碑',
      title: item.name,
      updatedAt: formatDate(item.plannedCompletionDate),
    })),
  ];

  return boardPage(
    '项目工作台',
    '项目、任务、风险提醒与近期里程碑均来自当前账号可见的数据。',
    [
      countStat('ongoing', '进行中项目', workspace.ongoingProjectCount, 'primary'),
      countStat('todos', '我的待办', workspace.myTodoCount, 'warning'),
      countStat('blocked', '阻塞任务', workspace.blockedTaskCount, 'danger'),
      countStat('overdue', '逾期任务', workspace.overdueTaskCount, 'warning'),
      countStat('risk', '高风险事项', workspace.highRiskCount, 'danger'),
    ],
    rows,
  );
}

export function buildMyRelatedPageModel(dashboard: ProjectApi.MyRelatedDashboard): PageModel {
  const rows: BoardRow[] = [
    ...dashboard.myProjects.map((item) => ({
      code: item.projectCode,
      id: `project-${item.id}`,
      name: item.name,
      owner: item.myRelation,
      projectStatus: item.status,
      reason: `待处理问题 ${item.activeIssueCount} 项 · 近期任务 ${item.dueSoonTaskCount} 项`,
      status: '我的项目',
      title: item.name,
      updatedAt: item.nextSprintName,
      weekHours: item.weekHours,
    })),
    ...dashboard.myTasks.map((item) => ({
      code: item.taskCode,
      id: `task-${item.id}`,
      name: item.title,
      owner: item.ownerName,
      projectStatus: item.status,
      reason: item.projectName,
      status: '我的任务',
      title: item.title,
      updatedAt: formatDate(item.plannedEndTime),
    })),
    ...dashboard.myIssues.map((item) => ({
      code: item.type,
      id: `issue-${item.id}`,
      name: item.title,
      owner: item.ownerName,
      projectStatus: item.status,
      reason: `${item.level} · ${item.projectName}`,
      status: '我的问题',
      title: item.title,
      updatedAt: formatDate(item.dueDate),
    })),
    ...dashboard.actionItems.map(actionBoardRow),
    ...dashboard.recentWorklogs.map((item) => ({
      code: item.taskTitle,
      hours: item.hours,
      id: `worklog-${item.id}`,
      name: item.summary || item.projectName,
      owner: item.userName,
      projectStatus: item.projectName,
      status: '近期工时',
      title: item.summary || item.projectName,
      updatedAt: formatDate(item.workDate),
    })),
  ];

  return {
    ...boardPage('我的项目事项', '我的项目、任务、问题、待处理事项和工时记录。', metricStats(dashboard.metrics), rows),
    path: '/projects-new/my-projects',
    routeName: 'page.projects-new.my-projects',
    pageSlug: 'my-projects',
  };
}

export function buildManagerDashboardPageModel(dashboard: ProjectApi.PmDashboard | ProjectApi.PmoDashboard): PageModel {
  const rows: BoardRow[] = [
    ...dashboard.projects.map(projectBoardRow),
    ...dashboard.actionItems.map(actionBoardRow),
    ...dashboard.businessRisks.map((item) => ({
      code: item.riskLabel,
      id: `business-risk-${item.projectId}`,
      name: item.projectName,
      owner: item.businessCloseStatus,
      projectStatus: item.businessCloseStatus,
      reason: item.riskLabel,
      status: '经营预警',
      title: item.projectName,
      updatedAt: '—',
    })),
  ];

  return boardPage('项目组合看板', '健康、进度与待处理事项采用后端定义口径。', metricStats(dashboard.metrics), rows);
}
