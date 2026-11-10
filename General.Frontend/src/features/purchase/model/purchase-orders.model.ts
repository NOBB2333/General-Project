import type { PageModel } from '@/components/page';

export const purchaseOrdersModel = {
  key: '采购管理|采购订单',
  module: '采购管理',
  moduleSlug: 'purchase',
  label: '采购订单',
  path: '/purchase/orders',
  routeName: 'page.purchase.purchase-orders',
  pageSlug: 'purchase-orders',
  demoPath: '/purchase/orders',
  surface: 'list',
  description: '正式采购订单下达、跟踪到货状态、应付与发票管理',
  searchPlaceholder: '搜索订单号、供应商...',
  tabs: ['全部', '草稿(1)', '待审核(1)', '已审核(2)', '部分到货(3)', '已完成(2)', '已取消(1)'],
  tools: ['进项发票', '导出', '新建采购单'],
  filters: [
    { key: 'scope', label: '管辖范围', options: ['所有', '我负责的', '下属负责的', '我关注的'] },
    { key: 'supplier', label: '供应商', options: ['全部供应商'] },
  ],
  columns: [
    { key: 'code', title: '订单号', width: 140 },
    { key: 'supplier', title: '供应商', width: 170 },
    { key: 'project', title: '关联项目', width: 150 },
    { key: 'payableRule', title: '应付款确认方式', width: 150 },
    { key: 'buyer', title: '采购员', width: 100 },
    { key: 'amount', title: '金额', width: 110 },
    { key: 'invoiceProgress', title: '开票', width: 100 },
    { key: 'stockProgress', title: '入库', width: 100 },
    { key: 'paymentProgress', title: '付款', width: 100 },
    { key: 'status', title: '状态', width: 100 },
    { key: 'date', title: '日期', width: 100 },
  ],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '新建采购单',
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/purchase/orders',
    referenceScreenshot: '.codex-artifacts/replication/purchase__purchase-orders/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/purchase__purchase-orders/local-default.png',
    evidenceDocument: 'outputs/page-evidence/purchase__purchase-orders.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type PurchaseOrdersModel = typeof purchaseOrdersModel;
