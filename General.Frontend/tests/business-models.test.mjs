import assert from 'node:assert/strict';
import test from 'node:test';

import {
  buildBusinessDetailRows,
  buildBusinessDetailSummaryRows,
  buildBusinessOverviewModel,
  buildBusinessOverviewStats,
  buildBusinessProjectRows,
  buildBusinessReportQuery,
  buildBusinessReportRows,
  formatBusinessAmount,
  formatBusinessDate,
  formatBusinessIndex,
  formatBusinessRate,
} from '../src/features/business/model/business.models.ts';

const projectSummary = {
  businessCloseStatus: '经营执行中',
  contractAmount: 1200.5,
  costAmount: 800,
  costPerformanceIndex: 1.04,
  customerName: '示例客户',
  grossProfitAmount: 400.5,
  grossProfitRate: 33.4,
  id: 'project-1',
  invoicedAmount: 600,
  managerName: '项目经理',
  name: '示例项目',
  procurementAmount: 300,
  projectCode: 'PRJ-001',
  receivedAmount: 400,
  schedulePerformanceIndex: 0.96,
  status: '进行中',
  yearEndReceivableForecast: 800,
};

test('amount formatting preserves null as hidden and does not display it as zero', () => {
  assert.equal(formatBusinessAmount(null), '—');
  assert.equal(formatBusinessAmount(1234.5), '1,234.50');
  assert.equal(formatBusinessRate(12.34), '12.3%');
  assert.equal(formatBusinessIndex(1.236), '1.24');
  assert.equal(formatBusinessDate('2026-10-14T00:00:00+08:00'), '2026-10-14');
  assert.equal(formatBusinessDate(null), '—');
});

test('project rows format server summaries and retain masked financial values', () => {
  const [row] = buildBusinessProjectRows([{ ...projectSummary, contractAmount: null, grossProfitAmount: null }]);

  assert.equal(row?.projectCode, 'PRJ-001');
  assert.equal(row?.contractAmount, '—');
  assert.equal(row?.grossProfitAmount, '—');
  assert.equal(row?.receivedAmount, '400.00');
});

test('overview cards use server aggregates and signals use the returned focus and warning records', () => {
  const overview = {
    canViewProfitAnalysis: false,
    canViewSensitiveAmounts: false,
    contractAmount: null,
    costAmount: null,
    focusProjects: [
      {
        actionTab: 'overview',
        description: '需关注经营收尾',
        projectId: 'project-1',
        projectName: '示例项目',
        type: '经营收尾',
      },
    ],
    grossProfitAmount: null,
    grossProfitRate: null,
    lowProfitProjectCount: 2,
    procurementAmount: null,
    projects: [projectSummary],
    receivableRiskProjectCount: 1,
    receivedAmount: null,
    tailClosingProjectCount: 3,
    warningItems: [
      {
        actionTab: 'receivables',
        description: '计划回款已逾期',
        id: 'warning-1',
        lastUpdatedTime: '2026-10-13T12:30:00',
        level: 'High',
        ownerName: '负责人',
        ownerUserId: 'user-1',
        projectCode: 'PRJ-001',
        projectId: 'project-1',
        projectName: '示例项目',
        status: '待处理',
        type: '应收风险',
      },
    ],
    yearEndReceivableForecast: null,
  };
  const stats = buildBusinessOverviewStats(overview);
  const model = buildBusinessOverviewModel(overview, false, '');

  assert.equal(stats.find((item) => item.key === 'contract')?.value, '—');
  assert.equal(stats.find((item) => item.key === 'low-profit')?.value, '2');
  assert.deepEqual(
    model.rows.map((row) => row.signalType),
    ['应收风险', '经营收尾'],
  );
  assert.equal(model.rows[1]?.projectCode, 'PRJ-001');
});

test('business detail rows and summary preserve nullable amount masking', () => {
  const detail = {
    acceptances: [],
    budgetExecutions: [
      {
        adjustedAmount: null,
        budgetCode: 'BUD-1',
        category: '研发',
        executedAmount: 0,
        id: 'budget-1',
        projectId: 'project-1',
        projectName: '示例项目',
        sortOrder: 0,
        varianceAmount: null,
      },
    ],
    businessChains: [],
    canViewProfitAnalysis: false,
    canViewSensitiveAmounts: false,
    contracts: [],
    forecastHistories: [],
    invoices: [],
    procurements: [],
    receivables: [],
    summary: { ...projectSummary, contractAmount: null, grossProfitAmount: null },
  };
  const summary = buildBusinessDetailSummaryRows(detail.summary);
  const rows = buildBusinessDetailRows(detail);

  assert.equal(summary.find((row) => row.label === '合同金额')?.value, '—');
  assert.equal(summary.find((row) => row.label === '毛利金额')?.value, '—');
  assert.match(rows[0]?.secondary ?? '', /调整后 — · 已执行 0\.00 · 差异 —/u);
});

test('report query sends only supported nonempty fields with whitespace trimmed', () => {
  assert.deepEqual(
    buildBusinessReportQuery({
      businessCloseStatus: '  收尾中 ',
      contractType: '',
      counterpartyName: ' 客户甲 ',
      keyword: ' 项目 ',
      plannedDateFrom: '2026-10-01',
      plannedDateTo: '',
      projectCode: ' P-1 ',
      receivableStatus: '',
      signDateFrom: '',
      signDateTo: '2026-10-31',
      supplierName: '  ',
      warningLevel: ' 高 ',
    }),
    {
      businessCloseStatus: '收尾中',
      counterpartyName: '客户甲',
      keyword: '项目',
      plannedDateFrom: '2026-10-01',
      projectCode: 'P-1',
      signDateTo: '2026-10-31',
      warningLevel: '高',
    },
  );
});

test('report rows render backend values for the selected ledger without deriving totals', () => {
  const report = {
    acceptanceItems: [],
    budgetItems: [],
    contractItems: [],
    invoiceItems: [],
    profitItems: [{ ...projectSummary, contractAmount: null, grossProfitAmount: null }],
    procurementItems: [],
    projectItems: [],
    receivableItems: [],
    warningItems: [],
  };
  const rows = buildBusinessReportRows(report, 'profit');

  assert.equal(rows[0]?.contractAmount, '—');
  assert.equal(rows[0]?.grossProfitAmount, '—');
  assert.equal(rows[0]?.grossProfitRate, '33.4%');
});
