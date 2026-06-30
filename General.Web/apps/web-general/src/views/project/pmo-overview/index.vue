<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Card, Col, Empty, Progress, Row, Space, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getProjectPmoDashboardApi, type ProjectApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'ProjectPmoOverviewPage' });

const router = useRouter();
const loading = ref(false);
const dashboard = ref<null | ProjectApi.PmoDashboard>(null);
const actionRows = computed(() => dashboard.value?.actionItems || []);
const actionPagination = useLocalVxePagination(actionRows, { defaultPageSize: 20 });

const projectColumns = [
  { field: 'projectCode', title: '项目编号', width: 140 },
  { align: 'left', field: 'name', slots: { default: 'name' }, title: '项目名称', width: 240 },
  { field: 'managerName', title: 'PM', width: 120 },
  { field: 'organizationUnitName', title: '部门', width: 160 },
  { field: 'healthStatus', slots: { default: 'healthStatus' }, title: '健康', width: 100 },
  { field: 'progress', slots: { default: 'progress' }, title: '进度', width: 150 },
  { field: 'schedulePerformanceIndex', slots: { default: 'spi' }, title: 'SPI', width: 90 },
  { field: 'costPerformanceIndex', slots: { default: 'cpi' }, title: 'CPI', width: 90 },
  { field: 'overdueTaskCount', title: '逾期任务', width: 100 },
  { field: 'highRiskCount', title: 'P0风险', width: 90 },
  { fixed: 'right', slots: { default: 'actions' }, title: '操作', width: 120 },
] satisfies VxeGridProps<ProjectApi.DashboardProjectCard>['columns'];

const actionColumns = [
  { field: 'level', slots: { default: 'level' }, title: '等级', width: 90 },
  { align: 'left', field: 'title', slots: { default: 'title' }, title: '治理事项' },
  { field: 'projectName', title: '项目', width: 220 },
  { field: 'sourceType', title: '类型', width: 100 },
  { field: 'reason', title: '触发原因', width: 130 },
  { field: 'dueDate', slots: { default: 'dueDate' }, title: '到期', width: 120 },
] satisfies VxeGridProps<ProjectApi.DashboardActionItem>['columns'];

const projectGridOptions = computed<VxeGridProps<ProjectApi.DashboardProjectCard>>(() => ({
  columns: projectColumns,
  data: dashboard.value?.projects || [],
  loading: loading.value,
  pagerConfig: { enabled: false },
  rowConfig: { keyField: 'id' },
  scrollX: { enabled: true },
}));

const actionGridOptions = computed<VxeGridProps<ProjectApi.DashboardActionItem>>(() => ({
  columns: actionColumns,
  data: actionPagination.pagedRows.value,
  loading: loading.value,
  pagerConfig: actionPagination.pagerConfig.value,
  rowConfig: { keyField: 'sourceId' },
  scrollX: { enabled: true },
  size: 'small',
}));

const [ProjectGrid] = useVbenVxeGrid<ProjectApi.DashboardProjectCard>({ gridOptions: {} });
const actionGridEvents: NativeVxeGridEvents<ProjectApi.DashboardActionItem> = {
  'page-change': actionPagination.handlePageChange,
};
const [ActionGrid] = useVbenVxeGrid<ProjectApi.DashboardActionItem>({
  gridEvents: actionGridEvents,
  gridOptions: {},
});

const exceptionProjects = computed(() => dashboard.value?.exceptionProjects || []);
const departmentPressures = computed(() => dashboard.value?.departmentPressures || []);
const businessRisks = computed(() => dashboard.value?.businessRisks || []);
const earnedValueMetrics = computed(() => {
  const summary = dashboard.value?.earnedValueSummary;
  return [
    { label: 'PV', value: formatMoney(summary?.plannedValue) },
    { label: 'EV', value: formatMoney(summary?.earnedValue) },
    { label: 'AC', value: formatMoney(summary?.actualCost) },
    { label: 'SPI', value: formatRatio(summary?.schedulePerformanceIndex) },
    { label: 'CPI', value: formatRatio(summary?.costPerformanceIndex) },
  ];
});

async function loadDashboard() {
  loading.value = true;
  try {
    dashboard.value = await getProjectPmoDashboardApi();
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

function formatMoney(value?: null | number) {
  if (value === null || value === undefined) {
    return '-';
  }

  return new Intl.NumberFormat('zh-CN', {
    maximumFractionDigits: 1,
    minimumFractionDigits: 0,
    style: 'currency',
    currency: 'CNY',
  }).format(value);
}

function formatRatio(value?: null | number) {
  return value === null || value === undefined ? '-' : value.toFixed(2);
}

function getHealthColor(status?: null | string) {
  if (status === '高风险') {
    return 'error';
  }

  if (status === '关注中') {
    return 'warning';
  }

  return 'success';
}

function getPriorityColor(priority?: null | string) {
  if (priority === 'P0') {
    return 'error';
  }

  if (priority === 'P1') {
    return 'warning';
  }

  return 'blue';
}

function openDetail(projectId: string) {
  localStorage.setItem('general:last-project-id', projectId);
  router.push({ path: '/project/detail' });
}

onMounted(loadDashboard);
</script>

<template>
  <Page description="面向 PMO 的项目组合治理视图，按权限范围汇总执行异常和治理动作。" title="PMO 执行总览">
    <section class="pmo-overview">
      <Row :gutter="[16, 16]">
        <Col v-for="metric in dashboard?.metrics || []" :key="metric.label" :xl="3" :lg="6" :md="8" :span="12">
          <Card :bordered="false" class="pmo-overview__metric rounded-2xl">
            <span>{{ metric.label }}</span>
            <strong>{{ formatMetric(metric) }}</strong>
          </Card>
        </Col>
      </Row>

      <Card :bordered="false" class="pmo-overview__evm-summary rounded-2xl" title="组合挣值摘要">
        <div class="pmo-overview__evm-summary-grid">
          <article v-for="item in earnedValueMetrics" :key="item.label">
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </article>
        </div>
        <small>最新快照：{{ formatDate(dashboard?.earnedValueSummary.snapshotDate) }}</small>
      </Card>

      <Row :gutter="[16, 16]">
        <Col :xl="8" :span="24">
          <Card :bordered="false" class="pmo-overview__panel rounded-2xl" title="异常项目排行">
            <div v-if="!loading && exceptionProjects.length === 0" class="pmo-overview__empty pmo-overview__empty--compact">
              <Empty description="暂无异常项目" />
            </div>
            <div v-else class="pmo-overview__exception-list">
              <article v-for="item in exceptionProjects" :key="item.id" @click="openDetail(item.id)">
                <div>
                  <Tag :color="getHealthColor(item.healthStatus)">{{ item.healthStatus }}</Tag>
                  <strong>{{ item.name }}</strong>
                </div>
                <span>逾期 {{ item.overdueTaskCount }} · P0风险 {{ item.highRiskCount }} · PM {{ item.managerName }}</span>
              </article>
            </div>
          </Card>
        </Col>
        <Col :xl="8" :span="24">
          <Card :bordered="false" class="pmo-overview__panel rounded-2xl" title="部门压力">
            <div v-if="!loading && departmentPressures.length === 0" class="pmo-overview__empty pmo-overview__empty--compact">
              <Empty description="暂无部门数据" />
            </div>
            <div v-else class="pmo-overview__department-list">
              <article v-for="item in departmentPressures" :key="item.name">
                <div>
                  <strong>{{ item.name }}</strong>
                  <span>{{ item.projectCount }} 个项目</span>
                </div>
                <Progress :percent="Math.min(100, item.riskScore)" size="small" :show-info="false" />
              </article>
            </div>
          </Card>
        </Col>
        <Col :xl="8" :span="24">
          <Card :bordered="false" class="pmo-overview__panel rounded-2xl" title="经营联动风险">
            <div v-if="!loading && businessRisks.length === 0" class="pmo-overview__empty pmo-overview__empty--compact">
              <Empty description="暂无经营风险" />
            </div>
            <div v-else class="pmo-overview__business-list">
              <article v-for="item in businessRisks" :key="item.projectId" @click="openDetail(item.projectId)">
                <div>
                  <Tag color="warning">{{ item.riskLabel }}</Tag>
                  <strong>{{ item.projectName }}</strong>
                </div>
                <span>合同 {{ formatMoney(item.contractAmount) }} · 回款 {{ formatMoney(item.receivedAmount) }}</span>
              </article>
            </div>
          </Card>
        </Col>
      </Row>

      <Card id="pmo-overview-exceptions" :bordered="false" class="rounded-2xl" title="项目组合健康矩阵">
        <ProjectGrid class="app-vxe-card-grid" :grid-options="projectGridOptions">
          <template #name="{ row }">
            <Space>
              <Button size="small" type="link" @click="openDetail(row.id)">{{ row.name }}</Button>
              <Tag v-if="row.isKeyProject" color="gold">重点</Tag>
            </Space>
          </template>
          <template #healthStatus="{ row }">
            <Tag :color="getHealthColor(row.healthStatus)">{{ row.healthStatus }}</Tag>
          </template>
          <template #progress="{ row }">
            <Progress :percent="row.progress" size="small" />
          </template>
          <template #spi="{ row }">
            {{ formatRatio(row.schedulePerformanceIndex) }}
          </template>
          <template #cpi="{ row }">
            {{ formatRatio(row.costPerformanceIndex) }}
          </template>
          <template #actions="{ row }">
            <Button size="small" type="link" @click="openDetail(row.id)">进入详情</Button>
          </template>
        </ProjectGrid>
      </Card>

      <Card :bordered="false" class="rounded-2xl" title="PMO 治理行动队列">
        <ActionGrid
          class="app-vxe-card-grid"
          :grid-events="actionGridEvents"
          :grid-options="actionGridOptions"
        >
          <template #level="{ row }">
            <Tag :color="getPriorityColor(row.level)">{{ row.level || row.sourceType }}</Tag>
          </template>
          <template #title="{ row }">
            <Button size="small" type="link" @click="openDetail(row.projectId)">{{ row.title }}</Button>
          </template>
          <template #dueDate="{ row }">
            {{ formatDate(row.dueDate) }}
          </template>
        </ActionGrid>
      </Card>
    </section>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.pmo-overview {
  display: flex;
  flex-direction: column;
  @apply gap-4;
}

.pmo-overview__metric span {
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.pmo-overview__metric strong {
  display: block;
  @apply mt-2;
  color: var(--ant-color-text);
  font-size: 24px;
  line-height: 1.2;
  white-space: nowrap;
}

.pmo-overview__panel {
  height: 100%;
}

.pmo-overview__evm-summary-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  @apply gap-3;
}

.pmo-overview__evm-summary-grid article {
  min-width: 0;
  @apply px-3 py-2.5;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.pmo-overview__evm-summary-grid span {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.pmo-overview__evm-summary-grid strong {
  display: block;
  @apply mt-1;
  overflow: hidden;
  color: var(--ant-color-text);
  font-size: 18px;
  line-height: 1.25;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.pmo-overview__evm-summary small {
  display: block;
  @apply mt-2.5;
  color: var(--ant-color-text-secondary);
}

.pmo-overview__exception-list,
.pmo-overview__department-list,
.pmo-overview__business-list {
  display: grid;
  @apply gap-2.5;
}

.pmo-overview__exception-list article,
.pmo-overview__department-list article,
.pmo-overview__business-list article {
  @apply p-3;
  cursor: pointer;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.pmo-overview__department-list article {
  cursor: default;
}

.pmo-overview__exception-list article > div,
.pmo-overview__business-list article > div,
.pmo-overview__department-list article > div {
  display: flex;
  @apply gap-2;
  align-items: center;
  justify-content: space-between;
}

.pmo-overview__exception-list strong,
.pmo-overview__business-list strong,
.pmo-overview__department-list strong {
  min-width: 0;
}

.pmo-overview__exception-list span,
.pmo-overview__business-list span,
.pmo-overview__department-list span {
  display: block;
  @apply mt-1.5;
  color: var(--ant-color-text-secondary);
  font-size: 13px;
}

.pmo-overview__empty {
  display: grid;
  min-height: 220px;
  place-items: center;
}

.pmo-overview__empty--compact {
  min-height: 160px;
}

@media (max-width: 1200px) {
  .pmo-overview__evm-summary-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .pmo-overview__evm-summary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
