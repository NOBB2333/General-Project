export type BorrowItemType = '仪器' | '设备';
export type BorrowStatus = '待审批' | '审批中' | '已归还' | '逾期未还' | '使用中' | '损坏报废';
export interface EquipmentBorrow {
  id: string;
  code: string;
  applicant: string;
  department: string;
  item: string;
  assetCode: string;
  type: BorrowItemType;
  quantity: string;
  business: string;
  borrowedAt: string;
  dueAt: string;
  overdueHint?: string;
  status: BorrowStatus;
  approvers: string;
}
export interface BorrowableEquipment {
  id: string;
  name: string;
  assetCode: string;
  type: BorrowItemType;
  available: boolean;
  location: string;
}
export interface EquipmentBorrowDraft {
  item: string;
  assetCode: string;
  type: BorrowItemType;
  quantity: string;
  business: string;
  borrowedAt: string;
  dueAt: string;
}
