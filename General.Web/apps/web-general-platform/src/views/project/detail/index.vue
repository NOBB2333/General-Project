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

const taskCalendarCells = computed(() => {
  const current = taskCalendarReferenceDate.value;
  const monthStart = new Date(current.getFullYear(), current.getMonth(), 1);
  const monthEnd = new Date(current.getFullYear(), current.getMonth() + 1, 0);
  const rangeStart = new Date(monthStart);
  rangeStart.setDate(monthStart.getDate() - monthStart.getDay());
  const rangeEnd = new Date(monthEnd);
  rangeEnd.setDate(monthEnd.getDate() + (6 - monthEnd.getDay()));
  const todayKey = toDateKey(new Date());
  const cells = [] as Array<{
    date: Date;
    dayLabel: number;
    inCurrentMonth: boolean;
    isToday: boolean;
    items: ProjectApi.TaskItem[];
    key: string;
  }>;

  for (let cursor = new Date(rangeStart); cursor <= rangeEnd; cursor.setDate(cursor.getDate() + 1)) {
    const day = new Date(cursor);
    cells.push({
      date: day,
      dayLabel: day.getDate(),
      inCurrentMonth: day.getMonth() === current.getMonth(),
      isToday: toDateKey(day) === todayKey,
      items: filteredTasks.value.filter((item) => taskOccursOnDate(item, day)),
      key: toDateKey(day),
    });
  }

  return cells;
});

const workflowPreviewSteps = computed(() => {
  const issues = detail.value?.issues || [];
  const closedKeywords = ['关闭', '完成', '解决'];
  return [
    {
      key: 'submit',
      title: '登记',
      hint: '创建缺陷/问题并挂接需求或任务。',
      count: issues.length,
    },
    {
      key: 'requirement',
      title: '需求确认',
      hint: '明确关联需求与影响范围。',
      count: issues.filter((item) => Boolean(item.requirementTitle)).length,
    },
    {
      key: 'product',
      title: '产品分析',
      hint: '产品侧确认优先级与处理方案。',
      count: issues.filter((item) => Boolean(item.productOwnerName)).length,
    },
    {
      key: 'develop',
      title: '开发处理',
      hint: '开发排查、修复、联调。',
      count: issues.filter((item) => Boolean(item.developerOwnerName)).length,
    },
    {
      key: 'test',
      title: '测试验证',
      hint: '测试回归与结果确认。',
      count: issues.filter((item) => Boolean(item.testerOwnerName)).length,
    },
    {
      key: 'close',
      title: '关闭归档',
      hint: '验证通过后关闭，后端暂不落库。',
      count: issues.filter((item) => closedKeywords.some((keyword) => item.status.includes(keyword))).length,
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

function taskOccursOnDate(item: ProjectApi.TaskItem, date: Date) {
  const dayKey = toDateKey(date);
  const start = parseDate(item.plannedStartTime || item.plannedEndTime);
  const end = parseDate(item.plannedEndTime || item.plannedStartTime);

  if (!start || !end) {
    return false;
  }

  return dayKey >= toDateKey(start) && dayKey <= toDateKey(end);
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
        <div v-if="selectedProjectId" class="project-detail__tab-nav">
          <button
            v-for="tab in tabItems"
            :key="tab.key"
            :class="['project-detail__tab-button', { 'project-detail__tab-button--active': activeTab === tab.key }]"
            type="button"
            @click="activeTab = tab.key"
          >
            {{ tab.label }}
          </button>
        </div>
      </Card>

      <div v-if="!selectedProjectId" class="project-detail__empty">
        <Empty description="请选择项目" />
      </div>

      <div v-else class="project-detail__tab-shell" :style="tabBodyStyle">
        <div :ref="bindTabContent">
          <template v-if="activeTab === 'overview'">
              <section class="project-detail__section">
                <Row :gutter="[16, 16]">
                  <Col :lg="10" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="项目详细信息">
                      <div class="project-detail__info-grid">
                        <div><span>项目类型</span><strong>{{ detail?.projectType || '-' }}</strong></div>
                        <div><span>项目来源</span><strong>{{ detail?.projectSource || '-' }}</strong></div>
                        <div><span>合同金额</span><strong>{{ formatMoney(detail?.contractTotalAmount) }}</strong></div>
                        <div><span>预算金额</span><strong>{{ formatMoney(detail?.budgetTotalAmount) }}</strong></div>
                        <div><span>已回款</span><strong>{{ formatMoney(detail?.receivedAmount) }}</strong></div>
                        <div><span>发起人</span><strong>{{ detail?.sponsorName || '-' }}</strong></div>
                      </div>
                      <p class="project-detail__summary">{{ detail?.description || '暂无项目说明。' }}</p>
                    </Card>
                  </Col>
                  <Col :lg="7" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="当前健康度">
                      <div class="project-detail__health">
                        <Progress
                          :percent="currentHealth.percent"
                          :status="currentHealth.color === 'warning' ? 'active' : currentHealth.color as any"
                          type="circle"
                        />
                        <div class="project-detail__health-copy">
                          <strong>{{ currentHealth.label }}</strong>
                          <span>高风险 {{ detail?.highRiskCount || 0 }} 项</span>
                          <span>未闭环问题 {{ detail?.openIssueCount || 0 }} 项</span>
                        </div>
                      </div>
                    </Card>
                  </Col>
                  <Col :lg="7" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="与我相关">
                      <div class="project-detail__overview-list">
                        <div><span>我的项目关系</span><strong>{{ detail?.myRelation || '-' }}</strong></div>
                        <div><span>我负责事项</span><strong>{{ relatedTaskCount }}</strong></div>
                        <div><span>我跟进问题</span><strong>{{ relatedIssueCount }}</strong></div>
                        <div><span>我的工时记录</span><strong>{{ relatedWorklogCount }}</strong></div>
                      </div>
                    </Card>
                  </Col>
                </Row>

                <Row :gutter="[16, 16]">
                  <Col :lg="12" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="关键里程碑 / 周期摘要">
                      <div class="project-detail__stack-list">
                        <article
                          v-for="item in (detail?.milestones || []).slice(0, 3)"
                          :key="item.id"
                          class="project-detail__stack-item"
                        >
                          <strong>{{ item.name }}</strong>
                          <span>{{ item.ownerName }} / {{ formatDate(item.plannedCompletionDate) }} / {{ item.status }}</span>
                        </article>
                        <article
                          v-for="item in (detail?.cycles || []).slice(0, 3)"
                          :key="item.id"
                          class="project-detail__stack-item"
                        >
                          <strong>{{ item.type }} · {{ item.name }}</strong>
                          <span>{{ item.ownerName }} / {{ formatDate(item.endDate) }} / {{ item.progress }}%</span>
                        </article>
                      </div>
                    </Card>
                  </Col>
                  <Col :lg="12" :span="24">
                    <Card :bordered="false" class="project-detail__inner-card" title="风险预警">
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
                    </Card>
                  </Col>
                </Row>
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
                    message="“与我相关 / 我负责 / 我参与”已收敛为统一入口；任务列表补充了关联合同条款、产品、开发、测试字段，并提供列表 / 看板 / 甘特 / 日历四种查看方式。"
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
                    <div class="project-detail__calendar-head">
                      <strong>{{ taskCalendarTitle }}</strong>
                      <Space>
                        <Button size="small" @click="shiftTaskCalendarMonth(-1)">上月</Button>
                        <Button size="small" @click="taskCalendarAnchorDate = null">回到任务月份</Button>
                        <Button size="small" @click="shiftTaskCalendarMonth(1)">下月</Button>
                      </Space>
                    </div>
                    <div class="project-detail__calendar-weekdays">
                      <span v-for="label in ['日', '一', '二', '三', '四', '五', '六']" :key="label">{{ label }}</span>
                    </div>
                    <div class="project-detail__calendar-grid">
                      <article
                        v-for="cell in taskCalendarCells"
                        :key="cell.key"
                        :class="[
                          'project-detail__calendar-cell',
                          {
                            'project-detail__calendar-cell--muted': !cell.inCurrentMonth,
                            'project-detail__calendar-cell--today': cell.isToday,
                          },
                        ]"
                      >
                        <header>
                          <strong>{{ cell.dayLabel }}</strong>
                          <span>{{ cell.items.length }} 项</span>
                        </header>
                        <div v-if="cell.items.length === 0" class="project-detail__muted">暂无任务</div>
                        <div v-else class="project-detail__calendar-list">
                          <div v-for="item in cell.items.slice(0, 3)" :key="`${cell.key}-${item.id}`" class="project-detail__calendar-item">
                            <span class="project-detail__calendar-dot" :style="{ background: resolveTaskColor(item) }"></span>
                            <span>{{ item.taskCode }} · {{ item.title }}</span>
                          </div>
                          <div v-if="cell.items.length > 3" class="project-detail__muted">+{{ cell.items.length - 3 }} 项继续</div>
                        </div>
                      </article>
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
                  <Col :lg="14" :span="24">
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
                  <Col :lg="10" :span="24">
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

                <Card :bordered="false" class="project-detail__inner-card" title="工作流预览（仅前端示意）">
                  <div class="project-detail__workflow-list">
                    <article v-for="step in workflowPreviewSteps" :key="step.key" class="project-detail__workflow-step">
                      <div class="project-detail__workflow-badge">{{ step.count }}</div>
                      <div>
                        <strong>{{ step.title }}</strong>
                        <p>{{ step.hint }}</p>
                      </div>
                    </article>
                  </div>
                </Card>
              </section>
            </template>

            <template v-else-if="activeTab === 'docs'">
              <section class="project-detail__section">
                <Alert
                  class="project-detail__alert"
                  message="这里展示项目相关文档与知识沉淀；全局文件管理负责统一存储，后续会做项目文档与全局文件双向打通。"
                  show-icon
                  type="info"
                />
                <Card :bordered="false" class="project-detail__inner-card" title="文档 / 评论 / 版本历史">
                  <Table
                    :columns="documentColumns"
                    :data-source="detail?.documents || []"
                    :loading="loading"
                    :pagination="false"
                    row-key="id"
                  >
                    <template #bodyCell="{ column, record }">
                      <template v-if="column.key === 'status'">
                        <Tag color="processing">{{ (record as ProjectApi.DocumentItem).status }}</Tag>
                      </template>
                    </template>
                  </Table>
                </Card>
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

.project-detail__tab-nav {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 16px;
}

.project-detail__tab-button {
  padding: 10px 14px;
  color: var(--ant-color-text-secondary);
  cursor: pointer;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid transparent;
  border-radius: 999px;
  transition:
    color 0.2s ease,
    background 0.2s ease,
    border-color 0.2s ease,
    transform 0.2s ease;
}

.project-detail__tab-button:hover {
  color: var(--ant-color-primary);
  transform: translateY(-1px);
}

.project-detail__tab-button--active {
  color: var(--ant-color-primary);
  background: rgb(22 119 255 / 0.08);
  border-color: rgb(22 119 255 / 0.22);
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

.project-detail__inner-card {
  border: 1px solid var(--ant-color-border-secondary);
}

.project-detail__info-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(3, minmax(0, 1fr));
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

.project-detail__summary {
  margin: 16px 0 0;
  color: var(--ant-color-text-secondary);
  line-height: 1.7;
}

.project-detail__health {
  display: flex;
  gap: 18px;
  align-items: center;
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

  .project-detail__selector,
  .project-detail__info-grid,
  .project-detail__kanban,
  .project-detail__calendar-grid {
    width: 100%;
    grid-template-columns: 1fr;
  }

  .project-detail__calendar-head,
  .project-detail__tab-nav,
  .project-detail__workflow-step {
    align-items: stretch;
    flex-direction: column;
  }

  .project-detail__compact-stat {
    min-width: calc(50% - 8px);
  }
}
</style>
