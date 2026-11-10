import type { PageModel } from '@/components/page';

export const transferLoanModel = {
  key: '库存管理|调拨与借出',
  module: '库存管理',
  moduleSlug: 'inventory',
  label: '调拨与借出',
  path: '/inventory/transfer-loan',
  routeName: 'page.inventory.transfer-loan',
  pageSlug: 'transfer-loan',
  demoPath: '/inventory/transfer',
  surface: 'list',
  description: '跨仓库调拨、借出归还与外协领用单据管理',
  searchPlaceholder: '搜索单号、仓库、单位...',
  tabs: ['全部', '待审核(1)', '已审核(1)', '调拨中(3)', '已完成(4)', '已取消(1)'],
  tools: ['导出', '新建单据'],
  filters: [],
  columns: [
    { key: 'code', title: '单号', width: 150 },
    { key: 'type', title: '业务类型', width: 130 },
    { key: 'sender', title: '发出方', width: 150 },
    { key: 'receiver', title: '接收方', width: 170 },
    { key: 'reason', title: '原因', width: 180 },
    { key: 'quantity', title: '商品/数量', width: 120 },
    { key: 'amount', title: '金额', width: 120 },
    { key: 'status', title: '状态', width: 110 },
    { key: 'relatedCode', title: '关联', width: 130 },
    { key: 'applicant', title: '日期/申请人', width: 150 },
  ],
  stats: [],
  panels: [],
  rows: [],
  primaryAction: '新建单据',
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/inventory/transfer',
    referenceScreenshot: '.codex-artifacts/replication/inventory__transfer-loan/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/inventory__transfer-loan/local-default.png',
    evidenceDocument: 'outputs/page-evidence/inventory__transfer-loan.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type TransferLoanModel = typeof transferLoanModel;
