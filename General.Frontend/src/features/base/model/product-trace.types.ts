import type { ListPageRow } from '@/components/page';

export type TraceSource = '外购' | '自制' | '委外';
export type TraceStatus = '在库' | '已出库' | '已报修' | '维修中' | '已换货' | '已退回' | '已报废';
export type WarrantyStatus = '未开始' | '在保' | '即将到期' | '已过保';
export type TraceView = 'product-codes' | 'batch-codes' | 'instances' | 'warranty';

export interface ProductTraceRow extends ListPageRow {
  id: string;
  traceCode: string;
  externalSn: string;
  productName: string;
  model: string;
  source: TraceSource;
  status: TraceStatus;
  warrantyStatus: WarrantyStatus;
  customer: string;
  outboundDate: string;
}
