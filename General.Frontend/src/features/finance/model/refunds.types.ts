export type RefundKind = 'customer' | 'supplier';
export type RefundDocumentStatus = '草稿' | '待审批' | '审批中' | '已批准' | '已付款' | '已核销' | '已驳回';
export type RefundStatusFilter = '全部' | RefundDocumentStatus;
export type RefundApprovalStatus = '-' | '待审批' | '审批中' | '已通过' | '已驳回';
export type RefundPaymentStatus = '-' | '待付款' | '已付款';
export type RefundWriteoffStatus = '-' | '待核销' | '已核销';
export type RefundCurrency = 'CNY' | 'USD' | 'EUR';
export type RefundMethod = '银行转账' | '原路退回' | '电汇' | '承兑汇票';

export interface RefundRow {
  id: string;
  kind: RefundKind;
  code: string;
  contract: string;
  party: string;
  description: string;
  currency: RefundCurrency;
  amount: number;
  method: RefundMethod;
  status: RefundDocumentStatus;
  approvalStatus: RefundApprovalStatus;
  paymentStatus: RefundPaymentStatus;
  writeoffStatus: RefundWriteoffStatus;
  requestedAt: string;
}

export interface RefundMetric {
  id: 'approval' | 'processing' | 'pendingAmount' | 'writtenOff';
  label: string;
  value: string;
  tone: 'warning' | 'primary' | 'violet' | 'success';
}

export interface RefundDraft {
  party: string;
  contract: string;
  description: string;
  currency: RefundCurrency;
  amount: string;
  method: RefundMethod;
}
