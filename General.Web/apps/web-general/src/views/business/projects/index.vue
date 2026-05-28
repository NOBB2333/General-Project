<script lang="ts" setup>
import { computed, provide } from 'vue';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, Empty, Input, Select, Spin, Tabs, Tag, Tooltip } from 'ant-design-vue';

import BusinessBudgetTab from './components/BusinessBudgetTab.vue';
import BusinessChainTab from './components/BusinessChainTab.vue';
import BusinessIncomeTab from './components/BusinessIncomeTab.vue';
import BusinessOverviewTab from './components/BusinessOverviewTab.vue';
import BusinessProfitTab from './components/BusinessProfitTab.vue';
import BusinessReceivableTab from './components/BusinessReceivableTab.vue';
import { useBusinessProjectsPage } from './composables/useBusinessProjectsPage';
import { businessProjectsContextKey } from './context';

defineOptions({ name: 'BusinessProjectsPage' });

const tabItems = [
  { key: 'overview', label: '经营概览' },
  { key: 'budget', label: '预算与执行' },
  { key: 'business', label: '商务与分包' },
  { key: 'income', label: '收入与合同' },
  { key: 'receivable', label: '开票与回款' },
  { key: 'profit', label: '利润与预测' },
];

const tabComponentMap = {
  overview: BusinessOverviewTab,
  budget: BusinessBudgetTab,
  business: BusinessChainTab,
  income: BusinessIncomeTab,
  receivable: BusinessReceivableTab,
  profit: BusinessProfitTab,
};

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

const page = useBusinessProjectsPage();
const {
  activeTab,
  detail,
  formatMoney,
  getCollapsedProjectLabel,
  handleProjectChange,
  listKeyword,
  loadingDetail,
  loadingList,
  openGlobalSearch,
  projects,
  refreshingDetail,
  resetProjects,
  searchProjects,
  selectProject,
  selectedProjectId,
  sidebarCollapsed,
} = page;
const activeTabComponent = computed(() => tabComponentMap[activeTab.value as keyof typeof tabComponentMap] ?? BusinessOverviewTab);

provide(businessProjectsContextKey, {
  ...page,
  budgetColumns,
  businessColumns,
  contractColumns,
  detailTableScroll,
  historyColumns,
  procurementColumns,
  receivableColumns,
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
          @change="handleProjectChange"
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

              <component :is="activeTabComponent" />
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

<style>
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

.business-projects__detail-card .ant-card-body {
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

.business-projects__tabs .ant-tabs-nav {
  margin-bottom: 20px;
}

.business-projects__tabs .ant-tabs-tab {
  font-weight: 500;
  padding-left: 4px;
  padding-right: 4px;
}

.business-projects__tabs .ant-tabs-content-holder,
.business-projects__tabs .ant-tabs-content,
.business-projects__tabs .ant-tabs-tabpane {
  min-width: 0;
}

.business-projects__tab-grid {
  display: grid;
  gap: 16px;
  min-width: 0;
}

.business-projects__tab-grid > .ant-card,
.business-projects__tab-panel > .ant-table-wrapper {
  min-width: 0;
}

.business-projects__tab-grid .ant-card-body,
.business-projects__tab-panel .ant-card-body {
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

.business-projects__table .ant-table-cell,
.business-projects__project-item-head strong,
.business-projects__summary-item strong {
  white-space: nowrap;
  word-break: keep-all;
}

.business-projects__table {
  min-width: 0;
}

.business-projects__table .ant-table-wrapper,
.business-projects__table .ant-spin-nested-loading,
.business-projects__table .ant-spin-container,
.business-projects__table .ant-table,
.business-projects__table .ant-table-container {
  min-width: 0;
  width: 100%;
}

.business-projects__table .ant-table-content {
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

.business-projects__table table {
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
