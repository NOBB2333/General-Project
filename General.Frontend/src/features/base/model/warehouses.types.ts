export type WarehouseReferenceType = '主仓' | '项目仓' | '备品仓' | '委外仓';

export type WarehouseType = WarehouseReferenceType | '退货仓' | '中转仓';

export type WarehouseEditableType = WarehouseType;

export type WarehouseFilter = '全部' | WarehouseReferenceType;

export type WarehouseView = 'card' | 'table';

export interface WarehouseRow {
  id: string;
  name: string;
  type: WarehouseType;
  address: string;
  manager: string;
  phone: string;
  totalSKU: number;
  totalQty: number;
  totalValue: number;
  totalPurchase: number;
  estimatedSales: number;
  noPriceSKU: number;
  noPricePurchaseTotal: number;
  status: '正常';
  isVirtual: boolean;
  supplierCode?: string;
  remark?: string;
}

export interface WarehouseDraft {
  name: string;
  type: WarehouseEditableType;
  address: string;
  manager: string;
  phone: string;
  remark: string;
}

export interface WarehouseTrend {
  labels: readonly string[];
  values: readonly number[];
}
