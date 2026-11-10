import type { PageModel } from '@/components/page';

export const loansModel = {
  key: '财务管理|借款贷款管理',
  module: '财务管理',
  moduleSlug: 'finance',
  label: '借款贷款管理',
  path: '/finance/loans',
  routeName: 'page.finance.loans',
  pageSlug: 'loans',
  demoPath: '/finance/loans',
  surface: 'list',
  description: '员工借款（公司→员工）与银行贷款（银行→公司）统一管理',
  searchPlaceholder: '搜索...',
  tabs: ['全部', '待审核(1)', '审核中(1)', '已打款(2)', '部分核销(2)', '已核销(3)', '已驳回(1)'],
  tools: ['员工借款 12', '银行贷款', '导出', '申请员工借款'],
  filters: [],
  columns: [],
  stats: [
    { key: 'amount', label: '借款总额', value: '¥11.50万', tone: 'primary' },
    { key: 'pending', label: '待审批', value: '2', tone: 'warning' },
    { key: 'balance', label: '未核销余额', value: '¥30,000', tone: 'danger' },
    { key: 'writtenOff', label: '已核销笔数', value: '3', tone: 'success' },
  ],
  panels: [],
  rows: [],
  primaryAction: '申请员工借款',
  moreTools: [],
  pageSize: 12,
  emptyHint: '当前筛选下没有借款记录',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/finance/loans',
    referenceScreenshot: '.codex-artifacts/replication/finance__loans/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/finance__loans/local-default.png',
    evidenceDocument: 'outputs/page-evidence/finance__loans.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type LoansModel = typeof loansModel;
