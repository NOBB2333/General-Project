export type PaymentFlowStatus = 'allocated' | 'partial' | 'unallocated';

export interface PaymentFlowRow {
  id: string;
  code: string;
  imported: boolean;
  supplier: string;
  date: string;
  method: string;
  account: string;
  amount: number;
  allocatedAmount: number;
  status: PaymentFlowStatus;
  assignmentMode?: '登记指定' | '手动分配';
  operator: string;
}

export interface PaymentFlowDraft {
  supplier: string;
  date: string;
  method: string;
  account: string;
  amount: string;
}

export interface PaymentAllocationDraft {
  orderCode: string;
  amount: string;
  note: string;
}

export type PaymentFlowScope = 'all' | 'incomplete' | 'allocated';
