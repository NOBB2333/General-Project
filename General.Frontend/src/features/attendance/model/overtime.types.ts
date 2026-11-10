export type OvertimeStatus = '待审批' | '已批准' | '已驳回' | '已撤回';
export type OvertimeCompensation = '调休' | '加班费';

export interface OvertimeRecord {
  id: string;
  code: string;
  date: string;
  period: string;
  hours: number;
  reason: string;
  projectCount: number;
  compensation: OvertimeCompensation;
  compensationLabel: string;
  status: OvertimeStatus;
}

export interface OvertimeDraft {
  date: string;
  startTime: string;
  endTime: string;
  reason: string;
  compensation: OvertimeCompensation;
  projectCount: string;
}
