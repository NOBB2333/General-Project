export type PricingTabId = 'pricing' | 'pricegroup' | 'agreement' | 'special' | 'alert' | 'adjustment' | 'history';

export interface PriceProduct {
  id: string;
  materialCode: string;
  name: string;
  spec: string;
  unit: string;
  buId: string;
  buName: string;
  priceGroupName: string;
  listPrice: number;
  costPrice: number;
  suggestedPrice: number;
  minPrice: number;
  status: '生效中' | '待生效' | '已过期' | '已停用';
  brand: string;
  updatedAt: string;
  updatedBy: string;
}

export interface PricingGroupNode {
  id: string;
  label: string;
  count: number;
  match?: string;
}

export interface PricingBusinessRecord {
  id: string;
  code: string;
  title: string;
  subtitle: string;
  amount: string;
  status: string;
  updatedAt: string;
}

export interface PriceDraft {
  costPrice: string;
  minPrice: string;
  suggestedPrice: string;
  listPrice: string;
  reason: string;
}
