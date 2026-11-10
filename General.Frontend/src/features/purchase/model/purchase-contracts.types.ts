export type PurchaseContractType = '框架合同' | '采购合同' | '服务合同';
export type PurchaseContractStatus = '履约中' | '待审批' | '已完成' | '已逾期' | '草稿' | '已关闭';
export type PurchaseContractSortKey = 'id' | 'name' | 'amount' | 'dueDate';
export interface PurchaseContractRow {
  id: string;
  name: string;
  project: string;
  type: PurchaseContractType;
  supplier: string;
  amount: number;
  executedAmount: number;
  orderCount: number;
  retentionRate: number | null;
  dueDate: string;
  status: PurchaseContractStatus;
  warning: boolean;
  owner: string;
}
export interface PurchaseContractDraft {
  name: string;
  project: string;
  type: PurchaseContractType;
  supplier: string;
  amount: string;
  retentionRate: string;
  dueDate: string;
  owner: string;
}
