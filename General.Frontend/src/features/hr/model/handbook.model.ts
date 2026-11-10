import type { PageModel } from '@/components/page';

export const handbookModel = {
  key: '人事管理|员工手册',
  module: '人事管理',
  moduleSlug: 'hr',
  label: '员工手册',
  path: '/hr/handbook',
  routeName: 'page.hr.handbook',
  pageSlug: 'handbook',
  demoPath: '/hr/handbook',
  surface: 'board',
  description: '7 章 · 18 条 · 当前版本 V3.2 · 全员已读率 74%',
  searchPlaceholder: '搜索条款...',
  tabs: ['总览'],
  tools: ['版本记录', '阅读看板', '打印', '导出PDF', '编辑手册'],
  filters: [],
  columns: [],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '编辑手册',
  moreTools: [],
  pageSize: 18,
  emptyHint: '没有匹配的手册条款',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/hr/handbook',
    referenceScreenshot: '.codex-artifacts/replication/hr__handbook/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/hr__handbook/local-default.png',
    evidenceDocument: 'outputs/page-evidence/hr__handbook.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type HandbookModel = typeof handbookModel;
