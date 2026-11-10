export type InboundType = '期初入库' | '采购入库' | '生产入库' | '退货入库' | '其他入库';
export type InboundStatus = '草稿' | '待审批' | '已审批' | '已入库' | '已驳回' | '已作废';

export interface InboundRecord {
  id: string;
  source: string;
  batchCode: string;
  attachmentCount: number;
  arrivalCode: string;
  supplier: string;
  type: InboundType;
  zeroPurchase?: boolean;
  status: InboundStatus;
  date: string;
  warehouse: string;
  quantity: number;
  amount: number;
  handler: string;
  remark: string;
}

export interface InboundDraft {
  type: InboundType;
  date: string;
  warehouse: string;
  supplier: string;
  quantity: string;
  amount: string;
  remark: string;
}
