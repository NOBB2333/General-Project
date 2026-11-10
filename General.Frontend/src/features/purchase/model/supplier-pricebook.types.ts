export type SupplierPricebookStatus = '草稿' | '生效中' | '已过期' | '已废弃';
export interface SupplierDiscount {
  label: string;
  rate: number;
}
export interface SupplierPricebookRow {
  id: string;
  name: string;
  supplier: string;
  status: SupplierPricebookStatus;
  pricingMethod: '按物料单独定价' | '多级折扣';
  discounts: readonly SupplierDiscount[];
  itemCount: number;
  validFrom: string;
  validTo: string;
  createdBy: string;
  updatedAt: string;
}
export interface SupplierPricebookDraft {
  name: string;
  supplier: string;
  itemCount: string;
  validFrom: string;
  validTo: string;
  discountRate: string;
}
