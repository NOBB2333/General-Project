export type AssetCategory = '电子设备' | '办公家具' | '生产设备' | '交通工具' | '仪器仪表';
export type AssetStatus = '在用' | '闲置' | '维修中' | '已报废' | '待处置';

export interface FixedAsset {
  id: string;
  code: string;
  name: string;
  brand: string;
  specification: string;
  category: AssetCategory;
  originalValue: number;
  netValue: number;
  monthlyDepreciation?: number;
  user: string;
  status: AssetStatus;
  location: string;
  department: string;
}

export interface FixedAssetDraft {
  code: string;
  name: string;
  category: AssetCategory;
  originalValue: string;
  user: string;
  location: string;
}
