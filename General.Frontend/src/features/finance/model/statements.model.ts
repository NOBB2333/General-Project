import type { PageModel } from '@/components/page';

export const statementsModel = {
  key: '财务管理|对账单',
  module: '财务管理',
  moduleSlug: 'finance',
  label: '对账单',
  path: '/finance/statements',
  routeName: 'page.finance.statements',
  pageSlug: 'statements',
  demoPath: '/finance/reconciliations',
  surface: 'board',
  description: '管理客户和供应商对账单，支持多维度对账与差异处理',
  searchPlaceholder: '',
  tabs: [],
  tools: [],
  primaryAction: '查看对账单',
  filters: [],
  columns: [],
  stats: [
    { key: 'pending', label: '待处理', value: '4', tone: 'warning' },
    { key: 'confirmed', label: '已确认', value: '2', tone: 'success' },
    { key: 'difference', label: '有差异', value: '2', tone: 'danger' },
  ],
  panels: [],
  rows: [],
  moreTools: [],
  pageSize: 20,
  emptyHint: '暂无对账单',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/finance/reconciliations',
    referenceScreenshot: '.codex-artifacts/replication/finance__statements/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/finance__statements/local-default.png',
    evidenceDocument: 'outputs/page-evidence/finance__statements.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type StatementsModel = typeof statementsModel;
