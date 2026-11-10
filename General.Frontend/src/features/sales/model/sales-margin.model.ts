import type { PageModel } from '@/components/page';

export const salesMarginModel = {
  key: '销售业务|销售毛利',
  module: '销售业务',
  moduleSlug: 'sales',
  label: '销售毛利',
  path: '/sales/sales-margin',
  routeName: 'page.sales.sales-margin',
  pageSlug: 'sales-margin',
  demoPath: '/sales/gross-profit',
  surface: 'dashboard',
  description: '收入/成本/毛利率多维度对比，洞察盈利能力与趋势',
  searchPlaceholder: '',
  tabs: ['按商品分析', '按销售订单分析', '按客户分析'],
  tools: ['本月', '本季', '半年', '全年', '导出'],
  filters: [{ key: 'scope', label: '管辖范围', options: ['所有', '我负责的', '下属负责的', '我关注的'] }],
  columns: [
    { key: 'name', title: '商品名称', width: 220 },
    { key: 'category', title: '品类', width: 110 },
    { key: 'qty', title: '销量', width: 80 },
    { key: 'revenue', title: '销售收入', width: 120 },
    { key: 'cost', title: '销售成本', width: 120 },
    { key: 'grossProfit', title: '毛利额', width: 120 },
    { key: 'margin', title: '毛利率', width: 100 },
  ],
  stats: [
    { key: 'revenue', label: '销售收入', value: '¥2,661,600', tone: 'primary' },
    { key: 'cost', label: '销售成本', value: '¥1,853,732', tone: 'danger' },
    { key: 'profit', label: '销售毛利', value: '¥807,868', tone: 'success' },
    { key: 'margin', label: '综合毛利率', value: '30.4%', tone: 'success' },
  ],
  panels: [],
  rows: [],
  primaryAction: '导出',
  pageSize: 20,
  emptyHint: '当前范围内暂无毛利数据',
  rowActions: ['view'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/sales/gross-profit',
    referenceScreenshot: '.codex-artifacts/replication/sales__sales-margin/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/sales__sales-margin/local-default.png',
    evidenceDocument: 'outputs/page-evidence/sales__sales-margin.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type SalesMarginModel = typeof salesMarginModel;
