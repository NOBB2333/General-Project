export type DeliveryScope = 'all' | 'mine' | 'team' | 'watched';
export type DeliveryView = 'list' | 'detail';
export type DeliveryStatus = '待发货' | '部分发货' | '已发货' | '已签收';
export type OutboundStatus = '待出库' | '部分出库' | '已出库';

export interface DeliveryOutbound {
  id: string;
  warehouse: string;
  qty: number;
  status: '已出库';
  date: string;
  carrier: string;
  tracking: string;
  operator: string;
  batchNo: string;
}

export interface DeliveryItem {
  sourceOrderId?: string;
  name: string;
  spec: string;
  qty: number;
  unit: string;
  price: number;
  shippedQty: number;
  outboundStatus: OutboundStatus;
  warehouse: string;
}

export interface SalesDeliveryRecord {
  id: string;
  soId: string;
  soIds?: string[];
  customer: string;
  project: string;
  receiver: string;
  phone: string;
  address: string;
  totalQty: number;
  shippedQty: number;
  status: DeliveryStatus;
  date: string;
  amount: number;
  operator: string;
  watched: boolean;
  outbounds: DeliveryOutbound[];
  items: DeliveryItem[];
}

export interface DeliveryDetailRow extends DeliveryItem {
  key: string;
  shipmentId: string;
  soId: string;
  date: string;
  customer: string;
  operator: string;
  shipmentStatus: DeliveryStatus;
  itemCode: string;
  pendingQty: number;
  amount: number;
}

export interface DeliveryDraft {
  customer: string;
  order: string;
  receiver: string;
  date: string;
}
