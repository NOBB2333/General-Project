import type { PageModel } from '@/components/page';

export const myPerformanceModel = {
  key: '销售绩效|我的绩效',
  module: '销售绩效',
  moduleSlug: 'sales-performance',
  label: '我的绩效',
  path: '/sales-perf/my-performance',
  routeName: 'page.sales-performance.my-performance',
  pageSlug: 'my-performance',
  demoPath: '/sales/commission/my',
  surface: 'dashboard',
  description: '销售视角 张经理 · 实时查看你的业绩、奖金构成、冲刺进度与适用方案',
  searchPlaceholder: '',
  tabs: ['本期动态', '我的业绩银行', '奖金台账', '我适用的方案'],
  tools: ['本期动态', '我的业绩银行', '奖金台账', '我适用的方案'],
  filters: [],
  columns: [],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '',
  moreTools: [],
  pageSize: 20,
  emptyHint: '当前没有绩效数据',
  rowActions: ['view'],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/sales/commission/my',
    referenceScreenshot: '.codex-artifacts/replication/sales-performance__my-performance/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/sales-performance__my-performance/local-default.png',
    evidenceDocument: 'outputs/page-evidence/sales-performance__my-performance.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type MyPerformanceModel = typeof myPerformanceModel;
