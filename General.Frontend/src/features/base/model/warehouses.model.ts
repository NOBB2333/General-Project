import type { ListPageScope, ListPageTab, PageModel } from '@/components/page';

export const warehousesModel = {
  key: '基础资料|仓库管理',
  module: '基础资料',
  moduleSlug: 'base',
  label: '仓库管理',
  path: '/base/warehouses',
  routeName: 'page.base.warehouses',
  pageSlug: 'warehouses',
  demoPath: '/base/warehouses',
  surface: 'cards',
  description: '管理主仓 / 项目仓 / 备品仓，掌握库存、采购、销售全景',
  searchPlaceholder: '搜索仓库编号、名称或负责人...',
  tabs: ['全部', '主仓', '项目仓', '备品仓', '委外仓'],
  tools: ['新增仓库'],
  filters: [{ key: 'type', label: '仓库类型', options: ['全部', '主仓', '项目仓', '备品仓', '委外仓'] }],
  columns: [
    { key: 'id', title: '仓库编号', width: 110 },
    { key: 'name', title: '仓库名称', width: 240 },
    { key: 'type', title: '类型', width: 90 },
    { key: 'manager', title: '负责人', width: 100 },
    { key: 'totalSKU', title: 'SKU', width: 80 },
    { key: 'totalQty', title: '库存数量', width: 100 },
    { key: 'totalValue', title: '库存金额', width: 110 },
    { key: 'totalPurchase', title: '采购总额', width: 110 },
    { key: 'estimatedSales', title: '预估销售额', width: 120 },
    { key: 'status', title: '状态', width: 80 },
  ],
  stats: [
    { key: 'total', label: '总仓库数', value: '9', tone: 'primary' },
    { key: 'sku', label: '总库存 SKU', value: '133', tone: 'neutral' },
    { key: 'quantity', label: '总库存数量', value: '1,001', tone: 'success' },
    { key: 'purchase', label: '总采购金额', value: '¥220.5万', tone: 'primary' },
    { key: 'sales', label: '预估销售总额', value: '¥391.4万', tone: 'success' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新增仓库',
  moreTools: [],
  pageSize: 20,
  emptyHint: '暂无匹配的仓库',
  rowActions: ['view', 'edit'],
  actions: [{ id: 'create', label: '新增仓库', kind: 'create', tone: 'primary' }],
  listScopes: [{ id: 'all', label: '全部', count: 9 }],
  listViews: [
    { id: 'card', label: '卡片' },
    { id: 'table', label: '表格' },
  ],
  listTabs: [{ id: 'all', label: '全部' }],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/base/warehouses',
    referenceScreenshot: '.codex-artifacts/replication/base__warehouses/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/base__warehouses/local-default.png',
    evidenceDocument: 'outputs/page-evidence/base__warehouses.md',
    capturedAt: '2026-09-16',
  },
} satisfies PageModel & {
  listScopes: readonly ListPageScope[];
  listViews: readonly ListPageTab[];
  listTabs: readonly ListPageTab[];
};

export type WarehousesModel = typeof warehousesModel;
