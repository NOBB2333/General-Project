import type { PageModel } from '@/components/page';

export const paymentsModel = {
  key: '财务管理|付款管理',
  module: '财务管理',
  moduleSlug: 'finance',
  label: '付款管理',
  path: '/finance/payments',
  routeName: 'page.finance.payments',
  pageSlug: 'payments',
  demoPath: '/finance/payment-writeoffs',
  surface: 'list',
  description: '付款任务统筹、付款单核销、银行流水匹配、供应商预付款',
  searchPlaceholder: '搜索应付单号/供应商/PO...',
  tabs: ['全部(12)', '待付款(9)', '审批中(2)', '已驳回(1)', '已完成'],
  tools: ['费用中心 · 付款申请', '新建独立付款申请', '导出'],
  filters: [],
  columns: [
    { key: 'code', title: '付款任务编号', width: 210 },
    { key: 'source', title: '来源', width: 120 },
    { key: 'reference', title: '关联单据号', width: 180 },
    { key: 'payee', title: '收款方', width: 190 },
    { key: 'requestedAmount', title: '申请付款金额', width: 160 },
    { key: 'payableAmount', title: '应付款金额', width: 160 },
    { key: 'approvalStatus', title: '付款审批状态', width: 150 },
    { key: 'action', title: '操作', width: 120 },
  ],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '新建独立付款申请',
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/finance/payment-writeoffs',
    referenceScreenshot: '.codex-artifacts/replication/finance__payments/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/finance__payments/local-default.png',
    evidenceDocument: 'outputs/page-evidence/finance__payments.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type PaymentsModel = typeof paymentsModel;
