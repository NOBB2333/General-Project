import { defineReferenceFixture } from '@/components/page';
import type { AnalysisPanel, FinanceReportMetric, HealthScore } from './finance-reports.types';

export const healthScores: readonly HealthScore[] = [
  { label: '经营健康分', value: 67, status: '良好', detail: '良好 · 关注个别指标', benchmark: '', tone: 'primary' },
  { label: '盈利能力', value: 72, status: '良好', detail: '净利率 15.2%', benchmark: '行业中位 12%', tone: 'success' },
  { label: '偿付能力', value: 68, status: '健康', detail: '可支撑 3.2 个月', benchmark: '安全线 ≥ 3', tone: 'success' },
  {
    label: '运营效率',
    value: 48,
    status: '偏弱',
    detail: '应收周转 65 天',
    benchmark: '行业良好 ≤ 45',
    tone: 'warning',
  },
  { label: '成长能力', value: 78, status: '积极', detail: '签约同比 +15.1%', benchmark: '目标 +10%', tone: 'success' },
];

export const reportMetrics: readonly FinanceReportMetric[] = [
  {
    id: 'contracted',
    label: '本月签约额',
    value: '¥428.00万',
    description: '新签销售合同金额',
    badge: '↗ +15.1%',
    tone: 'green',
    size: 'large',
  },
  {
    id: 'received',
    label: '本月已回款',
    value: '¥285.00万',
    description: '实际到账金额',
    badge: '↗ +8.4%',
    tone: 'orange',
    size: 'large',
  },
  {
    id: 'receivable',
    label: '待收款',
    value: '¥259.10万',
    description: '客户已欠未收金额',
    badge: '↘ 逾期占30%',
    tone: 'orange',
    size: 'large',
  },
  {
    id: 'cash',
    label: '可用资金',
    value: '¥285.00万',
    description: '银行账户可用余额',
    badge: '↗ 可支撑3.2个月',
    tone: 'green',
    size: 'large',
  },
  {
    id: 'advance-in',
    label: '客户预收',
    value: '¥156.00万',
    description: '已提前付款，待交付结转',
    tone: 'cyan',
    size: 'small',
  },
  {
    id: 'advance-out',
    label: '采购预付',
    value: '¥68.00万',
    description: '已付供应商，货/票未结清',
    tone: 'purple',
    size: 'small',
  },
  {
    id: 'payable',
    label: '待付款',
    value: '¥98.00万',
    description: '本月到期 ¥42.00万',
    badge: '需关注',
    tone: 'red',
    size: 'small',
  },
  {
    id: 'inventory',
    label: '库存占用',
    value: '¥234.00万',
    description: '周转45天 · 呆滞¥28.00万',
    tone: 'orange',
    size: 'small',
  },
  {
    id: 'overdue',
    label: '逾期应收',
    value: '¥78.64万',
    description: '4笔逾期，最长130天',
    badge: '风险',
    tone: 'red',
    size: 'small',
  },
  {
    id: 'collection-rate',
    label: '回款率',
    value: '22.3%',
    description: '目标30%，需关注',
    badge: '低于目标',
    tone: 'red',
    size: 'small',
  },
];

export const analysisPanels: readonly AnalysisPanel[] = [
  {
    view: '老板驾驶舱',
    title: '营收 · 毛利 · 净利（12 个月）',
    subtitle: '经营趋势',
    values: [28, 35, 31, 48, 44, 57, 54, 66, 63, 75, 72, 86],
  },
  {
    view: '收入分析',
    title: '收入构成与同比趋势',
    subtitle: '按客户与业务分类',
    values: [42, 55, 48, 72, 68, 84, 77, 92],
  },
  {
    view: '成本费用',
    title: '成本与费用结构',
    subtitle: '材料、人工、制造费用',
    values: [62, 48, 55, 39, 44, 33, 41, 28],
  },
  {
    view: '现金流',
    title: '现金余额 · 净现金流',
    subtitle: '经营现金流趋势',
    values: [38, 52, 45, 58, 71, 66, 80, 76],
  },
  { view: '项目盈利', title: '项目盈利能力', subtitle: '项目毛利与回款', values: [48, 63, 57, 74, 68, 82] },
  {
    view: '合同签约',
    title: '签约 vs 回款（8 个月）',
    subtitle: '销售合同转化',
    values: [35, 47, 52, 61, 72, 69, 84, 91],
  },
  {
    view: '人力资源支出',
    title: '人力成本趋势',
    subtitle: '薪酬、社保与福利',
    values: [54, 57, 59, 63, 66, 68, 71, 73],
  },
];

export const financeReportsFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/finance/report',
  capturedAt: '2026-08-16',
  rows: reportMetrics,
  defaults: { view: '老板驾驶舱', period: '本月' },
});
