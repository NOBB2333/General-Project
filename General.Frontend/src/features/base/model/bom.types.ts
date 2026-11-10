import type { ListPageRow } from '@/components/page';

export type BomStatus = '草稿' | '待评审' | '已生效';
export type BomType = '标准' | '项目';
export type BomStatusFilter = 'all' | 'draft' | 'review' | 'active' | 'invalid' | 'archived';
export type BomTypeFilter = 'all' | 'standard' | 'project';
export type BomCategoryFilter =
  'all' | 'standard' | 'control' | 'dcs' | 'sis' | 'drive' | 'vfd' | 'project' | 'huaneng';

export interface BomRow extends ListPageRow {
  id: string;
  code: string;
  name: string;
  productCode: string;
  type: BomType;
  version: string;
  status: BomStatus;
  project: string;
  materialCount: number;
  cost: string;
  updatedAt: string;
  creator: string;
  category: Exclude<BomCategoryFilter, 'all' | 'standard' | 'control' | 'drive' | 'project'>;
}
