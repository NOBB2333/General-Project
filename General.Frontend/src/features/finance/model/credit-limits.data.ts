import { defineReferenceFixture } from '@/components/page';
import type { CreditLimitRow, CreditMetric, PendingCreditRequest } from './credit-limits.types';

export const creditMetrics = [
  { id: 'customers', label: '授信客户', value: '5', hint: '已冻结 1 家', tone: 'primary' },
  { id: 'limit', label: '总授信额度', value: '¥320万', hint: '已用 ¥91.8万', tone: 'neutral' },
  { id: 'usage', label: '整体使用率', value: '29%', hint: '可用额度 ¥228.2万', tone: 'warning' },
  { id: 'overdue', label: '超期金额', value: '¥324,000', hint: '涉及 3 家客户', tone: 'danger' },
] as const satisfies readonly CreditMetric[];

const creditRows: readonly CreditLimitRow[] = [
  {
    id: 'credit-huaneng',
    customer: '华能电力科技有限公司',
    customerType: '终端客户',
    limit: 500_000,
    used: 128_000,
    overdueAmount: 28_000,
    overdueCount: 1,
    riskStatus: '正常',
    freezeStatus: '正常',
    owner: '张明',
  },
  {
    id: 'credit-sinopec',
    customer: '中石化仪表部',
    customerType: '终端客户',
    limit: 800_000,
    used: 256_000,
    overdueAmount: 116_000,
    overdueCount: 1,
    riskStatus: '预警',
    freezeStatus: '正常',
    owner: '张明',
  },
  {
    id: 'credit-nari',
    customer: '国电南瑞集团',
    customerType: '集成商',
    limit: 1_000_000,
    used: 380_000,
    overdueAmount: 180_000,
    overdueCount: 2,
    riskStatus: '超限',
    freezeStatus: '已冻结',
    owner: '李强',
  },
  {
    id: 'credit-baosteel',
    customer: '宝钢工程技术',
    customerType: 'EPC',
    limit: 600_000,
    used: 85_600,
    overdueAmount: 0,
    overdueCount: 0,
    riskStatus: '正常',
    freezeStatus: '正常',
    owner: '王芳',
  },
  {
    id: 'credit-hongkong',
    customer: '上海宏控自动化',
    customerType: '集成商',
    limit: 300_000,
    used: 68_000,
    overdueAmount: 0,
    overdueCount: 0,
    riskStatus: '正常',
    freezeStatus: '正常',
    owner: '张明',
  },
];

export const pendingCreditRequests: readonly PendingCreditRequest[] = [
  {
    id: 'CREDIT-REQ-2026-001',
    customer: '上海智联工控有限公司',
    customerType: '集成商',
    requestedLimit: 450_000,
    currentLimit: 200_000,
    owner: '陈销售',
    requestedAt: '2026-03-18',
    reason: '年度框架合同扩容，申请上调授信额度',
    status: '待审批',
  },
];

export const creditLimitsReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/finance/credit-management',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  defaults: {
    activeView: 'customers',
    riskFilter: '全部',
    freezeFilter: '全部',
  },
  rows: creditRows,
});
