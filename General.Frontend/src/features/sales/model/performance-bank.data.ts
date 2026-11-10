import { defineReferenceFixture } from '@/components/page';
import type { PerformanceRow } from './performance-bank.types';

const rows: readonly PerformanceRow[] = [
  {
    id: 'PER-2026-0001',
    direction: '原始确认',
    customer: '国电南瑞 · 南瑞二期扩容合同',
    amount: 638860,
    rate: 100,
    date: '2026-02-25',
    status: '已完成',
  },
  {
    id: 'PER-2026-0002',
    direction: '原始确认',
    customer: '华能电力科技 · DCS 改造框架协议',
    amount: 443352,
    rate: 100,
    date: '2026-01-20',
    status: '已完成',
  },
  {
    id: 'PER-2026-0003',
    direction: '业绩分出',
    customer: '华能电力科技 · DCS 改造框架协议',
    amount: -133005.6,
    rate: 30,
    date: '2026-01-26',
    status: '已完成',
  },
  {
    id: 'PER-2026-0004',
    direction: '原始确认',
    customer: '国电南瑞 · 南瑞二期扩容合同',
    amount: 638860,
    rate: 100,
    date: '2026-02-25',
    status: '待审批',
  },
  {
    id: 'PER-2026-0005',
    direction: '业绩分入',
    customer: '上海智远工业 · 自动化改造',
    amount: 60100,
    rate: 50,
    date: '2026-03-11',
    status: '进行中',
  },
  {
    id: 'PER-2026-0006',
    direction: '原始确认',
    customer: '宝钢设备部 · 产线升级项目',
    amount: 93995,
    rate: 100,
    date: '2026-03-08',
    status: '待审批',
  },
];

export const performanceBankReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/sales/performance-bank',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  defaults: { tab: 'mine', period: '本年' },
  rows,
});
