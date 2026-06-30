<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Skeleton, Tag } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getBusinessOverviewApi, type BusinessApi } from '#/api/core';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'BusinessOverviewPage' });

const router = useRouter();
const loading = ref(false);
const overview = ref<BusinessApi.Overview | null>(null);

const overviewColumns = [
  { field: 'projectCode', title: '项目编号', width: 140 },
  { align: 'left', field: 'name', title: '项目名称', width: 260 },
  { field: 'customerName', title: '客户', width: 180 },
  { field: 'managerName', title: '项目经理', width: 140 },
  {
    field: 'contractAmount',
    slots: { default: 'contractAmount' },
    title: '合同收入',
    width: 150,
  },
  {
    field: 'procurementAmount',
    slots: { default: 'procurementAmount' },
    title: '分包采购',
    width: 150,
  },
  {
    field: 'receivedAmount',
    slots: { default: 'receivedAmount' },
    title: '已回款',
    width: 150,
  },
  {
    field: 'grossProfitRate',
    slots: { default: 'grossProfitRate' },
    title: '毛利率',
    width: 110,
  },
  {
    field: 'businessCloseStatus',
    slots: { default: 'businessCloseStatus' },
    title: '经营状态',
    width: 150,
  },
  { fixed: 'right', slots: { default: 'action' }, title: '跳转', width: 110 },
] satisfies VxeGridProps<BusinessApi.ProjectSummary>['columns'];

const projectRows = computed(() => overview.value?.projects || []);
const projectPagination = useLocalVxePagination(projectRows, {
  defaultPageSize: 20,
});

const overviewGridOptions = computed<VxeGridProps<BusinessApi.ProjectSummary>>(
  () => ({
    columns: overviewColumns,
    data: projectPagination.pagedRows.value,
    height: 560,
    pagerConfig: projectPagination.pagerConfig.value,
    rowConfig: { keyField: 'id' },
    scrollX: { enabled: true },
    size: 'medium',
  }),
);

const overviewGridEvents: NativeVxeGridEvents<BusinessApi.ProjectSummary> = {
  'page-change': projectPagination.handlePageChange,
};

const [OverviewGrid] = useVbenVxeGrid<BusinessApi.ProjectSummary>({
  gridEvents: overviewGridEvents,
  gridOptions: {},
});

const metricItems = computed(() => {
  if (!overview.value) {
    return [];
  }

  return [
    {
      key: 'contract',
      label: '合同收入',
      value: formatMoney(overview.value.contractAmount),
    },
    {
      key: 'cost',
      label: '经营成本',
      value: formatMoney(overview.value.costAmount),
    },
    {
      key: 'profit',
      label: '累计毛利',
      value: formatMoney(overview.value.grossProfitAmount),
    },
    {
      key: 'received',
      label: '已回款',
      value: formatMoney(overview.value.receivedAmount),
    },
    {
      key: 'forecast',
      label: '年底预计回款',
      value: formatMoney(overview.value.yearEndReceivableForecast),
    },
    {
      key: 'rate',
      label: '综合毛利率',
      value:
        typeof overview.value.grossProfitRate === 'number'
          ? `${overview.value.grossProfitRate.toFixed(1)}%`
          : '--',
    },
  ];
});

const warningSummaryItems = computed(() => {
  if (!overview.value) {
    return [];
  }

  return [
    {
      key: 'profit-warning',
      label: '低毛利项目',
      value: `${overview.value.lowProfitProjectCount} 个`,
      description: '优先复核成本执行与补充合同影响',
      actionLabel: '利润分析',
      tab: 'profit-analysis',
    },
    {
      key: 'closing-warning',
      label: '待收尾项目',
      value: `${overview.value.tailClosingProjectCount} 个`,
      description: '执行接近结束但经营仍未闭环',
      actionLabel: '回款检索',
      tab: 'receivable-search',
    },
    {
      key: 'receivable-warning',
      label: '回款风险项目',
      value: `${overview.value.receivableRiskProjectCount} 个`,
      description: '存在待回款或年底回款压力',
      actionLabel: '预警明细',
      tab: 'warning-center',
    },
  ];
});

const warningDetailItems = computed(() => {
  if (!overview.value) {
    return [];
  }

  return overview.value.warningItems
    .filter((item) => !item.type.includes('绩效风险'))
    .slice(0, 4);
});

function formatMoney(value?: null | number) {
  if (typeof value !== 'number') {
    return '--';
  }

  return new Intl.NumberFormat('zh-CN', {
    maximumFractionDigits: 0,
    minimumFractionDigits: 0,
    style: 'currency',
    currency: 'CNY',
  }).format(value);
}

function resolvePriorityColor(priority?: null | string) {
  if (priority === 'P0') {
    return 'red';
  }

  if (priority === 'P1') {
    return 'warning';
  }

  if (priority === 'P2') {
    return 'blue';
  }

  return 'default';
}

function openProject(projectId: string, tab = 'overview') {
  router.push({
    path: '/business/projects',
    query: { projectId, tab },
  });
}

function openReport(tab: string) {
  router.push({
    path: '/business/reports',
    query: { tab },
  });
}

async function loadOverview() {
  loading.value = true;
  try {
    overview.value = await getBusinessOverviewApi();
  } finally {
    loading.value = false;
  }
}

onMounted(loadOverview);
</script>

<template>
  <Page
    description="管理层先看收入、成本、回款、利润与收尾风险，再进入单项目经营详情。"
    title="经营总览"
  >
    <Alert
      banner
      class="business-overview__notice"
      message="经营管理区二期已收敛为资金与合同视角，不再混入 EV / PV / CV、验收和项目执行健康度。"
      type="info"
    />

    <Skeleton v-if="loading" active :paragraph="{ rows: 8 }" />

    <template v-else-if="overview">
      <Card :bordered="false" class="business-overview__hero rounded-2xl">
        <div class="business-overview__hero-head">
          <div>
            <span class="business-overview__eyebrow">经营驾驶舱</span>
            <h3>当前执行中项目经营总览</h3>
          </div>
          <p>聚焦正在执行项目的收入、成本、回款、利润与收尾风险。</p>
        </div>
        <div class="business-overview__metrics">
          <article
            v-for="item in metricItems"
            :key="item.key"
            class="business-overview__metric-card"
          >
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </article>
        </div>
      </Card>

      <Card
        :bordered="false"
        class="business-overview__warning-board rounded-2xl"
        title="经营预警"
      >
        <div class="business-overview__warning-summary">
          <article
            v-for="item in warningSummaryItems"
            :key="item.key"
            :class="[
              'business-overview__warning-summary-item',
              `is-${item.key}`,
            ]"
          >
            <div class="business-overview__warning-summary-main">
              <div>
                <span>{{ item.label }}</span>
                <strong>{{ item.value }}</strong>
              </div>
              <Button class="shrink-0 px-0" size="small" type="link" @click="openReport(item.tab)">{{
                item.actionLabel
              }}</Button>
            </div>
            <p>{{ item.description }}</p>
          </article>
        </div>

        <div class="business-overview__warning-detail">
          <div class="business-overview__warning-header">
            <strong>重点异常</strong>
            <Button
              size="small"
              type="link"
              @click="openReport('warning-center')"
              >查看全部预警</Button
            >
          </div>
          <div class="business-overview__warning-rows">
            <div
              v-for="item in warningDetailItems"
              :key="item.id"
              class="business-overview__warning-row"
            >
              <div class="business-overview__warning-row-main">
                <div class="business-overview__warning-row-head">
                  <strong>{{ item.projectName }}</strong>
                  <Tag :color="resolvePriorityColor(item.level)">{{
                    item.level
                  }}</Tag>
                </div>
                <p>{{ item.type }}：{{ item.description }}</p>
                <span class="business-overview__warning-meta">
                  {{ item.projectCode }} · {{ item.ownerName }} ·
                  {{ item.lastUpdatedTime }}
                </span>
              </div>
              <Button
                class="shrink-0"
                size="small"
                @click="openProject(item.projectId, item.actionTab)"
                >查看</Button
              >
            </div>
          </div>
        </div>
      </Card>

      <Card
        :bordered="false"
        class="business-overview__table-card min-w-0 rounded-2xl"
        title="执行中经营项目"
      >
        <div class="min-w-0">
          <OverviewGrid
            class="app-vxe-card-grid business-overview__table"
            :grid-events="overviewGridEvents"
            :grid-options="overviewGridOptions"
          >
            <template #contractAmount="{ row }">{{
              formatMoney(row.contractAmount)
            }}</template>
            <template #procurementAmount="{ row }">{{
              formatMoney(row.procurementAmount)
            }}</template>
            <template #receivedAmount="{ row }">{{
              formatMoney(row.receivedAmount)
            }}</template>
            <template #grossProfitRate="{ row }">
              <Tag
                :color="
                  typeof row.grossProfitRate === 'number' &&
                  row.grossProfitRate < 25
                    ? 'red'
                    : 'green'
                "
              >
                {{
                  typeof row.grossProfitRate === 'number'
                    ? `${row.grossProfitRate.toFixed(1)}%`
                    : '--'
                }}
              </Tag>
            </template>
            <template #businessCloseStatus="{ row }">
              <Tag
                :color="
                  row.businessCloseStatus.includes('收尾') ||
                  row.businessCloseStatus.includes('待回款')
                    ? 'gold'
                    : 'blue'
                "
              >
                {{ row.businessCloseStatus }}
              </Tag>
            </template>
            <template #action="{ row }">
              <Button size="small" type="link" @click="openProject(row.id)"
                >进入项目</Button
              >
            </template>
          </OverviewGrid>
        </div>
      </Card>
    </template>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.business-overview__notice {
  @apply mb-4;
}

.business-overview__hero {
  background:
    linear-gradient(135deg, rgb(22 119 255 / 0.08), rgb(37 99 235 / 0.03)),
    var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.08);
}

.business-overview__warning-board,
.business-overview__table-card {
  @apply mt-1;
}

.business-overview__hero-head {
  align-items: end;
  display: flex;
  justify-content: space-between;
  @apply gap-4;
}

.business-overview__hero-head h3 {
  @apply mt-2;
  font-size: 24px;
}

.business-overview__hero-head p {
  color: var(--ant-color-text-secondary);
  @apply m-0;
  max-width: 420px;
  text-align: right;
}

.business-overview__eyebrow {
  color: var(--ant-color-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.business-overview__metrics {
  display: grid;
  @apply gap-3;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  @apply mt-4;
}

.business-overview__metric-card,
.business-overview__warning-metric,
.business-overview__list-item {
  background:
    linear-gradient(180deg, rgb(22 119 255 / 0.06), transparent 46%),
    var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.16);
  box-shadow: 0 8px 24px rgb(15 23 42 / 0.08);
  @apply rounded-2xl;
  display: grid;
  @apply gap-2;
  min-width: 0;
  @apply px-4 py-3.5;
  position: relative;
}

.business-overview__metric-card::before {
  background: linear-gradient(
    90deg,
    var(--ant-color-primary),
    rgb(22 119 255 / 0.18)
  );
  @apply rounded-full;
  content: '';
  height: 3px;
  left: 16px;
  position: absolute;
  top: 0;
  width: 36px;
}

.business-overview__metric-card span,
.business-overview__warning-metric span,
.business-overview__list-item p {
  color: var(--ant-color-text-secondary);
}

.business-overview__metric-card strong,
.business-overview__warning-metric strong {
  color: var(--ant-color-text);
  font-size: 24px;
}

.business-overview__warning-summary,
.business-overview__warning-rows {
  display: grid;
  @apply gap-3;
}

.business-overview__warning-summary {
  grid-template-columns: repeat(3, minmax(0, 1fr));
  @apply mb-4;
}

.business-overview__warning-summary-item {
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-2xl;
  display: grid;
  @apply gap-1.5;
  @apply px-4 py-3.5;
}

.business-overview__warning-summary-item.is-profit-warning {
  border-top: 3px solid #f59e0b;
}

.business-overview__warning-summary-item.is-closing-warning {
  border-top: 3px solid #1677ff;
}

.business-overview__warning-summary-item.is-receivable-warning {
  border-top: 3px solid #ef4444;
}

.business-overview__warning-summary-main {
  align-items: baseline;
  display: flex;
  @apply gap-3;
  justify-content: space-between;
}

.business-overview__warning-summary-main > div {
  display: grid;
  @apply gap-1.5;
}

.business-overview__warning-summary-main strong {
  color: var(--ant-color-text);
  font-size: 24px;
}

.business-overview__warning-summary-item p,
.business-overview__warning-row p {
  color: var(--ant-color-text-secondary);
  @apply m-0;
}

.business-overview__warning-detail {
  border-top: 1px solid var(--ant-color-border-secondary);
  @apply pt-4;
}

.business-overview__warning-header {
  align-items: center;
  display: flex;
  justify-content: space-between;
  @apply mb-3;
}

.business-overview__warning-row {
  align-items: center;
  display: flex;
  @apply gap-4;
  justify-content: space-between;
  @apply py-3;
}

.business-overview__warning-row-head {
  align-items: center;
  display: flex;
  @apply gap-2.5;
}

.business-overview__warning-row + .business-overview__warning-row {
  border-top: 1px solid var(--ant-color-border-secondary);
}

.business-overview__warning-row-main {
  min-width: 0;
}

.business-overview__warning-row strong {
  color: var(--ant-color-text);
  display: block;
}

.business-overview__warning-row-main,
.business-overview__warning-row p,
.business-overview__table {
  min-width: 0;
}

.business-overview__warning-row p {
  @apply mt-1;
}

.business-overview__warning-meta {
  color: var(--ant-color-text-tertiary);
  display: inline-block;
  font-size: 12px;
  @apply mt-1.5;
}

@media (max-width: 1400px) {
  .business-overview__metrics {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 1200px) {
  .business-overview__hero-head {
    align-items: flex-start;
    flex-direction: column;
  }

  .business-overview__hero-head p {
    max-width: none;
    text-align: left;
  }

  .business-overview__warning-summary {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .business-overview__metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
