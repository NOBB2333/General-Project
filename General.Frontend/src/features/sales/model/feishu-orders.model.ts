import type { PageModel } from '@/components/page';

export const feishuOrdersModel = {
  key: '销售业务|对话开单(飞书)',
  module: '销售业务',
  moduleSlug: 'sales',
  label: '对话开单(飞书)',
  path: '/sales/feishu-orders',
  routeName: 'page.sales.feishu-orders',
  pageSlug: 'feishu-orders',
  demoPath: '/sales/conversational',
  surface: 'workbench',
  description: '客户在飞书 IM 里通过自然语言或上传PO，完成销售订单、回款登记、发货单全链路。',
  searchPlaceholder: '搜索',
  tabs: [],
  tools: ['重置演示', '本次会话产生的单据', '接入说明 / 飞书配置'],
  filters: [],
  columns: [],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '重置演示',
  pageSize: 0,
  emptyHint: '会话尚未产生单据',
  rowActions: [],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/sales/conversational',
    referenceScreenshot: '.codex-artifacts/replication/sales__feishu-orders/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/sales__feishu-orders/local-default.png',
    evidenceDocument: 'outputs/page-evidence/sales__feishu-orders.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type FeishuOrdersModel = typeof feishuOrdersModel;
