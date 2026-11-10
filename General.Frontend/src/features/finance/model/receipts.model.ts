import type { PageModel } from '@/components/page';

export const receiptsModel = {
  key: '财务管理|收款管理',
  module: '财务管理',
  moduleSlug: 'finance',
  label: '收款管理',
  path: '/finance/receipts',
  routeName: 'page.finance.receipts',
  pageSlug: 'receipts',
  demoPath: '/finance/payment-collections',
  surface: 'list',
  description: '登记收款单据、银行流水匹配、客户预收款核销',
  searchPlaceholder: '搜索订单、客户...',
  tabs: ['收款单', '收款核销', '预收款管理', '自动流水匹配', '未匹配收款流水', '回款看板'],
  tools: ['导出'],
  filters: [],
  columns: [
    { key: 'risk', title: '风险', width: 90 },
    { key: 'orderCustomer', title: '订单 / 客户', width: 210 },
    { key: 'plannedReceived', title: '计划/已到', width: 150 },
    { key: 'overdueAmount', title: '逾期金额', width: 130 },
    { key: 'overdueUpcoming', title: '逾期 / 即将', width: 120 },
    { key: 'longestOverdue', title: '最长逾期', width: 110 },
    { key: 'nextDue', title: '下一期 · 倒计时', width: 180 },
    { key: 'action', title: '操作', width: 80 },
  ],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '导出',
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/finance/payment-collections',
    referenceScreenshot: '.codex-artifacts/replication/finance__receipts/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/finance__receipts/local-default.png',
    evidenceDocument: 'outputs/page-evidence/finance__receipts.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type ReceiptsModel = typeof receiptsModel;
