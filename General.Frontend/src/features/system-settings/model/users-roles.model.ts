import type { PageModel } from '@/components/page';

export const usersRolesModel = {
  key: '系统设置|用户与角色',
  module: '系统设置',
  moduleSlug: 'system-settings',
  label: '用户与角色',
  path: '/system/users-roles',
  routeName: 'page.system-settings.users-roles',
  pageSlug: 'users-roles',
  demoPath: '/settings/accounts',
  surface: 'list',
  description: '账号管理 · 账号、角色和数据权限统一配置',
  searchPlaceholder: '搜索姓名、账号、邮箱...',
  tabs: ['账号列表', '角色权限', '数据权限'],
  tools: ['新建账号', '导出', '导入'],
  filters: [
    {
      key: 'role',
      label: '角色',
      options: ['全部角色', '管理员', '项目经理', '销售经理', '财务', '普通员工', '仓库管理员'],
    },
    { key: 'status', label: '状态', options: ['全部状态', '启用', '禁用', '待激活'] },
  ],
  columns: [
    { key: 'user', title: '用户', width: 220 },
    { key: 'account', title: '账号/邮箱', width: 270 },
    { key: 'department', title: '部门', width: 190 },
    { key: 'role', title: '角色', width: 160 },
    { key: 'status', title: '状态', width: 130 },
    { key: 'lastLogin', title: '最近登录', width: 180 },
    { key: 'action', title: '操作', width: 130 },
  ],
  stats: [
    { key: 'total', label: '总账号', value: '12', tone: 'primary' },
    { key: 'enabled', label: '已启用', value: '9', tone: 'success' },
    { key: 'pending', label: '待激活', value: '2', tone: 'warning' },
    { key: 'roles', label: '角色数', value: '6', tone: 'neutral' },
  ],
  panels: [],
  rows: [{ id: 'account-001', name: '张明远', status: '启用' }],
  primaryAction: '新建账号',
  moreTools: ['打印', '刷新', '列设置', '批量启用', '批量停用'],
  pageSize: 20,
  emptyHint: '没有找到匹配的账号',
  rowActions: ['view', 'edit', 'status'],
  actions: [
    { id: 'create-account', label: '新建账号', kind: 'create', tone: 'primary' },
    { id: 'export-accounts', label: '导出', kind: 'export' },
    { id: 'import-accounts', label: '导入', kind: 'import' },
  ],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/settings/accounts',
    referenceScreenshot: '.codex-artifacts/replication/system-settings__users-roles/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/system-settings__users-roles/local-default.png',
    evidenceDocument: 'outputs/page-evidence/system-settings__users-roles.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type UsersRolesModel = typeof usersRolesModel;
