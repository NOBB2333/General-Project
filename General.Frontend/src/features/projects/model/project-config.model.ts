import type { PageModel } from '@/components/page';

export const projectConfigModel = {
  key: '项目管理|项目配置中心',
  module: '项目管理',
  moduleSlug: 'projects',
  label: '项目配置中心',
  path: '/projects/project-config',
  routeName: 'page.projects.project-config',
  pageSlug: 'project-config',
  demoPath: '/projects/project-config',
  surface: 'board',
  description: '维护项目计划模板，并复用平台工时费率配置。',
  searchPlaceholder: '搜索模板编码、名称或说明',
  tabs: [],
  tools: ['新建模板', '刷新'],
  filters: [],
  columns: [
    { key: 'code', title: '模板编码', width: 180 },
    { key: 'name', title: '模板名称', width: 240 },
    { key: 'value', title: 'WBS条目', width: 120 },
    { key: 'status', title: '状态', width: 100 },
    { key: 'updatedAt', title: '更新时间', width: 140 },
  ],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '新建模板',
  replication: { status: 'implemented', referenceUrl: '/projects/project-config' },
  pageSize: 10,
  emptyHint: '暂无项目计划模板。',
  rowActions: ['view', 'edit', 'delete'],
} as const satisfies PageModel;

export type ProjectConfigModel = typeof projectConfigModel;
