import type { PageModel } from '@/components/page';

export const onboardingModel = {
  key: '人事管理|入职离职',
  module: '人事管理',
  moduleSlug: 'hr',
  label: '入职离职',
  path: '/hr/onboarding',
  routeName: 'page.hr.onboarding',
  pageSlug: 'onboarding',
  demoPath: '/hr/onboarding',
  surface: 'list',
  description: '员工入职流程、离职手续、试用期评估与人事变动跟踪',
  searchPlaceholder: '搜索...',
  tabs: ['全部 (7)', '入职 (4)', '离职 (3)'],
  tools: ['新增记录'],
  filters: [],
  columns: [],
  stats: [
    { key: 'onboarding', label: '入职记录', value: '4', tone: 'primary' },
    { key: 'offboarding', label: '离职记录', value: '3', tone: 'danger' },
    { key: 'month', label: '本月入职', value: '2', tone: 'success' },
    { key: 'pending', label: '待处理', value: '4', tone: 'warning' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新增记录',
  moreTools: [],
  pageSize: 20,
  emptyHint: '当前筛选下没有入职离职记录',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/hr/onboarding',
    referenceScreenshot: '.codex-artifacts/replication/hr__onboarding/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/hr__onboarding/local-default.png',
    evidenceDocument: 'outputs/page-evidence/hr__onboarding.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type OnboardingModel = typeof onboardingModel;
