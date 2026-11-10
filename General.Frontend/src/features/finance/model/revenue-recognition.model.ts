import type { PageModel } from '@/components/page';

export const revenueRecognitionModel = {
  key: '财务管理|销售收入确认',
  module: '财务管理',
  moduleSlug: 'finance',
  label: '销售收入确认',
  path: '/finance/revenue-recognition',
  routeName: 'page.finance.revenue-recognition',
  pageSlug: 'revenue-recognition',
  demoPath: '/finance/revenue-recognition',
  surface: 'list',
  description: '按发货、里程碑、验收、周期确认销售收入，跟踪开票与回款',
  searchPlaceholder: '搜索确认单号/合同/客户...',
  tabs: ['全部', '草稿(1)', '待审核(3)', '已审核(8)', '已驳回(1)', '已作废(1)'],
  tools: ['导出', '新建确认单'],
  filters: [],
  columns: [],
  stats: [
    { key: 'month', label: '本月确认收入', value: '¥0.0万', tone: 'primary' },
    { key: 'year', label: '本年累计确认', value: '¥145.0万', tone: 'success' },
    { key: 'pending', label: '待审批金额', value: '¥48.3万', tone: 'warning' },
    { key: 'uncollected', label: '已确认未收款', value: '¥40.8万', tone: 'neutral' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新建确认单',
  moreTools: [],
  pageSize: 20,
  emptyHint: '当前筛选下没有收入确认单',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/finance/revenue-recognition',
    referenceScreenshot: '.codex-artifacts/replication/finance__revenue-recognition/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/finance__revenue-recognition/local-default.png',
    evidenceDocument: 'outputs/page-evidence/finance__revenue-recognition.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type RevenueRecognitionModel = typeof revenueRecognitionModel;
