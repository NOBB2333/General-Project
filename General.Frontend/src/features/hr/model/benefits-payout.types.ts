export type BenefitView = '发放记录' | '兑换码库' | '实物库';
export type BenefitKind = '兑换码' | '实物';
export interface BenefitDistribution {
  id: string;
  title: string;
  kind: BenefitKind;
  date: string;
  owner: string;
  source: string;
  audience: string;
  people: number;
  quantityPerPerson: number;
  totalValue: number;
  note: string;
  status: '已完成';
}
export interface BenefitInventoryItem {
  id: string;
  name: string;
  source: string;
  remaining: number;
  unitValue: string;
  status: '可用' | '库存充足';
}
export interface BenefitDraft {
  title: string;
  kind: BenefitKind;
  audience: string;
  source: string;
  people: string;
  quantityPerPerson: string;
  totalValue: string;
  note: string;
}
