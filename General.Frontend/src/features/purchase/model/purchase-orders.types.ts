export type PurchaseOrderStatus = '草稿' | '待审核' | '已审核' | '部分到货' | '已完成' | '已取消';
export type PurchaseOrderScope = 'all' | 'mine' | 'team' | 'followed';
export type PurchaseOrderView = 'orders' | 'items';
export type PurchaseOrderProgressFilter = 'all' | 'invoice-pending' | 'stock-pending' | 'payment-pending';

export interface PurchaseOrderRow {
  id: string;
  code: string;
  scopeGroup: 'mine' | 'team';
  followed: boolean;
  supplier: string;
  project: string;
  payableRule: '按收货入库';
  buyer: string;
  amount: number;
  invoiceProgress: number;
  stockProgress: number;
  paymentProgress: number;
  status: PurchaseOrderStatus;
  date: string;
  itemCount: number;
}

export interface PurchaseOrderDraft {
  supplier: string;
  project: string;
  buyer: string;
  amount: string;
  itemCount: string;
}
