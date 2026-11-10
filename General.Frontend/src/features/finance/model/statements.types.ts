export type StatementParty = '客户' | '供应商';
export type StatementStatus = '待发送' | '已发送待确认' | '已确认' | '有差异' | '草稿';

export interface StatementRow {
  id: string;
  code: string;
  party: StatementParty;
  company: string;
  period: string;
  amount: number;
  status: StatementStatus;
  owner: string;
  difference: number;
}

export interface StatementCategory {
  party: StatementParty;
  count: number;
  balance: number;
  subtitle: string;
  statuses: ReadonlyArray<{ label: string; value: number }>;
}
