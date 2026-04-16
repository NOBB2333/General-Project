<script lang="ts" setup>
import { onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Alert, Button, Card, DatePicker, Input, Select, Skeleton, Table, Tag, Tabs } from 'ant-design-vue';

import { getBusinessReportApi, type BusinessApi } from '#/api/core';

defineOptions({ name: 'BusinessReportsPage' });

const route = useRoute();
const router = useRouter();

const loading = ref(false);
const report = ref<BusinessApi.Report | null>(null);
const activeTab = ref(typeof route.query.tab === 'string' ? route.query.tab : 'project-search');

const reportTabs = [
  { key: 'project-search', label: '项目检索' },
  { key: 'contract-search', label: '合同检索' },
  { key: 'procurement-search', label: '采购检索' },
  { key: 'receivable-search', label: '回款检索' },
  { key: 'profit-analysis', label: '利润分析' },
  { key: 'warning-center', label: '经营预警' },
];

const query = reactive<BusinessApi.ReportQuery>({
  businessCloseStatus: undefined,
  contractType: undefined,
  counterpartyName: '',
  keyword: '',
  plannedDateFrom: undefined,
  plannedDateTo: undefined,
  projectCode: '',
  receivableStatus: undefined,
  signDateFrom: undefined,
  signDateTo: undefined,
  supplierName: '',
});

const businessCloseStatusOptions = ['经营执行中', '经营收尾中', '执行关闭待回款'];
const contractTypeOptions = ['主合同', '补充合同', '分包合同', '采购合同'];
const receivableStatusOptions = ['已回款', '待回款'];
const tabTextMap: Record<string, string> = {
  business: '商务与分包',
  income: '收入与合同',
  overview: '经营概览',
  profit: '利润与预测',
  receivable: '开票与回款',
};

const projectColumns = [
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 150 },
  { dataIndex: 'name', key: 'name', title: '项目名称', width: 260 },
  { dataIndex: 'customerName', key: 'customerName', title: '客户', width: 180 },
  { dataIndex: 'managerName', key: 'managerName', title: '项目经理', width: 140 },
  { dataIndex: 'contractAmount', key: 'contractAmount', title: '合同收入', width: 140 },
  { dataIndex: 'receivedAmount', key: 'receivedAmount', title: '已回款', width: 130 },
  { dataIndex: 'businessCloseStatus', key: 'businessCloseStatus', title: '经营状态', width: 150 },
  { dataIndex: 'action', key: 'action', title: '跳转', width: 100 },
];

const contractColumns = [
  { dataIndex: 'contractCode', key: 'contractCode', title: '合同编号', width: 160 },
  { dataIndex: 'title', key: 'title', title: '合同名称', width: 260 },
  { dataIndex: 'type', key: 'type', title: '类型', width: 110 },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 200 },
  { dataIndex: 'counterpartyName', key: 'counterpartyName', title: '相对方', width: 180 },
  { dataIndex: 'signDate', key: 'signDate', title: '签订日期', width: 130 },
  { dataIndex: 'amount', key: 'amount', title: '金额', width: 130 },
  { dataIndex: 'action', key: 'action', title: '跳转', width: 100 },
];

const procurementColumns = [
  { dataIndex: 'procurementCode', key: 'procurementCode', title: '采购编号', width: 160 },
  { dataIndex: 'title', key: 'title', title: '采购/分包事项', width: 260 },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 200 },
  { dataIndex: 'supplierName', key: 'supplierName', title: '供应商/分包方', width: 180 },
  { dataIndex: 'stage', key: 'stage', title: '阶段', width: 120 },
  { dataIndex: 'signDate', key: 'signDate', title: '签订日期', width: 130 },
  { dataIndex: 'amount', key: 'amount', title: '金额', width: 130 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
  { dataIndex: 'action', key: 'action', title: '跳转', width: 100 },
];

const receivableColumns = [
  { dataIndex: 'receivableCode', key: 'receivableCode', title: '回款编号', width: 160 },
  { dataIndex: 'title', key: 'title', title: '回款事项', width: 260 },
  { dataIndex: 'projectName', key: 'projectName', title: '所属项目', width: 200 },
  { dataIndex: 'plannedDate', key: 'plannedDate', title: '计划日期', width: 130 },
  { dataIndex: 'linkedContractCode', key: 'linkedContractCode', title: '关联合同', width: 160 },
  { dataIndex: 'totalAmount', key: 'totalAmount', title: '应回金额', width: 130 },
  { dataIndex: 'receivedAmount', key: 'receivedAmount', title: '已回金额', width: 130 },
  { dataIndex: 'status', key: 'status', title: '状态', width: 110 },
  { dataIndex: 'action', key: 'action', title: '跳转', width: 100 },
];

const profitColumns = [
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 150 },
  { dataIndex: 'name', key: 'name', title: '项目名称', width: 260 },
  { dataIndex: 'grossProfitAmount', key: 'grossProfitAmount', title: '毛利额', width: 130 },
  { dataIndex: 'grossProfitRate', key: 'grossProfitRate', title: '毛利率', width: 110 },
  { dataIndex: 'yearEndReceivableForecast', key: 'yearEndReceivableForecast', title: '年底预计回款', width: 150 },
  { dataIndex: 'businessCloseStatus', key: 'businessCloseStatus', title: '经营状态', width: 150 },
  { dataIndex: 'action', key: 'action', title: '跳转', width: 100 },
];

const warningColumns = [
  { dataIndex: 'type', key: 'type', title: '预警类型', width: 120 },
  { dataIndex: 'level', key: 'level', title: '等级', width: 90 },
  { dataIndex: 'projectCode', key: 'projectCode', title: '项目编号', width: 150 },
  { dataIndex: 'projectName', key: 'projectName', title: '项目名称', width: 260 },
  { dataIndex: 'description', key: 'description', title: '预警说明', width: 360 },
  { dataIndex: 'ownerName', key: 'ownerName', title: '责任人', width: 120 },
  { dataIndex: 'lastUpdatedTime', key: 'lastUpdatedTime', title: '最近更新时间', width: 170 },
  { dataIndex: 'status', key: 'status', title: '当前状态', width: 150 },
  { dataIndex: 'actionTab', key: 'actionTab', title: '定位页签', width: 120 },
  { dataIndex: 'action', key: 'action', title: '跳转', width: 100 },
];

const reportTableScroll = { x: 'max-content' as const, y: 560 };

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

function openProject(projectId: string, tab: string) {
  router.push({ path: '/business/projects', query: { projectId, tab } });
}

async function loadReport() {
  loading.value = true;
  try {
    report.value = await getBusinessReportApi({
      businessCloseStatus: query.businessCloseStatus || undefined,
      contractType: query.contractType || undefined,
      counterpartyName: query.counterpartyName?.trim() || undefined,
      keyword: query.keyword?.trim() || undefined,
      plannedDateFrom: query.plannedDateFrom || undefined,
      plannedDateTo: query.plannedDateTo || undefined,
      projectCode: query.projectCode?.trim() || undefined,
      receivableStatus: query.receivableStatus || undefined,
      signDateFrom: query.signDateFrom || undefined,
      signDateTo: query.signDateTo || undefined,
      supplierName: query.supplierName?.trim() || undefined,
    });
  } finally {
    loading.value = false;
  }
}

function resetQuery() {
  query.businessCloseStatus = undefined;
  query.contractType = undefined;
  query.counterpartyName = '';
  query.keyword = '';
  query.plannedDateFrom = undefined;
  query.plannedDateTo = undefined;
  query.projectCode = '';
  query.receivableStatus = undefined;
  query.signDateFrom = undefined;
  query.signDateTo = undefined;
  query.supplierName = '';
  void loadReport();
}

onMounted(loadReport);
</script>

<template>
  <Page description="报表中心承担跨项目检索和利润分析，解决大量合同与采购记录的定位问题。" title="报表中心">
    <Alert banner class="mb-4" message="筛选在当前页完成，检索结果支持直接跳转到项目经营详情并定位到对应页签。" type="info" />

    <Card :bordered="false">
      <Tabs v-model:activeKey="activeTab" class="business-reports__tabs" size="large">
        <Tabs.TabPane v-for="tab in reportTabs" :key="tab.key" :tab="tab.label" />
      </Tabs>

      <div class="business-reports__filters">
        <div class="business-reports__section">
          <div class="business-reports__section-header">
            <span class="business-reports__section-title">关键字检索</span>
          </div>
          <div class="business-reports__field business-reports__field--single">
            <span class="business-reports__field-label">搜索内容</span>
            <Input v-model:value="query.keyword" allow-clear placeholder="项目、合同、供应商、客户、状态" />
          </div>
        </div>

        <div class="business-reports__section">
          <div class="business-reports__section-header">
            <span class="business-reports__section-title">筛选条件</span>
          </div>
          <div class="business-reports__field-grid">
            <div class="business-reports__field">
              <span class="business-reports__field-label">项目编号</span>
              <Input v-model:value="query.projectCode" allow-clear placeholder="输入项目编号" />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">经营状态</span>
              <Select
                v-model:value="query.businessCloseStatus"
                allow-clear
                :options="businessCloseStatusOptions.map((item) => ({ label: item, value: item }))"
                placeholder="选择经营状态"
              />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">合同类型</span>
              <Select
                v-model:value="query.contractType"
                allow-clear
                :options="contractTypeOptions.map((item) => ({ label: item, value: item }))"
                placeholder="选择合同类型"
              />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">回款状态</span>
              <Select
                v-model:value="query.receivableStatus"
                allow-clear
                :options="receivableStatusOptions.map((item) => ({ label: item, value: item }))"
                placeholder="选择回款状态"
              />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">客户 / 相对方</span>
              <Input v-model:value="query.counterpartyName" allow-clear placeholder="输入客户或相对方" />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">供应商 / 分包方</span>
              <Input v-model:value="query.supplierName" allow-clear placeholder="输入供应商或分包方" />
            </div>
            <div class="business-reports__field business-reports__field--range">
              <span class="business-reports__field-label">签订日期</span>
              <div class="business-reports__date-row">
                <DatePicker v-model:value="query.signDateFrom" class="business-reports__date" placeholder="开始日期" value-format="YYYY-MM-DD" />
                <span class="business-reports__date-divider">至</span>
                <DatePicker v-model:value="query.signDateTo" class="business-reports__date" placeholder="结束日期" value-format="YYYY-MM-DD" />
              </div>
            </div>
            <div class="business-reports__field business-reports__field--range">
              <span class="business-reports__field-label">计划日期</span>
              <div class="business-reports__date-row">
                <DatePicker v-model:value="query.plannedDateFrom" class="business-reports__date" placeholder="开始日期" value-format="YYYY-MM-DD" />
                <span class="business-reports__date-divider">至</span>
                <DatePicker v-model:value="query.plannedDateTo" class="business-reports__date" placeholder="结束日期" value-format="YYYY-MM-DD" />
              </div>
            </div>
          </div>
        </div>

        <div class="business-reports__filter-actions">
          <Button type="primary" @click="loadReport">查询</Button>
          <Button @click="resetQuery">重置</Button>
        </div>
      </div>

      <Skeleton v-if="loading" active :paragraph="{ rows: 10 }" />

      <template v-else-if="report">
        <Table
          v-if="activeTab === 'project-search'"
          class="business-reports__table"
          :columns="projectColumns"
          :data-source="report.projectItems"
          :scroll="reportTableScroll"
          row-key="id"
          size="middle"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'contractAmount'">{{ formatMoney(record.contractAmount) }}</template>
            <template v-else-if="column.key === 'receivedAmount'">{{ formatMoney(record.receivedAmount) }}</template>
            <template v-else-if="column.key === 'businessCloseStatus'">
              <Tag :color="record.businessCloseStatus.includes('收尾') || record.businessCloseStatus.includes('待回款') ? 'gold' : 'blue'">
                {{ record.businessCloseStatus }}
              </Tag>
            </template>
            <template v-else-if="column.key === 'action'">
              <Button size="small" type="link" @click="openProject(record.id, 'overview')">打开</Button>
            </template>
          </template>
        </Table>

        <Table
          v-else-if="activeTab === 'contract-search'"
          class="business-reports__table"
          :columns="contractColumns"
          :data-source="report.contractItems"
          :scroll="reportTableScroll"
          row-key="id"
          size="middle"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'amount'">{{ formatMoney(record.amount) }}</template>
            <template v-else-if="column.key === 'signDate'">{{ record.signDate?.slice(0, 10) }}</template>
            <template v-else-if="column.key === 'action'">
              <Button size="small" type="link" @click="openProject(record.projectId, 'income')">项目详情</Button>
            </template>
          </template>
        </Table>

        <Table
          v-else-if="activeTab === 'procurement-search'"
          class="business-reports__table"
          :columns="procurementColumns"
          :data-source="report.procurementItems"
          :scroll="reportTableScroll"
          row-key="id"
          size="middle"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'amount'">{{ formatMoney(record.amount) }}</template>
            <template v-else-if="column.key === 'signDate'">{{ record.signDate?.slice(0, 10) }}</template>
            <template v-else-if="column.key === 'action'">
              <Button size="small" type="link" @click="openProject(record.projectId, 'business')">项目详情</Button>
            </template>
          </template>
        </Table>

        <Table
          v-else-if="activeTab === 'receivable-search'"
          class="business-reports__table"
          :columns="receivableColumns"
          :data-source="report.receivableItems"
          :scroll="reportTableScroll"
          row-key="id"
          size="middle"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'totalAmount'">{{ formatMoney(record.totalAmount) }}</template>
            <template v-else-if="column.key === 'receivedAmount'">{{ formatMoney(record.receivedAmount) }}</template>
            <template v-else-if="column.key === 'plannedDate'">{{ record.plannedDate?.slice(0, 10) }}</template>
            <template v-else-if="column.key === 'action'">
              <Button size="small" type="link" @click="openProject(record.projectId, 'receivable')">项目详情</Button>
            </template>
          </template>
        </Table>

        <Table
          v-else-if="activeTab === 'profit-analysis'"
          class="business-reports__table"
          :columns="profitColumns"
          :data-source="report.profitItems"
          :scroll="reportTableScroll"
          row-key="id"
          size="middle"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'grossProfitAmount' || column.key === 'yearEndReceivableForecast'">
              {{ formatMoney(record[column.key]) }}
            </template>
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
              <Button size="small" type="link" @click="openProject(record.id, 'profit')">项目详情</Button>
            </template>
          </template>
        </Table>

        <Table
          v-else
          class="business-reports__table"
          :columns="warningColumns"
          :data-source="report.warningItems"
          :scroll="reportTableScroll"
          row-key="id"
          size="middle"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'type'">
              <Tag :color="record.type.includes('利润') ? 'red' : record.type.includes('回款') ? 'gold' : 'blue'">{{ record.type }}</Tag>
            </template>
            <template v-else-if="column.key === 'level'">
              <Tag :color="record.level === '高' ? 'red' : 'gold'">{{ record.level }}级</Tag>
            </template>
            <template v-else-if="column.key === 'actionTab'">{{ tabTextMap[record.actionTab] ?? record.actionTab }}</template>
            <template v-else-if="column.key === 'action'">
              <Button size="small" type="link" @click="openProject(record.projectId, record.actionTab)">项目详情</Button>
            </template>
          </template>
        </Table>
      </template>
    </Card>
  </Page>
</template>

<style scoped>
.business-reports__tabs :deep(.ant-tabs-nav) {
  margin-bottom: 20px;
}

.business-reports__tabs :deep(.ant-tabs-tab) {
  font-weight: 500;
  padding-left: 4px;
  padding-right: 4px;
}

.business-reports__filters {
  background: var(--ant-color-fill-quaternary);
  border-radius: 18px;
  margin-bottom: 16px;
  padding: 18px;
}

.business-reports__section + .business-reports__section {
  margin-top: 16px;
}

.business-reports__section-header {
  margin-bottom: 12px;
}

.business-reports__section-title {
  font-size: 14px;
  font-weight: 600;
}

.business-reports__field-grid {
  display: grid;
  gap: 14px 16px;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.business-reports__field {
  display: grid;
  gap: 8px;
  min-width: 0;
}

.business-reports__field--single {
  width: 100%;
}

.business-reports__field--range {
  grid-column: span 2;
}

.business-reports__field-label {
  color: var(--ant-color-text-secondary);
  font-size: 12px;
  line-height: 1;
}

.business-reports__date-row {
  align-items: center;
  display: grid;
  gap: 10px;
  grid-template-columns: minmax(0, 1fr) 24px minmax(0, 1fr);
}

.business-reports__date-divider {
  color: var(--ant-color-text-tertiary);
  text-align: center;
}

.business-reports__date {
  width: 100%;
}

.business-reports__filter-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  margin-top: 18px;
}

.business-reports__table :deep(.ant-table-wrapper),
.business-reports__table :deep(.ant-spin-nested-loading),
.business-reports__table :deep(.ant-spin-container),
.business-reports__table :deep(.ant-table),
.business-reports__table :deep(.ant-table-container) {
  min-width: 0;
  width: 100%;
}

.business-reports__table :deep(.ant-table-cell) {
  white-space: nowrap;
  word-break: keep-all;
}

.business-reports__table :deep(.ant-table-content) {
  overflow-x: auto;
}

@media (max-width: 1200px) {
  .business-reports__field-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .business-reports__field--range {
    grid-column: span 2;
  }
}

@media (max-width: 768px) {
  .business-reports__field-grid {
    grid-template-columns: 1fr;
  }

  .business-reports__field--range {
    grid-column: span 1;
  }

  .business-reports__date-row {
    grid-template-columns: 1fr;
  }

  .business-reports__date-divider {
    display: none;
  }

  .business-reports__filter-actions {
    justify-content: flex-start;
  }
}
</style>
