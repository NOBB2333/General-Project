export type StocktakingType = '全盘' | '抽盘' | '清盘';
export type StocktakingStatus = '计划中' | '盘点中' | '待审核' | '已完成' | '已作废';

export interface StocktakingRecord {
  id: string;
  code: string;
  attachmentCount: number;
  name: string;
  description: string;
  warehouse: string;
  type: StocktakingType;
  plannedDate: string;
  actualDate: string;
  checked: number;
  total: number;
  differences: number;
  differenceAmount: number;
  status: StocktakingStatus;
  accountLocked: boolean;
  lockDeadline: string;
  operator: string;
}

export interface StocktakingDraft {
  name: string;
  description: string;
  warehouse: string;
  type: StocktakingType;
  plannedDate: string;
}
