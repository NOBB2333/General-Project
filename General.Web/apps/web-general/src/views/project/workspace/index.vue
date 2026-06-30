<script lang="ts" setup>
import type { VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, Empty, Row, Space, Statistic, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getProjectWorkspaceApi, type ProjectApi } from '#/api/core';

defineOptions({ name: 'ProjectWorkspacePage' });

const router = useRouter();
const loading = ref(false);
const workspace = ref<null | ProjectApi.WorkspaceItem>(null);

const projectColumns = [
  { field: 'projectCode', title: '项目编号', width: 140 },
  { align: 'left', field: 'name', title: '项目名称' },
  { field: 'managerName', title: '项目经理', width: 120 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 120 },
  { field: 'overdueTaskCount', title: '逾期任务', width: 100 },
] satisfies VxeGridProps<ProjectApi.ProjectListItem>['columns'];

const taskColumns = [
  { field: 'taskCode', title: '任务编号', width: 120 },
  { align: 'left', field: 'title', title: '任务标题' },
  { field: 'projectName', title: '所属项目', width: 180 },
  { field: 'status', slots: { default: 'status' }, title: '状态', width: 120 },
  { field: 'plannedEndTime', slots: { default: 'plannedEndTime' }, title: '计划完成', width: 180 },
] satisfies VxeGridProps<ProjectApi.TaskItem>['columns'];

const milestoneColumns = [
  { align: 'left', field: 'name', title: '里程碑' },
  { field: 'projectName', title: '所属项目', width: 180 },
  { field: 'ownerName', title: '责任人', width: 120 },
  { field: 'plannedCompletionDate', slots: { default: 'plannedCompletionDate' }, title: '计划日期', width: 160 },
] satisfies VxeGridProps<ProjectApi.WorkspaceMilestoneItem>['columns'];

const raidColumns = [
  { field: 'type', title: '类型', width: 120 },
  { align: 'left', field: 'title', title: '事项' },
  { field: 'projectName', title: '所属项目', width: 180 },
  { field: 'level', slots: { default: 'level' }, title: '等级', width: 100 },
  { field: 'plannedResolveDate', slots: { default: 'plannedResolveDate' }, title: '计划处理', width: 160 },
] satisfies VxeGridProps<ProjectApi.RaidItem>['columns'];

const projectGridOptions = computed<VxeGridProps<ProjectApi.ProjectListItem>>(() => ({
  columns: projectColumns,
  data: workspace.value?.keyProjects || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  size: 'small',
}));

const taskGridOptions = computed<VxeGridProps<ProjectApi.TaskItem>>(() => ({
  columns: taskColumns,
  data: workspace.value?.myTasks || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  size: 'small',
}));

const milestoneGridOptions = computed<VxeGridProps<ProjectApi.WorkspaceMilestoneItem>>(() => ({
  columns: milestoneColumns,
  data: workspace.value?.weeklyMilestones || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  size: 'small',
}));

const raidGridOptions = computed<VxeGridProps<ProjectApi.RaidItem>>(() => ({
  columns: raidColumns,
  data: workspace.value?.raidAlerts || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  size: 'small',
}));

const [ProjectGrid] = useVbenVxeGrid<ProjectApi.ProjectListItem>({ gridOptions: {} });
const [TaskGrid] = useVbenVxeGrid<ProjectApi.TaskItem>({ gridOptions: {} });
const [MilestoneGrid] = useVbenVxeGrid<ProjectApi.WorkspaceMilestoneItem>({ gridOptions: {} });
const [RaidGrid] = useVbenVxeGrid<ProjectApi.RaidItem>({ gridOptions: {} });

async function loadWorkspace() {
  loading.value = true;
  try {
    workspace.value = await getProjectWorkspaceApi();
  } finally {
    loading.value = false;
  }
}

function formatDate(value?: null | string) {
  return value ? new Date(value).toLocaleDateString() : '-';
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

onMounted(loadWorkspace);
</script>

<template>
  <Page description="项目工作台按一期口径聚合我的任务、关键项目、周里程碑和 RAID 预警。" title="工作台">
    <section class="project-workspace">
      <Card :bordered="false" class="project-workspace__hero rounded-2xl">
        <div class="project-workspace__copy">
          <p class="project-workspace__eyebrow">Phase 1 Delivery</p>
          <h2>项目执行闭环已经接入真实数据：项目、任务、里程碑、RAID 同屏查看。</h2>
          <p>
            当前工作台优先服务 PM / PMO 的高频路径，后续继续补项目立项、PM 工作台和 PMO 总览的深度交互。
          </p>
          <Space wrap>
            <Button type="primary" @click="router.push('/project/projects')">进入项目列表</Button>
            <Button @click="router.push('/project/tasks')">进入任务页</Button>
            <Button @click="router.push('/project/raid')">查看 RAID</Button>
          </Space>
        </div>
      </Card>

      <Row :gutter="[16, 16]">
        <Col :lg="6" :md="12" :span="24">
          <Card class="rounded-2xl" :bordered="false"><Statistic title="进行中项目" :value="workspace?.ongoingProjectCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card class="rounded-2xl" :bordered="false"><Statistic title="我的待办任务" :value="workspace?.myTodoCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card class="rounded-2xl" :bordered="false"><Statistic title="阻塞任务" :value="workspace?.blockedTaskCount || 0" /></Card>
        </Col>
        <Col :lg="6" :md="12" :span="24">
          <Card class="rounded-2xl" :bordered="false"><Statistic title="高风险事项" :value="workspace?.highRiskCount || 0" /></Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :lg="14" :span="24">
          <Card class="rounded-2xl" id="workspace-key-projects" :bordered="false" title="关键项目">
            <div v-if="!loading && (workspace?.keyProjects.length || 0) === 0" class="project-workspace__empty">
              <Empty description="暂无项目数据" />
            </div>
            <ProjectGrid
              v-else
              class="app-vxe-card-grid"
              :grid-options="projectGridOptions"
            >
              <template #status="{ row }">
                <Tag :color="row.isKeyProject ? 'gold' : 'blue'">
                  {{ row.status }}
                </Tag>
              </template>
            </ProjectGrid>
          </Card>
        </Col>

        <Col :lg="10" :span="24">
          <Card class="rounded-2xl" id="workspace-my-tasks" :bordered="false" title="我的任务">
            <div v-if="!loading && (workspace?.myTasks.length || 0) === 0" class="project-workspace__empty">
              <Empty description="暂无任务" />
            </div>
            <TaskGrid
              v-else
              class="app-vxe-card-grid"
              :grid-options="taskGridOptions"
            >
              <template #plannedEndTime="{ row }">
                {{ formatDate(row.plannedEndTime) }}
              </template>
              <template #status="{ row }">
                <Tag :color="row.isBlocked ? 'error' : 'processing'">
                  {{ row.status }}
                </Tag>
              </template>
            </TaskGrid>
          </Card>
        </Col>
      </Row>

      <Row :gutter="[16, 16]">
        <Col :lg="12" :span="24">
          <Card class="rounded-2xl" id="workspace-milestones" :bordered="false" title="本周关键里程碑">
            <MilestoneGrid class="app-vxe-card-grid" :grid-options="milestoneGridOptions">
              <template #plannedCompletionDate="{ row }">
                {{ formatDate(row.plannedCompletionDate) }}
              </template>
            </MilestoneGrid>
          </Card>
        </Col>

        <Col :lg="12" :span="24">
          <Card class="rounded-2xl" id="workspace-raid" :bordered="false" title="RAID 预警">
            <RaidGrid class="app-vxe-card-grid" :grid-options="raidGridOptions">
              <template #level="{ row }">
                <Tag :color="resolvePriorityColor(row.level)">
                  {{ row.level }}
                </Tag>
              </template>
              <template #plannedResolveDate="{ row }">
                {{ formatDate(row.plannedResolveDate) }}
              </template>
            </RaidGrid>
          </Card>
        </Col>
      </Row>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.project-workspace {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.project-workspace__hero {
  background:
    radial-gradient(circle at top left, rgb(37 99 235 / 0.18), transparent 34%),
    linear-gradient(135deg, var(--ant-color-bg-container), var(--ant-color-fill-quaternary));
}

.project-workspace__copy h2 {
  @apply mb-3;
  color: var(--ant-color-text);
  font-size: 24px;
  line-height: 1.25;
}

.project-workspace__copy p {
  color: var(--ant-color-text-secondary);
  line-height: 1.8;
}

.project-workspace__eyebrow {
  @apply mb-2.5;
  color: var(--ant-color-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.project-workspace__empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}
</style>
