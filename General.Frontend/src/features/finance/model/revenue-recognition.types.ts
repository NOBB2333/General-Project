export type RevenueStatus = '草稿' | '待审核' | '已审核' | '已驳回' | '已作废';
export type RevenueStatusFilter = '全部' | RevenueStatus;
export type RevenueMethod = '按发货' | '按里程碑' | '按验收' | '按周期' | '手动' | '混合' | '服务';
export type RevenueInvoiceStatus = '已开票' | '部分开票' | '已认证' | '-';

export interface RevenueRecognitionRow {
  id: string;
  code: string;
  contract: string;
  customer: string;
  project: string;
  method: RevenueMethod;
  amount: number | null;
  accumulated: number;
  contractAmount: number;
  remaining: number;
  confirmedAt: string;
  period: string;
  invoiceStatus: RevenueInvoiceStatus;
  status: RevenueStatus;
  creator: string;
  createdAt: string;
  reviewer: string;
  reviewedAt: string;
}

export interface RevenueRecognitionDraft {
  contract: string;
  customer: string;
  project: string;
  method: RevenueMethod;
  amount: string;
  confirmedAt: string;
  period: string;
}

export interface RevenueMetric {
  id: 'month' | 'year' | 'pending' | 'uncollected';
  label: string;
  value: string;
  hint: string;
  tone: 'primary' | 'success' | 'warning' | 'neutral';
}
