<script lang="ts" setup>
import type { NativeVxeGridEvents, VxeGridProps } from '#/adapter/vxe-table';

import { computed } from 'vue';
import { onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';
import { useAccess } from '@vben/access';

import {
  Alert,
  Button,
  Card,
  DatePicker,
  Input,
  Select,
  Skeleton,
  Tag,
  Tabs,
  message,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import {
  exportBusinessReportApi,
  getBusinessReportApi,
  type BusinessApi,
} from '#/api/core';
import {
  createDictOptionsFallback,
  useDictOptions,
} from '#/composables/platform/use-dict-options';
import { useLocalVxePagination } from '#/composables/platform/use-local-vxe-pagination';

defineOptions({ name: 'BusinessReportsPage' });

const route = useRoute();
const router = useRouter();
const { hasAccessByCodes } = useAccess();

const loading = ref(false);
const exporting = ref(false);
const report = ref<BusinessApi.Report | null>(null);
const activeTab = ref(
  typeof route.query.tab === 'string' ? route.query.tab : 'project-search',
);
const reportDicts = useDictOptions(
  ['business_close_status', 'contract_type'],
  createDictOptionsFallback({
    business_close_status: ['经营执行中', '经营收尾中', '执行关闭待回款'],
    contract_type: ['主合同', '补充合同', '分包合同', '采购合同'],
  }),
);

const reportTabs = [
  { key: 'project-search', label: '项目检索' },
  { key: 'contract-search', label: '合同检索' },
  { key: 'procurement-search', label: '采购检索' },
  { key: 'budget-search', label: '预算检索' },
  { key: 'receivable-search', label: '回款检索' },
  { key: 'invoice-search', label: '开票检索' },
  { key: 'acceptance-search', label: '验收检索' },
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
  warningLevel: undefined,
});

const BUSINESS_REPORT_EXPORT_CODE = 'Business.Report.Export';
const businessCloseStatusOptions = reportDicts.getOptions(
  'business_close_status',
);
const contractTypeOptions = reportDicts.getOptions('contract_type');
const receivableStatusOptions = ['已回款', '待回款'];
const warningLevelOptions = ['P0', 'P1', 'P2', 'P3'].map((item) => ({
  label: item,
  value: item,
}));
const canExportReport = computed(() =>
  hasAccessByCodes([BUSINESS_REPORT_EXPORT_CODE]),
);
const activeLedgerKind = computed(() => {
  const kindMap: Record<string, string> = {
    'acceptance-search': 'acceptance',
    'budget-search': 'budget',
    'contract-search': 'contract',
    'invoice-search': 'invoice',
    'procurement-search': 'procurement',
    'profit-analysis': 'profit',
    'project-search': 'project',
    'receivable-search': 'receivable',
    'warning-center': 'warning',
  };
  return kindMap[activeTab.value] ?? 'project';
});
const tabTextMap: Record<string, string> = {
  business: '商务与分包',
  income: '收入与合同',
  overview: '经营概览',
  profit: '利润与预测',
  receivable: '开票与回款',
  budget: '预算与执行',
};

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

const projectColumns = [
  {
    field: 'projectCode',
    title: '项目编号',
    width: 150,
  },
  { align: 'left', field: 'name', title: '项目名称', width: 260 },
  { align: 'left', field: 'customerName', title: '客户', width: 180 },
  {
    field: 'managerName',
    title: '项目经理',
    width: 140,
  },
  {
    field: 'contractAmount',
    slots: { default: 'contractAmount' },
    title: '合同收入',
    width: 140,
  },
  {
    field: 'receivedAmount',
    slots: { default: 'receivedAmount' },
    title: '已回款',
    width: 130,
  },
  {
    field: 'businessCloseStatus',
    slots: { default: 'businessCloseStatus' },
    title: '经营状态',
    width: 150,
  },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.ProjectSummary>['columns'];

const contractColumns = [
  {
    field: 'contractCode',
    title: '合同编号',
    width: 160,
  },
  { align: 'left', field: 'title', title: '合同名称', width: 260 },
  { field: 'type', title: '类型', width: 110 },
  {
    align: 'left',
    field: 'projectName',
    title: '所属项目',
    width: 200,
  },
  {
    align: 'left',
    field: 'counterpartyName',
    title: '相对方',
    width: 180,
  },
  {
    field: 'signDate',
    slots: { default: 'signDate' },
    title: '签订日期',
    width: 130,
  },
  { field: 'amount', slots: { default: 'amount' }, title: '金额', width: 130 },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.ContractItem>['columns'];

const procurementColumns = [
  {
    field: 'procurementCode',
    title: '采购编号',
    width: 160,
  },
  { align: 'left', field: 'title', title: '采购/分包事项', width: 260 },
  {
    align: 'left',
    field: 'projectName',
    title: '所属项目',
    width: 200,
  },
  {
    align: 'left',
    field: 'supplierName',
    title: '供应商/分包方',
    width: 180,
  },
  { field: 'stage', title: '阶段', width: 120 },
  {
    field: 'signDate',
    slots: { default: 'signDate' },
    title: '签订日期',
    width: 130,
  },
  { field: 'amount', slots: { default: 'amount' }, title: '金额', width: 130 },
  { field: 'status', title: '状态', width: 110 },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.ProcurementItem>['columns'];

const budgetColumns = [
  {
    field: 'budgetCode',
    title: '预算编号',
    width: 160,
  },
  { align: 'left', field: 'category', title: '预算项', width: 220 },
  {
    align: 'left',
    field: 'projectName',
    title: '所属项目',
    width: 200,
  },
  {
    field: 'adjustedAmount',
    slots: { default: 'adjustedAmount' },
    title: '调整后预算',
    width: 140,
  },
  {
    field: 'executedAmount',
    slots: { default: 'executedAmount' },
    title: '已执行金额',
    width: 140,
  },
  {
    field: 'varianceAmount',
    slots: { default: 'varianceAmount' },
    title: '偏差金额',
    width: 130,
  },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.BudgetExecutionItem>['columns'];

const receivableColumns = [
  {
    field: 'receivableCode',
    title: '回款编号',
    width: 160,
  },
  { align: 'left', field: 'title', title: '回款事项', width: 260 },
  {
    align: 'left',
    field: 'projectName',
    title: '所属项目',
    width: 200,
  },
  {
    field: 'plannedDate',
    slots: { default: 'plannedDate' },
    title: '计划日期',
    width: 130,
  },
  {
    field: 'linkedContractCode',
    title: '关联合同',
    width: 160,
  },
  {
    field: 'totalAmount',
    slots: { default: 'totalAmount' },
    title: '应回金额',
    width: 130,
  },
  {
    field: 'receivedAmount',
    slots: { default: 'receivedAmount' },
    title: '已回金额',
    width: 130,
  },
  { field: 'status', title: '状态', width: 110 },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.ReceivableItem>['columns'];

const invoiceColumns = [
  {
    field: 'invoiceCode',
    title: '发票编号',
    width: 160,
  },
  { align: 'left', field: 'title', title: '开票标题', width: 240 },
  {
    align: 'left',
    field: 'projectName',
    title: '所属项目',
    width: 200,
  },
  {
    field: 'invoiceDate',
    slots: { default: 'invoiceDate' },
    title: '开票日期',
    width: 130,
  },
  {
    field: 'linkedContractCode',
    title: '关联合同',
    width: 160,
  },
  { field: 'amount', slots: { default: 'amount' }, title: '金额', width: 130 },
  {
    field: 'taxAmount',
    slots: { default: 'taxAmount' },
    title: '税额',
    width: 130,
  },
  { field: 'status', title: '状态', width: 110 },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.InvoiceItem>['columns'];

const acceptanceColumns = [
  {
    field: 'acceptanceCode',
    title: '验收编号',
    width: 160,
  },
  { align: 'left', field: 'title', title: '验收标题', width: 240 },
  {
    align: 'left',
    field: 'projectName',
    title: '所属项目',
    width: 200,
  },
  {
    field: 'acceptanceDate',
    slots: { default: 'acceptanceDate' },
    title: '验收日期',
    width: 130,
  },
  {
    field: 'linkedContractCode',
    title: '关联合同',
    width: 160,
  },
  {
    field: 'acceptanceResult',
    title: '验收结果',
    width: 120,
  },
  { field: 'status', title: '状态', width: 110 },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.AcceptanceItem>['columns'];

const profitColumns = [
  {
    field: 'projectCode',
    title: '项目编号',
    width: 150,
  },
  { align: 'left', field: 'name', title: '项目名称', width: 260 },
  {
    field: 'grossProfitAmount',
    slots: { default: 'grossProfitAmount' },
    title: '毛利额',
    width: 130,
  },
  {
    field: 'grossProfitRate',
    slots: { default: 'grossProfitRate' },
    title: '毛利率',
    width: 110,
  },
  {
    field: 'yearEndReceivableForecast',
    slots: { default: 'yearEndReceivableForecast' },
    title: '年底预计回款',
    width: 150,
  },
  {
    field: 'businessCloseStatus',
    slots: { default: 'businessCloseStatus' },
    title: '经营状态',
    width: 150,
  },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.ProjectSummary>['columns'];

const warningColumns = [
  { field: 'type', slots: { default: 'type' }, title: '预警类型', width: 120 },
  { field: 'level', slots: { default: 'level' }, title: '等级', width: 90 },
  {
    field: 'projectCode',
    title: '项目编号',
    width: 150,
  },
  {
    align: 'left',
    field: 'projectName',
    title: '项目名称',
    width: 260,
  },
  {
    align: 'left',
    field: 'description',
    title: '预警说明',
    width: 360,
  },
  { field: 'ownerName', title: '责任人', width: 120 },
  {
    field: 'lastUpdatedTime',
    title: '最近更新时间',
    width: 170,
  },
  { field: 'status', title: '当前状态', width: 150 },
  {
    field: 'actionTab',
    slots: { default: 'actionTab' },
    title: '定位页签',
    width: 120,
  },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '跳转',
    width: 100,
  },
] satisfies VxeGridProps<BusinessApi.WarningItem>['columns'];

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

function createReportGridOptions<T extends Record<string, any>>(
  columns: VxeGridProps<T>['columns'],
  data: T[],
  pagerConfig: VxeGridProps<T>['pagerConfig'],
  keyField = 'id',
): VxeGridProps<T> {
  return {
    columns,
    data,
    emptyText: '暂无数据',
    maxHeight: 560,
    pagerConfig,
    rowConfig: { keyField },
    scrollX: { enabled: true },
    size: 'medium',
  };
}

function useReportGrid<T extends Record<string, any>>(
  columns: VxeGridProps<T>['columns'],
  rows: { readonly value: T[] },
  keyField = 'id',
) {
  const pagination = useLocalVxePagination(rows, { defaultPageSize: 20 });
  const gridOptions = computed(() =>
    createReportGridOptions(
      columns,
      pagination.pagedRows.value,
      pagination.pagerConfig.value,
      keyField,
    ),
  );
  const gridEvents: NativeVxeGridEvents<T> = {
    'page-change': pagination.handlePageChange,
  };

  return { gridEvents, gridOptions };
}

const projectRows = computed(() => report.value?.projectItems || []);
const contractRows = computed(() => report.value?.contractItems || []);
const procurementRows = computed(() => report.value?.procurementItems || []);
const budgetRows = computed(() => report.value?.budgetItems || []);
const receivableRows = computed(() => report.value?.receivableItems || []);
const invoiceRows = computed(() => report.value?.invoiceItems || []);
const acceptanceRows = computed(() => report.value?.acceptanceItems || []);
const profitRows = computed(() => report.value?.profitItems || []);
const warningRows = computed(() => report.value?.warningItems || []);

const projectGrid = useReportGrid(projectColumns, projectRows);
const contractGrid = useReportGrid(contractColumns, contractRows);
const procurementGrid = useReportGrid(procurementColumns, procurementRows);
const budgetGrid = useReportGrid(budgetColumns, budgetRows);
const receivableGrid = useReportGrid(receivableColumns, receivableRows);
const invoiceGrid = useReportGrid(invoiceColumns, invoiceRows);
const acceptanceGrid = useReportGrid(acceptanceColumns, acceptanceRows);
const profitGrid = useReportGrid(profitColumns, profitRows);
const warningGrid = useReportGrid(warningColumns, warningRows);

const [ProjectGrid] = useVbenVxeGrid<BusinessApi.ProjectSummary>({
  gridOptions: {},
});
const [ContractGrid] = useVbenVxeGrid<BusinessApi.ContractItem>({
  gridOptions: {},
});
const [ProcurementGrid] = useVbenVxeGrid<BusinessApi.ProcurementItem>({
  gridOptions: {},
});
const [BudgetGrid] = useVbenVxeGrid<BusinessApi.BudgetExecutionItem>({
  gridOptions: {},
});
const [ReceivableGrid] = useVbenVxeGrid<BusinessApi.ReceivableItem>({
  gridOptions: {},
});
const [InvoiceGrid] = useVbenVxeGrid<BusinessApi.InvoiceItem>({
  gridOptions: {},
});
const [AcceptanceGrid] = useVbenVxeGrid<BusinessApi.AcceptanceItem>({
  gridOptions: {},
});
const [ProfitGrid] = useVbenVxeGrid<BusinessApi.ProjectSummary>({
  gridOptions: {},
});
const [WarningGrid] = useVbenVxeGrid<BusinessApi.WarningItem>({
  gridOptions: {},
});

function openProject(
  projectId: string,
  tab: string,
  focusType?: string,
  focusId?: string,
) {
  router.push({
    path: '/business/projects',
    query: {
      projectId,
      tab,
      ...(focusType && focusId ? { focusId, focusType } : {}),
    },
  });
}

function buildReportParams(): BusinessApi.ReportQuery {
  return {
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
    warningLevel: query.warningLevel || undefined,
  };
}

async function loadReport() {
  loading.value = true;
  try {
    report.value = await getBusinessReportApi(buildReportParams());
  } catch (error) {
    const description =
      error instanceof Error ? error.message : '经营报表加载失败';
    message.error(description || '经营报表加载失败');
  } finally {
    loading.value = false;
  }
}

async function exportCurrentReport() {
  if (!canExportReport.value) {
    message.warning('当前账号没有导出报表权限');
    return;
  }

  exporting.value = true;
  try {
    const blob = await exportBusinessReportApi({
      ...buildReportParams(),
      ledgerKind: activeLedgerKind.value,
    });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `经营报表_${getExportTimestamp()}.csv`;
    anchor.click();
    URL.revokeObjectURL(url);
    message.success('导出任务已完成');
  } catch (error) {
    const description =
      error instanceof Error ? error.message : '经营报表导出失败';
    message.error(description || '经营报表导出失败');
  } finally {
    exporting.value = false;
  }
}

function getExportTimestamp() {
  return new Date()
    .toISOString()
    .slice(0, 19)
    .replaceAll(':', '')
    .replace('T', '');
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
  query.warningLevel = undefined;
  void loadReport();
}

onMounted(async () => {
  await Promise.all([reportDicts.load(), loadReport()]);
});
</script>

<template>
  <Page
    description="报表中心承担跨项目检索和利润分析，解决大量合同与采购记录的定位问题。"
    title="报表中心"
  >
    <Alert
      banner
      class="business-reports__notice"
      message="筛选在当前页完成，检索结果支持直接跳转到项目经营详情并定位到对应页签。"
      type="info"
    />

    <Card class="rounded-2xl" :bordered="false">
      <Tabs
        v-model:activeKey="activeTab"
        class="business-reports__tabs"
        size="large"
      >
        <Tabs.TabPane
          v-for="tab in reportTabs"
          :key="tab.key"
          :tab="tab.label"
        />
      </Tabs>

      <div class="business-reports__filters">
        <div class="business-reports__section">
          <div class="business-reports__section-header">
            <span class="business-reports__section-title">关键字检索</span>
          </div>
          <div class="business-reports__field business-reports__field--single">
            <span class="business-reports__field-label">搜索内容</span>
            <Input
              v-model:value="query.keyword"
              allow-clear
              placeholder="项目、合同、供应商、客户、状态"
            />
          </div>
        </div>

        <div class="business-reports__section">
          <div class="business-reports__section-header">
            <span class="business-reports__section-title">筛选条件</span>
          </div>
          <div class="business-reports__field-grid">
            <div class="business-reports__field">
              <span class="business-reports__field-label">项目编号</span>
              <Input
                v-model:value="query.projectCode"
                allow-clear
                placeholder="输入项目编号"
              />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">经营状态</span>
              <Select
                v-model:value="query.businessCloseStatus"
                allow-clear
                :options="businessCloseStatusOptions"
                placeholder="选择经营状态"
              />
            </div>
            <div
              v-if="activeTab === 'warning-center'"
              class="business-reports__field"
            >
              <span class="business-reports__field-label">预警等级</span>
              <Select
                v-model:value="query.warningLevel"
                allow-clear
                :options="warningLevelOptions"
                placeholder="选择预警等级"
              />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">合同类型</span>
              <Select
                v-model:value="query.contractType"
                allow-clear
                :options="contractTypeOptions"
                placeholder="选择合同类型"
              />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">回款状态</span>
              <Select
                v-model:value="query.receivableStatus"
                allow-clear
                :options="
                  receivableStatusOptions.map((item) => ({
                    label: item,
                    value: item,
                  }))
                "
                placeholder="选择回款状态"
              />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">客户 / 相对方</span>
              <Input
                v-model:value="query.counterpartyName"
                allow-clear
                placeholder="输入客户或相对方"
              />
            </div>
            <div class="business-reports__field">
              <span class="business-reports__field-label">供应商 / 分包方</span>
              <Input
                v-model:value="query.supplierName"
                allow-clear
                placeholder="输入供应商或分包方"
              />
            </div>
            <div class="business-reports__field business-reports__field--range">
              <span class="business-reports__field-label">签订日期</span>
              <div class="business-reports__date-row">
                <DatePicker
                  v-model:value="query.signDateFrom"
                  class="business-reports__date"
                  placeholder="开始日期"
                  value-format="YYYY-MM-DD"
                />
                <span class="business-reports__date-divider">至</span>
                <DatePicker
                  v-model:value="query.signDateTo"
                  class="business-reports__date"
                  placeholder="结束日期"
                  value-format="YYYY-MM-DD"
                />
              </div>
            </div>
            <div class="business-reports__field business-reports__field--range">
              <span class="business-reports__field-label">计划日期</span>
              <div class="business-reports__date-row">
                <DatePicker
                  v-model:value="query.plannedDateFrom"
                  class="business-reports__date"
                  placeholder="开始日期"
                  value-format="YYYY-MM-DD"
                />
                <span class="business-reports__date-divider">至</span>
                <DatePicker
                  v-model:value="query.plannedDateTo"
                  class="business-reports__date"
                  placeholder="结束日期"
                  value-format="YYYY-MM-DD"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="business-reports__filter-actions">
          <Button type="primary" @click="loadReport">查询</Button>
          <Button
            :disabled="!canExportReport"
            :loading="exporting"
            @click="exportCurrentReport"
          >
            导出当前页签
          </Button>
          <Button @click="resetQuery">重置</Button>
        </div>
      </div>

      <Skeleton v-if="loading" active :paragraph="{ rows: 10 }" />

      <template v-else-if="report">
        <ProjectGrid
          v-if="activeTab === 'project-search'"
          class="app-vxe-card-grid business-reports__table"
          :grid-events="projectGrid.gridEvents"
          :grid-options="projectGrid.gridOptions.value"
        >
          <template #contractAmount="{ row }">{{
            formatMoney(row.contractAmount)
          }}</template>
          <template #receivedAmount="{ row }">{{
            formatMoney(row.receivedAmount)
          }}</template>
          <template #businessCloseStatus="{ row }">
            <Tag
              :color="
                reportDicts.getColor(
                  'business_close_status',
                  row.businessCloseStatus,
                  row.businessCloseStatus.includes('收尾') ||
                    row.businessCloseStatus.includes('待回款')
                    ? 'gold'
                    : 'blue',
                )
              "
            >
              {{ row.businessCloseStatus }}
            </Tag>
          </template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="openProject(row.id, 'overview')"
            >
              打开
            </Button>
          </template>
        </ProjectGrid>

        <ContractGrid
          v-else-if="activeTab === 'contract-search'"
          class="app-vxe-card-grid business-reports__table"
          :grid-events="contractGrid.gridEvents"
          :grid-options="contractGrid.gridOptions.value"
        >
          <template #amount="{ row }">{{ formatMoney(row.amount) }}</template>
          <template #signDate="{ row }">{{
            row.signDate?.slice(0, 10)
          }}</template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="openProject(row.projectId, 'income', 'contract', row.id)"
            >
              项目详情
            </Button>
          </template>
        </ContractGrid>

        <ProcurementGrid
          v-else-if="activeTab === 'procurement-search'"
          class="app-vxe-card-grid business-reports__table"
          :grid-events="procurementGrid.gridEvents"
          :grid-options="procurementGrid.gridOptions.value"
        >
          <template #amount="{ row }">{{ formatMoney(row.amount) }}</template>
          <template #signDate="{ row }">{{
            row.signDate?.slice(0, 10)
          }}</template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="
                openProject(row.projectId, 'business', 'procurement', row.id)
              "
            >
              项目详情
            </Button>
          </template>
        </ProcurementGrid>

        <BudgetGrid
          v-else-if="activeTab === 'budget-search'"
          class="app-vxe-card-grid business-reports__table"
          :grid-events="budgetGrid.gridEvents"
          :grid-options="budgetGrid.gridOptions.value"
        >
          <template #adjustedAmount="{ row }">{{
            formatMoney(row.adjustedAmount)
          }}</template>
          <template #executedAmount="{ row }">{{
            formatMoney(row.executedAmount)
          }}</template>
          <template #varianceAmount="{ row }">{{
            formatMoney(row.varianceAmount)
          }}</template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="openProject(row.projectId, 'budget', 'budget', row.id)"
            >
              项目详情
            </Button>
          </template>
        </BudgetGrid>

        <ReceivableGrid
          v-else-if="activeTab === 'receivable-search'"
          class="app-vxe-card-grid business-reports__table"
          :grid-events="receivableGrid.gridEvents"
          :grid-options="receivableGrid.gridOptions.value"
        >
          <template #totalAmount="{ row }">{{
            formatMoney(row.totalAmount)
          }}</template>
          <template #receivedAmount="{ row }">{{
            formatMoney(row.receivedAmount)
          }}</template>
          <template #plannedDate="{ row }">{{
            row.plannedDate?.slice(0, 10)
          }}</template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="
                openProject(row.projectId, 'receivable', 'receivable', row.id)
              "
            >
              项目详情
            </Button>
          </template>
        </ReceivableGrid>

        <InvoiceGrid
          v-else-if="activeTab === 'invoice-search'"
          class="app-vxe-card-grid business-reports__table"
          :grid-events="invoiceGrid.gridEvents"
          :grid-options="invoiceGrid.gridOptions.value"
        >
          <template #amount="{ row }">{{ formatMoney(row.amount) }}</template>
          <template #taxAmount="{ row }">{{
            formatMoney(row.taxAmount)
          }}</template>
          <template #invoiceDate="{ row }">{{
            row.invoiceDate?.slice(0, 10)
          }}</template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="
                openProject(row.projectId, 'receivable', 'invoice', row.id)
              "
            >
              项目详情
            </Button>
          </template>
        </InvoiceGrid>

        <AcceptanceGrid
          v-else-if="activeTab === 'acceptance-search'"
          class="app-vxe-card-grid business-reports__table"
          :grid-events="acceptanceGrid.gridEvents"
          :grid-options="acceptanceGrid.gridOptions.value"
        >
          <template #acceptanceDate="{ row }">{{
            row.acceptanceDate?.slice(0, 10)
          }}</template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="
                openProject(row.projectId, 'profit', 'acceptance', row.id)
              "
            >
              项目详情
            </Button>
          </template>
        </AcceptanceGrid>

        <ProfitGrid
          v-else-if="activeTab === 'profit-analysis'"
          class="app-vxe-card-grid business-reports__table"
          :grid-events="profitGrid.gridEvents"
          :grid-options="profitGrid.gridOptions.value"
        >
          <template #grossProfitAmount="{ row }">{{
            formatMoney(row.grossProfitAmount)
          }}</template>
          <template #yearEndReceivableForecast="{ row }">
            {{ formatMoney(row.yearEndReceivableForecast) }}
          </template>
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
                reportDicts.getColor(
                  'business_close_status',
                  row.businessCloseStatus,
                  row.businessCloseStatus.includes('收尾') ||
                    row.businessCloseStatus.includes('待回款')
                    ? 'gold'
                    : 'blue',
                )
              "
            >
              {{ row.businessCloseStatus }}
            </Tag>
          </template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="openProject(row.id, 'profit')"
            >
              项目详情
            </Button>
          </template>
        </ProfitGrid>

        <WarningGrid
          v-else
          class="app-vxe-card-grid business-reports__table"
          :grid-events="warningGrid.gridEvents"
          :grid-options="warningGrid.gridOptions.value"
        >
          <template #type="{ row }">
            <Tag
              :color="
                row.type.includes('利润')
                  ? 'red'
                  : row.type.includes('回款')
                    ? 'gold'
                    : 'blue'
              "
            >
              {{ row.type }}
            </Tag>
          </template>
          <template #level="{ row }">
            <Tag :color="resolvePriorityColor(row.level)">
              {{ row.level }}
            </Tag>
          </template>
          <template #actionTab="{ row }">
            {{ tabTextMap[row.actionTab] ?? row.actionTab }}
          </template>
          <template #action="{ row }">
            <Button
              size="small"
              type="link"
              @click="openProject(row.projectId, row.actionTab)"
            >
              项目详情
            </Button>
          </template>
        </WarningGrid>
      </template>
    </Card>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
.business-reports__notice {
  @apply mb-4;
}

.business-reports__tabs {
  @apply mb-5;
}

.business-reports__tabs :deep(.ant-tabs-tab) {
  font-weight: 500;
  @apply pl-1;
  @apply pr-1;
}

.business-reports__filters {
  background: var(--ant-color-fill-quaternary);
  @apply rounded-xl;
  @apply mb-4;
  @apply p-4;
}

.business-reports__section + .business-reports__section {
  @apply mt-4;
}

.business-reports__section-header {
  @apply mb-3;
}

.business-reports__section-title {
  color: var(--ant-color-text);
  font-size: 14px;
  font-weight: 600;
}

.business-reports__field-grid {
  display: grid;
  @apply gap-x-4 gap-y-3.5;
  grid-template-columns: repeat(4, minmax(0, 1fr));
}

.business-reports__field {
  display: grid;
  @apply gap-2;
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
  @apply gap-2.5;
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
  flex-wrap: wrap;
  @apply gap-2;
  justify-content: flex-end;
  @apply mt-4;
}

.business-reports__table {
  min-width: 0;
}

@media (max-width: 1200px) {
  .business-reports__field-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
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
