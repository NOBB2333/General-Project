export type LeaveType = '年假' | '病假' | '事假' | '婚假' | '产假' | '陪产假' | '丧假' | '调休' | '哺乳假';

export type LeaveStatus = '待审批' | '已批准' | '已驳回' | '已撤回';

export interface LeaveBalance {
  type: LeaveType;
  remaining: number;
  total: number;
  used: number;
  pending: number;
  tone: 'orange' | 'red' | 'purple' | 'green' | 'navy';
  note?: string;
}

export interface LeaveRecord {
  id: string;
  code: string;
  type: LeaveType;
  startDate: string;
  endDate: string;
  days: number;
  reason: string;
  status: LeaveStatus;
  approver: string;
}

export interface LeaveApproval {
  id: string;
  applicant: string;
  department: string;
  type: LeaveType;
  period: string;
  days: number;
  reason: string;
}

export interface CompTimeAdjustment {
  id: string;
  source: string;
  earned: number;
  expiresAt: string;
  status: '待确认' | '已入账';
}

export interface LeaveDraft {
  type: LeaveType;
  startDate: string;
  endDate: string;
  reason: string;
}
