export type InventoryOverviewTab = '总览' | '商品库存' | '在途库存' | '分类汇总' | '仓库视图';

export interface WarehouseAmount {
  name: string;
  amount: number;
  label: string;
}
export interface CategoryStock {
  name: string;
  quantity: number;
  tone: string;
}
export interface InventoryMovement {
  id: string;
  kind: string;
  description: string;
  quantity: string;
  amount: string;
}
export interface StockOverviewFixture {
  sourceUrl: string;
  capturedAt: string;
  viewport: { width: number; height: number };
  rows: readonly InventoryMovement[];
  warehouses: readonly WarehouseAmount[];
  categories: readonly CategoryStock[];
}
