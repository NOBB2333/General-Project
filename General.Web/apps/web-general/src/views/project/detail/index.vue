<script lang="ts" setup>
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch, type ComponentPublicInstance } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import {
  Alert,
  Button,
  Card,
  Col,
  Empty,
  Progress,
  Row,
  Select,
  Space,
  Table,
  Tabs,
  Tag,
} from 'ant-design-vue';

import { getProjectDetailApi, getProjectListApi, getUserInfoApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectDetailPage' });

const DETAIL_PATH = '/project/detail';
const STORAGE_KEY = 'general:last-project-id';

const route = useRoute();
const router = useRouter();

const activeTab = ref('overview');
const detail = ref<null | ProjectApi.ProjectDetail>(null);
const currentUserName = ref('');
const currentUsername = ref('');
const loading = ref(false);
const loadingProjects = ref(false);
const projectOptions = ref<ProjectApi.ProjectListItem[]>([]);
const selectedProjectId = ref<string | undefined>(undefined);
const taskScope = ref<'all' | 'related'>('related');
const taskView = ref<'board' | 'calendar' | 'gantt' | 'list'>('list');
const taskCalendarAnchorDate = ref<Date | null>(null);
const tabContentElement = ref<HTMLElement | null>(null);
const tabBodyHeight = ref<number>(520);
let tabContentObserver: null | ResizeObserver = null;

const taskColumns = [
  { dataIndex: 'taskCode', key: 'taskCode', title: '事项编号', width: 120 },
  { dataIndex: 'title', key: 'title', title: '事项标题', minWidth: 280 },
  { dataIndex: 'contractClause', key: 'contractClause', title: '关联合同条款', width: 180 },
  { dataIndex: 'productOwnerName', key: 'productOwnerName', title: '产品', width: 120 },
  { dataIndex: 'developerOwnerName', key: 'developerOwnerName', title: '开发', width: 120 },
  { dataIndex: 'testerOwnerName', key: 'testerOwnerName', title: '测试', width: 120 },
  { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
  { dataIndex: 'plannedEndTime', key: 'plannedEndTime', title: '计划完成', width: 140 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];
const cycleColumns = [
  { dataIndex: 'type', key: 'type', title: '类型', width: 100 },
  { dataIndex: 'name', key: 'name', title: '名称' },
  { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
  { dataIndex: 'progress', key: 'progress', title: '进度', width: 180 },
  { dataIndex: 'endDate', key: 'endDate', title: '结束日期', width: 140 },
];
const milestoneColumns = [
  { dataIndex: 'name', key: 'name', title: '里程碑' },
  { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
  { dataIndex: 'plannedCompletionDate', key: 'plannedCompletionDate', title: '计划完成', width: 140 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];
const issueColumns = [
  { dataIndex: 'title', key: 'title', title: '问题项' },
  { dataIndex: 'requirementTitle', key: 'requirementTitle', title: '需求', width: 180 },
  { dataIndex: 'productOwnerName', key: 'productOwnerName', title: '产品', width: 120 },
  { dataIndex: 'developerOwnerName', key: 'developerOwnerName', title: '开发', width: 120 },
  { dataIndex: 'testerOwnerName', key: 'testerOwnerName', title: '测试', width: 120 },
  { dataIndex: 'type', key: 'type', title: '类型', width: 100 },
  { dataIndex: 'level', key: 'level', title: '等级', width: 100 },
  { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
  { dataIndex: 'dueDate', key: 'dueDate', title: '到期', width: 140 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];
const documentColumns = [
  { dataIndex: 'category', key: 'category', title: '分类', width: 100 },
  { dataIndex: 'title', key: 'title', title: '文档标题' },
  { dataIndex: 'version', key: 'version', title: '版本', width: 100 },
  { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];
const memberColumns = [
  { dataIndex: 'userName', key: 'userName', title: '成员' },
  { dataIndex: 'organizationUnitName', key: 'organizationUnitName', title: '所属部门', width: 140 },
  { dataIndex: 'roleNames', key: 'roleNames', title: '项目角色', width: 220 },
  { dataIndex: 'accessLevel', key: 'accessLevel', title: '访问级别', width: 120 },
  { dataIndex: 'joinDate', key: 'joinDate', title: '加入时间', width: 140 },
];
const worklogColumns = [
  { dataIndex: 'workDate', key: 'workDate', title: '日期', width: 140 },
  { dataIndex: 'userName', key: 'userName', title: '成员', width: 120 },
  { dataIndex: 'taskTitle', key: 'taskTitle', title: '关联事项' },
  { dataIndex: 'summary', key: 'summary', title: '工作内容' },
  { dataIndex: 'hours', key: 'hours', title: '工时', width: 90 },
];
const reportColumns = [
  { dataIndex: 'category', key: 'category', title: '类型', width: 100 },
  { dataIndex: 'title', key: 'title', title: '报告标题' },
  { dataIndex: 'version', key: 'version', title: '版本', width: 100 },
  { dataIndex: 'ownerName', key: 'ownerName', title: '负责人', width: 120 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];

const tabItems = [
  { key: 'overview', label: '项目详细信息' },
  { key: 'plan', label: '计划与周期' },
  { key: 'tasks', label: '需求与任务' },
  { key: 'issues', label: '缺陷与问题' },
  { key: 'docs', label: '文档与知识' },
  { key: 'resources', label: '人员与资源' },
];
const taskScopeOptions = [
  { key: 'related', label: '与我相关' },
  { key: 'all', label: '全部' },
] as const;
const taskViewOptions = [
  { key: 'list', label: '列表' },
  { key: 'board', label: '看板' },
  { key: 'gantt', label: '甘特' },
  { key: 'calendar', label: '日历' },
] as const;
const kanbanOrder = ['未开始', '进行中', '阻塞', '已完成', '已关闭'];

const currentUserAliases = computed(() =>
  [currentUserName.value, currentUsername.value].filter((item) => item.trim().length > 0),
);

const compactMetrics = computed(() => {
  const current = detail.value;
  return [
    { label: '健康度', value: resolveHealthStatus().label },
    { label: '任务完成率', value: `${current?.taskCompletionRate || 0}%` },
    { label: '待处理问题', value: `${current?.openIssueCount || 0}` },
    { label: '成员 / 工时', value: `${current?.memberCount || 0} / ${current?.totalWorklogHours || 0}小时` },
  ];
});

const reportItems = computed(() =>
  (detail.value?.documents || []).filter((item) =>
    ['日报', '周报', '月报', '阶段报告', '发布报告'].some((keyword) => item.category.includes(keyword) || item.title.includes(keyword)),
  ),
);

const knowledgeCollections = computed(() => {
  const docs = detail.value?.documents || [];
  const groups = new Map<string, ProjectApi.DocumentItem[]>();

  for (const doc of docs) {
    const key = doc.category || '未分类';
    const items = groups.get(key) || [];
    items.push(doc);
    groups.set(key, items);
  }

  return Array.from(groups.entries()).map(([name, items]) => ({
    name,
    count: items.length,
    description: items[0]?.summary || '沉淀该分类下的标准文档、评论记录与知识要点。',
    items,
  }));
});

const featuredDocuments = computed(() => (detail.value?.documents || []).slice(0, 6));

const issueMetrics = computed(() => {
  const items = detail.value?.issues || [];
  return [
    { label: '当前问题总数', value: items.length },
    { label: '高优先级', value: items.filter((item) => item.level === '高').length },
    { label: '待测试验证', value: items.filter((item) => item.testerOwnerName).length },
    { label: '已关联需求', value: items.filter((item) => item.requirementTitle).length },
  ];
});

const riskAlerts = computed(() =>
  (detail.value?.raidItems || [])
    .filter((item) => item.level === '高' || item.isOverdue)
    .slice(0, 4),
);

const currentHealth = computed(() => resolveHealthStatus());

const relatedTaskCount = computed(() =>
  (detail.value?.tasks || []).filter((item) => isTaskRelatedToCurrentUser(item)).length,
);

const relatedIssueCount = computed(() =>
  (detail.value?.issues || []).filter((item) =>
    [item.ownerName, item.productOwnerName, item.developerOwnerName, item.testerOwnerName]
      .filter((name): name is string => Boolean(name))
      .some((name) => matchesCurrentUser(name)),
  ).length,
);

const relatedWorklogCount = computed(() =>
  (detail.value?.worklogs || []).filter((item) => matchesCurrentUser(item.userName)).length,
);

const overviewDetailSections = computed(() => {
  const current = detail.value;

  return [
    {
      items: [
        { label: '项目编号', value: current?.projectCode || '-' },
        { label: '项目类型', value: current?.projectType || '-' },
        { label: '项目来源', value: current?.projectSource || '-' },
        { label: '所属部门', value: current?.organizationUnitName || '-' },
        { label: '项目经理', value: current?.managerName || '-' },
        { label: '发起人', value: current?.sponsorName || '-' },
      ],
      title: '基础信息',
    },
    {
      items: [
        { label: '计划开始', value: formatDate(current?.plannedStartDate) },
        { label: '计划结束', value: formatDate(current?.plannedEndDate) },
        { label: '合同金额', value: formatMoney(current?.contractTotalAmount) },
        { label: '预算金额', value: formatMoney(current?.budgetTotalAmount) },
        { label: '已回款', value: formatMoney(current?.receivedAmount) },
        { label: '关键项目', value: current?.isKeyProject ? '是' : '否' },
      ],
      title: '计划与经营',
    },
  ];
});

const healthMetrics = computed(() => {
  const current = detail.value;
  return [
    { label: '任务完成率', value: `${current?.taskCompletionRate || 0}%` },
    { label: '高风险事项', value: `${current?.highRiskCount || 0} 项` },
    { label: '未闭环问题', value: `${current?.openIssueCount || 0} 项` },
  ];
});

const collaborationMetrics = computed(() => {
  const current = detail.value;
  return [
    { label: '我的项目关系', value: current?.myRelation || '-' },
    { label: '我负责事项', value: `${relatedTaskCount.value}` },
    { label: '我跟进问题', value: `${relatedIssueCount.value}` },
    { label: '我的工时记录', value: `${relatedWorklogCount.value}` },
    { label: '成员规模', value: `${current?.memberCount || 0} 人` },
    { label: '累计工时', value: `${current?.totalWorklogHours || 0} 小时` },
  ];
});

const milestonePreview = computed(() => (detail.value?.milestones || []).slice(0, 3));
const cyclePreview = computed(() => (detail.value?.cycles || []).slice(0, 3));

const filteredTasks = computed(() => {
  const tasks = detail.value?.tasks || [];
  if (taskScope.value === 'all') {
    return tasks;
  }

  return tasks.filter((item) => isTaskRelatedToCurrentUser(item));
});

const kanbanColumns = computed(() =>
  kanbanOrder.map((status) => ({
    items: filteredTasks.value.filter((item) => item.status === status),
    status,
  })),
);

const taskTimelineDays = computed(() => {
  const items = filteredTasks.value.filter((item) => item.plannedStartTime || item.plannedEndTime);
  if (items.length === 0) {
    return [];
  }

  const start = new Date(
    Math.min(
      ...items.map((item) => new Date(item.plannedStartTime || item.plannedEndTime || new Date()).getTime()),
    ),
  );
  const end = new Date(
    Math.max(
      ...items.map((item) => new Date(item.plannedEndTime || item.plannedStartTime || new Date()).getTime()),
    ),
  );
  const diff = Math.min(21, Math.max(6, Math.ceil((end.getTime() - start.getTime()) / 86_400_000)));

  return Array.from({ length: diff + 1 }, (_, index) => {
    const date = new Date(start);
    date.setDate(start.getDate() + index);
    return date;
  });
});

const taskTimelineGridStyle = computed(() => ({
  gridTemplateColumns: `repeat(${Math.max(taskTimelineDays.value.length, 1)}, minmax(56px, 1fr))`,
}));

const taskTimelineRows = computed(() => {
  const days = taskTimelineDays.value;
  if (days.length === 0) {
    return [];
  }

  const firstDay = days[0]?.setHours(0, 0, 0, 0) ?? Date.now();
  const lastDay = days[days.length - 1]?.setHours(0, 0, 0, 0) ?? firstDay;

  return filteredTasks.value
    .filter((item) => item.plannedStartTime || item.plannedEndTime)
    .map((item) => {
      const startDate = new Date(item.plannedStartTime || item.plannedEndTime || new Date()).setHours(0, 0, 0, 0);
      const endDate = new Date(item.plannedEndTime || item.plannedStartTime || new Date()).setHours(0, 0, 0, 0);
      const clampedStart = Math.max(startDate, firstDay);
      const clampedEnd = Math.min(endDate, lastDay);
      return {
        ...item,
        color: resolveTaskColor(item),
        span: Math.max(1, Math.floor((clampedEnd - clampedStart) / 86_400_000) + 1),
        startOffset: Math.floor((clampedStart - firstDay) / 86_400_000),
      };
    });
});

const taskCalendarReferenceDate = computed(() => {
  if (taskCalendarAnchorDate.value) {
    return taskCalendarAnchorDate.value;
  }

  const firstTaskDate = filteredTasks.value
    .flatMap((item) => [item.plannedStartTime, item.plannedEndTime])
    .find((value) => Boolean(value));

  return parseDate(firstTaskDate) || new Date();
});

const taskCalendarTitle = computed(() => {
  const current = taskCalendarReferenceDate.value;
  return `${current.getFullYear()} 年 ${current.getMonth() + 1} 月`;
});

const taskCalendarRows = computed(() => {
  const current = taskCalendarReferenceDate.value;
  const monthStart = new Date(current.getFullYear(), current.getMonth(), 1);
  const monthEnd = new Date(current.getFullYear(), current.getMonth() + 1, 0);
  const rangeStart = new Date(monthStart);
  rangeStart.setDate(monthStart.getDate() - monthStart.getDay());
  const rangeEnd = new Date(monthEnd);
  rangeEnd.setDate(monthEnd.getDate() + (6 - monthEnd.getDay()));
  const todayKey = toDateKey(new Date());
  const rows = [] as Array<{
    key: string;
    label: string;
    days: Array<{ date: Date; dayLabel: number; inCurrentMonth: boolean; isToday: boolean; key: string }>;
    bars: Array<{
      key: string;
      title: string;
      subtitle: string;
      color: string;
      startColumn: number;
      span: number;
    }>;
  }>;

  for (let cursor = new Date(rangeStart); cursor <= rangeEnd; cursor.setDate(cursor.getDate() + 7)) {
    const weekStart = new Date(cursor);
    const weekEnd = new Date(cursor);
    weekEnd.setDate(weekStart.getDate() + 6);
    const days = Array.from({ length: 7 }, (_, index) => {
      const date = new Date(weekStart);
      date.setDate(weekStart.getDate() + index);
      return {
        date,
        dayLabel: date.getDate(),
        inCurrentMonth: date.getMonth() === current.getMonth(),
        isToday: toDateKey(date) === todayKey,
        key: toDateKey(date),
      };
    });

    const bars = filteredTasks.value
      .filter((item) => {
        const start = parseDate(item.plannedStartTime || item.plannedEndTime);
        const end = parseDate(item.plannedEndTime || item.plannedStartTime);
        return start && end && start <= weekEnd && end >= weekStart;
      })
      .map((item) => {
        const start = parseDate(item.plannedStartTime || item.plannedEndTime) || weekStart;
        const end = parseDate(item.plannedEndTime || item.plannedStartTime) || weekEnd;
        const startColumn = Math.max(0, Math.floor((start.setHours(0, 0, 0, 0) - weekStart.setHours(0, 0, 0, 0)) / 86_400_000));
        const endColumn = Math.min(6, Math.floor((end.setHours(0, 0, 0, 0) - weekStart.setHours(0, 0, 0, 0)) / 86_400_000));
        return {
          key: item.id,
          title: item.title,
          subtitle: `${item.taskCode} · ${item.ownerName}`,
          color: resolveTaskColor(item),
          startColumn: Math.max(0, startColumn),
          span: Math.max(1, endColumn - Math.max(0, startColumn) + 1),
        };
      })
      .sort((left, right) => left.startColumn - right.startColumn || right.span - left.span);

    rows.push({
      key: `${toDateKey(weekStart)}-${toDateKey(weekEnd)}`,
      label: `${weekStart.getMonth() + 1}/${weekStart.getDate()} - ${weekEnd.getMonth() + 1}/${weekEnd.getDate()}`,
      days,
      bars,
    });
  }

  return rows;
});

const workflowPreviewGroups = computed(() => {
  const issues = detail.value?.issues || [];
  const closedKeywords = ['关闭', '完成', '解决'];
  return [
    {
      key: 'defect-flow',
      title: '缺陷处理流',
      tone: 'danger',
      steps: [
        { key: 'capture', title: '登记', count: issues.filter((item) => item.type.includes('缺陷')).length },
        { key: 'analysis', title: '分析', count: issues.filter((item) => item.type.includes('缺陷') && item.productOwnerName).length },
        { key: 'fix', title: '修复', count: issues.filter((item) => item.type.includes('缺陷') && item.developerOwnerName).length },
        { key: 'verify', title: '验证', count: issues.filter((item) => item.type.includes('缺陷') && item.testerOwnerName).length },
        { key: 'done', title: '关闭', count: issues.filter((item) => item.type.includes('缺陷') && closedKeywords.some((keyword) => item.status.includes(keyword))).length },
      ],
    },
    {
      key: 'issue-flow',
      title: '问题跟踪流',
      tone: 'warning',
      steps: [
        { key: 'raise', title: '提出', count: issues.filter((item) => item.type.includes('问题')).length },
        { key: 'scope', title: '定责', count: issues.filter((item) => item.type.includes('问题') && item.requirementTitle).length },
        { key: 'coordination', title: '协同', count: issues.filter((item) => item.type.includes('问题') && item.productOwnerName).length },
        { key: 'tracking', title: '跟踪', count: issues.filter((item) => item.type.includes('问题') && item.developerOwnerName).length },
        { key: 'archive', title: '归档', count: issues.filter((item) => item.type.includes('问题') && closedKeywords.some((keyword) => item.status.includes(keyword))).length },
      ],
    },
    {
      key: 'change-flow',
      title: '变更评审流',
      tone: 'primary',
      steps: [
        { key: 'collect', title: '收集', count: issues.length },
        { key: 'requirement', title: '需求确认', count: issues.filter((item) => item.requirementTitle).length },
        { key: 'product', title: '产品评审', count: issues.filter((item) => item.productOwnerName).length },
        { key: 'develop', title: '开发排期', count: issues.filter((item) => item.developerOwnerName).length },
        { key: 'release', title: '发布闭环', count: issues.filter((item) => closedKeywords.some((keyword) => item.status.includes(keyword))).length },
      ],
    },
  ];
});

const tabBodyStyle = computed(() => ({
  height: `${tabBodyHeight.value}px`,
}));

const raciRows = computed(() =>
  (detail.value?.members || []).map((item) => ({
    accessLevel: item.accessLevel,
    approve: item.roleNames.includes('PM') || item.roleNames.includes('PMO') ? 'A' : '-',
    consult: item.roleNames.includes('运维') || item.roleNames.includes('测试') ? 'C' : '-',
    execute: item.roleNames.some((role) => ['开发', '实施', '测试', '运维'].includes(role)) ? 'R' : '-',
    inform: 'I',
    roleNames: item.roleNames.join(' / '),
    userName: item.userName,
  })),
);

async function loadProjects() {
  loadingProjects.value = true;
  try {
    const [projects, userInfo] = await Promise.all([getProjectListApi(), getUserInfoApi()]);
    const currentUser = userInfo as unknown as { realName?: string; username?: string };
    projectOptions.value = projects;
    currentUserName.value = currentUser.realName || '';
    currentUsername.value = currentUser.username || '';
    syncSelectedProject();
  } finally {
    loadingProjects.value = false;
  }
}

async function loadDetail(projectId: string) {
  loading.value = true;
  try {
    detail.value = await getProjectDetailApi(projectId);
    taskCalendarAnchorDate.value = null;
  } finally {
    loading.value = false;
    await nextTick();
    syncTabBodyHeight();
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

function formatMoney(value?: null | number) {
  return typeof value === 'number' ? value.toLocaleString('zh-CN') : '-';
}

function openDocumentWorkspace(item: ProjectApi.DocumentItem) {
  const docWindow = window.open('', '_blank');
  if (!docWindow) {
    return;
  }

  const safeTitle = item.title || '未命名文档';
  const safeSummary = item.summary || '暂无文档内容，可先在这里补充草稿。';
  const html = `<!doctype html>
  <html lang="zh-CN">
    <head>
      <meta charset="UTF-8" />
      <title>${safeTitle}</title>
      <style>
        body { margin: 0; font-family: Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #f6f8fb; color: #1f2937; }
        .shell { max-width: 1080px; margin: 0 auto; padding: 32px; }
        .meta { display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 16px; }
        .tag { padding: 6px 12px; border-radius: 999px; background: #e8f0ff; color: #2563eb; font-size: 12px; }
        .editor { min-height: 72vh; padding: 28px; border-radius: 20px; background: #fff; box-shadow: 0 20px 60px rgba(15, 23, 42, 0.08); line-height: 1.75; }
        .editor[contenteditable='true']:focus { outline: 2px solid rgba(37, 99, 235, 0.25); }
        h1 { margin: 0 0 12px; font-size: 32px; }
        p { margin: 0; white-space: pre-wrap; }
      </style>
    </head>
    <body>
      <div class="shell">
        <div class="meta">
          <span class="tag">${item.category || '知识文档'}</span>
          <span class="tag">${item.version || 'v1.0'}</span>
          <span class="tag">${item.status || '草稿'}</span>
          <span class="tag">${item.ownerName || '未指定负责人'}</span>
        </div>
        <div class="editor" contenteditable="true">
          <h1>${safeTitle}</h1>
          <p>${safeSummary}</p>
        </div>
      </div>
    </body>
  </html>`;
  docWindow.location.href = URL.createObjectURL(new Blob([html], { type: 'text/html;charset=utf-8' }));
}

function parseDate(value?: null | string) {
  if (!value) {
    return null;
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date;
}

function toDateKey(date: Date) {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, '0');
  const day = `${date.getDate()}`.padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function matchesCurrentUser(name: string) {
  return currentUserAliases.value.some((alias) => alias === name);
}

function isTaskRelatedToCurrentUser(item: ProjectApi.TaskItem) {
  return [item.ownerName, item.productOwnerName, item.developerOwnerName, item.testerOwnerName]
    .filter((name): name is string => Boolean(name))
    .some((name) => matchesCurrentUser(name));
}

function resolveHealthStatus() {
  const current = detail.value;
  if (!current) {
    return { color: 'default', label: '-', percent: 0 };
  }

  const riskScore = current.highRiskCount * 20 + current.openIssueCount * 10 + Math.max(0, 100 - current.taskCompletionRate);
  if (riskScore >= 70) {
    return { color: 'error', label: '高风险', percent: Math.max(20, 100 - riskScore) };
  }

  if (riskScore >= 35) {
    return { color: 'warning', label: '关注中', percent: Math.max(35, 100 - riskScore) };
  }

  return { color: 'success', label: '健康', percent: Math.max(60, 100 - riskScore) };
}

function resolveTaskColor(item: ProjectApi.TaskItem) {
  if (item.isBlocked) {
    return 'var(--ant-color-error)';
  }

  if (item.priority === '高') {
    return 'var(--ant-color-warning)';
  }

  if (item.status === '已完成' || item.status === '已关闭') {
    return 'var(--ant-color-success)';
  }

  return 'var(--ant-color-primary)';
}

function shiftTaskCalendarMonth(offset: number) {
  const base = taskCalendarReferenceDate.value;
  taskCalendarAnchorDate.value = new Date(base.getFullYear(), base.getMonth() + offset, 1);
}

function disconnectTabObserver() {
  tabContentObserver?.disconnect();
  tabContentObserver = null;
}

function syncTabBodyHeight() {
  if (!tabContentElement.value) {
    return;
  }

  tabBodyHeight.value = Math.max(520, tabContentElement.value.scrollHeight);
}

function bindTabContent(element: ComponentPublicInstance | Element | null) {
  disconnectTabObserver();
  tabContentElement.value = ((element as ComponentPublicInstance | null)?.$el ?? element) as HTMLElement | null;

  if (!tabContentElement.value) {
    return;
  }

  if (typeof ResizeObserver !== 'undefined') {
    tabContentObserver = new ResizeObserver(() => syncTabBodyHeight());
    tabContentObserver.observe(tabContentElement.value);
  }

  syncTabBodyHeight();
}

function syncSelectedProject() {
  if (route.path !== DETAIL_PATH) {
    return;
  }

  const routeProjectId = typeof route.query.projectId === 'string' ? route.query.projectId : null;
  const storedProjectId = localStorage.getItem(STORAGE_KEY);
  const candidate = routeProjectId || storedProjectId;

  if (!candidate) {
    selectedProjectId.value = undefined;
    detail.value = null;
    return;
  }

  const exists = projectOptions.value.some((item) => item.id === candidate);
  if (!exists) {
    selectedProjectId.value = undefined;
    detail.value = null;
    localStorage.removeItem(STORAGE_KEY);
    return;
  }

  selectedProjectId.value = candidate;
  localStorage.setItem(STORAGE_KEY, candidate);

  if (routeProjectId !== candidate) {
    router.replace({
      path: DETAIL_PATH,
      query: {
        projectId: candidate,
      },
    });
  }

  loadDetail(candidate);
}

async function handleProjectChange(projectId: string | undefined) {
  if (!projectId) {
    return;
  }

  selectedProjectId.value = projectId;
  localStorage.setItem(STORAGE_KEY, projectId);
  await router.replace({
    path: DETAIL_PATH,
    query: {
      projectId,
    },
  });
  await loadDetail(projectId);
}

watch(
  () => route.fullPath,
  () => {
    if (route.path === DETAIL_PATH && projectOptions.value.length > 0) {
      syncSelectedProject();
    }
  },
);

watch(
  () => activeTab.value,
  async () => {
    await nextTick();
    syncTabBodyHeight();
  },
  { flush: 'post' },
);

watch(
  () => taskView.value,
  async () => {
    await nextTick();
    syncTabBodyHeight();
  },
  { flush: 'post' },
);

onMounted(loadProjects);
onBeforeUnmount(disconnectTabObserver);
</script>

<template>
  <Page description="" title="项目详情">
    <section class="project-detail">
      <Card :bordered="false" class="project-detail__compact-header">
        <div class="project-detail__compact-main">
          <div class="project-detail__compact-copy">
            <div class="project-detail__meta-line">
              <Tag color="processing">项目详情</Tag>
              <Tag v-if="detail?.isKeyProject" color="gold">重点项目</Tag>
              <Tag color="blue">{{ detail?.myRelation || '请选择项目' }}</Tag>
            </div>
            <h2>{{ detail?.name || '选择一个项目开始查看' }}</h2>
            <div class="project-detail__meta-pills">
              <span>{{ detail?.projectCode || '-' }}</span>
              <span>{{ detail?.organizationUnitName || '-' }}</span>
              <span>{{ detail?.managerName || '-' }}</span>
              <span>{{ formatDate(detail?.plannedStartDate) }} - {{ formatDate(detail?.plannedEndDate) }}</span>
            </div>
          </div>
          <Select
            v-model:value="selectedProjectId"
            class="project-detail__selector"
            :loading="loadingProjects"
            :options="projectOptions.map((item) => ({ label: `${item.projectCode} · ${item.name}`, value: item.id }))"
            placeholder="切换项目"
            show-search
            @change="(value) => handleProjectChange(typeof value === 'string' ? value : undefined)"
          />
        </div>
        <div class="project-detail__compact-stats">
          <article v-for="metric in compactMetrics" :key="metric.label" class="project-detail__compact-stat">
            <span>{{ metric.label }}</span>
            <strong>{{ metric.value }}</strong>
          </article>
        </div>
        <Tabs v-if="selectedProjectId" v-model:activeKey="activeTab" class="project-detail__tabs" size="large">
          <Tabs.TabPane v-for="tab in tabItems" :key="tab.key" :tab="tab.label" />
        </Tabs>
      </Card>

      <div v-if="!selectedProjectId" class="project-detail__empty">
        <Empty description="请选择项目" />
      </div>

      <div v-else class="project-detail__tab-shell" :style="tabBodyStyle">
        <div :ref="bindTabContent">
            <template v-if="activeTab === 'overview'">
              <section class="project-detail__section">
                <div class="project-detail__overview-hero">
                  <div class="project-detail__overview-hero-main">
                    <span class="project-detail__eyebrow">项目总览</span>
                    <h3>{{ detail?.name || '项目详细信息' }}</h3>
                    <div class="project-detail__overview-meta">
                      <span>{{ detail?.projectCode || '-' }}</span>
                      <span>{{ detail?.organizationUnitName || '-' }}</span>
                      <span>{{ detail?.managerName || '-' }}</span>
                      <span>{{ formatDate(detail?.plannedStartDate) }} - {{ formatDate(detail?.plannedEndDate) }}</span>
                    </div>
                  </div>
                  <div class="project-detail__overview-kpis">
                    <article class="project-detail__overview-kpi">
                      <span>项目状态</span>
                      <strong>{{ detail?.status || '-' }}</strong>
                    </article>
                    <article class="project-detail__overview-kpi">
                      <span>优先级</span>
                      <strong>{{ detail?.priority || '-' }}</strong>
                    </article>
                    <article class="project-detail__overview-kpi">
                      <span>成员规模</span>
                      <strong>{{ detail?.memberCount || 0 }} 人</strong>
                    </article>
                  </div>
                </div>
                <div class="project-detail__overview-grid project-detail__overview-grid--cards">
                  <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card project-detail__overview-card--detail" title="项目详细信息">
                    <div class="project-detail__detail-sections">
                      <section v-for="section in overviewDetailSections" :key="section.title" class="project-detail__detail-section">
                        <header>{{ section.title }}</header>
                        <div class="project-detail__info-grid">
                          <div v-for="item in section.items" :key="item.label" class="project-detail__info-item">
                            <span>{{ item.label }}</span>
                            <strong>{{ item.value }}</strong>
                          </div>
                        </div>
                      </section>
                    </div>
                    <div class="project-detail__summary-card">
                      <span>项目说明</span>
                      <p>{{ detail?.description || '当前项目暂无补充说明，可在后续迭代中补录项目背景、范围和约束。' }}</p>
                    </div>
                  </Card>

                  <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card" title="当前健康度">
                    <div class="project-detail__health project-detail__health--panel">
                      <Progress
                        :percent="currentHealth.percent"
                        :status="currentHealth.color === 'warning' ? 'active' : currentHealth.color as any"
                        type="circle"
                      />
                      <div class="project-detail__health-copy">
                        <strong>{{ currentHealth.label }}</strong>
                        <span>项目当前处于 {{ detail?.status || '-' }} 状态，重点关注执行与问题闭环。</span>
                      </div>
                    </div>
                    <div class="project-detail__overview-list project-detail__overview-list--metrics">
                      <div v-for="item in healthMetrics" :key="item.label" class="project-detail__overview-item">
                        <span>{{ item.label }}</span>
                        <strong>{{ item.value }}</strong>
                      </div>
                    </div>
                    <div class="project-detail__inline-section">
                      <header>重点风险</header>
                      <div v-if="riskAlerts.length === 0" class="project-detail__muted">当前没有高等级或逾期预警。</div>
                      <div v-else class="project-detail__stack-list">
                        <article v-for="item in riskAlerts" :key="item.id" class="project-detail__stack-item">
                          <div class="project-detail__item-head">
                            <Tag :color="item.isOverdue ? 'error' : 'gold'">{{ item.type }}</Tag>
                            <Tag>{{ item.level }}</Tag>
                          </div>
                          <strong>{{ item.title }}</strong>
                          <span>{{ item.ownerName }} / {{ item.status }}</span>
                        </article>
                      </div>
                    </div>
                  </Card>

                  <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card" title="关键里程碑 / 周期摘要">
                    <div class="project-detail__split-section">
                      <section class="project-detail__inline-section">
                        <header>近期里程碑</header>
                        <div v-if="milestonePreview.length === 0" class="project-detail__muted">暂无里程碑数据。</div>
                        <div v-else class="project-detail__stack-list">
                          <article v-for="item in milestonePreview" :key="item.id" class="project-detail__stack-item">
                            <strong>{{ item.name }}</strong>
                            <span>{{ item.ownerName }} / {{ formatDate(item.plannedCompletionDate) }} / {{ item.status }}</span>
                          </article>
                        </div>
                      </section>
                      <section class="project-detail__inline-section">
                        <header>执行周期</header>
                        <div v-if="cyclePreview.length === 0" class="project-detail__muted">暂无周期摘要。</div>
                        <div v-else class="project-detail__stack-list">
                          <article v-for="item in cyclePreview" :key="item.id" class="project-detail__stack-item">
                            <strong>{{ item.type }} · {{ item.name }}</strong>
                            <span>{{ item.ownerName }} / {{ formatDate(item.endDate) }} / {{ item.progress }}%</span>
                          </article>
                        </div>
                      </section>
                    </div>
                  </Card>

                  <Card :bordered="false" class="project-detail__inner-card project-detail__overview-card" title="与我相关">
                    <div class="project-detail__overview-list project-detail__overview-list--dense">
                      <div v-for="item in collaborationMetrics" :key="item.label" class="project-detail__overview-item">
                        <span>{{ item.label }}</span>
                        <strong>{{ item.value }}</strong>
                      </div>
                    </div>
                  </Card>
                </div>
              </section>
            </template>

            <template v-else-if="activeTab === 'plan'">
              <section class="project-detail__section">
                <Row :gutter="[16, 16]">
                  <Col :lg="14" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="阶段 / 周期 / 发布计划">
                      <Table
                        :columns="cycleColumns"
                        :data-source="detail?.cycles || []"
                        :loading="loading"
                        :pagination="false"
                        row-key="id"
                        size="small"
                      >
                        <template #bodyCell="{ column, record }">
                          <template v-if="column.key === 'progress'">
                            <Progress :percent="(record as ProjectApi.CycleItem).progress" size="small" />
                          </template>
                          <template v-else-if="column.key === 'endDate'">
                            {{ formatDate((record as ProjectApi.CycleItem).endDate) }}
                          </template>
                        </template>
                      </Table>
                    </Card>
                  </Col>
                  <Col :lg="10" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="里程碑">
                      <Table
                        :columns="milestoneColumns"
                        :data-source="detail?.milestones || []"
                        :loading="loading"
                        :pagination="false"
                        row-key="id"
                        size="small"
                      >
                        <template #bodyCell="{ column, record }">
                          <template v-if="column.key === 'plannedCompletionDate'">
                            {{ formatDate((record as ProjectApi.WorkspaceMilestoneItem).plannedCompletionDate) }}
                          </template>
                        </template>
                      </Table>
                    </Card>
                  </Col>
                </Row>

                <Card :bordered="false" class="project-detail__inner-card" title="日报 / 周报 / 月报">
                  <div v-if="reportItems.length === 0" class="project-detail__muted">
                    当前未归档日报 / 周报 / 月报，后续可直接关联项目文档与周期。
                  </div>
                  <Table
                    v-else
                    :columns="reportColumns"
                    :data-source="reportItems"
                    :pagination="false"
                    row-key="id"
                    size="small"
                  />
                </Card>
              </section>
            </template>

            <template v-else-if="activeTab === 'tasks'">
              <section class="project-detail__section">
                <Card :bordered="false" class="project-detail__inner-card">
                  <div class="project-detail__toolbar">
                    <Space wrap>
                      <span class="project-detail__toolbar-label">范围</span>
                      <Button
                        v-for="option in taskScopeOptions"
                        :key="option.key"
                        :type="taskScope === option.key ? 'primary' : 'default'"
                        @click="taskScope = option.key"
                      >
                        {{ option.label }}
                      </Button>
                    </Space>
                    <Space wrap>
                      <span class="project-detail__toolbar-label">视图</span>
                      <Button
                        v-for="option in taskViewOptions"
                        :key="option.key"
                        :type="taskView === option.key ? 'primary' : 'default'"
                        @click="taskView = option.key"
                      >
                        {{ option.label }}
                      </Button>
                    </Space>
                  </div>

                  <Alert
                    class="project-detail__alert"
                    message="“与我相关 / 我负责 / 我参与”已收敛为统一入口；日历视图改为日历区间条，适合查看跨日任务与时间条排布。"
                    show-icon
                    type="info"
                  />

                  <template v-if="taskView === 'list'">
                    <Table
                      :columns="taskColumns"
                      :data-source="filteredTasks"
                      :loading="loading"
                      :pagination="false"
                      row-key="id"
                    >
                      <template #bodyCell="{ column, record }">
                        <template v-if="column.key === 'plannedEndTime'">
                          {{ formatDate((record as ProjectApi.TaskItem).plannedEndTime) }}
                        </template>
                        <template v-else-if="['contractClause', 'productOwnerName', 'developerOwnerName', 'testerOwnerName'].includes(String(column.key))">
                          {{ (record as Record<string, string | undefined | null>)[String(column.key)] || '-' }}
                        </template>
                        <template v-else-if="column.key === 'status'">
                          <Tag :color="(record as ProjectApi.TaskItem).isBlocked ? 'error' : 'blue'">
                            {{ (record as ProjectApi.TaskItem).status }}
                          </Tag>
                        </template>
                      </template>
                    </Table>
                  </template>

                  <template v-else-if="taskView === 'board'">
                    <div class="project-detail__kanban">
                      <article v-for="column in kanbanColumns" :key="column.status" class="project-detail__kanban-column">
                        <header>
                          <strong>{{ column.status }}</strong>
                          <span>{{ column.items.length }}</span>
                        </header>
                        <div v-if="column.items.length === 0" class="project-detail__muted">暂无事项</div>
                        <div v-else class="project-detail__kanban-list">
                          <section v-for="item in column.items" :key="item.id" class="project-detail__kanban-card">
                            <div class="project-detail__item-head">
                              <Tag :color="item.priority === '高' ? 'error' : 'processing'">{{ item.priority }}</Tag>
                              <Tag v-if="item.isBlocked" color="error">阻塞</Tag>
                            </div>
                            <strong>{{ item.title }}</strong>
                            <span>{{ item.ownerName }} / {{ formatDate(item.plannedEndTime) }}</span>
                          </section>
                        </div>
                      </article>
                    </div>
                  </template>

                  <template v-else-if="taskView === 'calendar'">
                    <div class="project-detail__calendar-range-panel">
                      <div class="project-detail__calendar-head">
                        <div>
                          <strong>{{ taskCalendarTitle }}</strong>
                          <p class="project-detail__calendar-subtitle">按周展示跨日事件条，适合看任务在自然日历中的连续占用区间。</p>
                        </div>
                        <Space>
                          <Button size="small" @click="shiftTaskCalendarMonth(-1)">上月</Button>
                          <Button size="small" @click="taskCalendarAnchorDate = null">回到任务月份</Button>
                          <Button size="small" @click="shiftTaskCalendarMonth(1)">下月</Button>
                        </Space>
                      </div>
                      <div class="project-detail__calendar-weekdays project-detail__calendar-weekdays--sticky">
                        <span>周区间</span>
                        <span v-for="label in ['日', '一', '二', '三', '四', '五', '六']" :key="label">{{ label }}</span>
                      </div>
                      <div class="project-detail__calendar-range-rows">
                        <article v-for="row in taskCalendarRows" :key="row.key" class="project-detail__calendar-range-row">
                          <div class="project-detail__calendar-range-label">{{ row.label }}</div>
                          <div class="project-detail__calendar-range-track">
                            <div class="project-detail__calendar-range-days">
                              <div
                                v-for="day in row.days"
                                :key="day.key"
                                :class="[
                                  'project-detail__calendar-range-day',
                                  {
                                    'project-detail__calendar-range-day--muted': !day.inCurrentMonth,
                                    'project-detail__calendar-range-day--today': day.isToday,
                                  },
                                ]"
                              >
                                {{ day.dayLabel }}
                              </div>
                            </div>
                            <div v-if="row.bars.length === 0" class="project-detail__calendar-range-empty">本周暂无跨日任务</div>
                            <div v-else class="project-detail__calendar-range-bars">
                              <div
                                v-for="bar in row.bars"
                                :key="bar.key"
                                class="project-detail__calendar-range-bar"
                                :style="{
                                  background: bar.color,
                                  gridColumn: `${bar.startColumn + 1} / span ${bar.span}`,
                                }"
                              >
                                <strong>{{ bar.title }}</strong>
                                <span>{{ bar.subtitle }}</span>
                              </div>
                            </div>
                          </div>
                        </article>
                      </div>
                    </div>
                  </template>

                  <template v-else>
                    <div v-if="taskTimelineDays.length === 0" class="project-detail__muted">
                      当前筛选下暂无可展示排期。
                    </div>
                    <div v-else class="project-detail__timeline-shell">
                      <div class="project-detail__timeline-head">
                        <div class="project-detail__timeline-side">事项</div>
                        <div class="project-detail__timeline-grid" :style="taskTimelineGridStyle">
                          <div v-for="day in taskTimelineDays" :key="day.toISOString()" class="project-detail__timeline-day">
                            {{ day.getMonth() + 1 }}/{{ day.getDate() }}
                          </div>
                        </div>
                      </div>
                      <div v-for="item in taskTimelineRows" :key="item.id" class="project-detail__timeline-row">
                        <div class="project-detail__timeline-side">
                          <strong>{{ item.title }}</strong>
                          <small>{{ item.ownerName }} / {{ item.status }}</small>
                        </div>
                        <div class="project-detail__timeline-grid" :style="taskTimelineGridStyle">
                          <div
                            class="project-detail__timeline-bar"
                            :style="{
                              background: item.color,
                              gridColumn: `${item.startOffset + 1} / span ${item.span}`,
                            }"
                          >
                            {{ `${formatDate(item.plannedStartTime)} - ${formatDate(item.plannedEndTime)}` }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </template>
                </Card>
              </section>
            </template>

            <template v-else-if="activeTab === 'issues'">
              <section class="project-detail__section">
                <Alert
                  class="project-detail__alert"
                  message="缺陷 / 问题补充了需求、产品、开发、测试字段；下方工作流仅做前端预览，方便你确认是否保留，暂不接后端配置。"
                  show-icon
                  type="info"
                />
                <Row :gutter="[16, 16]">
                  <Col :lg="16" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="缺陷 / 问题统一入口">
                      <Table
                        :columns="issueColumns"
                        :data-source="detail?.issues || []"
                        :loading="loading"
                        :pagination="false"
                        row-key="id"
                      >
                        <template #bodyCell="{ column, record }">
                          <template v-if="column.key === 'dueDate'">
                            {{ formatDate((record as ProjectApi.IssueItem).dueDate) }}
                          </template>
                          <template v-else-if="['requirementTitle', 'productOwnerName', 'developerOwnerName', 'testerOwnerName'].includes(String(column.key))">
                            {{ (record as Record<string, string | undefined | null>)[String(column.key)] || '-' }}
                          </template>
                          <template v-else-if="column.key === 'level'">
                            <Tag :color="(record as ProjectApi.IssueItem).level === '高' ? 'error' : 'gold'">
                              {{ (record as ProjectApi.IssueItem).level }}
                            </Tag>
                          </template>
                        </template>
                      </Table>
                    </Card>
                  </Col>
                  <Col :lg="8" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="处理概况">
                      <div class="project-detail__issue-metrics">
                        <article v-for="metric in issueMetrics" :key="metric.label" class="project-detail__issue-metric">
                          <span>{{ metric.label }}</span>
                          <strong>{{ metric.value }}</strong>
                        </article>
                      </div>
                    </Card>
                    <Card :bordered="false" class="project-detail__inner-card" title="风险联动">
                      <div class="project-detail__stack-list">
                        <article v-for="item in detail?.raidItems || []" :key="item.id" class="project-detail__stack-item">
                          <div class="project-detail__item-head">
                            <Tag :color="item.isOverdue ? 'error' : 'processing'">{{ item.type }}</Tag>
                            <Tag>{{ item.level }}</Tag>
                          </div>
                          <strong>{{ item.title }}</strong>
                          <span>{{ item.ownerName }} / {{ item.status }}</span>
                        </article>
                      </div>
                    </Card>
                  </Col>
                </Row>

                <Card :bordered="false" class="project-detail__inner-card" title="项目工作流视图">
                  <div class="project-detail__workflow-grid">
                    <article v-for="workflow in workflowPreviewGroups" :key="workflow.key" class="project-detail__workflow-card">
                      <header class="project-detail__workflow-card-head">
                        <strong>{{ workflow.title }}</strong>
                        <Tag :color="workflow.tone === 'danger' ? 'error' : workflow.tone === 'warning' ? 'gold' : 'processing'">
                          {{ workflow.steps.length }} 个节点
                        </Tag>
                      </header>
                      <div class="project-detail__workflow-canvas">
                        <div class="project-detail__workflow-chain">
                          <template v-for="(step, index) in workflow.steps" :key="step.key">
                            <section class="project-detail__workflow-node">
                              <span class="project-detail__workflow-node-count">{{ step.count }}</span>
                              <strong>{{ step.title }}</strong>
                            </section>
                            <div v-if="index < workflow.steps.length - 1" class="project-detail__workflow-arrow">→</div>
                          </template>
                        </div>
                      </div>
                    </article>
                  </div>
                </Card>
              </section>
            </template>

            <template v-else-if="activeTab === 'docs'">
              <section class="project-detail__section">
                <div class="project-detail__knowledge-layout">
                  <aside class="project-detail__knowledge-sidebar">
                    <Card :bordered="false" class="project-detail__inner-card" title="知识库">
                      <div class="project-detail__knowledge-groups">
                        <article v-for="group in knowledgeCollections" :key="group.name" class="project-detail__knowledge-group">
                          <div>
                            <strong>{{ group.name }}</strong>
                            <p>{{ group.description }}</p>
                          </div>
                          <span>{{ group.count }} 篇</span>
                        </article>
                      </div>
                    </Card>
                  </aside>
                  <div class="project-detail__knowledge-main">
                    <Card :bordered="false" class="project-detail__inner-card" title="知识文档">
                      <div class="project-detail__knowledge-list">
                        <article v-for="doc in featuredDocuments" :key="doc.id" class="project-detail__knowledge-doc">
                          <div class="project-detail__knowledge-doc-main">
                            <strong>{{ doc.title }}</strong>
                            <div class="project-detail__knowledge-meta project-detail__knowledge-meta--single-line">
                              <span>{{ doc.category || '未分类' }}</span>
                              <span>{{ doc.version || 'v1.0' }}</span>
                              <span>{{ doc.ownerName || '-' }}</span>
                              <span>{{ doc.status || '-' }}</span>
                            </div>
                          </div>
                          <Button type="link" @click="openDocumentWorkspace(doc)">新tab打开</Button>
                        </article>
                      </div>
                    </Card>
                    <Card :bordered="false" class="project-detail__inner-card" title="全部文档索引">
                      <Table
                        :columns="documentColumns"
                        :data-source="detail?.documents || []"
                        :loading="loading"
                        :pagination="false"
                        row-key="id"
                      >
                        <template #bodyCell="{ column, record }">
                          <template v-if="column.key === 'title'">
                            <Button type="link" @click="openDocumentWorkspace(record as ProjectApi.DocumentItem)">
                              {{ (record as ProjectApi.DocumentItem).title }}
                            </Button>
                          </template>
                          <template v-else-if="column.key === 'status'">
                            <Tag color="processing">{{ (record as ProjectApi.DocumentItem).status }}</Tag>
                          </template>
                        </template>
                      </Table>
                    </Card>
                  </div>
                </div>
              </section>
            </template>

            <template v-else-if="activeTab === 'resources'">
              <section class="project-detail__section">
                <Row :gutter="[16, 16]">
                  <Col :lg="14" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="项目成员 / 借调 / 离场">
                      <Table
                        :columns="memberColumns"
                        :data-source="detail?.members || []"
                        :loading="loading"
                        :pagination="false"
                        row-key="id"
                      >
                        <template #bodyCell="{ column, record }">
                          <template v-if="column.key === 'roleNames'">
                            <div class="project-detail__tag-list">
                              <Tag v-for="role in (record as ProjectApi.MemberItem).roleNames" :key="role" color="blue">
                                {{ role }}
                              </Tag>
                            </div>
                          </template>
                          <template v-else-if="column.key === 'joinDate'">
                            {{ formatDate((record as ProjectApi.MemberItem).joinDate) }}
                          </template>
                        </template>
                      </Table>
                    </Card>
                  </Col>
                  <Col :lg="10" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="RACI">
                      <div class="project-detail__raci-list">
                        <article v-for="item in raciRows" :key="item.userName" class="project-detail__raci-row">
                          <div>
                            <strong>{{ item.userName }}</strong>
                            <span>{{ item.roleNames }}</span>
                          </div>
                          <div class="project-detail__raci-tags">
                            <Tag color="blue">R {{ item.execute }}</Tag>
                            <Tag color="gold">A {{ item.approve }}</Tag>
                            <Tag color="green">C {{ item.consult }}</Tag>
                            <Tag>I {{ item.inform }}</Tag>
                          </div>
                        </article>
                      </div>
                    </Card>
                  </Col>
                </Row>

                <Card :bordered="false" class="project-detail__inner-card" title="工时系统（小时）">
                  <Alert
                    class="project-detail__alert"
                    message="人员与资源页统一按小时展示工时，便于后续直接衔接填报、审批和成本核算。"
                    show-icon
                    type="success"
                  />
                  <Table
                    :columns="worklogColumns"
                    :data-source="detail?.worklogs || []"
                    :loading="loading"
                    :pagination="false"
                    row-key="id"
                    size="small"
                  >
                    <template #bodyCell="{ column, record }">
                      <template v-if="column.key === 'workDate'">
                        {{ formatDate((record as ProjectApi.WorklogItem).workDate) }}
                      </template>
                    </template>
                  </Table>
                </Card>
              </section>
            </template>
        </div>
      </div>
    </section>
  </Page>
</template>

<style scoped>
.project-detail {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.project-detail__compact-header {
  background:
    linear-gradient(135deg, var(--ant-color-bg-container), var(--ant-color-fill-quaternary));
}

.project-detail__compact-main {
  display: flex;
  gap: 16px;
  align-items: center;
  justify-content: space-between;
}

.project-detail__compact-copy h2 {
  margin: 6px 0 8px;
  color: var(--ant-color-text);
  font-size: 22px;
  line-height: 1.2;
}

.project-detail__meta-line,
.project-detail__meta-pills,
.project-detail__compact-stats,
.project-detail__item-head,
.project-detail__tag-list,
.project-detail__raci-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.project-detail__meta-pills span {
  padding: 4px 10px;
  color: var(--ant-color-text-secondary);
  background: var(--ant-color-fill-tertiary);
  border-radius: 999px;
  font-size: 12px;
}

.project-detail__selector {
  width: 320px;
}

.project-detail__compact-stats {
  margin-top: 12px;
}

.project-detail__compact-stat {
  min-width: 160px;
  padding: 10px 12px;
  background: var(--ant-color-fill-quaternary);
  border-radius: 12px;
}

.project-detail__compact-stat span,
.project-detail__toolbar-label,
.project-detail__info-grid span,
.project-detail__raci-row span,
.project-detail__timeline-side small {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.project-detail__compact-stat strong,
.project-detail__metric-value {
  display: block;
  margin-top: 4px;
  color: var(--ant-color-text);
  font-size: 18px;
}

.project-detail__tabs {
  margin-top: 16px;
}

.project-detail__tabs :deep(.ant-tabs-nav) {
  margin-bottom: 0;
}

.project-detail__tabs :deep(.ant-tabs-tab) {
  padding: 8px 0 14px;
}

.project-detail__tab-shell {
  overflow: hidden;
  transition: height 0.28s ease;
}

.project-detail__empty {
  display: grid;
  min-height: 320px;
  place-items: center;
}

.project-detail__section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.project-detail__overview-grid {
  display: grid;
  gap: 16px;
}

.project-detail__overview-grid--cards {
  grid-template-columns: minmax(0, 1.45fr) minmax(0, 1fr);
}

.project-detail__overview-card {
  height: 100%;
}

.project-detail__overview-card--detail {
  display: grid;
  gap: 16px;
}

.project-detail__inner-card {
  border: 1px solid var(--ant-color-border-secondary);
}

.project-detail__overview-hero {
  align-items: center;
  display: grid;
  gap: 14px;
  grid-template-columns: minmax(0, 1.5fr) minmax(320px, 1fr);
  padding: 16px 18px;
  background:
    linear-gradient(135deg, rgb(22 119 255 / 0.08), rgb(37 99 235 / 0.03)),
    var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.08);
  border-radius: 18px;
}

.project-detail__overview-hero-main {
  min-width: 0;
}

.project-detail__eyebrow {
  color: var(--ant-color-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.project-detail__overview-hero-main h3 {
  margin: 8px 0 0;
  font-size: 24px;
}

.project-detail__overview-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 12px;
}

.project-detail__overview-meta span,
.project-detail__knowledge-group p,
.project-detail__knowledge-doc p,
.project-detail__calendar-subtitle {
  background: var(--ant-color-fill-tertiary);
  border-radius: 999px;
  color: var(--ant-color-text-secondary);
  padding: 4px 10px;
}

.project-detail__knowledge-group p,
.project-detail__knowledge-doc p,
.project-detail__calendar-subtitle {
  line-height: 1.7;
}

.project-detail__overview-kpis {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  min-width: 0;
}

.project-detail__overview-kpi,
.project-detail__issue-metric {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 12px 14px;
  background: var(--ant-color-fill-secondary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 14px;
}

.project-detail__info-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.project-detail__detail-sections,
.project-detail__split-section {
  display: grid;
  gap: 14px;
}

.project-detail__detail-section,
.project-detail__inline-section,
.project-detail__summary-card {
  display: grid;
  gap: 10px;
}

.project-detail__detail-section header,
.project-detail__inline-section header,
.project-detail__summary-card span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  font-weight: 600;
  letter-spacing: 0.04em;
}

.project-detail__info-item,
.project-detail__overview-item {
  padding: 12px 14px;
  background: var(--ant-color-fill-secondary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 14px;
}

.project-detail__info-grid div,
.project-detail__overview-list div {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.project-detail__overview-list {
  display: grid;
  gap: 12px;
}

.project-detail__overview-list--metrics {
  grid-template-columns: repeat(3, minmax(0, 1fr));
  margin-top: 14px;
}

.project-detail__overview-list--dense {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.project-detail__summary {
  margin: 16px 0 0;
  color: var(--ant-color-text-secondary);
  line-height: 1.7;
}

.project-detail__summary-card {
  padding: 14px 16px;
  background: var(--ant-color-fill-tertiary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 14px;
}

.project-detail__summary-card p {
  margin: 0;
  color: var(--ant-color-text-secondary);
  line-height: 1.7;
}

.project-detail__health {
  display: flex;
  gap: 18px;
  align-items: center;
}

.project-detail__health--panel {
  align-items: flex-start;
}

.project-detail__health-copy {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.project-detail__health-copy strong,
.project-detail__stack-item strong,
.project-detail__kanban-card strong,
.project-detail__timeline-side strong,
.project-detail__raci-row strong {
  color: var(--ant-color-text);
}

.project-detail__stack-list,
.project-detail__kanban-list,
.project-detail__raci-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.project-detail__stack-item,
.project-detail__kanban-card,
.project-detail__raci-row {
  padding: 12px 14px;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 12px;
}

.project-detail__stack-item span,
.project-detail__kanban-card span,
.project-detail__muted {
  color: var(--ant-color-text-secondary);
}

.project-detail__toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: space-between;
  margin-bottom: 12px;
}

.project-detail__alert {
  margin-bottom: 12px;
}

.project-detail__kanban {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.project-detail__kanban-column {
  padding: 12px;
  background: var(--ant-color-fill-tertiary);
  border-radius: 16px;
}

.project-detail__kanban-column header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
}

.project-detail__calendar-head,
.project-detail__calendar-weekdays,
.project-detail__calendar-item,
.project-detail__workflow-step {
  display: flex;
}

.project-detail__calendar-head {
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.project-detail__calendar-range-panel {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.project-detail__calendar-weekdays {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  margin-bottom: 8px;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  text-align: center;
}

.project-detail__calendar-grid {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.project-detail__calendar-weekdays--sticky {
  grid-template-columns: 180px repeat(7, minmax(0, 1fr));
}

.project-detail__calendar-range-rows {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.project-detail__calendar-range-row {
  display: grid;
  gap: 12px;
  grid-template-columns: 180px minmax(0, 1fr);
}

.project-detail__calendar-range-label {
  padding: 18px 14px;
  font-weight: 600;
  background: var(--ant-color-fill-quaternary);
  border-radius: 14px;
}

.project-detail__calendar-range-track {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.project-detail__calendar-range-days,
.project-detail__calendar-range-bars {
  display: grid;
  gap: 8px;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.project-detail__calendar-range-day {
  padding: 10px 0;
  text-align: center;
  background: var(--ant-color-fill-quaternary);
  border-radius: 12px;
}

.project-detail__calendar-range-day--muted {
  opacity: 0.55;
}

.project-detail__calendar-range-day--today {
  color: var(--ant-color-primary);
  box-shadow: inset 0 0 0 1px rgb(22 119 255 / 0.18);
}

.project-detail__calendar-range-empty {
  padding: 14px 16px;
  color: var(--ant-color-text-secondary);
  background: var(--ant-color-fill-quaternary);
  border-radius: 12px;
}

.project-detail__calendar-range-bar {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-height: 58px;
  padding: 10px 12px;
  color: white;
  border-radius: 16px;
  box-shadow: 0 10px 24px rgb(15 23 42 / 0.14);
}

.project-detail__calendar-range-bar span {
  color: rgb(255 255 255 / 0.86);
  font-size: 12px;
}

.project-detail__calendar-cell {
  min-height: 148px;
  padding: 12px;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 16px;
}

.project-detail__calendar-cell header,
.project-detail__workflow-step {
  justify-content: space-between;
}

.project-detail__calendar-cell header {
  display: flex;
  margin-bottom: 10px;
}

.project-detail__calendar-cell--muted {
  opacity: 0.58;
}

.project-detail__calendar-cell--today {
  border-color: rgb(22 119 255 / 0.36);
  box-shadow: inset 0 0 0 1px rgb(22 119 255 / 0.16);
}

.project-detail__calendar-list,
.project-detail__workflow-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.project-detail__calendar-item {
  gap: 8px;
  align-items: center;
  color: var(--ant-color-text);
  font-size: 12px;
  line-height: 1.5;
}

.project-detail__calendar-dot {
  width: 8px;
  height: 8px;
  flex: none;
  border-radius: 999px;
}

.project-detail__workflow-step {
  gap: 14px;
  align-items: center;
  padding: 14px 16px;
  background: var(--ant-color-fill-quaternary);
  border-radius: 14px;
}

.project-detail__workflow-grid {
  display: grid;
  gap: 16px;
}

.project-detail__workflow-card {
  padding: 18px;
  background: linear-gradient(180deg, rgb(248 250 252), var(--ant-color-bg-container));
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 20px;
}

.project-detail__workflow-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.project-detail__workflow-canvas {
  overflow-x: auto;
}

.project-detail__workflow-chain {
  display: flex;
  align-items: center;
  min-width: 920px;
}

.project-detail__workflow-node {
  display: flex;
  flex-direction: column;
  gap: 8px;
  min-width: 152px;
  padding: 18px 16px;
  background: white;
  border: 1px solid rgb(22 119 255 / 0.1);
  border-radius: 16px;
  box-shadow: 0 14px 32px rgb(15 23 42 / 0.08);
}

.project-detail__workflow-node-count {
  display: inline-grid;
  width: 34px;
  height: 34px;
  color: var(--ant-color-primary);
  background: rgb(22 119 255 / 0.08);
  border-radius: 10px;
  place-items: center;
}

.project-detail__workflow-arrow {
  padding: 0 12px;
  color: var(--ant-color-text-secondary);
  font-size: 24px;
}

.project-detail__issue-metrics {
  display: grid;
  gap: 12px;
  margin-bottom: 16px;
}

.project-detail__workflow-step p {
  margin: 4px 0 0;
  color: var(--ant-color-text-secondary);
}

.project-detail__workflow-badge {
  display: grid;
  width: 44px;
  height: 44px;
  flex: none;
  font-size: 16px;
  font-weight: 700;
  color: var(--ant-color-primary);
  background: rgb(22 119 255 / 0.08);
  border-radius: 14px;
  place-items: center;
}

.project-detail__knowledge-layout {
  display: grid;
  gap: 16px;
  grid-template-columns: 300px minmax(0, 1fr);
}

.project-detail__knowledge-main {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.project-detail__knowledge-groups,
.project-detail__knowledge-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.project-detail__knowledge-group,
.project-detail__knowledge-doc {
  display: flex;
  gap: 14px;
  align-items: center;
  justify-content: space-between;
  padding: 14px 0;
  background: transparent;
  border-bottom: 1px solid var(--ant-color-border-secondary);
  border-radius: 0;
}

.project-detail__knowledge-list {
  border-top: 1px solid var(--ant-color-border-secondary);
}

.project-detail__knowledge-group span,
.project-detail__knowledge-meta {
  color: var(--ant-color-text-secondary);
}

.project-detail__knowledge-doc-main {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 6px;
}

.project-detail__knowledge-meta {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.project-detail__knowledge-meta--single-line {
  white-space: nowrap;
}

.project-detail__timeline-shell {
  overflow-x: auto;
}

.project-detail__timeline-head,
.project-detail__timeline-row {
  display: grid;
  gap: 8px;
  min-width: 980px;
  grid-template-columns: 240px minmax(0, 1fr);
}

.project-detail__timeline-head {
  margin-bottom: 8px;
}

.project-detail__timeline-side {
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.project-detail__timeline-grid {
  display: grid;
  gap: 6px;
  align-items: center;
}

.project-detail__timeline-day {
  padding: 8px 0;
  color: var(--ant-color-text-secondary);
  text-align: center;
  border-bottom: 1px solid var(--ant-color-border-secondary);
  font-size: 12px;
}

.project-detail__timeline-bar {
  padding: 8px 10px;
  color: white;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 600;
  text-align: center;
}

.project-detail__raci-row {
  display: flex;
  gap: 12px;
  align-items: center;
  justify-content: space-between;
}

@media (max-width: 1080px) {
  .project-detail__compact-main,
  .project-detail__toolbar,
  .project-detail__health {
    align-items: stretch;
    flex-direction: column;
  }

  .project-detail__overview-grid--cards,
  .project-detail__selector,
  .project-detail__info-grid,
  .project-detail__kanban,
  .project-detail__calendar-grid,
  .project-detail__knowledge-layout {
    width: 100%;
    grid-template-columns: 1fr;
  }

  .project-detail__calendar-head,
  .project-detail__workflow-step,
  .project-detail__workflow-card-head,
  .project-detail__knowledge-group,
  .project-detail__knowledge-doc,
  .project-detail__overview-hero {
    align-items: stretch;
    flex-direction: column;
  }

  .project-detail__overview-hero {
    grid-template-columns: 1fr;
  }

  .project-detail__overview-kpis {
    grid-template-columns: 1fr;
  }

  .project-detail__overview-list--metrics,
  .project-detail__overview-list--dense,
  .project-detail__split-section {
    grid-template-columns: 1fr;
  }

  .project-detail__calendar-weekdays--sticky,
  .project-detail__calendar-range-row {
    grid-template-columns: 1fr;
  }

  .project-detail__workflow-chain {
    min-width: 100%;
    flex-direction: column;
    align-items: stretch;
  }

  .project-detail__workflow-arrow {
    text-align: center;
    transform: rotate(90deg);
  }

  .project-detail__compact-stat {
    min-width: calc(50% - 8px);
  }
}
</style>
