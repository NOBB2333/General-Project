import type { PageModel } from '@/components/page';

export const sealsModel = {
  key: '行政管理|用章管理',
  module: '行政管理',
  moduleSlug: 'admin',
  label: '用章管理',
  path: '/admin/seals',
  routeName: 'page.admin.seals',
  pageSlug: 'seals',
  demoPath: '/ops/seals',
  surface: 'list',
  description: '印章登记、申请审批、外带跟踪与风险监控，确保用印合规',
  searchPlaceholder: '搜索申请...',
  tabs: ['用章申请', '待盖章任务', '印章台账', '用章统计'],
  tools: ['新建用章申请'],
  filters: [
    { key: 'status', label: '状态', options: ['全部状态', '待审批', '审批中', '已通过', '已驳回', '已盖章', '已归还'] },
  ],
  columns: [
    { key: 'code', title: '申请编号', width: 150 },
    { key: 'company', title: '公司抬头', width: 130 },
    { key: 'applicant', title: '申请人', width: 130 },
    { key: 'sealType', title: '印章类型', width: 150 },
    { key: 'fileName', title: '文件名称', width: 260 },
    { key: 'purpose', title: '用途', width: 170 },
    { key: 'status', title: '状态', width: 100 },
    { key: 'appliedAt', title: '申请日期', width: 120 },
  ],
  stats: [
    { key: 'active', label: '在用印章', value: '7', tone: 'primary' },
    { key: 'pending', label: '待审批', value: '2', tone: 'warning' },
    { key: 'month', label: '本月用章', value: '8', tone: 'primary' },
    { key: 'outside', label: '外带记录', value: '2', tone: 'danger' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新建用章申请',
  emptyHint: '当前条件下没有用章申请',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/ops/seals',
    referenceScreenshot: '.codex-artifacts/replication/admin__seals/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/admin__seals/local-default.png',
    evidenceDocument: 'outputs/page-evidence/admin__seals.md',
    capturedAt: '2026-09-16',
  },
} satisfies PageModel;

export type SealsModel = typeof sealsModel;
