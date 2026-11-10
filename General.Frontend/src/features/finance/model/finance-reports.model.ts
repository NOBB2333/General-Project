import type { PageModel } from '@/components/page';

export const financeReportsModel = {
  key: '财务管理|财务管理报告',
  module: '财务管理',
  moduleSlug: 'finance',
  label: '财务管理报告',
  path: '/finance/finance-reports',
  routeName: 'page.finance.finance-reports',
  pageSlug: 'finance-reports',
  demoPath: '/finance/report',
  surface: 'board',
  description: '全面了解公司经营状况 · 数据截至 2026年2月',
  searchPlaceholder: '',
  tabs: ['老板驾驶舱', '收入分析', '成本费用', '现金流', '项目盈利', '合同签约', '人力资源支出'],
  tools: ['定时发送', '导出PDF', '导出Excel'],
  primaryAction: '导出PDF',
  filters: [],
  columns: [],
  stats: [],
  panels: [],
  rows: [],
  moreTools: [],
  pageSize: 20,
  emptyHint: '暂无财务报告数据',
  rowActions: [],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/finance/report',
    referenceScreenshot: '.codex-artifacts/replication/finance__finance-reports/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/finance__finance-reports/local-default.png',
    evidenceDocument: 'outputs/page-evidence/finance__finance-reports.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type FinanceReportsModel = typeof financeReportsModel;
