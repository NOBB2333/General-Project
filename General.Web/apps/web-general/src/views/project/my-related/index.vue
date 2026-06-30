<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, Empty, Progress, Row, Space, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getProjectMyRelatedDashboardApi, type ProjectApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'ProjectMyRelatedPage' });

const router = useRouter();
const loading = ref(false);
const payload = ref<null | ProjectApi.MyExecutionDashboard>(null);
const recentWorklogs = computed(() => payload.value?.recentWorklogs || []);
const worklogPagination = useLocalVxePagination(recentWorklogs, { defaultPageSize: 10 });

const projectColumns = [
  { field: 'projectCode', title: '项目编号', width: 140 },
  { align: 'left', field: 'name', slots: { default: 'name' }, title: '项目名称' },
  { field: 'myRelation', slots: { default: 'myRelation' }, title: '我的关系', width: 140 },
  { field: 'nextSprintName', title: '当前排期', width: 160 },
  { field: 'dueSoonTaskCount', title: '近期待办', width: 100 },
  { field: 'activeIssueCount', title: '待处理问题', width: 110 },
  { field: 'weekHours', title: '本周工时', width: 100 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<ProjectApi.MyRelatedItem>['columns'];

const taskColumns = [
  { field: 'taskCode', title: '编号', width: 110 },
  { align: 'left', field: 'title', slots: { default: 'title' }, title: '任务' },
  { field: 'projectName', title: '项目', width: 180 },
  { field: 'plannedEndTime', slots: { default: 'plannedEndTime' }, title: '计划完成', width: 130 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 110 },
] satisfies VxeGridProps<ProjectApi.TaskItem>['columns'];

const issueColumns = [
  { align: 'left', field: 'title', slots: { default: 'title' }, title: '问题项' },
  { field: 'projectName', title: '项目', width: 180 },
  { field: 'level', slots: { default: 'level' }, title: '等级', width: 90 },
  { field: 'dueDate', slots: { default: 'dueDate' }, title: '到期', width: 130 },
] satisfies VxeGridProps<ProjectApi.IssueItem>['columns'];

const worklogColumns = [
  { field: 'workDate', slots: { default: 'workDate' }, title: '日期', width: 120 },
  { align: 'left', field: 'projectName', title: '项目', width: 180 },
  { align: 'left', field: 'taskTitle', title: '关联事项', width: 220 },
  { align: 'left', field: 'summary', title: '工作摘要' },
  { field: 'hours', title: '工时', width: 90 },
] satisfies VxeGridProps<ProjectApi.WorklogItem>['columns'];

const projectGridOptions = computed<VxeGridProps<ProjectApi.MyRelatedItem>>(() => ({
  columns: projectColumns,
  data: payload.value?.myProjects || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const taskGridOptions = computed<VxeGridProps<ProjectApi.TaskItem>>(() => ({
  columns: taskColumns,
  data: payload.value?.myTasks || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'small',
}));

const issueGridOptions = computed<VxeGridProps<ProjectApi.IssueItem>>(() => ({
  columns: issueColumns,
  data: payload.value?.myIssues || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'small',
}));

const worklogGridOptions = computed<VxeGridProps<ProjectApi.WorklogItem>>(() => ({
  columns: worklogColumns,
  data: worklogPagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: worklogPagination.pagerConfig.value,
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
  size: 'small',
}));

const [ProjectGrid] = useVbenVxeGrid<ProjectApi.MyRelatedItem>({ gridOptions: {} });
const [TaskGrid] = useVbenVxeGrid<ProjectApi.TaskItem>({ gridOptions: {} });
const [IssueGrid] = useVbenVxeGrid<ProjectApi.IssueItem>({ gridOptions: {} });
const worklogGridEvents: NativeVxeGridEvents<ProjectApi.WorklogItem> = {
  'page-change': worklogPagination.handlePageChange,
};
const [WorklogGrid] = useVbenVxeGrid<ProjectApi.WorklogItem>({
  gridEvents: worklogGridEvents,
  gridOptions: {},
});

const primaryActions = computed(() => (payload.value?.actionItems || []).slice(0, 6));
const worklogCompletion = computed(() => {
  const weekHours = payload.value?.metrics.find((item) => item.label === '本周工时')?.value || 0;
  return Math.min(100, Math.round((weekHours / 40) * 100));
});

async function loadData() {
  loading.value = true;
  try {
    payload.value = await getProjectMyRelatedDashboardApi();
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
}

function formatMetric(metric: ProjectApi.DashboardMetric) {
  return `${metric.value}${metric.unit ? ` ${metric.unit}` : ''}`;
}

function resolvePriorityColor(priority?: null | string) {
  if (priority === 'P0') {
    return 'error';
  }

  if (priority === 'P1') {
    return 'warning';
  }

  if (priority === 'P2') {
    return 'blue';
  }

  return 'default';
}

function openDetail(projectId: string) {
  localStorage.setItem('general:last-project-id', projectId);
  router.push({ path: '/project/detail' });
}

function openWorklog() {
  router.push({
    path: '/project/detail',
    query: {
      tab: 'resources',
      worklog: '1',
    },
  });
}

onMounted(loadData);
</script>

<template>
  <Page description="聚合当前用户参与项目、待办问题和工时回填，作为个人执行入口。" title="与我相关">
    <section class="my-related">
      <Card :bordered="false" class="my-related__hero rounded-2xl">
        <div class="my-related__hero-body">
          <div class="my-related__hero-main">
            <span class="my-related__eyebrow">个人执行台</span>
            <h2>我的项目事项</h2>
            <p>优先处理阻塞、逾期和 7 天内到期事项，工时和问题从这里快速回到项目详情闭环。</p>
          </div>
          <div class="my-related__worklog-ring">
            <Progress type="circle" :percent="worklogCompletion" :width="86" />
            <span>本周工时完成度</span>
          </div>
        </div>
      </Card>

      <Row :gutter="[16, 16]">
        <Col v-for="metric in payload?.metrics || []" :key="metric.label" :lg="6" :md="12" :span="24">
          <Card :bordered="false" class="my-related__metric-card rounded-2xl">
            <span>{{ metric.label }}</span>
            <strong>{{ formatMetric(metric) }}</strong>
          </Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :xl="9" :span="24">
          <Card :bordered="false" class="my-related__action-card rounded-2xl" title="优先行动">
            <div v-if="!loading && primaryActions.length === 0" class="my-related__empty my-related__empty--compact">
              <Empty description="暂无需要优先处理的事项" />
            </div>
            <div v-else class="my-related__action-list">
              <article v-for="item in primaryActions" :key="item.sourceId" class="my-related__action-item">
                <div>
                  <Tag :color="resolvePriorityColor(item.level)">{{ item.level || item.sourceType }}</Tag>
                  <strong>{{ item.title }}</strong>
                </div>
                <p>{{ item.projectName }} · {{ item.reason }} · {{ formatDate(item.dueDate) }}</p>
              </article>
            </div>
          </Card>
        </Col>
        <Col :xl="15" :span="24">
          <Card :bordered="false" class="rounded-2xl" title="我参与的项目">
            <div v-if="!loading && (payload?.myProjects.length || 0) === 0" class="my-related__empty">
              <Empty description="暂无项目关系" />
            </div>
            <ProjectGrid
              v-else
              class="app-vxe-card-grid"
              :grid-options="projectGridOptions"
            >
              <template #name="{ row }">
                <Button size="small" type="link" @click="openDetail(row.id)">
                  {{ row.name }}
                </Button>
              </template>
              <template #myRelation="{ row }">
                <Tag color="processing">{{ row.myRelation }}</Tag>
              </template>
              <template #actions="{ row }">
                <Button size="small" type="link" @click="openDetail(row.id)">进入详情</Button>
              </template>
            </ProjectGrid>
          </Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :xl="12" :span="24">
          <Card :bordered="false" class="rounded-2xl" title="我的任务">
            <TaskGrid class="app-vxe-card-grid" :grid-options="taskGridOptions">
              <template #title="{ row }">
                <Space>
                  <span>{{ row.title }}</span>
                  <Tag v-if="row.isBlocked" color="error">阻塞</Tag>
                </Space>
              </template>
              <template #plannedEndTime="{ row }">
                {{ formatDate(row.plannedEndTime) }}
              </template>
              <template #status="{ row }">
                <Tag :color="row.isBlocked ? 'error' : 'blue'">{{ row.status }}</Tag>
              </template>
            </TaskGrid>
          </Card>
        </Col>
        <Col :xl="12" :span="24">
          <Card :bordered="false" class="rounded-2xl" title="我的问题">
            <IssueGrid class="app-vxe-card-grid" :grid-options="issueGridOptions">
              <template #title="{ row }">
                <span>{{ row.title }}</span>
              </template>
              <template #dueDate="{ row }">
                {{ formatDate(row.dueDate) }}
              </template>
              <template #level="{ row }">
                <Tag :color="resolvePriorityColor(row.level)">{{ row.level }}</Tag>
              </template>
            </IssueGrid>
          </Card>
        </Col>
      </Row>

      <Card :bordered="false" class="rounded-2xl" title="最近工时回填">
        <template #extra>
          <Button type="primary" @click="openWorklog">填写工时</Button>
        </template>
        <WorklogGrid
          class="app-vxe-card-grid"
          :grid-events="worklogGridEvents"
          :grid-options="worklogGridOptions"
        >
          <template #workDate="{ row }">
            {{ formatDate(row.workDate) }}
          </template>
        </WorklogGrid>
      </Card>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.my-related {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.my-related__hero {
  overflow: hidden;
}

.my-related__hero-body {
  display: flex;
  @apply gap-6;
  align-items: center;
  justify-content: space-between;
}

.my-related__hero-main {
  min-width: 0;
}

.my-related__eyebrow {
  color: var(--ant-color-primary);
  font-size: 13px;
  font-weight: 600;
}

.my-related__hero h2 {
  @apply my-1.5;
  font-size: 24px;
}

.my-related__hero p {
  max-width: 760px;
  @apply m-0;
  color: var(--ant-color-text-secondary);
}

.my-related__worklog-ring {
  display: grid;
  flex: 0 0 auto;
  @apply gap-2;
  place-items: center;
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.my-related__metric-card span {
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.my-related__metric-card strong {
  display: block;
  @apply mt-2;
  color: var(--ant-color-text);
  font-size: 24px;
}

.my-related__action-card {
  height: 100%;
}

.my-related__action-list {
  display: grid;
  @apply gap-2.5;
}

.my-related__action-item {
  @apply p-3;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.my-related__action-item > div {
  display: flex;
  @apply gap-2;
  align-items: center;
}

.my-related__action-item p {
  @apply mt-2;
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.my-related__empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}

.my-related__empty--compact {
  min-height: 180px;
}

@media (max-width: 768px) {
  .my-related__hero-body {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
