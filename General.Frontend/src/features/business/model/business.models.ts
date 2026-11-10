import type { BusinessApi } from '@/api/core/business';
import type { ListPageModel, ListPageRow, PageColumn, PageStat } from '@/components/page';

export interface BusinessProjectRow extends ListPageRow {
  businessCloseStatus: string;
  contractAmount: string;
  customerName: string;
  grossProfitAmount: string;
  grossProfitRate: string;
  managerName: string;
  name: string;
  projectCode: string;
  receivedAmount: string;
  status: string;
}

export interface BusinessSignalRow extends ListPageRow {
  description: string;
  lastUpdatedTime: string;
  level: string;
  ownerName: string;
  projectCode: string;
  projectId: string;
  projectName: string;
  signalType: string;
  status: string;
}

export interface BusinessReportRow extends ListPageRow {
  [key: string]: boolean | number | string | null | undefined;
}

export interface BusinessDetailViewRow {
  id: string;
  label: string;
  primary: string;
  secondary: string;
  status?: string;
}

export type BusinessReportSection =
  | 'acceptances'
  | 'budgets'
  | 'contracts'
  | 'invoices'
  | 'profit'
  | 'procurements'
  | 'projects'
  | 'receivables'
  | 'warnings';

const reportSectionLabels: Record<BusinessReportSection, string> = {
  acceptances: '验收',
  budgets: '预算执行',
  contracts: '合同',
  invoices: '开票',
  profit: '利润分析',
  procurements: '采购',
  projects: '项目经营',
  receivables: '应收',
  warnings: '经营预警',
};

const reportSectionColumns: Record<BusinessReportSection, PageColumn[]> = {
  acceptances: [
    { key: 'acceptanceCode', title: '验收编号', width: 140 },
    { key: 'title', title: '验收事项', width: 220 },
    { key: 'projectName', title: '项目', width: 180 },
    { key: 'acceptanceDate', title: '验收日期', width: 120 },
    { key: 'acceptanceResult', title: '结果', width: 120 },
    { key: 'status', title: '状态', width: 100, presentation: 'status' },
  ],
  budgets: [
    { key: 'budgetCode', title: '预算编号', width: 140 },
    { key: 'category', title: '类别', width: 140 },
    { key: 'projectName', title: '项目', width: 180 },
    { key: 'adjustedAmount', title: '调整后金额', width: 140 },
    { key: 'executedAmount', title: '已执行金额', width: 140 },
    { key: 'varianceAmount', title: '差异金额', width: 140 },
  ],
  contracts: [
    { key: 'contractCode', title: '合同编号', width: 140 },
    { key: 'title', title: '合同名称', width: 220 },
    { key: 'projectName', title: '项目', width: 180 },
    { key: 'counterpartyName', title: '合同方', width: 180 },
    { key: 'amount', title: '金额', width: 140 },
    { key: 'signDate', title: '签约日期', width: 120 },
  ],
  invoices: [
    { key: 'invoiceCode', title: '发票编号', width: 140 },
    { key: 'title', title: '发票事项', width: 220 },
    { key: 'projectName', title: '项目', width: 180 },
    { key: 'invoiceDate', title: '开票日期', width: 120 },
    { key: 'amount', title: '金额', width: 140 },
    { key: 'status', title: '状态', width: 100, presentation: 'status' },
  ],
  profit: [
    { key: 'projectCode', title: '项目编号', width: 140 },
    { key: 'name', title: '项目名称', width: 220 },
    { key: 'customerName', title: '客户', width: 180 },
    { key: 'contractAmount', title: '合同金额', width: 140 },
    { key: 'costAmount', title: '成本金额', width: 140 },
    { key: 'grossProfitAmount', title: '毛利金额', width: 140 },
    { key: 'grossProfitRate', title: '毛利率', width: 100 },
  ],
  procurements: [
    { key: 'procurementCode', title: '采购编号', width: 140 },
    { key: 'title', title: '采购事项', width: 220 },
    { key: 'projectName', title: '项目', width: 180 },
    { key: 'supplierName', title: '供应商', width: 180 },
    { key: 'amount', title: '金额', width: 140 },
    { key: 'signDate', title: '签约日期', width: 120 },
  ],
  projects: [
    { key: 'projectCode', title: '项目编号', width: 140 },
    { key: 'name', title: '项目名称', width: 220 },
    { key: 'customerName', title: '客户', width: 180 },
    { key: 'managerName', title: '项目经理', width: 140 },
    { key: 'businessCloseStatus', title: '经营收尾', width: 150 },
    { key: 'status', title: '项目状态', width: 120, presentation: 'status' },
  ],
  receivables: [
    { key: 'receivableCode', title: '应收编号', width: 140 },
    { key: 'title', title: '应收事项', width: 220 },
    { key: 'projectName', title: '项目', width: 180 },
    { key: 'plannedDate', title: '计划日期', width: 120 },
    { key: 'totalAmount', title: '应收金额', width: 140 },
    { key: 'receivedAmount', title: '已收金额', width: 140 },
    { key: 'status', title: '状态', width: 100, presentation: 'status' },
  ],
  warnings: [
    { key: 'projectCode', title: '项目编号', width: 140 },
    { key: 'projectName', title: '项目名称', width: 220 },
    { key: 'type', title: '预警类型', width: 140 },
    { key: 'level', title: '级别', width: 100, presentation: 'status' },
    { key: 'description', title: '说明', width: 300 },
    { key: 'ownerName', title: '负责人', width: 120 },
    { key: 'status', title: '状态', width: 100, presentation: 'status' },
  ],
};

const amountFormatter = new Intl.NumberFormat('zh-CN', {
  maximumFractionDigits: 2,
  minimumFractionDigits: 2,
});
const rateFormatter = new Intl.NumberFormat('zh-CN', {
  maximumFractionDigits: 1,
  minimumFractionDigits: 1,
});
const indexFormatter = new Intl.NumberFormat('zh-CN', { maximumFractionDigits: 2 });

export function formatBusinessAmount(value: null | number): string {
  return value === null || !Number.isFinite(value) ? '—' : amountFormatter.format(value);
}

export function formatBusinessRate(value: null | number): string {
  return value === null || !Number.isFinite(value) ? '—' : `${rateFormatter.format(value)}%`;
}

export function formatBusinessIndex(value: null | number): string {
  return value === null || !Number.isFinite(value) ? '—' : indexFormatter.format(value);
}

export function formatBusinessDate(value: null | string): string {
  return value ? value.slice(0, 10) : '—';
}

export function buildBusinessProjectRows(projects: readonly BusinessApi.ProjectSummary[]): BusinessProjectRow[] {
  return projects.map((item) => ({
    businessCloseStatus: item.businessCloseStatus,
    contractAmount: formatBusinessAmount(item.contractAmount),
    customerName: item.customerName,
    grossProfitAmount: formatBusinessAmount(item.grossProfitAmount),
    grossProfitRate: formatBusinessRate(item.grossProfitRate),
    id: item.id,
    managerName: item.managerName,
    name: item.name,
    projectCode: item.projectCode,
    receivedAmount: formatBusinessAmount(item.receivedAmount),
    status: item.status,
  }));
}

export function buildBusinessOverviewStats(overview: BusinessApi.Overview | null): PageStat[] {
  if (!overview) return [];

  return [
    { key: 'contract', label: '合同金额', value: formatBusinessAmount(overview.contractAmount), tone: 'primary' },
    { key: 'cost', label: '经营成本', value: formatBusinessAmount(overview.costAmount), tone: 'neutral' },
    { key: 'profit', label: '毛利金额', value: formatBusinessAmount(overview.grossProfitAmount), tone: 'success' },
    { key: 'received', label: '已回款金额', value: formatBusinessAmount(overview.receivedAmount), tone: 'primary' },
    { key: 'low-profit', label: '低利润项目', value: String(overview.lowProfitProjectCount), tone: 'warning' },
    {
      key: 'receivable-risk',
      label: '应收风险项目',
      value: String(overview.receivableRiskProjectCount),
      tone: 'danger',
    },
    { key: 'tail-closing', label: '经营收尾项目', value: String(overview.tailClosingProjectCount), tone: 'warning' },
  ];
}

export function buildBusinessProjectListModel(
  projects: readonly BusinessApi.ProjectSummary[],
  loading: boolean,
  error: string,
  pageSize = 10,
): ListPageModel<BusinessProjectRow> {
  return {
    actions: [{ id: 'refresh', kind: 'refresh', label: '刷新' }],
    columns: [
      { key: 'projectCode', title: '项目编号', width: 140 },
      { key: 'name', title: '项目名称', width: 220 },
      { key: 'customerName', title: '客户', width: 180 },
      { key: 'managerName', title: '项目经理', width: 140 },
      { key: 'status', title: '项目状态', width: 110, presentation: 'status' },
      { key: 'businessCloseStatus', title: '经营收尾', width: 150 },
      { key: 'contractAmount', title: '合同金额', width: 140 },
      { key: 'receivedAmount', title: '已回款金额', width: 140 },
      { key: 'grossProfitAmount', title: '毛利金额', width: 140 },
      { key: 'grossProfitRate', title: '毛利率', width: 110 },
    ],
    demoPath: '/business/projects',
    description: '项目经营摘要、状态和金额口径来自后端项目经营汇总。',
    emptyHint: loading ? '正在读取经营项目…' : error ? '经营项目读取失败，可重试。' : '当前条件下没有可见经营项目。',
    filters: [],
    key: '经营管理|项目经营',
    label: '项目经营',
    listScopes: [{ count: projects.length, id: 'all', label: '全部项目' }],
    listTabs: [{ id: 'all', label: '全部项目' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: '经营管理',
    moduleSlug: 'business',
    pageSize: Math.max(1, pageSize),
    pageSlug: 'business-projects',
    panels: [],
    path: '/business/projects',
    primaryAction: '',
    replication: { referenceUrl: 'https://demo.risemap.cn/business/projects', status: 'implemented' },
    routeName: 'business-projects',
    rows: buildBusinessProjectRows(projects),
    rowActions: ['view'],
    searchPlaceholder: '搜索项目编号、名称、客户或负责人',
    stats: [],
    surface: 'list',
    tabs: ['全部项目'],
    tools: [],
  };
}

export function buildBusinessOverviewModel(
  overview: BusinessApi.Overview | null,
  loading: boolean,
  error: string,
): ListPageModel<BusinessSignalRow> {
  const projectCodes = new Map((overview?.projects ?? []).map((item) => [item.id, item.projectCode]));
  const warnings: BusinessSignalRow[] = (overview?.warningItems ?? []).map((item) => ({
    description: item.description,
    id: item.id,
    lastUpdatedTime: formatBusinessDate(item.lastUpdatedTime),
    level: item.level,
    ownerName: item.ownerName,
    projectCode: item.projectCode,
    projectId: item.projectId,
    projectName: item.projectName,
    signalType: item.type,
    status: item.status,
  }));
  const focusItems: BusinessSignalRow[] = (overview?.focusProjects ?? []).map((item, index) => ({
    description: item.description,
    id: `focus-${item.projectId}-${index}`,
    lastUpdatedTime: '—',
    level: '—',
    ownerName: '—',
    projectCode: projectCodes.get(item.projectId) ?? '—',
    projectId: item.projectId,
    projectName: item.projectName,
    signalType: item.type,
    status: '—',
  }));

  return {
    actions: [{ id: 'refresh', kind: 'refresh', label: '刷新' }],
    columns: [
      { key: 'projectCode', title: '项目编号', width: 140 },
      { key: 'projectName', title: '项目名称', width: 200 },
      { key: 'signalType', title: '关注事项', width: 140 },
      { key: 'level', title: '级别', width: 90, presentation: 'status' },
      { key: 'description', title: '说明', width: 300 },
      { key: 'ownerName', title: '负责人', width: 120 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
      { key: 'lastUpdatedTime', title: '更新时间', width: 140 },
    ],
    demoPath: '/business/overview',
    description: '后端聚合的经营指标、重点事项和经营预警；金额权限按服务端结果显示。',
    emptyHint: loading
      ? '正在读取经营总览…'
      : error
        ? '经营总览读取失败，可重试。'
        : '当前数据范围内没有经营关注或预警。',
    filters: [],
    key: '经营管理|经营总览',
    label: '经营总览',
    listScopes: [{ count: warnings.length + focusItems.length, id: 'all', label: '重点关注' }],
    listTabs: [{ id: 'all', label: '重点关注与预警' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: '经营管理',
    moduleSlug: 'business',
    pageSize: 10,
    pageSlug: 'business-overview',
    panels: [],
    path: '/business/overview',
    primaryAction: '',
    replication: { referenceUrl: 'https://demo.risemap.cn/business/overview', status: 'implemented' },
    routeName: 'business-overview',
    rows: [...warnings, ...focusItems],
    rowActions: ['view'],
    searchPlaceholder: '搜索项目编号、名称或经营事项',
    stats: buildBusinessOverviewStats(overview),
    surface: 'list',
    tabs: ['重点关注与预警'],
    tools: [],
  };
}

export function buildBusinessDetailSummaryRows(
  summary: BusinessApi.ProjectSummary,
): Array<{ label: string; value: string }> {
  return [
    { label: '项目编号', value: summary.projectCode },
    { label: '项目名称', value: summary.name },
    { label: '客户', value: summary.customerName },
    { label: '项目经理', value: summary.managerName },
    { label: '项目状态', value: summary.status },
    { label: '经营收尾', value: summary.businessCloseStatus },
    { label: '合同金额', value: formatBusinessAmount(summary.contractAmount) },
    { label: '经营成本', value: formatBusinessAmount(summary.costAmount) },
    { label: '采购金额', value: formatBusinessAmount(summary.procurementAmount) },
    { label: '开票金额', value: formatBusinessAmount(summary.invoicedAmount) },
    { label: '已回款金额', value: formatBusinessAmount(summary.receivedAmount) },
    { label: '毛利金额', value: formatBusinessAmount(summary.grossProfitAmount) },
    { label: '毛利率', value: formatBusinessRate(summary.grossProfitRate) },
    { label: 'SPI', value: formatBusinessIndex(summary.schedulePerformanceIndex) },
    { label: 'CPI', value: formatBusinessIndex(summary.costPerformanceIndex) },
    { label: '年末应收预测', value: formatBusinessAmount(summary.yearEndReceivableForecast) },
  ];
}

export function buildBusinessDetailRows(detail: BusinessApi.ProjectDetail): BusinessDetailViewRow[] {
  return [
    ...detail.businessChains.map((item) => ({
      id: `chain-${item.id}`,
      label: `${item.stage} · ${item.type}`,
      primary: item.title,
      secondary: `${item.chainCode} · ${item.ownerName} · ${item.summary}`,
      status: item.status,
    })),
    ...detail.contracts.map((item) => ({
      id: `contract-${item.id}`,
      label: `${item.contractCode} · ${item.type}`,
      primary: item.title,
      secondary: `${item.projectName} · ${item.counterpartyName} · ${formatBusinessAmount(item.amount)} · ${formatBusinessDate(item.signDate)}`,
      status: item.status,
    })),
    ...detail.budgetExecutions.map((item) => ({
      id: `budget-${item.id}`,
      label: `${item.budgetCode} · ${item.category}`,
      primary: item.projectName,
      secondary: `调整后 ${formatBusinessAmount(item.adjustedAmount)} · 已执行 ${formatBusinessAmount(item.executedAmount)} · 差异 ${formatBusinessAmount(item.varianceAmount)}`,
    })),
    ...detail.procurements.map((item) => ({
      id: `procurement-${item.id}`,
      label: `${item.procurementCode} · ${item.supplierName}`,
      primary: item.title,
      secondary: `${item.projectName} · ${item.stage} · ${formatBusinessAmount(item.amount)} · ${formatBusinessDate(item.signDate)}`,
      status: item.status,
    })),
    ...detail.acceptances.map((item) => ({
      id: `acceptance-${item.id}`,
      label: `${item.acceptanceCode} · ${formatBusinessDate(item.acceptanceDate)}`,
      primary: item.title,
      secondary: `${item.projectName} · ${item.acceptanceResult} · ${item.linkedContractCode}`,
      status: item.status,
    })),
    ...detail.invoices.map((item) => ({
      id: `invoice-${item.id}`,
      label: `${item.invoiceCode} · ${formatBusinessDate(item.invoiceDate)}`,
      primary: item.title,
      secondary: `${item.projectName} · ${formatBusinessAmount(item.amount)} · ${item.linkedContractCode}`,
      status: item.status,
    })),
    ...detail.receivables.map((item) => ({
      id: `receivable-${item.id}`,
      label: `${item.receivableCode} · ${formatBusinessDate(item.plannedDate)}`,
      primary: item.title,
      secondary: `${item.projectName} · 应收 ${formatBusinessAmount(item.totalAmount)} · 已收 ${formatBusinessAmount(item.receivedAmount)}`,
      status: item.status,
    })),
    ...detail.forecastHistories.map((item) => ({
      id: `forecast-${item.id}`,
      label: `${item.metric} · ${item.changeType}`,
      primary: `${item.oldValue} → ${item.newValue}`,
      secondary: `${item.changedAt} · ${item.changedBy} · ${item.reason}`,
    })),
  ];
}

export function businessReportSectionLabel(section: BusinessReportSection): string {
  return reportSectionLabels[section];
}

export function buildBusinessReportRows(
  report: BusinessApi.Report | null,
  section: BusinessReportSection,
): BusinessReportRow[] {
  if (!report) return [];

  switch (section) {
    case 'acceptances':
      return report.acceptanceItems.map((item) => ({
        acceptanceCode: item.acceptanceCode,
        acceptanceDate: formatBusinessDate(item.acceptanceDate),
        acceptanceResult: item.acceptanceResult,
        id: item.id,
        linkedContractCode: item.linkedContractCode,
        projectName: item.projectName,
        status: item.status,
        title: item.title,
      }));
    case 'budgets':
      return report.budgetItems.map((item) => ({
        adjustedAmount: formatBusinessAmount(item.adjustedAmount),
        budgetCode: item.budgetCode,
        category: item.category,
        executedAmount: formatBusinessAmount(item.executedAmount),
        id: item.id,
        projectName: item.projectName,
        varianceAmount: formatBusinessAmount(item.varianceAmount),
      }));
    case 'contracts':
      return report.contractItems.map((item) => ({
        amount: formatBusinessAmount(item.amount),
        contractCode: item.contractCode,
        counterpartyName: item.counterpartyName,
        id: item.id,
        projectName: item.projectName,
        signDate: formatBusinessDate(item.signDate),
        status: item.status,
        title: item.title,
        type: item.type,
      }));
    case 'invoices':
      return report.invoiceItems.map((item) => ({
        amount: formatBusinessAmount(item.amount),
        id: item.id,
        invoiceCode: item.invoiceCode,
        invoiceDate: formatBusinessDate(item.invoiceDate),
        projectName: item.projectName,
        status: item.status,
        taxAmount: formatBusinessAmount(item.taxAmount),
        title: item.title,
      }));
    case 'profit':
      return report.profitItems.map((item) => ({
        businessCloseStatus: item.businessCloseStatus,
        contractAmount: formatBusinessAmount(item.contractAmount),
        costAmount: formatBusinessAmount(item.costAmount),
        customerName: item.customerName,
        grossProfitAmount: formatBusinessAmount(item.grossProfitAmount),
        grossProfitRate: formatBusinessRate(item.grossProfitRate),
        id: item.id,
        name: item.name,
        projectCode: item.projectCode,
        status: item.status,
      }));
    case 'procurements':
      return report.procurementItems.map((item) => ({
        amount: formatBusinessAmount(item.amount),
        id: item.id,
        procurementCode: item.procurementCode,
        projectName: item.projectName,
        signDate: formatBusinessDate(item.signDate),
        stage: item.stage,
        status: item.status,
        supplierName: item.supplierName,
        title: item.title,
      }));
    case 'projects':
      return report.projectItems.map((item) => ({
        businessCloseStatus: item.businessCloseStatus,
        customerName: item.customerName,
        id: item.id,
        managerName: item.managerName,
        name: item.name,
        projectCode: item.projectCode,
        status: item.status,
      }));
    case 'receivables':
      return report.receivableItems.map((item) => ({
        id: item.id,
        plannedDate: formatBusinessDate(item.plannedDate),
        projectName: item.projectName,
        receivedAmount: formatBusinessAmount(item.receivedAmount),
        receivableCode: item.receivableCode,
        status: item.status,
        title: item.title,
        totalAmount: formatBusinessAmount(item.totalAmount),
      }));
    case 'warnings':
      return report.warningItems.map((item) => ({
        description: item.description,
        id: item.id,
        lastUpdatedTime: formatBusinessDate(item.lastUpdatedTime),
        level: item.level,
        ownerName: item.ownerName,
        projectCode: item.projectCode,
        projectName: item.projectName,
        status: item.status,
        type: item.type,
      }));
  }
}

export function buildBusinessReportListModel(
  report: BusinessApi.Report | null,
  section: BusinessReportSection,
  loading: boolean,
  error: string,
): ListPageModel<BusinessReportRow> {
  const rows = buildBusinessReportRows(report, section);
  return {
    actions: [{ id: 'refresh', kind: 'refresh', label: '重新查询' }],
    columns: reportSectionColumns[section],
    demoPath: '/business/reports',
    description: '查询结果按报表权限、项目数据范围和后端字段口径返回。',
    emptyHint: loading
      ? '正在查询经营报表…'
      : error
        ? '报表查询失败，可重试。'
        : `当前条件下没有${reportSectionLabels[section]}记录。`,
    filters: [],
    key: `经营报表|${reportSectionLabels[section]}`,
    label: reportSectionLabels[section],
    listScopes: [{ count: rows.length, id: 'all', label: `全部${reportSectionLabels[section]}` }],
    listTabs: [{ id: 'all', label: `全部${reportSectionLabels[section]}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: '经营管理',
    moduleSlug: 'business',
    pageSize: 10,
    pageSlug: `business-report-${section}`,
    panels: [],
    path: '/business/reports',
    primaryAction: '',
    replication: { referenceUrl: 'https://demo.risemap.cn/business/reports', status: 'implemented' },
    routeName: 'business-reports',
    rows,
    rowActions: ['view'],
    searchPlaceholder: '在当前查询结果中搜索',
    stats: [{ key: 'total', label: '查询结果', value: String(rows.length), tone: 'primary' }],
    surface: 'list',
    tabs: [`全部${reportSectionLabels[section]}`],
    tools: [],
  };
}

export function buildBusinessReportQuery(form: BusinessApi.ReportQuery): BusinessApi.ReportQuery {
  const query: BusinessApi.ReportQuery = {};
  for (const key of [
    'businessCloseStatus',
    'contractType',
    'counterpartyName',
    'keyword',
    'plannedDateFrom',
    'plannedDateTo',
    'projectCode',
    'receivableStatus',
    'signDateFrom',
    'signDateTo',
    'supplierName',
    'warningLevel',
  ] as const) {
    const value = form[key]?.trim();
    if (value) query[key] = value;
  }
  return query;
}

export function getBusinessReportColumns(section: BusinessReportSection): readonly PageColumn[] {
  return reportSectionColumns[section];
}
