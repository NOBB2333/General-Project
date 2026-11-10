import type { PageModel } from '@/components/page';

export const paymentFlowModel = {
  key: '采购管理|付款流水',
  module: '采购管理',
  moduleSlug: 'purchase',
  label: '付款流水',
  path: '/purchase/payment-flow',
  routeName: 'page.purchase.payment-flow',
  pageSlug: 'payment-flow',
  demoPath: '/purchase/payment-flow',
  surface: 'list',
  description: '登记并展示所有付款流水，完成采购订单付款分配',
  searchPlaceholder: '搜索流水号、供应商、订单、备注...',
  tabs: ['全部(5)', '未完成分配(3)', '已分配(2)'],
  tools: ['导出', '申请付款'],
  filters: [{ key: 'supplier', label: '供应商', options: ['全部供应商'] }],
  columns: [
    { key: 'code', title: '流水号', width: 150 },
    { key: 'supplier', title: '供应商', width: 180 },
    { key: 'date', title: '付款日期', width: 110 },
    { key: 'method', title: '方式', width: 100 },
    { key: 'account', title: '付款账户', width: 180 },
    { key: 'amount', title: '付款金额', width: 120 },
    { key: 'allocation', title: '已分配 / 未分配', width: 170 },
    { key: 'status', title: '状态', width: 100 },
    { key: 'operator', title: '经办人', width: 90 },
    { key: 'action', title: '操作', width: 100 },
  ],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '申请付款',
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/purchase/payment-flow',
    referenceScreenshot: '.codex-artifacts/replication/purchase__payment-flow/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/purchase__payment-flow/local-default.png',
    evidenceDocument: 'outputs/page-evidence/purchase__payment-flow.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type PaymentFlowModel = typeof paymentFlowModel;
