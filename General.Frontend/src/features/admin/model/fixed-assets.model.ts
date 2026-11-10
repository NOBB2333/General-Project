import type { PageModel } from '@/components/page';

export const fixedAssetsModel = {
  key: '行政管理|固定资产',
  module: '行政管理',
  moduleSlug: 'admin',
  label: '固定资产',
  path: '/admin/fixed-assets',
  routeName: 'page.admin.fixed-assets',
  pageSlug: 'fixed-assets',
  demoPath: '/ops/assets',
  surface: 'list',
  description: '资产台账、折旧核算、使用人变更、报废流程的统一管控',
  searchPlaceholder: '搜索资产编号、名称、品牌...',
  tabs: ['全部 14', '电子设备 8', '办公家具 2', '生产设备 1', '交通工具 1', '仪器仪表 2'],
  tools: ['批量导入', '导出', '新增资产'],
  filters: [
    { key: 'status', label: '状态', options: ['全部状态', '在用', '闲置', '维修中', '已报废', '待处置'] },
    {
      key: 'department',
      label: '部门',
      options: ['全部部门', '工程部', '销售部', '技术部', '行政部', '总经办', '市场部', 'IT部'],
    },
  ],
  columns: [
    { key: 'code', title: '资产编号', width: 130 },
    { key: 'name', title: '资产名称', width: 280 },
    { key: 'category', title: '分类', width: 120 },
    { key: 'originalValue', title: '原值(¥)', width: 130 },
    { key: 'netValue', title: '净值(¥)', width: 130 },
    { key: 'monthlyDepreciation', title: '月折旧', width: 100 },
    { key: 'user', title: '使用人', width: 120 },
    { key: 'status', title: '状态', width: 100 },
    { key: 'location', title: '存放地点', width: 180 },
  ],
  stats: [
    { key: 'total', label: '资产总数', value: '14件', tone: 'primary' },
    { key: 'original', label: '资产原值', value: '¥424,600.00', tone: 'success' },
    { key: 'net', label: '资产净值', value: '¥291,672.76', tone: 'primary' },
    { key: 'depreciation', label: '月折旧额', value: '¥4,823.35', tone: 'warning' },
    { key: 'rate', label: '综合折旧率', value: '31.3%', tone: 'danger' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新增资产',
  emptyHint: '当前筛选下没有固定资产',
  rowActions: ['view', 'edit'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/ops/assets',
    referenceScreenshot: '.codex-artifacts/replication/admin__fixed-assets/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/admin__fixed-assets/local-default.png',
    evidenceDocument: 'outputs/page-evidence/admin__fixed-assets.md',
    capturedAt: '2026-09-17',
  },
} satisfies PageModel;

export type FixedAssetsModel = typeof fixedAssetsModel;
