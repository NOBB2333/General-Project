export type PerfMgmtTab = 'calculate' | 'review' | 'paid' | 'pricing' | 'params' | 'analysis';
export type PerfMgmtStatus = '未审' | '待审批' | '已审批' | '已驳回' | '已发放';

export interface PerfMgmtTabDefinition {
  id: PerfMgmtTab;
  label: string;
  badge?: string;
}

export interface PerfMgmtRow {
  id: string;
  owner: string;
  orderCount: number;
  planName: string;
  planCode: string;
  period: string;
  baseAmount: string;
  commission: string;
  reward: string;
  rewardCount: string;
  deduction: string;
  amount: string;
  status: PerfMgmtStatus;
}

export interface PerfMgmtDetail {
  title: string;
  description: string;
  facts: readonly string[];
}
