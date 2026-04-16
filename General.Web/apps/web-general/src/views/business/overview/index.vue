<script lang="ts" setup>
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Skeleton, Table, Tag } from 'ant-design-vue';

import { getBusinessOverviewApi, type BusinessApi } from '#/api/core';

defineOptions({ name: 'BusinessOverviewPage' });

const router = useRouter();
const loading = ref(false);
const overview = ref<BusinessApi.Overview | null>(null);

const overviewColumns = [
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 140 },
  { dataIndex: 'name', key: 'name', title: '项目名称', width: 260 },
  { dataIndex: 'customerName', key: 'customerName', title: '客户', width: 180 },
  { dataIndex: 'managerName', key: 'managerName', title: '项目经理', width: 140 },
  { dataIndex: 'contractAmount', key: 'contractAmount', title: '合同收入', width: 150 },
  { dataIndex: 'procurementAmount', key: 'procurementAmount', title: '分包采购', width: 150 },
  { dataIndex: 'receivedAmount', key: 'receivedAmount', title: '已回款', width: 150 },
  { dataIndex: 'grossProfitRate', key: 'grossProfitRate', title: '毛利率', width: 110 },
  { dataIndex: 'businessCloseStatus', key: 'businessCloseStatus', title: '经营状态', width: 150 },
  { dataIndex: 'action', key: 'action', title: '跳转', width: 110 },
];

const metricItems = computed(() => {
  if (!overview.value) {
    return [];
  }

  return [
    { key: 'contract', label: '合同收入', value: formatMoney(overview.value.contractAmount) },
    { key: 'cost', label: '经营成本', value: formatMoney(overview.value.costAmount) },
    { key: 'profit', label: '累计毛利', value: formatMoney(overview.value.grossProfitAmount) },
    { key: 'received', label: '已回款', value: formatMoney(overview.value.receivedAmount) },
    { key: 'forecast', label: '年底预计回款', value: formatMoney(overview.value.yearEndReceivableForecast) },
    {
      key: 'rate',
      label: '综合毛利率',
      value: typeof overview.value.grossProfitRate === 'number' ? `${overview.value.grossProfitRate.toFixed(1)}%` : '--',
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

  return overview.value.warningItems.slice(0, 4);
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
  <Page description="管理层先看收入、成本、回款、利润与收尾风险，再进入单项目经营详情。" title="经营总览">
    <Alert
      banner
      class="mb-4"
      message="经营管理区二期已收敛为资金与合同视角，不再混入 EV / PV / CV、验收和项目执行健康度。"
      type="info"
    />

    <Skeleton v-if="loading" active :paragraph="{ rows: 8 }" />

    <template v-else-if="overview">
      <Card :bordered="false" class="business-overview__hero">
        <div class="business-overview__hero-head">
          <div>
            <span class="business-overview__eyebrow">经营驾驶舱</span>
            <h3>当前执行中项目经营总览</h3>
          </div>
          <p>聚焦正在执行项目的收入、成本、回款、利润与收尾风险。</p>
        </div>
        <div class="business-overview__metrics">
          <article v-for="item in metricItems" :key="item.key" class="business-overview__metric-card">
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </article>
        </div>
      </Card>

      <Card :bordered="false" class="business-overview__warning-board mt-1" title="经营预警">
        <div class="business-overview__warning-summary">
          <article
            v-for="item in warningSummaryItems"
            :key="item.key"
            :class="['business-overview__warning-summary-item', `is-${item.key}`]"
          >
            <div class="business-overview__warning-summary-main">
              <div>
                <span>{{ item.label }}</span>
                <strong>{{ item.value }}</strong>
              </div>
              <Button size="small" type="link" @click="openReport(item.tab)">{{ item.actionLabel }}</Button>
            </div>
            <p>{{ item.description }}</p>
          </article>
        </div>

        <div class="business-overview__warning-detail">
          <div class="business-overview__warning-header">
            <strong>重点异常</strong>
            <Button size="small" type="link" @click="openReport('warning-center')">查看全部预警</Button>
          </div>
          <div class="business-overview__warning-rows">
            <div v-for="item in warningDetailItems" :key="item.id" class="business-overview__warning-row">
              <div class="business-overview__warning-row-main">
                <div class="business-overview__warning-row-head">
                  <strong>{{ item.projectName }}</strong>
                  <Tag :color="item.level === '高' ? 'red' : 'gold'">{{ item.level }}级</Tag>
                </div>
                <p>{{ item.type }}：{{ item.description }}</p>
                <span class="business-overview__warning-meta">
                  {{ item.projectCode }} · {{ item.ownerName }} · {{ item.lastUpdatedTime }}
                </span>
              </div>
              <Button size="small" @click="openProject(item.projectId, item.actionTab)">查看</Button>
            </div>
          </div>
        </div>
      </Card>

      <Card :bordered="false" class="business-overview__table-card mt-1" title="执行中经营项目">
        <Table
          class="business-overview__table"
          :columns="overviewColumns"
          :data-source="overview.projects"
          :pagination="false"
          row-key="id"
          :scroll="{ x: 'max-content', y: 560 }"
          size="middle"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'contractAmount'">{{ formatMoney(record.contractAmount) }}</template>
            <template v-else-if="column.key === 'procurementAmount'">{{ formatMoney(record.procurementAmount) }}</template>
            <template v-else-if="column.key === 'receivedAmount'">{{ formatMoney(record.receivedAmount) }}</template>
            <template v-else-if="column.key === 'grossProfitRate'">
              <Tag :color="typeof record.grossProfitRate === 'number' && record.grossProfitRate < 25 ? 'red' : 'green'">
                {{ typeof record.grossProfitRate === 'number' ? `${record.grossProfitRate.toFixed(1)}%` : '--' }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'businessCloseStatus'">
              <Tag :color="record.businessCloseStatus.includes('收尾') || record.businessCloseStatus.includes('待回款') ? 'gold' : 'blue'">
                {{ record.businessCloseStatus }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'action'">
              <Button size="small" type="link" @click="openProject(record.id)">进入项目</Button>
            </template>
          </template>
        </Table>
      </Card>
    </template>
  </Page>
</template>

<style scoped>
.business-overview__hero {
  background:
    linear-gradient(135deg, rgb(22 119 255 / 0.08), rgb(37 99 235 / 0.03)),
    var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.08);
}

.business-overview__hero-head {
  align-items: end;
  display: flex;
  justify-content: space-between;
  gap: 16px;
}

.business-overview__hero-head h3 {
  margin: 8px 0 0;
  font-size: 24px;
}

.business-overview__hero-head p {
  color: var(--ant-color-text-secondary);
  margin: 0;
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
  gap: 12px;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  margin-top: 16px;
}

.business-overview__metric-card,
.business-overview__warning-metric,
.business-overview__list-item {
  background:
    linear-gradient(180deg, rgb(22 119 255 / 0.06), transparent 46%),
    var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.16);
  box-shadow: 0 8px 24px rgb(15 23 42 / 0.08);
  border-radius: 16px;
  display: grid;
  gap: 8px;
  min-width: 0;
  padding: 14px 16px;
  position: relative;
}

.business-overview__metric-card::before {
  background: linear-gradient(90deg, var(--ant-color-primary), rgb(22 119 255 / 0.18));
  border-radius: 999px;
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
  font-size: 22px;
}

.business-overview__warning-summary,
.business-overview__warning-rows {
  display: grid;
  gap: 12px;
}

.business-overview__warning-summary {
  grid-template-columns: repeat(3, minmax(0, 1fr));
  margin-bottom: 16px;
}

.business-overview__warning-summary-item {
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  border-radius: 16px;
  display: grid;
  gap: 6px;
  padding: 14px 16px;
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
  gap: 12px;
  justify-content: space-between;
}

.business-overview__warning-summary-main > div {
  display: grid;
  gap: 6px;
}

.business-overview__warning-summary-main strong {
  color: var(--ant-color-text);
  font-size: 22px;
}

.business-overview__warning-summary-item :deep(.ant-btn-link) {
  flex-shrink: 0;
  padding-inline: 0;
}

.business-overview__warning-summary-item p,
.business-overview__warning-row p {
  color: var(--ant-color-text-secondary);
  margin: 0;
}

.business-overview__warning-detail {
  border-top: 1px solid var(--ant-color-border-secondary);
  padding-top: 16px;
}

.business-overview__warning-header {
  align-items: center;
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
}

.business-overview__warning-row {
  align-items: center;
  display: flex;
  gap: 16px;
  justify-content: space-between;
  padding: 12px 0;
}

.business-overview__warning-row-head {
  align-items: center;
  display: flex;
  gap: 10px;
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
.business-overview__table-card :deep(.ant-card-body) {
  min-width: 0;
}

.business-overview__warning-row p {
  margin: 4px 0 0;
}

.business-overview__warning-meta {
  color: var(--ant-color-text-tertiary);
  display: inline-block;
  font-size: 12px;
  margin-top: 6px;
}

.business-overview__warning-row :deep(.ant-btn) {
  flex-shrink: 0;
}

.business-overview__table :deep(.ant-table-wrapper),
.business-overview__table :deep(.ant-spin-nested-loading),
.business-overview__table :deep(.ant-spin-container),
.business-overview__table :deep(.ant-table),
.business-overview__table :deep(.ant-table-container) {
  min-width: 0;
  width: 100%;
}

.business-overview__table :deep(.ant-table-cell) {
  white-space: nowrap;
  word-break: keep-all;
}

.business-overview__table :deep(.ant-table-content) {
  overflow-x: auto;
}

.business-overview__table :deep(table) {
  min-width: max-content;
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
