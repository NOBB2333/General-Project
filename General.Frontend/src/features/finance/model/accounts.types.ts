export type FinanceAccountSurface = 'receivable' | 'payable' | 'opening-receivable' | 'opening-payable';

export type FinanceAccountSide = 'receivable' | 'payable';

export type ReceivableAging = '未到期' | '逾期1-30天' | '逾期31-60天' | '逾期61-90天' | '逾期90天+' | '已完成';

export type PayableAging = '未到期' | '逾期' | '已结清';

export type FinanceAccountStatus = ReceivableAging | PayableAging;

export interface FinanceAccountRow {
  id: string;
  side: FinanceAccountSide;
  code: string;
  sourceCode: string;
  source: string;
  party: string;
  contactRole: '业务员' | '联系人';
  contact: string;
  contract: string;
  project: string;
  amount: number;
  settled: number;
  dueDate: string;
  terms: string;
  status: FinanceAccountStatus;
  invoice: string;
  bill: string;
}

export interface OpeningBalanceRow {
  id: string;
  side: FinanceAccountSide;
  party: string;
  sourceCode: string;
  contract: string;
  amount: number;
  dueDate: string;
  age: string;
  note: string;
  origin: 'system' | 'local';
}

export interface OpeningBalanceDraft {
  party: string;
  contract: string;
  totalAmount: string;
  settledAmount: string;
  dueDate: string;
  note: string;
}

export type FinanceAccountView = 'list' | 'party' | 'contract' | 'project';

export interface FinanceAccountSummary {
  totalLabel: string;
  totalValue: string;
  totalHint: string;
  overdueLabel: string;
  overdueValue: string;
  overdueHint: string;
  countLabel: string;
  countValue: string;
  countHint: string;
  cycleLabel: string;
  cycleValue: string;
  cycleHint: string;
}

export type FinanceAccountRowAction = 'view' | 'settle' | 'remind' | 'copy' | 'reverse';
