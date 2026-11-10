export type OutboundStatus = '已出库' | '待出库';
export type RevenueStatus = '已确认' | '未确认' | '-';

export interface OutboundRecord {
  id: string;
  shipmentCode: string;
  customer: string;
  contractCode: string;
  material: string;
  warehouse: string;
  date: string;
  status: OutboundStatus;
  carrier: string;
  trackingCode: string;
  revenueStatus: RevenueStatus;
  operator: string;
}
