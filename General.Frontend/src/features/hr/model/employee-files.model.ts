import type { PageModel } from '@/components/page';

export const employeeFilesModel = {
  key: '人事管理|员工档案',
  module: '人事管理',
  moduleSlug: 'hr',
  label: '员工档案',
  path: '/hr/employee-files',
  routeName: 'page.hr.employee-files',
  pageSlug: 'employee-files',
  demoPath: '/hr/employees',
  surface: 'list',
  description: '员工花名册、组织架构、个人档案与人事变动记录',
  searchPlaceholder: '搜索姓名/工号/部门/岗位...',
  tabs: ['全部(12)', '在职(8)', '试用期(2)', '休假中(1)', '已离职(1)'],
  tools: ['导出', '批量导入', '新增员工'],
  filters: [],
  columns: [],
  stats: [
    { key: 'active', label: '在职员工', value: '8', tone: 'success' },
    { key: 'probation', label: '试用期', value: '2', tone: 'primary' },
    { key: 'leave', label: '休假中', value: '1', tone: 'warning' },
    { key: 'left', label: '已离职', value: '1', tone: 'neutral' },
    { key: 'contracts', label: '近期合同到期', value: '0', tone: 'danger' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新增员工',
  moreTools: [],
  pageSize: 20,
  emptyHint: '当前筛选下没有员工',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/hr/employees',
    referenceScreenshot: '.codex-artifacts/replication/hr__employee-files/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/hr__employee-files/local-default.png',
    evidenceDocument: 'outputs/page-evidence/hr__employee-files.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type EmployeeFilesModel = typeof employeeFilesModel;
