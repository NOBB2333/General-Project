import { defineReferenceFixture } from '@/components/page';
import type { OvertimeRecord } from './overtime.types';

const rows: readonly OvertimeRecord[] = [
  {
    id: 'overtime-001',
    code: 'OT-2026-001',
    date: '2026-03-08',
    period: '18:00 - 22:00',
    hours: 4,
    reason: '紧急项目上线准备',
    projectCount: 1,
    compensation: '调休',
    compensationLabel: '调休（0.5天）',
    status: '已批准',
  },
  {
    id: 'overtime-002',
    code: 'OT-2026-002',
    date: '2026-03-15',
    period: '09:00 - 18:00',
    hours: 8,
    reason: '周末版本发布',
    projectCount: 1,
    compensation: '调休',
    compensationLabel: '调休',
    status: '待审批',
  },
  {
    id: 'overtime-003',
    code: 'OT-2026-003',
    date: '2026-03-18',
    period: '18:30 - 21:30',
    hours: 3,
    reason: '客户演示准备',
    projectCount: 1,
    compensation: '加班费',
    compensationLabel: '加班费',
    status: '已批准',
  },
  {
    id: 'overtime-008',
    code: 'OT-2026-008',
    date: '2026-02-22',
    period: '09:00 - 18:00',
    hours: 8,
    reason: '周末冲刺迭代',
    projectCount: 1,
    compensation: '调休',
    compensationLabel: '调休（1天）',
    status: '已批准',
  },
  {
    id: 'overtime-009',
    code: 'OT-2026-009',
    date: '2026-03-25',
    period: '18:00 - 21:00',
    hours: 3,
    reason: '需求评审准备',
    projectCount: 2,
    compensation: '调休',
    compensationLabel: '调休（0.4天）',
    status: '已批准',
  },
];

export const overtimeReferenceFixture = defineReferenceFixture<
  OvertimeRecord,
  {
    sourceUrl: string;
    capturedAt: string;
    viewport: { width: number; height: number };
    rows: readonly OvertimeRecord[];
    defaults: { tab: string; status: string; view: string };
  }
>({
  sourceUrl: 'https://demo.risemap.cn/hr/overtime',
  capturedAt: '2026-09-16',
  viewport: { width: 1618, height: 899 },
  rows,
  defaults: { tab: '我的加班', status: '全部状态', view: 'list' },
});
