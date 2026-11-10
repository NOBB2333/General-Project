import type { ListPageRow } from '@/components/page';

export type ProductBundleStatus = '启用' | '停用';

export interface ProductBundleRow extends ListPageRow {
  id: string;
  code: string;
  name: string;
  description: string;
  productCount: number;
  cost: string;
  suggestedPrice: string;
  margin: string;
  status: ProductBundleStatus;
  referenceCount: number;
  creator: string;
  createdAt: string;
}

export type ProductBundleStatusFilter = 'all' | 'active' | 'inactive';
