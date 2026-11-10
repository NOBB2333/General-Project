export interface OutsourceTransitRecord {
  id: string;
  workOrder: string;
  workOrderName: string;
  purchaseOrder: string;
  supplier: string;
  supplierCode: string;
  productCode: string;
  productName: string;
  specification: string;
  sent: number;
  consumed: number;
  returned: number;
  balance: number;
  ageDays: number;
  amount: number;
  unit: string;
}

export interface OutsourceFeedDraft {
  purchaseOrder: string;
  supplier: string;
  productCode: string;
  productName: string;
  specification: string;
  quantity: string;
  unit: string;
}
