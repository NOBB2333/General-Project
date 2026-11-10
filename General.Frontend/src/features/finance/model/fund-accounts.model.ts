import type { PageModel } from '@/components/page';

export const fundAccountsModel = {
  key: '财务管理|资金账户',
  module: '财务管理',
  moduleSlug: 'finance',
  label: '资金账户',
  path: '/finance/fund-accounts',
  routeName: 'page.finance.fund-accounts',
  pageSlug: 'fund-accounts',
  demoPath: '/finance/bank-accounts',
  surface: 'cards',
  description: '管理银行、微信、支付宝、现金等全部资金账户',
  searchPlaceholder: '搜索账户名称、账号、持有人...',
  tabs: ['全部类型', '银行账户', '微信', '支付宝', '现金账户'],
  tools: ['立即建账', '新增账户', '导出'],
  filters: [
    {
      key: 'status',
      label: '状态',
      options: ['全部状态', '草稿', '生效中', '已过期', '正常', '冻结'],
    },
    {
      key: 'time',
      label: '时间',
      options: ['全部时间', '本周', '本月', '本季', '本年'],
    },
  ],
  columns: [
    {
      key: 'name',
      title: '账户',
      width: 160,
    },
    {
      key: 'type',
      title: '类型',
      width: 100,
    },
    {
      key: 'category',
      title: '用途',
      width: 100,
    },
    {
      key: 'metric',
      title: '余额',
      width: 140,
    },
    {
      key: 'status',
      title: '状态',
      width: 90,
    },
  ],
  stats: [
    {
      key: 'total',
      label: '全部',
      value: '8',
      tone: 'primary',
    },
    {
      key: 'active',
      label: '进行中',
      value: '0',
      tone: 'warning',
      filterKey: 'status',
      filterValue: '执行',
    },
    {
      key: 'done',
      label: '已完成',
      value: '0',
      tone: 'success',
    },
  ],
  panels: [],
  rows: [],
  primaryAction: '新增账户',
  moreTools: ['导入', '打印', '刷新', '列设置', '批量启用', '批量停用'],
  pageSize: 10,
  emptyHint: '当前筛选下没有资金账户，可重置条件或新增账户',
  rowActions: ['view', 'edit', 'copy', 'delete'],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/finance/bank-accounts',
    referenceScreenshot: '.codex-artifacts/replication/finance__fund-accounts/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/finance__fund-accounts/local-default.png',
    evidenceDocument: 'outputs/page-evidence/finance__fund-accounts.md',
    capturedAt: '2026-07-15',
  },
} as const satisfies PageModel;

export type FundAccountsModel = typeof fundAccountsModel;
