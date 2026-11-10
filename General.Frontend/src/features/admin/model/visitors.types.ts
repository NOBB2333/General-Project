export type VisitorStatus = '已通过' | '已签到' | '待接待人审批' | '已离开' | '进行中';

export interface VisitorRecord {
  id: string;
  code: string;
  visitor: string;
  unit: string;
  type: string;
  reason: string;
  host: string;
  submitter: string;
  visitAt: string;
  status: VisitorStatus;
}

export interface VisitorDraft {
  visitor: string;
  unit: string;
  type: string;
  reason: string;
  host: string;
  visitAt: string;
}
