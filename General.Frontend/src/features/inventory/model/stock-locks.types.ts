export type StockLockType = '订单锁库' | '手动锁库';
export type StockLockStatus = '锁定中' | '部分释放' | '已释放';
export type StockLockOrderStatus = '审批通过' | '审批中' | '无需审批';

export interface StockLockRecord {
  id: string;
  lockCode: string;
  note: string;
  product: string;
  specification: string;
  batchCode: string;
  type: StockLockType;
  reasonCode: string;
  reasonName: string;
  quantity: number;
  unit: string;
  released: number;
  status: StockLockStatus;
  orderStatus: StockLockOrderStatus;
  lockDate: string;
  releaseDate: string;
  countdown: string;
  warehouse: string;
}

export interface StockLockDraft {
  product: string;
  specification: string;
  batchCode: string;
  type: StockLockType;
  reason: string;
  quantity: string;
  warehouse: string;
}
