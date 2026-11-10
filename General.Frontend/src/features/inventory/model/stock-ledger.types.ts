export type StockLedgerDirection = '入库' | '出库';
export type StockLedgerStatus = '已入库' | '已出库' | '待审批';

export interface StockLedgerRecord {
  id: string;
  flowCode: string;
  direction: StockLedgerDirection;
  businessType: string;
  date: string;
  warehouse: string;
  operator: string;
  quantity: number;
  amount: number;
  status: StockLedgerStatus;
  sourceCode: string;
  counterparty: string;
}
