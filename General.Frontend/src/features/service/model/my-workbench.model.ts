import type { PageModel } from '@/components/page';

export const myWorkbenchModel = {
  key: '服务管理|我的工作台',
  module: '服务管理',
  moduleSlug: 'service',
  label: '我的工作台',
  path: '/service/my-workbench',
  routeName: 'page.service.my-workbench',
  pageSlug: 'my-workbench',
  demoPath: '/after-sales/my-workspace',
  surface: 'board',
  description: '工程师工作台：今日行程、急单、备件与费用',
  searchPlaceholder: '',
  tabs: ['今日', '我的工单', '业绩', '我的备件', '我的费用单'],
  tools: ['去「待我处理」', '派工日历'],
  filters: [],
  columns: [],
  stats: [
    {
      key: 'today',
      label: '今日待办',
      value: '0',
      tone: 'primary',
    },
    {
      key: 'week',
      label: '本周排程',
      value: '0',
      tone: 'primary',
      hint: '08-10—08-16',
    },
    {
      key: 'completion',
      label: '本月完成率',
      value: '0%',
      tone: 'success',
    },
    { key: 'response', label: '平均响应', value: '0 分', tone: 'primary' },
    { key: 'rating', label: '客户评分', value: '—', tone: 'warning' },
  ],
  panels: [],
  rows: [],
  primaryAction: '派工日历',
  pageSize: 20,
  emptyHint: '当前没有工作台数据',
  rowActions: ['view'],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/after-sales/my-workspace',
    referenceScreenshot: '.codex-artifacts/replication/service__my-workbench/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/service__my-workbench/local-default.png',
    evidenceDocument: 'outputs/page-evidence/service__my-workbench.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type MyWorkbenchModel = typeof myWorkbenchModel;
