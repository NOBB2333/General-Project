export type PurchaseReturnStatus = '草稿' | '待审核' | '已审核' | '退货中' | '已退货' | '已退款';
export type PurchaseReturnTab = '全部' | PurchaseReturnStatus;
export type PurchaseReturnTimeFilter = 'all' | 'today' | '7d' | 'month' | '3m' | 'custom';

export interface PurchaseReturnRecord {
  id: string;
  poId: string;
  supplier: string;
  reason: string;
  items: number;
  amount: number;
  status: PurchaseReturnStatus;
  date: string;
  attachments: number;
  refunded: boolean;
  operator: string;
}

export interface PurchaseReturnDraft {
  poId: string;
  supplier: string;
  reason: string;
  items: string;
  amount: string;
  date: string;
  note: string;
}

export interface PurchaseReturnTimeOption {
  label: string;
  value: PurchaseReturnTimeFilter;
}
