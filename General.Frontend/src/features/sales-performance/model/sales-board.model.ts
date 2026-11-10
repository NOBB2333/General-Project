import type { PageModel } from '@/components/page';

export const salesBoardModel = {
  key: '销售绩效|销售看板',
  module: '销售绩效',
  moduleSlug: 'sales-performance',
  label: '销售看板',
  path: '/sales-perf/sales-board',
  routeName: 'page.sales-performance.sales-board',
  pageSlug: 'sales-board',
  demoPath: '/sales/insights',
  surface: 'board',
  description: '多角色 · 多维度 · 一站式销售数据洞察',
  searchPlaceholder: '搜索销售看板编号/名称/负责人...',
  tabs: ['综合概览', '客户洞察', '商机管道', '合同回款', '团队排行', '销售个人看板'],
  tools: ['我的', '我的团队', '全公司', '本月', '本季度', '本年度'],
  filters: [],
  columns: [],
  stats: [],
  panels: [
    {
      key: 'trend',
      title: '业绩 / 合同 / 回款 趋势',
      items: ['过去 6 个月', '确认业绩', '合同总额', '实际回款'],
    },
    {
      key: 'annual-target',
      title: '年度目标完成率',
      items: ['2026 年度', '完成率 7.3%', '已完成 ¥1,742,161.60', '年度目标 ¥24,000,000.00'],
    },
    {
      key: 'ranking',
      title: '本期 Top 5 业绩榜',
      items: ['张经理', '李娜', '王强', '赵杰', '周采购'],
    },
  ],
  rows: [],
  primaryAction: '全公司',
  moreTools: [],
  pageSize: 9,
  emptyHint: '当前视角下暂无销售洞察数据',
  rowActions: ['view'],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/sales/insights',
    referenceScreenshot: '.codex-artifacts/replication/sales-performance__sales-board/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/sales-performance__sales-board/local-default.png',
    evidenceDocument: 'outputs/page-evidence/sales-performance__sales-board.md',
    capturedAt: '2026-07-15',
  },
} as const satisfies PageModel;

export type SalesBoardModel = typeof salesBoardModel;
