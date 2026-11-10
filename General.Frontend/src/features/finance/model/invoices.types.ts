export type InvoiceDirection = '销项' | '进项';
export type InvoiceType = '专票' | '普票';
export type InvoiceSection = 'overview' | 'outbound' | 'inbound' | 'uninvoiced' | 'tasks';
export type InvoiceRange = 'month' | 'quarter' | 'half-year' | 'year';
export type InvoiceTaskStatus = '待处理' | '已开票';

export interface InvoiceRecord {
  id: string;
  number: string;
  direction: InvoiceDirection;
  date: string;
  party: string;
  type: InvoiceType;
  amount: number;
  tax: number;
  referenceCode: string;
  remark: string;
  verified: boolean;
}

export interface InvoiceMetric {
  id: 'outbound' | 'inbound' | 'tax' | 'uninvoiced';
  label: string;
  value: string;
  hint: string;
  tone: 'primary' | 'danger' | 'warning';
}

export interface InvoiceTask {
  id: string;
  code: string;
  party: string;
  requestedAt: string;
  amount: number;
  type: InvoiceType;
  applicant: string;
  status: InvoiceTaskStatus;
}

export interface UninvoicedItem {
  id: string;
  direction: InvoiceDirection;
  referenceCode: string;
  party: string;
  amount: number;
  dueDate: string;
  status: '待开票' | '待收票';
}

export interface InvoiceTrendPoint {
  month: string;
  outbound: number;
  inbound: number;
  net: number;
}
