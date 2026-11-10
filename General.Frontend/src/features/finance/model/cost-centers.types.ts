export type CostCenterView = 'collection' | 'results' | 'rules';
export type CostCollectionTab = 'pending-costs' | 'sales-transfer' | 'confirmations';
export type CostQueue = 'pending' | 'suspended';
export type CostStatus = '待归集' | '部分归集' | '已归集' | '暂挂';

export interface CostCenterRow {
  id: string;
  code: string;
  sourceType: string;
  name: string;
  costType: string;
  customer: string;
  contract: string;
  date: string;
  totalAmount: number;
  collectedAmount: number;
  status: CostStatus;
}

export interface CostCenterMetric {
  id: 'total' | 'collected' | 'pending' | 'todo' | 'complete' | 'rate';
  label: string;
  value: string;
  tone: 'neutral' | 'success' | 'warning' | 'danger' | 'primary';
}

export interface CostRule {
  id: string;
  name: string;
  description: string;
  enabled: boolean;
}
