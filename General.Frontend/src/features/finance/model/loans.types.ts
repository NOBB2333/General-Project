export type LoanView = 'employee' | 'bank';
export type EmployeeLoanStatus = '待审核' | '审核中' | '已通过' | '已打款' | '部分核销' | '已核销' | '已驳回';
export type EmployeeLoanFilter = '全部' | Exclude<EmployeeLoanStatus, '已通过'>;

export interface EmployeeLoanRow {
  id: string;
  code: string;
  title: string;
  category: '差旅预支' | '项目备用金' | '采购预支' | '其他借款';
  amount: number;
  balance: number;
  status: EmployeeLoanStatus;
  borrower: string;
  department: string;
  requestedAt: string;
  project: string;
}

export interface BankLoanRow {
  id: string;
  code: string;
  bank: string;
  amount: number;
  balance: number;
  rate: string;
  termMonths: number;
  monthlyPayment: number;
  startDate: string;
  status: '还款中' | '已结清';
}

export interface EmployeeLoanDraft {
  title: string;
  category: EmployeeLoanRow['category'];
  borrower: string;
  department: string;
  amount: string;
  project: string;
}

export interface LoanMetric {
  id: 'amount' | 'pending' | 'balance' | 'writtenOff';
  label: string;
  value: string;
  hint: string;
  tone: 'primary' | 'warning' | 'danger' | 'success';
}
