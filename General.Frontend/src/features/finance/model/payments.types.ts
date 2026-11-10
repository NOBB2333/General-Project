export type PaymentSection = 'tasks' | 'writeoffs' | 'advances' | 'auto-match' | 'unmatched';
export type PaymentStage = '待付款' | '审批中' | '已驳回' | '已完成';
export type PaymentApprovalStatus = '已审批' | '部分登记' | '审批中' | '已驳回';

export interface PaymentTaskRow {
  id: string;
  code: string;
  urgent: boolean;
  submittedAt: string;
  requester: string;
  source: string;
  referenceCode: string;
  payableCode: string;
  payee: string;
  requestedAmount: number;
  expectedDate: string;
  payableAmount: number | null;
  overpaidAmount: number;
  approvalStatus: PaymentApprovalStatus;
  registeredAmount: number;
  stage: PaymentStage;
  automatic: boolean;
}

export interface PaymentMetric {
  id: string;
  label: string;
  value: string;
  hint?: string;
  tone: 'purple' | 'primary' | 'success' | 'danger';
}

export interface PaymentDraft {
  amount: string;
  account: string;
  date: string;
  note: string;
}
