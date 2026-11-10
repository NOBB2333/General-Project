export type ExpenseView = 'reimbursements' | 'enterprise' | 'payments' | 'statistics' | 'guide';
export type ExpenseOwnership = '项目成本' | '期间费用';
export type ExpenseOwnershipFilter = '全部' | ExpenseOwnership;
export type ExpenseStatus = '待审核' | '审核中' | '已通过' | '已驳回' | '已打款';
export type ExpenseStatusFilter = '全部' | ExpenseStatus;

export interface ExpenseRow {
  id: string;
  kind: 'reimbursement' | 'enterprise';
  code: string;
  name: string;
  ownership: ExpenseOwnership;
  applicant: string;
  department: string;
  date: string;
  project: string;
  amount: number;
  status: ExpenseStatus;
  hasPaymentApplication: boolean;
}

export interface ExpenseDraft {
  name: string;
  ownership: ExpenseOwnership;
  applicant: string;
  department: string;
  project: string;
  amount: string;
  date: string;
}

export interface ExpenseMetric {
  id: 'total' | 'pending' | 'reimbursed' | 'enterprise' | 'period';
  label: string;
  value: string;
  hint: string;
  tone: 'primary' | 'warning' | 'success' | 'purple' | 'orange';
}
