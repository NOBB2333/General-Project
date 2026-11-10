import type { PageModel } from '@/components/page';

export const compensationModel = {
  key: '人事管理|薪酬福利',
  module: '人事管理',
  moduleSlug: 'hr',
  label: '薪酬福利',
  path: '/hr/compensation',
  routeName: 'page.hr.compensation',
  pageSlug: 'compensation',
  demoPath: '/hr/compensation',
  surface: 'board',
  description: '工资条、薪酬结构、社保公积金、年终奖与福利计划',
  searchPlaceholder: '',
  tabs: ['薪酬概览', '工资条', '福利明细'],
  tools: ['隐藏金额', '我的薪酬', '管理后台'],
  filters: [],
  columns: [],
  stats: [
    { key: 'net', label: '本月实发工资', value: '¥19,834.00', tone: 'primary' },
    { key: 'average', label: '月均实发(近12月)', value: '¥20,305.50', tone: 'success' },
    { key: 'social', label: '年度社保公积金', value: '¥6.0万', tone: 'primary' },
    { key: 'benefit', label: '福利年度价值', value: '¥8.2万', tone: 'warning' },
  ],
  panels: [],
  rows: [],
  primaryAction: '隐藏金额',
  moreTools: [],
  pageSize: 20,
  emptyHint: '暂无薪酬数据',
  rowActions: [],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/hr/compensation',
    referenceScreenshot: '.codex-artifacts/replication/hr__compensation/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/hr__compensation/local-default.png',
    evidenceDocument: 'outputs/page-evidence/hr__compensation.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type CompensationModel = typeof compensationModel;
