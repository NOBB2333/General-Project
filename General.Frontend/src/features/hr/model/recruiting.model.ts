import type { PageModel } from '@/components/page';

export const recruitingModel = {
  key: '人事管理|招聘管理',
  module: '人事管理',
  moduleSlug: 'hr',
  label: '招聘管理',
  path: '/hr/recruiting',
  routeName: 'page.hr.recruiting',
  pageSlug: 'recruiting',
  demoPath: '/hr/recruitment',
  surface: 'list',
  description: '岗位发布、候选人漏斗、面试评估、Offer 管理',
  searchPlaceholder: '搜索岗位...',
  tabs: [],
  tools: ['岗位说明', '招聘岗位', '招聘管理', '新增岗位'],
  filters: [],
  columns: [],
  stats: [
    { key: 'total', label: '岗位总数', value: '7', tone: 'primary' },
    { key: 'active', label: '使用中', value: '6', tone: 'success' },
    { key: 'headcount', label: '总编制', value: '19', tone: 'primary' },
    { key: 'vacancies', label: '缺编', value: '7', tone: 'danger' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新增岗位',
  moreTools: [],
  pageSize: 20,
  emptyHint: '当前筛选下没有岗位',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/hr/recruitment',
    referenceScreenshot: '.codex-artifacts/replication/hr__recruiting/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/hr__recruiting/local-default.png',
    evidenceDocument: 'outputs/page-evidence/hr__recruiting.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type RecruitingModel = typeof recruitingModel;
