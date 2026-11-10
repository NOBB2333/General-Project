export type SpecialPriceApprovalStatus = '草稿' | '审批中' | '已通过' | '已驳回' | '已撤回';
export type SpecialPriceApprovalFilter = '全部' | SpecialPriceApprovalStatus;
export type SpecialPriceUrgency = '普通' | '加急' | '特急';
export type SpecialPriceUrgencyFilter = '全部' | SpecialPriceUrgency;
export type SpecialPriceOrderStatus = '未下单' | '已下单' | '部分下单' | '已作废';
export type SpecialPriceOrderFilter = '全部' | SpecialPriceOrderStatus;

export interface SpecialPriceRecord {
  id: string;
  code: string;
  customer: string;
  contact: string;
  reason: string;
  sourceQuotationCode: string;
  itemCount: number;
  totalNormalAmount: number;
  totalRequestedAmount: number;
  totalLostAmount: number;
  urgency: SpecialPriceUrgency;
  status: SpecialPriceApprovalStatus;
  orderStatus: SpecialPriceOrderStatus;
  linkedOrderCode: string;
  createdBy: string;
  createdAt: string;
}

export interface SpecialPriceDraft {
  customer: string;
  contact: string;
  sourceQuotationCode: string;
  itemCount: string;
  totalNormalAmount: string;
  totalRequestedAmount: string;
  urgency: SpecialPriceUrgency;
  createdBy: string;
  reason: string;
}
