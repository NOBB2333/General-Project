export type CreditView = 'customers' | 'pending';
export type CreditRiskStatus = '正常' | '预警' | '超限';
export type CreditRiskFilter = '全部' | CreditRiskStatus;
export type CreditFreezeStatus = '正常' | '已冻结';
export type CreditFreezeFilter = '全部' | CreditFreezeStatus;

export interface CreditLimitRow {
  id: string;
  customer: string;
  customerType: '终端客户' | '集成商' | 'EPC';
  limit: number;
  used: number;
  overdueAmount: number;
  overdueCount: number;
  riskStatus: CreditRiskStatus;
  freezeStatus: CreditFreezeStatus;
  owner: string;
}

export interface PendingCreditRequest {
  id: string;
  customer: string;
  customerType: CreditLimitRow['customerType'];
  requestedLimit: number;
  currentLimit: number;
  owner: string;
  requestedAt: string;
  reason: string;
  status: '待审批' | '已批准';
}

export interface CreditMetric {
  id: 'customers' | 'limit' | 'usage' | 'overdue';
  label: string;
  value: string;
  hint: string;
  tone: 'primary' | 'neutral' | 'warning' | 'danger';
}
