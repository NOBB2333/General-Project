import type { PageModel } from '@/components/page';

export const outsourceTransitModel = {
  key: '库存管理|委外在途',
  module: '库存管理',
  moduleSlug: 'inventory',
  label: '委外在途',
  path: '/inventory/outsource-transit',
  routeName: 'page.inventory.outsource-transit',
  pageSlug: 'outsource-transit',
  demoPath: '/inventory/subcontract-stock',
  surface: 'list',
  description: '按工单、采购订单和物料维度查询发到外协厂的在途库存',
  searchPlaceholder: '搜索工单号 / 采购订单 / 物料 / 规格 / 供应商',
  tabs: ['工单 / 物料明细', '按供应商汇总'],
  tools: ['去发料', '导出'],
  filters: [
    { key: 'age', label: '账龄', options: ['全部', '0-7 天', '8-30 天', '31-60 天', '>60 天'] },
    { key: 'supplier', label: '供应商', options: ['全部', '苏州精机外协加工厂', '东莞顺达注塑科技'] },
  ],
  columns: [
    { key: 'workOrder', title: '工单号', width: 180 },
    { key: 'purchaseOrder', title: '采购订单', width: 160 },
    { key: 'supplier', title: '供应商', width: 180 },
    { key: 'productCode', title: '产品编号', width: 150 },
    { key: 'productName', title: '品名', width: 150 },
    { key: 'specification', title: '规格', width: 140 },
    { key: 'flow', title: '已发 / 已耗 / 已退', width: 150 },
    { key: 'balance', title: '在外余量', width: 120 },
    { key: 'amount', title: '在外金额', width: 110 },
    { key: 'unit', title: '单位', width: 70 },
    { key: 'action', title: '操作', width: 80 },
  ],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '去发料',
  replication: { status: 'implemented', referenceUrl: 'https://demo.risemap.cn/inventory/subcontract-stock' },
} as const satisfies PageModel;

export type OutsourceTransitModel = typeof outsourceTransitModel;
