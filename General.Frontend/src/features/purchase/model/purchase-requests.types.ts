export type PurchaseRequestPriority = '高' | '中' | '低';

export type PurchaseRequestStatus = '草稿' | '审批中' | '询价中' | '比价中' | '采购中' | '已完成' | '已驳回';

export type PurchaseRequestSortKey = 'id' | 'title' | 'priority' | 'requestor' | 'items' | 'amount' | 'status' | 'date';

export interface PurchaseRequestRow {
  id: string;
  title: string;
  project: string;
  customer: string;
  requestor: string;
  requestorDept: string;
  supplier: string;
  priority: PurchaseRequestPriority;
  amount: number;
  items: number;
  status: PurchaseRequestStatus;
  date: string;
  approver: string;
  poList: readonly string[];
}

export interface PurchaseRequestDraft {
  title: string;
  project: string;
  requestor: string;
  supplier: string;
  priority: PurchaseRequestPriority;
  amount: string;
  items: string;
}
