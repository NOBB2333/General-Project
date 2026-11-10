import type { PageModel } from '@/components/page';

export const hrWorkbenchModel = {
  key: '人事管理|人事工作台',
  module: '人事管理',
  moduleSlug: 'hr',
  label: '人事工作台',
  path: '/hr/hr-workbench',
  routeName: 'page.hr.hr-workbench',
  pageSlug: 'hr-workbench',
  demoPath: '/hr/workbench',
  surface: 'board',
  description: '人力资源工作台 · 数据截止 2026-03-15',
  searchPlaceholder: '',
  tabs: [],
  tools: ['员工档案', '招聘岗位'],
  primaryAction: '员工档案',
  filters: [],
  columns: [],
  stats: [
    { key: 'active', label: '在职人数', value: '38', tone: 'primary' },
    { key: 'probation', label: '试用期', value: '3', tone: 'warning' },
    { key: 'joined', label: '本月入职', value: '1', tone: 'success' },
    { key: 'left', label: '本月离职', value: '1', tone: 'danger' },
    { key: 'recruiting', label: '招聘中岗位', value: '4', tone: 'primary' },
    { key: 'salary', label: '月薪资总额', value: '¥78.3万', tone: 'success' },
  ],
  panels: [],
  rows: [],
  moreTools: [],
  pageSize: 20,
  emptyHint: '暂无人事数据',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/hr/workbench',
    referenceScreenshot: '.codex-artifacts/replication/hr__hr-workbench/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/hr__hr-workbench/local-default.png',
    evidenceDocument: 'outputs/page-evidence/hr__hr-workbench.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type HrWorkbenchModel = typeof hrWorkbenchModel;
