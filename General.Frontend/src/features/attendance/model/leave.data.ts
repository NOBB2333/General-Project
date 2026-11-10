import { defineReferenceFixture } from '@/components/page';
import type { CompTimeAdjustment, LeaveApproval, LeaveBalance, LeaveRecord } from './leave.types';

export const leaveBalances: readonly LeaveBalance[] = [
  { type: '年假', remaining: 6, total: 10, used: 3, pending: 1, tone: 'orange' },
  { type: '病假', remaining: 13, total: 15, used: 2, pending: 0, tone: 'red' },
  { type: '事假', remaining: 9, total: 10, used: 1, pending: 0, tone: 'purple' },
  { type: '婚假', remaining: 3, total: 3, used: 0, pending: 0, tone: 'red' },
  { type: '调休', remaining: 2, total: 5, used: 2, pending: 1, tone: 'green', note: '含加班转调休额度' },
  { type: '丧假', remaining: 3, total: 3, used: 0, pending: 0, tone: 'navy' },
];

const rows: readonly LeaveRecord[] = [
  {
    id: 'leave-001',
    code: 'LV-2026-001',
    type: '年假',
    startDate: '2026-03-15',
    endDate: '2026-03-17',
    days: 3,
    reason: '家庭旅行',
    status: '已批准',
    approver: '李总监',
  },
  {
    id: 'leave-002',
    code: 'LV-2026-002',
    type: '调休',
    startDate: '2026-03-20',
    endDate: '2026-03-20',
    days: 1,
    reason: '处理个人事务',
    status: '待审批',
    approver: '李总监',
  },
  {
    id: 'leave-007',
    code: 'LV-2026-007',
    type: '病假',
    startDate: '2026-02-20',
    endDate: '2026-02-21',
    days: 2,
    reason: '身体不适',
    status: '已批准',
    approver: '李总监',
  },
];

export const leaveApprovals: readonly LeaveApproval[] = [
  {
    id: 'approval-1',
    applicant: '王小红',
    department: '产品部',
    type: '年假',
    period: '03-22 至 03-24',
    days: 3,
    reason: '家庭安排',
  },
  {
    id: 'approval-2',
    applicant: '周文博',
    department: '销售部',
    type: '病假',
    period: '03-18 至 03-19',
    days: 2,
    reason: '身体不适',
  },
  {
    id: 'approval-3',
    applicant: '陈大伟',
    department: '财务部',
    type: '事假',
    period: '03-25',
    days: 1,
    reason: '个人事务',
  },
];

export const compTimeAdjustments: readonly CompTimeAdjustment[] = [
  { id: 'comp-1', source: '3 月 8 日周末加班', earned: 1, expiresAt: '2026-09-08', status: '待确认' },
  { id: 'comp-2', source: '3 月 12 日晚间加班', earned: 0.5, expiresAt: '2026-09-12', status: '待确认' },
];

export const leaveReferenceFixture = defineReferenceFixture<
  LeaveRecord,
  {
    sourceUrl: string;
    capturedAt: string;
    viewport: { width: number; height: number };
    rows: readonly LeaveRecord[];
    defaults: { tab: string; type: string; status: string };
  }
>({
  sourceUrl: 'https://demo.risemap.cn/hr/leave',
  capturedAt: '2026-09-16',
  viewport: { width: 1618, height: 899 },
  rows,
  defaults: { tab: '我的假期', type: '全部类型', status: '全部状态' },
});
