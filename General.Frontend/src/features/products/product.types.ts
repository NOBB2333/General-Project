export type ProductStatus = '启用' | '停用';

export interface UnitConversion {
  id: string;
  unit: string;
  rateType: 'fixed' | 'range';
  conversionRate: number;
  minRate?: number;
  maxRate?: number;
  pricePerUnit: number;
  isDefault: boolean;
}

export interface SpecValue {
  id: string;
  value: string;
}

export interface SpecDimension {
  id: string;
  name: string;
  values: SpecValue[];
}

export interface ProductSku {
  id: string;
  combination: string;
  salePrice: number;
  costPrice: number;
  barcode: string;
  enabled: boolean;
}

export interface ProductDraft {
  category: string;
  subCategory: string;
  name: string;
  spec: string;
  model: string;
  brand: string;
  unit: string;
  referenceCost: number;
  purchasePrice: number;
  salePrice: number;
  barcode: string;
  purchaseCategory: string;
  taxIncluded: boolean;
  defaultTaxRate: number;
  defaultSupplier: string;
  department: string;
  owner: string;
  allowDecimalQty: boolean;
  unitConversions: UnitConversion[];
  images: string[];
  documents: string[];
  enableBatchManagement: boolean;
  enableTraceability: boolean;
  shelfLifeDays?: number;
  batchCodeRule: 'auto' | 'supplier' | 'manual';
  hasSpecs: boolean;
  specDimensions: SpecDimension[];
  skus: ProductSku[];
  customFields: Record<string, string | string[]>;
}

export type ProductSaveAction = 'close' | 'new' | 'copy';

export interface Product {
  id: string;
  name: string;
  model: string;
  category: string;
  brand: string;
  spec: string;
  unit: string;
  barcode: string;
  purchasePrice: number;
  recentPurchasePrice: number;
  recentSalePrice: number;
  presetSalePrice1: number;
  createdAt: string;
  status: ProductStatus;
  department: string;
  owner: string;
  purchaseCategory: string;
  taxIncluded: boolean;
  defaultTaxRate: number;
  defaultSupplier: string;
  mainImage: string;
  stock?: number;
  subCategory?: string;
  referenceCost?: number;
  allowDecimalQty?: boolean;
  unitConversions?: UnitConversion[];
  documents?: string[];
  enableBatchManagement?: boolean;
  enableTraceability?: boolean;
  shelfLifeDays?: number;
  batchCodeRule?: 'auto' | 'supplier' | 'manual';
  hasSpecs?: boolean;
  specDimensions?: SpecDimension[];
  skus?: ProductSku[];
  customFields?: Record<string, string | string[]>;
}

export interface DeletedProduct extends Product {
  deletedAt: string;
}

export interface ProductColumn {
  key: keyof Product | 'operations';
  label: string;
  defaultVisible: boolean;
  alwaysVisible?: boolean;
  align?: 'left' | 'right' | 'center';
  minWidth: number;
}

export interface ProductAlias {
  id: string;
  customer: string;
  customerAbbr: string;
  product: string;
  model: string;
  alias: string;
  aliasModel: string;
  notes: string;
}
