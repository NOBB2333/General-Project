import { FileText, Landmark, TrendingUp, WalletCards } from 'lucide-vue-next';
import { defineReferenceFixture } from '@/components/page';
import type {
  MyPerformanceBankEntry,
  MyPerformanceLedgerEntry,
  MyPerformanceMetric,
  MyPerformancePlan,
  MyPerformanceReferenceRow,
  MyPerformanceReward,
  MyPerformanceSummary,
  MyPerformanceTabDefinition,
} from './my-performance.types';

export const myPerformanceTabs: readonly MyPerformanceTabDefinition[] = [
  { id: 'overview', label: '本期动态', badge: '实时', icon: TrendingUp },
  { id: 'bank', label: '我的业绩银行', icon: Landmark },
  { id: 'ledger', label: '奖金台账', icon: WalletCards },
  { id: 'plans', label: '我适用的方案', icon: FileText },
];

export const myPerformanceMetrics: readonly MyPerformanceMetric[] = [
  {
    id: 'estimated-bonus',
    kind: 'metric',
    label: '本期预计奖金',
    value: '¥122,600',
    hint: '主 ¥117,200 + 奖励 ¥20,400 - 扣减 ¥15,000',
    tone: 'primary',
    detail: ['主奖金：¥117,200', '额外奖励：¥20,400', '本期扣减：¥15,000'],
  },
  {
    id: 'ytd-bonus',
    kind: 'metric',
    label: 'YTD 累计',
    value: '¥2,212,600',
    hint: '2 期',
    tone: 'neutral',
    detail: ['累计周期：2 期', '本年累计奖金：¥2,212,600'],
  },
  {
    id: 'paid-bonus',
    kind: 'metric',
    label: '已发放',
    value: '¥0',
    hint: '0 期',
    tone: 'success',
    detail: ['已发放周期：0 期', '已发放奖金：¥0'],
  },
  {
    id: 'pending-bonus',
    kind: 'metric',
    label: '待发放',
    value: '¥2,212,600',
    hint: '2 期',
    tone: 'warning',
    detail: ['待发放周期：2 期', '待发放奖金：¥2,212,600'],
  },
];

export const myPerformanceRewards: readonly MyPerformanceReward[] = [
  {
    id: 'first-order',
    kind: 'reward',
    label: '新客户首单奖',
    amount: '+¥4,000',
    formula: '2000/单 × 2 单 = 4000',
  },
  {
    id: 'large-order',
    kind: 'reward',
    label: '大单冲击奖',
    amount: '+¥8,200',
    formula: '额外 1% × 82万（命中 SO-2026-0102） = 8200',
  },
  {
    id: 'strategic-product',
    kind: 'reward',
    label: '战略产品专项奖',
    amount: '+¥5,200',
    formula: '额外 1.5% × 命中订单合计 → 5200',
  },
  {
    id: 'fast-collection',
    kind: 'reward',
    label: '快速回款奖',
    amount: '+¥3,000',
    formula: '额外 0.5% × 60万（命中 ≤30天订单） = 3000',
  },
];

export const myPerformanceSummaries: readonly MyPerformanceSummary[] = [
  { id: 'orders', kind: 'summary', label: '本期订单数', value: '5 笔', tone: 'neutral' },
  { id: 'contracts', kind: 'summary', label: '合同金额', value: '¥2,496,200', tone: 'neutral' },
  { id: 'collections', kind: 'summary', label: '回款金额', value: '¥2,240,000', tone: 'success' },
  { id: 'gross-profit', kind: 'summary', label: '毛利金额', value: '¥975,000', tone: 'warning' },
];

export const myPerformanceBankEntries: readonly MyPerformanceBankEntry[] = [
  {
    id: 'bank-2026-04',
    kind: 'bank',
    period: '2026-04',
    performance: '¥1,296,000',
    credited: '¥1,296,000',
    status: '已入账',
  },
  {
    id: 'bank-2026-03',
    kind: 'bank',
    period: '2026-03',
    performance: '¥916,600',
    credited: '¥916,600',
    status: '已入账',
  },
];

export const myPerformanceLedgerEntries: readonly MyPerformanceLedgerEntry[] = [
  {
    id: 'ledger-2026-04',
    kind: 'ledger',
    period: '2026-04',
    category: '本期预计奖金',
    amount: '¥122,600',
    state: '待发放',
  },
  {
    id: 'ledger-2026-03',
    kind: 'ledger',
    period: '2026-03',
    category: '月度绩效奖金',
    amount: '¥2,090,000',
    state: '待发放',
  },
];

export const myPerformancePlans: readonly MyPerformancePlan[] = [
  {
    id: 'plan-sales-2026',
    kind: 'plan',
    name: '2026 销售绩效与提成方案',
    period: '2026-01-01 至 2026-12-31',
    description: '按确认业绩、目标完成率及专项奖励综合核算。',
    state: '生效中',
  },
  {
    id: 'plan-sprint-q2',
    kind: 'plan',
    name: '2026 Q2 销售冲刺奖励方案',
    period: '2026-04-01 至 2026-06-30',
    description: '完成率达到 120% 后解锁冲刺奖金及额外提成。',
    state: '生效中',
  },
];

const referenceRows: readonly MyPerformanceReferenceRow[] = [
  ...myPerformanceMetrics,
  ...myPerformanceRewards,
  ...myPerformanceSummaries,
  ...myPerformanceBankEntries,
  ...myPerformanceLedgerEntries,
  ...myPerformancePlans,
];

export const myPerformanceReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/sales/commission/my',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  rows: referenceRows,
  defaults: {
    activeTab: 'overview',
    owner: '张经理',
    targetAmount: '¥1,200,000',
    actualAmount: '¥1,296,000',
    completionRate: 108,
  },
});
