export type BeverageCategory = '白酒' | '红酒' | '啤酒' | '饮料' | '茶叶' | '其他';
export type BeverageRequestStatus = '待审核' | '已通过' | '已驳回' | '已领取';

export interface BeverageStock {
  id: string;
  code: string;
  name: string;
  category: Exclude<BeverageCategory, '其他'>;
  specification: string;
  stock: string;
  unitPrice: number;
  stockValue: number;
  location: string;
  status: '正常' | '预警';
}

export interface BeverageRequest {
  id: string;
  code: string;
  categories: BeverageCategory[];
  summary: string;
  amount: number;
  status: BeverageRequestStatus;
  applicant: string;
  department: string;
  appliedAt: string;
  purpose: string;
  project?: string;
  itemSummary: string;
}

export interface BeverageRequestDraft {
  category: BeverageCategory;
  itemName: string;
  quantity: string;
  purpose: string;
  project: string;
}
