import type { PageModel } from '@/components/page';

export const expenseCenterModel = {
  key: '财务管理|费用中心',
  module: '财务管理',
  moduleSlug: 'finance',
  label: '费用中心',
  path: '/finance/expense-center',
  routeName: 'page.finance.expense-center',
  pageSlug: 'expense-center',
  demoPath: '/finance/expense-center',
  surface: 'list',
  description: '管理员工报销申请与企业费用支出',
  searchPlaceholder: '搜索...',
  tabs: ['报销管理 11', '企业支出 9', '付款申请 6', '费用统计', '支出说明'],
  tools: ['导出', '费用报销', '企业支出登记'],
  filters: [],
  columns: [],
  stats: [
    { key: 'total', label: '费用总额', value: '¥25.79万', tone: 'primary' },
    { key: 'pending', label: '待处理审批', value: '4', tone: 'warning' },
    { key: 'reimbursed', label: '已报销金额', value: '¥2.11万', tone: 'success' },
    { key: 'enterprise', label: '企业支出', value: '¥22.96万', tone: 'neutral' },
    { key: 'period', label: '期间费用', value: '¥18.50万', tone: 'warning' },
  ],
  panels: [],
  rows: [],
  primaryAction: '费用报销',
  moreTools: [],
  pageSize: 20,
  emptyHint: '当前筛选下没有费用记录',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/finance/expense-center',
    referenceScreenshot: '.codex-artifacts/replication/finance__expense-center/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/finance__expense-center/local-default.png',
    evidenceDocument: 'outputs/page-evidence/finance__expense-center.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type ExpenseCenterModel = typeof expenseCenterModel;
