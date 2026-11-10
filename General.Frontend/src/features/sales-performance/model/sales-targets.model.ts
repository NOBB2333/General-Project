import type { PageModel } from '@/components/page';

export const salesTargetsModel = {
  key: '销售绩效|销售目标',
  module: '销售绩效',
  moduleSlug: 'sales-performance',
  label: '销售目标',
  path: '/sales-perf/sales-targets',
  routeName: 'page.sales-performance.sales-targets',
  pageSlug: 'sales-targets',
  demoPath: '/sales/target',
  surface: 'target-board',
  description: '设置和追踪销售目标，实时计算完成进度与动态调整建议',
  searchPlaceholder: '',
  tabs: ['总览', '目标拆解', '个人目标', '团队目标'],
  tools: ['新建目标'],
  filters: [],
  columns: [],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '新建目标',
  moreTools: [],
  pageSize: 20,
  emptyHint: '当前没有销售目标',
  rowActions: ['view'],
  actions: [{ id: 'create', label: '新建目标', kind: 'create', tone: 'primary' }],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/sales/target',
    referenceScreenshot: '.codex-artifacts/replication/sales-performance__sales-targets/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/sales-performance__sales-targets/local-default.png',
    evidenceDocument: 'outputs/page-evidence/sales-performance__sales-targets.md',
    capturedAt: '2026-07-21',
  },
} as const satisfies PageModel;

export type SalesTargetsModel = typeof salesTargetsModel;
