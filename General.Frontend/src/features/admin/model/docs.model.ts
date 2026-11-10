import type { PageModel } from '@/components/page';

export const docsModel = {
  key: '行政管理|文档中心',
  module: '行政管理',
  moduleSlug: 'admin',
  label: '文档中心',
  path: '/admin/docs',
  routeName: 'page.admin.docs',
  pageSlug: 'docs',
  demoPath: '/ops/docs',
  surface: 'list',
  description: '公司文档归档、版本管控、权限分配与跨业务模块文件引用',
  searchPlaceholder: '搜索文件名、描述、上传者...',
  tabs: [],
  tools: ['新建文件夹', '上传文件'],
  filters: [
    {
      key: 'publisher',
      label: '发布人',
      options: ['全部发布人', '市场部', '质量部', '张工', '财务部', '项目经理', '法务部'],
    },
    {
      key: 'department',
      label: '部门',
      options: ['全部部门', '市场部', '技术部', '财务部', '质量部', '项目部', '法务部'],
    },
    { key: 'type', label: '类型', options: ['全部类型', 'DOCX', 'PDF', 'PPTX', 'XLSX'] },
  ],
  columns: [
    { key: 'name', title: '文件名', width: 430 },
    { key: 'size', title: '大小', width: 100 },
    { key: 'publisher', title: '上传者', width: 130 },
    { key: 'updatedAt', title: '更新时间', width: 130 },
    { key: 'version', title: '版本', width: 90 },
    { key: 'downloads', title: '下载', width: 90 },
  ],
  stats: [
    { key: 'total', label: '文档数量', value: '24份', tone: 'primary' },
    { key: 'space', label: '占用空间', value: '157.7 MB', tone: 'success' },
    { key: 'downloads', label: '下载次数', value: '670次', tone: 'warning' },
    { key: 'references', label: '引用次数', value: '31次', tone: 'primary' },
  ],
  panels: [],
  rows: [],
  primaryAction: '上传文件',
  emptyHint: '当前筛选下没有文档',
  rowActions: ['view', 'download'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/ops/docs',
    referenceScreenshot: '.codex-artifacts/replication/admin__docs/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/admin__docs/local-default.png',
    evidenceDocument: 'outputs/page-evidence/admin__docs.md',
    capturedAt: '2026-09-17',
  },
} satisfies PageModel;

export type DocsModel = typeof docsModel;
