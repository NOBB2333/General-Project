import type { PageModel } from '@/components/page';

export const workReportsNewModel = {
  key: '行政管理|工作汇报-新',
  module: '行政管理',
  moduleSlug: 'admin',
  label: '工作汇报-新',
  path: '/admin/work-reports-new',
  routeName: 'page.admin.work-reports-new',
  pageSlug: 'work-reports-new',
  demoPath: '/reportx',
  surface: 'list',
  description: '按模板汇总组织工作汇报，并提供 AI 分析与规则配置',
  searchPlaceholder: '',
  tabs: ['汇报', '统计', '模板'],
  tools: ['写汇报', 'AI 分析', '配置规则'],
  filters: [],
  columns: [
    { key: 'person', title: '人员', width: 120 },
    { key: 'keyResults', title: '本周关键成果', width: 380 },
    { key: 'details', title: '工作事项明细', width: 380 },
    { key: 'nextPlan', title: '下周计划', width: 320 },
    { key: 'risks', title: '问题与风险', width: 320 },
    { key: 'score', title: 'AI 评分', width: 100 },
  ],
  stats: [
    { key: 'total', label: '累计汇报', value: '1', tone: 'primary' },
    { key: 'people', label: '参与人数', value: '1', tone: 'primary' },
    { key: 'score', label: 'AI 平均分', value: '87.5', tone: 'primary' },
    { key: 'expected', label: '应提交人数', value: '4', tone: 'success' },
  ],
  panels: [],
  rows: [],
  primaryAction: '写汇报',
  emptyHint: '当前模板暂无汇报数据',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/reportx',
    referenceScreenshot: '.codex-artifacts/replication/admin__work-reports-new/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/admin__work-reports-new/local-default.png',
    evidenceDocument: 'outputs/page-evidence/admin__work-reports-new.md',
    capturedAt: '2026-09-17',
  },
} satisfies PageModel;

export type WorkReportsNewModel = typeof workReportsNewModel;
