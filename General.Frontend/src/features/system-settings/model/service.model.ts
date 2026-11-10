import type { PageModel } from '@/components/page';

export const serviceModel = {
  key: '系统设置|服务管理',
  module: '系统设置',
  moduleSlug: 'system-settings',
  label: '服务管理',
  path: '/system/service',
  routeName: 'page.system-settings.service',
  pageSlug: 'service',
  demoPath: '/settings/departments',
  surface: 'board',
  description: '维护企业部门层级、负责人和人数',
  searchPlaceholder: '搜索部门名称、编码、负责人...',
  tabs: [],
  tools: ['新建部门', '全部展开', '全部折叠'],
  filters: [],
  columns: [
    { key: 'name', title: '部门名称', width: 760 },
    { key: 'manager', title: '负责人', width: 130 },
    { key: 'headcount', title: '人数', width: 90 },
    { key: 'status', title: '状态', width: 90 },
  ],
  stats: [
    { key: 'root', label: '一级部门', value: '11', tone: 'primary' },
    { key: 'total', label: '全部部门', value: '26', tone: 'neutral' },
    { key: 'headcount', label: '总人数', value: '213', tone: 'success' },
    { key: 'forming', label: '筹建中', value: '1', tone: 'warning' },
  ],
  panels: [],
  rows: [
    { id: 'department-executive', name: '总经办', status: '正常' },
    { id: 'department-rd', name: '技术研发部', status: '正常' },
    { id: 'department-engineering', name: '工程部', status: '正常' },
    { id: 'department-sales', name: '销售部', status: '正常' },
  ],
  primaryAction: '新建部门',
  pageSize: 50,
  emptyHint: '没有找到匹配的部门',
  actions: [
    { id: 'create-department', label: '新建部门', kind: 'create', tone: 'primary' },
    { id: 'expand-all', label: '全部展开', kind: 'custom' },
    { id: 'collapse-all', label: '全部折叠', kind: 'custom' },
  ],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/settings/departments',
    referenceScreenshot: '.codex-artifacts/replication/system-settings__service/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/system-settings__service/local-default.png',
    evidenceDocument: 'outputs/page-evidence/system-settings__service.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type ServiceModel = typeof serviceModel;
