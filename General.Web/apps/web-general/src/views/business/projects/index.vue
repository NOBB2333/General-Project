<script lang="ts" setup>
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Empty, Input, Select, Spin, Table, Tabs, Tag, Tooltip } from 'ant-design-vue';

import {
  getBusinessProjectDetailApi,
  getBusinessProjectListApi,
  type BusinessApi,
} from '#/api/core';

defineOptions({ name: 'BusinessProjectsPage' });

const route = useRoute();
const router = useRouter();

const listKeyword = ref('');
const loadingList = ref(false);
const loadingDetail = ref(false);
const refreshingDetail = ref(false);
const sidebarCollapsed = ref(false);
const projects = ref<BusinessApi.ProjectSummary[]>([]);
const detail = ref<BusinessApi.ProjectDetail | null>(null);
const selectedProjectId = ref(typeof route.query.projectId === 'string' ? route.query.projectId : '');
const activeTab = ref(typeof route.query.tab === 'string' ? route.query.tab : 'overview');

const tabItems = [
  { key: 'overview', label: '经营概览' },
  { key: 'budget', label: '预算与执行' },
  { key: 'business', label: '商务与分包' },
  { key: 'income', label: '收入与合同' },
  { key: 'receivable', label: '开票与回款' },
  { key: 'profit', label: '利润与预测' },
];

const budgetColumns = [
  { dataIndex: 'budgetCode', key: 'budgetCode', title: '预算编号', width: 160 },
  { dataIndex: 'category', key: 'category', title: '预算项', width: 220 },
  { dataIndex: 'adjustedAmount', key: 'adjustedAmount', title: '调整后预算', width: 150 },
  { dataIndex: 'executedAmount', key: 'executedAmount', title: '已执行金额', width: 150 },
  { dataIndex: 'varianceAmount', key: 'varianceAmount', title: '偏差金额', width: 130 },
];
const detailTableScroll = { x: 'max-content' as const };

const businessColumns = [
  { dataIndex: 'chainCode', key: 'chainCode', title: '链路编号', width: 160 },
  { dataIndex: 'stage', key: 'stage', title: '链路', width: 110 },
  { dataIndex: 'type', key: 'type', title: '类型', width: 110 },
  { dataIndex: 'title', key: 'title', title: '事项标题', width: 260 },
  { dataIndex: 'linkedContractCode', key: 'linkedContractCode', title: '关联合同', width: 160 },
  { dataIndex: 'sourceChangeCode', key: 'sourceChangeCode', title: '变更来源', width: 160 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];

const contractColumns = [
  { dataIndex: 'contractCode', key: 'contractCode', title: '合同编号', width: 160 },
  { dataIndex: 'type', key: 'type', title: '类型', width: 110 },
  { dataIndex: 'title', key: 'title', title: '合同名称', width: 260 },
  { dataIndex: 'parentContractCode', key: 'parentContractCode', title: '父合同', width: 160 },
  { dataIndex: 'sourceChangeCode', key: 'sourceChangeCode', title: '变更编号', width: 160 },
  { dataIndex: 'counterpartyName', key: 'counterpartyName', title: '相对方', width: 180 },
  { dataIndex: 'amount', key: 'amount', title: '合同金额', width: 140 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];

const procurementColumns = [
  { dataIndex: 'procurementCode', key: 'procurementCode', title: '采购编号', width: 160 },
  { dataIndex: 'title', key: 'title', title: '分包/采购事项', width: 260 },
  { dataIndex: 'supplierName', key: 'supplierName', title: '供应商/分包方', width: 180 },
  { dataIndex: 'linkedContractCode', key: 'linkedContractCode', title: '来源合同', width: 160 },
  { dataIndex: 'sourceChangeCode', key: 'sourceChangeCode', title: '变更来源', width: 160 },
  { dataIndex: 'amount', key: 'amount', title: '金额', width: 130 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];

const receivableColumns = [
  { dataIndex: 'receivableCode', key: 'receivableCode', title: '回款编号', width: 160 },
  { dataIndex: 'title', key: 'title', title: '回款事项', width: 260 },
  { dataIndex: 'invoiceCode', key: 'invoiceCode', title: '开票编号', width: 160 },
  { dataIndex: 'linkedContractCode', key: 'linkedContractCode', title: '关联合同', width: 160 },
  { dataIndex: 'plannedDate', key: 'plannedDate', title: '计划日期', width: 140 },
  { dataIndex: 'totalAmount', key: 'totalAmount', title: '应回金额', width: 130 },
  { dataIndex: 'receivedAmount', key: 'receivedAmount', title: '已回金额', width: 130 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
];

const historyColumns = [
  { dataIndex: 'changeType', key: 'changeType', title: '历史类型', width: 120 },
  { dataIndex: 'metric', key: 'metric', title: '指标', width: 140 },
  { dataIndex: 'relatedCode', key: 'relatedCode', title: '关联编号', width: 160 },
  { dataIndex: 'oldValue', key: 'oldValue', title: '调整前', width: 120 },
  { dataIndex: 'newValue', key: 'newValue', title: '调整后', width: 120 },
  { dataIndex: 'changedBy', key: 'changedBy', title: '调整人', width: 120 },
  { dataIndex: 'changedAt', key: 'changedAt', title: '调整时间', width: 160 },
  { dataIndex: 'reason', key: 'reason', title: '原因', width: 280 },
];

let detailRequestSerial = 0;

const currentProject = computed(() =>
  projects.value.find((item) => item.id === selectedProjectId.value) ?? detail.value?.summary ?? null,
);

const contractRelationItems = computed(() => {
  if (!detail.value) {
    return [];
  }

  const incomeContracts = detail.value.contracts.filter((item) => item.isRevenueContract);
  const downstreamContracts = detail.value.contracts.filter((item) => !item.isRevenueContract);

  return incomeContracts.map((contract) => ({
    id: contract.id,
    title: contract.title,
    code: contract.contractCode,
    type: contract.type,
    status: contract.status,
    changeCode: contract.sourceChangeCode,
    linkedProcurements: detail.value?.procurements.filter((item) => item.linkedContractCode === contract.contractCode) ?? [],
    childContracts: downstreamContracts.filter((item) => item.parentContractCode === contract.contractCode),
  }));
});

const closingItems = computed(() => {
  if (!detail.value) {
    return [];
  }

  const totalReceivable = detail.value.receivables.reduce((sum, item) => sum + (item.totalAmount ?? 0), 0);
  const receivedAmount = detail.value.receivables.reduce((sum, item) => sum + (item.receivedAmount ?? 0), 0);
  const openReceivables = detail.value.receivables.filter((item) => item.status.includes('待回款'));
  const hasPendingProcurement = detail.value.procurements.some((item) => !item.status.includes('已签订') && !item.status.includes('完成'));

  return [
    {
      key: 'close-status',
      label: '经营收尾状态',
      value: detail.value.summary.businessCloseStatus,
      status: detail.value.summary.businessCloseStatus.includes('收尾') || detail.value.summary.businessCloseStatus.includes('待回款') ? 'warning' : 'normal',
    },
    {
      key: 'receivable-gap',
      label: '待回款余额',
      value: formatMoney(totalReceivable - receivedAmount),
      status: openReceivables.length > 0 ? 'warning' : 'normal',
    },
    {
      key: 'pending-receivable-count',
      label: '待回款事项',
      value: `${openReceivables.length} 项`,
      status: openReceivables.length > 0 ? 'warning' : 'normal',
    },
    {
      key: 'procurement-close',
      label: '分包采购闭环',
      value: hasPendingProcurement ? '仍有未闭环事项' : '已闭环',
      status: hasPendingProcurement ? 'warning' : 'normal',
    },
  ];
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

function getCollapsedProjectLabel(name: string) {
  const compact = name.replace(/\s+/g, '');
  const asciiWords = compact.match(/[A-Za-z0-9]+/g);
  if (asciiWords && asciiWords.length > 0) {
    const tail = asciiWords.join('').slice(-3).toUpperCase();
    if (tail) {
      return tail;
    }
  }

  const digitTail = compact.match(/\d+/g)?.join('').slice(-3);
  if (digitTail) {
    return digitTail;
  }

  return compact.length <= 1 ? compact : compact.slice(0, 1);
}

async function loadProjects() {
  loadingList.value = true;
  try {
    const nextProjects = await getBusinessProjectListApi(listKeyword.value.trim() || undefined);
    projects.value = nextProjects;

    if (nextProjects.length === 0) {
      selectedProjectId.value = '';
      detail.value = null;
      return;
    }

    const preferredProjectId =
      selectedProjectId.value ||
      (typeof route.query.projectId === 'string' ? route.query.projectId : '');
    const matchedProject = nextProjects.find((item) => item.id === preferredProjectId) ?? nextProjects[0];

    if (matchedProject && matchedProject.id !== selectedProjectId.value) {
      selectedProjectId.value = matchedProject.id;
    }
  } finally {
    loadingList.value = false;
  }
}

async function loadDetail(projectId = selectedProjectId.value) {
  if (!projectId) {
    detail.value = null;
    return;
  }

  const requestId = ++detailRequestSerial;
  if (detail.value) {
    refreshingDetail.value = true;
  } else {
    loadingDetail.value = true;
  }

  try {
    const nextDetail = await getBusinessProjectDetailApi(projectId);
    if (requestId !== detailRequestSerial) {
      return;
    }
    detail.value = nextDetail;
  } finally {
    if (requestId === detailRequestSerial) {
      loadingDetail.value = false;
      refreshingDetail.value = false;
    }
  }
}

function selectProject(projectId: string, tab = activeTab.value) {
  activeTab.value = tab;
  selectedProjectId.value = projectId;
}

function searchProjects() {
  void loadProjects();
}

function resetProjects() {
  listKeyword.value = '';
  void loadProjects();
}

function openGlobalSearch() {
  router.push('/business/reports');
}

watch(selectedProjectId, (projectId, previousProjectId) => {
  if (!projectId || projectId === previousProjectId) {
    return;
  }
  void loadDetail(projectId);
});

onMounted(async () => {
  await loadProjects();
  if (selectedProjectId.value && !detail.value) {
    await loadDetail(selectedProjectId.value);
  }
});
</script>

<template>
  <Page description="按项目聚合经营摘要、合同链、分包采购链、回款和利润预测。" title="项目经营">
    <Alert banner class="mb-4" message="左侧用于切换项目，右侧固定展示当前项目的经营详情，不再跳出新页面。" type="info" />

    <div :class="['business-projects__shell', { 'is-collapsed': sidebarCollapsed }]">
      <Card :bordered="false" class="business-projects__rail">
        <div class="business-projects__rail-header">
          <div>
            <strong>项目选择</strong>
            <p>左侧用于定位项目，右侧固定展示经营详情。</p>
          </div>
          <Button size="small" @click="sidebarCollapsed = !sidebarCollapsed">
            {{ sidebarCollapsed ? '展开' : '收起' }}
          </Button>
        </div>

        <div v-if="!sidebarCollapsed" class="business-projects__toolbar">
          <Input
            v-model:value="listKeyword"
            allow-clear
            placeholder="搜索项目编号、名称、客户、项目经理"
            @pressEnter="searchProjects"
          />
          <div class="business-projects__toolbar-actions">
            <Button type="primary" @click="searchProjects">查询</Button>
            <Button @click="resetProjects">重置</Button>
          <Button class="business-projects__report-button" @click="openGlobalSearch">报表检索</Button>
          </div>
        </div>

        <Select
          v-if="projects.length > 0 && !sidebarCollapsed"
          v-model:value="selectedProjectId"
          class="business-projects__selector"
          :options="projects.map((item) => ({ label: `${item.projectCode} · ${item.name}`, value: item.id }))"
          placeholder="切换项目"
          show-search
        />

        <div v-if="loadingList" class="business-projects__list-loading">
          <Spin />
        </div>

        <div v-else-if="projects.length > 0" :class="['business-projects__project-list', { 'is-collapsed': sidebarCollapsed }]">
          <Tooltip v-for="record in projects" :key="record.id" :title="sidebarCollapsed ? record.name : ''" placement="right">
            <button
              :class="[
                'business-projects__project-item',
                { 'is-active': record.id === selectedProjectId, 'is-collapsed': sidebarCollapsed },
              ]"
              type="button"
              @click="selectProject(record.id)"
            >
              <div class="business-projects__project-item-head">
                <strong :title="record.name">{{ sidebarCollapsed ? getCollapsedProjectLabel(record.projectCode || record.name) : record.name }}</strong>
                <Tag
                  v-if="!sidebarCollapsed"
                  :color="record.businessCloseStatus.includes('待回款') || record.businessCloseStatus.includes('收尾') ? 'gold' : 'blue'"
                >
                  {{ record.businessCloseStatus }}
                </Tag>
              </div>
              <p v-if="!sidebarCollapsed">{{ record.projectCode }} · {{ record.customerName }}</p>
              <div v-if="!sidebarCollapsed" class="business-projects__project-item-metrics">
                <span>合同 {{ formatMoney(record.contractAmount) }}</span>
                <span>回款 {{ formatMoney(record.receivedAmount) }}</span>
              </div>
            </button>
          </Tooltip>
        </div>

        <Empty v-else description="暂无匹配的项目经营数据" />
      </Card>

      <div class="business-projects__main">
        <Card v-if="loadingDetail && !detail" :bordered="false">
          <div class="business-projects__loading-panel">
            <Spin tip="正在加载项目经营详情..." />
          </div>
        </Card>

        <Card v-else-if="detail" :bordered="false" class="business-projects__detail-card">
          <Spin :spinning="refreshingDetail">
            <div class="business-projects__content">
              <div class="business-projects__hero">
                <div class="business-projects__hero-headline">
                  <div>
                    <span class="business-projects__eyebrow">项目经营</span>
                    <h3>{{ detail.summary.name }}</h3>
                    <p>
                      {{ detail.summary.projectCode }} · {{ detail.summary.customerName }} · {{ detail.summary.managerName }}
                    </p>
                  </div>
                  <Tag :color="detail.summary.businessCloseStatus.includes('待回款') || detail.summary.businessCloseStatus.includes('收尾') ? 'gold' : 'blue'">
                    {{ detail.summary.businessCloseStatus }}
                  </Tag>
                </div>

                <div class="business-projects__summary-strip">
                  <article class="business-projects__summary-strip-item">
                    <span>项目状态</span>
                    <strong>{{ detail.summary.status }}</strong>
                  </article>
                  <article class="business-projects__summary-strip-item">
                    <span>合同收入</span>
                    <strong>{{ formatMoney(detail.summary.contractAmount) }}</strong>
                  </article>
                  <article class="business-projects__summary-strip-item">
                    <span>经营成本</span>
                    <strong>{{ formatMoney(detail.summary.costAmount) }}</strong>
                  </article>
                  <article class="business-projects__summary-strip-item">
                    <span>已回款</span>
                    <strong>{{ formatMoney(detail.summary.receivedAmount) }}</strong>
                  </article>
                  <article class="business-projects__summary-strip-item">
                    <span>年底预计回款</span>
                    <strong>{{ formatMoney(detail.summary.yearEndReceivableForecast) }}</strong>
                  </article>
                  <article class="business-projects__summary-strip-item">
                    <span>分包与采购</span>
                    <strong>{{ formatMoney(detail.summary.procurementAmount) }}</strong>
                  </article>
                  <article class="business-projects__summary-strip-item">
                    <span>毛利额</span>
                    <strong>{{ formatMoney(detail.summary.grossProfitAmount) }}</strong>
                  </article>
                  <article class="business-projects__summary-strip-item">
                    <span>毛利率</span>
                    <strong>{{ typeof detail.summary.grossProfitRate === 'number' ? `${detail.summary.grossProfitRate.toFixed(1)}%` : '--' }}</strong>
                  </article>
                </div>
              </div>

              <Tabs v-model:activeKey="activeTab" class="business-projects__tabs" size="large">
                <Tabs.TabPane v-for="tab in tabItems" :key="tab.key" :tab="tab.label" />
              </Tabs>

              <div v-if="activeTab === 'overview'" class="business-projects__tab-grid">
                <Card :bordered="false" title="经营摘要">
                  <div class="business-projects__summary-grid">
                    <div class="business-projects__summary-item"><span>当前项目</span><strong>{{ currentProject?.name ?? '--' }}</strong></div>
                    <div class="business-projects__summary-item"><span>合同收入</span><strong>{{ formatMoney(detail.summary.contractAmount) }}</strong></div>
                    <div class="business-projects__summary-item"><span>分包与采购</span><strong>{{ formatMoney(detail.summary.procurementAmount) }}</strong></div>
                    <div class="business-projects__summary-item"><span>年底预计回款</span><strong>{{ formatMoney(detail.summary.yearEndReceivableForecast) }}</strong></div>
                    <div class="business-projects__summary-item"><span>经营收尾状态</span><strong>{{ detail.summary.businessCloseStatus }}</strong></div>
                    <div class="business-projects__summary-item"><span>项目阶段</span><strong>{{ detail.summary.status }}</strong></div>
                  </div>
                </Card>

                <Card :bordered="false" title="经营收尾视图">
                  <div class="business-projects__closing-grid">
                    <article
                      v-for="item in closingItems"
                      :key="item.key"
                      :class="['business-projects__closing-item', { 'is-warning': item.status === 'warning' }]"
                    >
                      <span>{{ item.label }}</span>
                      <strong>{{ item.value }}</strong>
                    </article>
                  </div>
                </Card>

                <Card :bordered="false" title="最近变化">
                  <Table
                    class="business-projects__table"
                    :columns="historyColumns"
                    :data-source="detail.forecastHistories"
                    :pagination="false"
                    row-key="id"
                    :scroll="detailTableScroll"
                    size="middle"
                  />
                </Card>
              </div>

              <div v-else-if="activeTab === 'budget'" class="business-projects__tab-panel">
                <Table
                  class="business-projects__table"
                  :columns="budgetColumns"
                  :data-source="detail.budgetExecutions"
                  :pagination="false"
                  row-key="id"
                  :scroll="detailTableScroll"
                  size="middle"
                >
                  <template #bodyCell="{ column, record }">
                    <template v-if="column.key === 'adjustedAmount'">{{ formatMoney(record.adjustedAmount) }}</template>
                    <template v-else-if="column.key === 'executedAmount'">{{ formatMoney(record.executedAmount) }}</template>
                    <template v-else-if="column.key === 'varianceAmount'">
                      <Tag :color="typeof record.varianceAmount === 'number' && record.varianceAmount > 0 ? 'red' : 'blue'">
                        {{ formatMoney(record.varianceAmount) }}
                      </Tag>
                    </template>
                  </template>
                </Table>
              </div>

              <div v-else-if="activeTab === 'business'" class="business-projects__tab-grid">
                <Card :bordered="false" title="商务链与分包链">
                  <Table
                    class="business-projects__table"
                    :columns="businessColumns"
                    :data-source="detail.businessChains"
                    :pagination="false"
                    row-key="id"
                    :scroll="detailTableScroll"
                    size="middle"
                  />
                </Card>

                <Card :bordered="false" title="分包与采购台账">
                  <Table
                    class="business-projects__table"
                    :columns="procurementColumns"
                    :data-source="detail.procurements"
                    :pagination="false"
                    row-key="id"
                    :scroll="detailTableScroll"
                    size="middle"
                  >
                    <template #bodyCell="{ column, record }">
                      <template v-if="column.key === 'amount'">{{ formatMoney(record.amount) }}</template>
                    </template>
                  </Table>
                </Card>
              </div>

              <div v-else-if="activeTab === 'income'" class="business-projects__tab-grid">
                <Card :bordered="false" title="合同关系链">
                  <div class="business-projects__relation-list">
                    <div v-for="relation in contractRelationItems" :key="relation.id" class="business-projects__relation-item">
                      <div class="business-projects__relation-main">
                        <div class="business-projects__relation-primary">
                          <span class="business-projects__relation-label">收入合同</span>
                          <strong>{{ relation.title }}</strong>
                          <p>{{ relation.code }} · {{ relation.type }} · {{ relation.status }}</p>
                        </div>
                        <Tag v-if="relation.changeCode" color="gold">{{ relation.changeCode }}</Tag>
                      </div>

                      <div class="business-projects__relation-body">
                        <div v-if="relation.childContracts.length > 0" class="business-projects__relation-sublist">
                          <div v-for="contract in relation.childContracts" :key="contract.id" class="business-projects__relation-subitem">
                            <span>衍生合同</span>
                            <strong>{{ contract.title }}</strong>
                            <p>{{ contract.contractCode }} · {{ contract.type }}</p>
                          </div>
                        </div>

                        <div v-if="relation.linkedProcurements.length > 0" class="business-projects__relation-sublist">
                          <div v-for="procurement in relation.linkedProcurements" :key="procurement.id" class="business-projects__relation-subitem">
                            <span>分包/采购</span>
                            <strong>{{ procurement.title }}</strong>
                            <p>{{ procurement.procurementCode }} · {{ procurement.status }}</p>
                          </div>
                        </div>

                        <div
                          v-if="relation.childContracts.length === 0 && relation.linkedProcurements.length === 0"
                          class="business-projects__relation-empty"
                        >
                          当前合同暂无衍生合同或分包/采购关联。
                        </div>
                      </div>
                    </div>
                  </div>
                </Card>

                <Card :bordered="false" title="合同台账">
                  <Table
                    class="business-projects__table"
                    :columns="contractColumns"
                    :data-source="detail.contracts"
                    :pagination="false"
                    row-key="id"
                    :scroll="detailTableScroll"
                    size="middle"
                  >
                    <template #bodyCell="{ column, record }">
                      <template v-if="column.key === 'amount'">{{ formatMoney(record.amount) }}</template>
                    </template>
                  </Table>
                </Card>
              </div>

              <div v-else-if="activeTab === 'receivable'" class="business-projects__tab-grid">
                <Card :bordered="false" title="收尾专项视图">
                  <div class="business-projects__summary-grid">
                    <div class="business-projects__summary-item"><span>经营收尾状态</span><strong>{{ detail.summary.businessCloseStatus }}</strong></div>
                    <div class="business-projects__summary-item"><span>项目状态</span><strong>{{ detail.summary.status }}</strong></div>
                    <div class="business-projects__summary-item"><span>待回款事项</span><strong>{{ detail.receivables.filter((item) => item.status.includes('待回款')).length }} 项</strong></div>
                    <div class="business-projects__summary-item"><span>年底预计回款</span><strong>{{ formatMoney(detail.summary.yearEndReceivableForecast) }}</strong></div>
                  </div>
                </Card>

                <Table
                  class="business-projects__table"
                  :columns="receivableColumns"
                  :data-source="detail.receivables"
                  :pagination="false"
                  row-key="id"
                  :scroll="detailTableScroll"
                  size="middle"
                >
                  <template #bodyCell="{ column, record }">
                    <template v-if="column.key === 'totalAmount'">{{ formatMoney(record.totalAmount) }}</template>
                    <template v-else-if="column.key === 'receivedAmount'">{{ formatMoney(record.receivedAmount) }}</template>
                  </template>
                </Table>
              </div>

              <div v-else class="business-projects__tab-grid">
                <Card :bordered="false" title="利润结果">
                  <div class="business-projects__summary-grid">
                    <div class="business-projects__summary-item"><span>合同收入</span><strong>{{ formatMoney(detail.summary.contractAmount) }}</strong></div>
                    <div class="business-projects__summary-item"><span>经营成本</span><strong>{{ formatMoney(detail.summary.costAmount) }}</strong></div>
                    <div class="business-projects__summary-item"><span>毛利额</span><strong>{{ formatMoney(detail.summary.grossProfitAmount) }}</strong></div>
                    <div class="business-projects__summary-item"><span>毛利率</span><strong>{{ typeof detail.summary.grossProfitRate === 'number' ? `${detail.summary.grossProfitRate.toFixed(1)}%` : '--' }}</strong></div>
                  </div>
                </Card>

                <Card :bordered="false" title="预测与历史">
                  <Table
                    class="business-projects__table"
                    :columns="historyColumns"
                    :data-source="detail.forecastHistories"
                    :pagination="false"
                    row-key="id"
                    :scroll="detailTableScroll"
                    size="middle"
                  />
                </Card>
              </div>
            </div>
          </Spin>
        </Card>

        <Card v-else :bordered="false" class="business-projects__empty">
          <Empty description="暂无可查看的项目经营数据" />
        </Card>
      </div>
    </div>
  </Page>
</template>

<style scoped>
.business-projects__shell {
  display: grid;
  gap: 16px;
  grid-template-columns: 320px minmax(0, 1fr);
}

.business-projects__shell.is-collapsed {
  grid-template-columns: 112px minmax(0, 1fr);
}

.business-projects__rail,
.business-projects__main {
  min-width: 0;
}

.business-projects__toolbar {
  display: grid;
  gap: 12px;
  margin-bottom: 16px;
}

.business-projects__rail-header {
  align-items: flex-start;
  display: flex;
  gap: 12px;
  justify-content: space-between;
  margin-bottom: 16px;
}

.business-projects__rail-header p {
  color: var(--ant-color-text-secondary);
  margin: 6px 0 0;
}

.business-projects__shell.is-collapsed .business-projects__rail-header p,
.business-projects__shell.is-collapsed .business-projects__rail-header strong {
  display: none;
}

.business-projects__toolbar-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.business-projects__report-button {
  margin-left: auto;
}

.business-projects__selector {
  margin-bottom: 16px;
  width: 100%;
}

.business-projects__list-loading,
.business-projects__loading-panel {
  align-items: center;
  display: flex;
  justify-content: center;
  min-height: 280px;
}

.business-projects__project-list {
  display: grid;
  gap: 12px;
  max-height: calc(100vh - 290px);
  overflow: auto;
  padding-right: 4px;
}

.business-projects__project-list.is-collapsed {
  gap: 10px;
  max-height: calc(100vh - 180px);
}

.business-projects__project-item {
  background: var(--ant-color-fill-quaternary);
  border: 1px solid transparent;
  border-radius: 16px;
  cursor: pointer;
  display: grid;
  gap: 8px;
  min-width: 0;
  padding: 14px 16px;
  position: relative;
  text-align: left;
  transition: border-color 0.2s ease, background-color 0.2s ease, transform 0.2s ease;
}

.business-projects__project-list.is-collapsed .business-projects__project-item {
  align-items: center;
  border-radius: 18px;
  justify-items: center;
  min-height: 72px;
  padding: 12px 10px;
}

.business-projects__project-item.is-active {
  background: linear-gradient(135deg, rgb(22 119 255 / 0.16), rgb(37 99 235 / 0.06));
  border-color: rgb(22 119 255 / 0.42);
  box-shadow: inset 0 0 0 1px rgb(22 119 255 / 0.1);
}

.business-projects__project-item:hover {
  transform: translateY(-1px);
}

.business-projects__project-item-head {
  align-items: flex-start;
  display: flex;
  gap: 8px;
  justify-content: space-between;
}

.business-projects__project-list.is-collapsed .business-projects__project-item-head {
  justify-content: center;
}

.business-projects__project-list.is-collapsed .business-projects__project-item-head strong {
  background: var(--ant-color-fill-secondary);
  border: 1px solid var(--ant-color-border);
  border-radius: 12px;
  color: var(--ant-color-text);
  display: inline-flex;
  font-size: 16px;
  font-weight: 700;
  justify-content: center;
  letter-spacing: 0.08em;
  min-height: 32px;
  min-width: 44px;
  padding: 0 8px;
}

.business-projects__project-item.is-collapsed {
  min-height: 72px;
}

.business-projects__project-item-head strong,
.business-projects__info-item strong,
.business-projects__summary-item strong {
  min-width: 0;
}

.business-projects__project-item p,
.business-projects__hero-headline p,
.business-projects__info-item span,
.business-projects__summary-item span {
  color: var(--ant-color-text-secondary);
}

.business-projects__project-item p {
  margin: 0;
  word-break: keep-all;
}

.business-projects__project-item-metrics {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.business-projects__content {
  display: grid;
  gap: 16px;
}

.business-projects__detail-card :deep(.ant-card-body) {
  min-width: 0;
  overflow: hidden;
  width: 100%;
}

.business-projects__hero {
  background:
    linear-gradient(135deg, rgb(22 119 255 / 0.08), rgb(37 99 235 / 0.03)),
    var(--ant-color-bg-container);
  border: 1px solid rgb(22 119 255 / 0.08);
  border-radius: 18px;
  display: grid;
  gap: 14px;
  padding: 18px 20px;
}

.business-projects__eyebrow {
  color: var(--ant-color-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.business-projects__hero-headline {
  align-items: flex-start;
  display: flex;
  gap: 16px;
  justify-content: space-between;
}

.business-projects__hero-headline h3 {
  margin: 8px 0 6px;
  font-size: 24px;
}

.business-projects__hero-headline p {
  margin: 0;
}

.business-projects__summary-strip {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.business-projects__summary-strip-item,
.business-projects__info-item,
.business-projects__summary-item {
  background: var(--ant-color-fill-secondary);
  border: 1px solid var(--ant-color-border);
  box-shadow: 0 1px 2px rgb(15 23 42 / 0.06);
  border-radius: 14px;
  display: grid;
  gap: 6px;
  min-width: 0;
  padding: 12px 14px;
}

.business-projects__summary-strip-item strong,
.business-projects__summary-item strong {
  color: var(--ant-color-text);
  font-size: 16px;
}

.business-projects__tabs :deep(.ant-tabs-nav) {
  margin-bottom: 20px;
}

.business-projects__tabs :deep(.ant-tabs-tab) {
  font-weight: 500;
  padding-left: 4px;
  padding-right: 4px;
}

.business-projects__tabs :deep(.ant-tabs-content-holder),
.business-projects__tabs :deep(.ant-tabs-content),
.business-projects__tabs :deep(.ant-tabs-tabpane) {
  min-width: 0;
}

.business-projects__tab-grid {
  display: grid;
  gap: 16px;
  min-width: 0;
}

.business-projects__tab-grid > :deep(.ant-card),
.business-projects__tab-panel > :deep(.ant-table-wrapper) {
  min-width: 0;
}

.business-projects__tab-grid :deep(.ant-card-body),
.business-projects__tab-panel :deep(.ant-card-body) {
  min-width: 0;
  overflow: hidden;
  width: 100%;
}

.business-projects__tab-panel {
  min-width: 0;
}

.business-projects__closing-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.business-projects__closing-item,
.business-projects__relation-item,
.business-projects__relation-subitem {
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border);
  box-shadow: 0 1px 2px rgb(15 23 42 / 0.06);
  border-radius: 16px;
  display: grid;
  gap: 6px;
  min-width: 0;
  padding: 14px 16px;
}

.business-projects__closing-item.is-warning {
  background: linear-gradient(135deg, rgb(250 173 20 / 0.12), rgb(250 173 20 / 0.04));
  border-color: rgb(250 173 20 / 0.25);
}

.business-projects__relation-list,
.business-projects__relation-sublist {
  display: grid;
  gap: 12px;
}

.business-projects__relation-main {
  align-items: flex-start;
  display: flex;
  gap: 12px;
  justify-content: space-between;
}

.business-projects__relation-primary {
  display: grid;
  gap: 6px;
  max-width: 320px;
}

.business-projects__relation-body {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.business-projects__relation-label {
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.business-projects__relation-main p,
.business-projects__relation-subitem p,
.business-projects__closing-item span {
  color: var(--ant-color-text-secondary);
  margin: 0;
}

.business-projects__relation-empty {
  align-items: center;
  background: var(--ant-color-fill-tertiary);
  border: 1px dashed var(--ant-color-border);
  border-radius: 14px;
  color: var(--ant-color-text-secondary);
  display: flex;
  justify-content: center;
  min-height: 112px;
  padding: 16px;
  text-align: center;
}

.business-projects__summary-grid {
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.business-projects__table :deep(.ant-table-cell),
.business-projects__project-item-head strong,
.business-projects__summary-item strong {
  white-space: nowrap;
  word-break: keep-all;
}

.business-projects__table {
  min-width: 0;
}

.business-projects__table :deep(.ant-table-wrapper),
.business-projects__table :deep(.ant-spin-nested-loading),
.business-projects__table :deep(.ant-spin-container),
.business-projects__table :deep(.ant-table),
.business-projects__table :deep(.ant-table-container) {
  min-width: 0;
  width: 100%;
}

.business-projects__table :deep(.ant-table-content) {
  display: block;
  max-width: 100%;
  overflow-x: auto;
  overflow-y: hidden;
  scrollbar-gutter: stable;
  width: 100%;
}

.business-projects__relation-main strong,
.business-projects__relation-subitem strong,
.business-projects__closing-item strong {
  color: var(--ant-color-text);
}

.business-projects__table :deep(table) {
  min-width: max-content;
}

.business-projects__empty {
  min-height: 320px;
}

@media (max-width: 1200px) {
  .business-projects__shell {
    grid-template-columns: 1fr;
  }

  .business-projects__shell.is-collapsed {
    grid-template-columns: 1fr;
  }

  .business-projects__project-list {
    max-height: none;
  }

  .business-projects__report-button {
    margin-left: 0;
  }

  .business-projects__summary-strip {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .business-projects__relation-body {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .business-projects__hero-headline {
    flex-direction: column;
  }

  .business-projects__summary-strip {
    grid-template-columns: 1fr;
  }
}
</style>
