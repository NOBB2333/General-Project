export type TransferLoanKind = 'transfer' | 'loan' | 'repricing';

export type TransferLoanStatus = '待审核' | '已审核' | '调拨中' | '已完成' | '已取消';

export interface TransferLoanRow {
  id: string;
  code: string;
  kind: TransferLoanKind;
  type: string;
  direction?: 'out' | 'return';
  sender: string;
  receiver: string;
  reason: string;
  productCount: number;
  quantity: number;
  amount: number;
  adjustment?: string;
  status: TransferLoanStatus;
  overdueDays?: number;
  relatedCode?: string;
  date: string;
  applicant: string;
}

export interface TransferLoanDraft {
  kind: TransferLoanKind;
  sender: string;
  receiver: string;
  reason: string;
  productCount: string;
  quantity: string;
  amount: string;
}

export interface TransferLoanCategory {
  id: 'all' | TransferLoanKind;
  label: string;
  description: string;
  count: number;
}

export interface TransferLoanOverdueItem {
  code: string;
  company: string;
  quantity: number;
  overdueDays: number;
  dueDate: string;
  returned?: string;
}
