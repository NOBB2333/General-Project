export type MaterialRequestStatus = '待审批' | '已通过' | '已驳回' | '已领取';
export interface MaterialLine {
  name: string;
  quantity: string;
}
export interface MaterialRequest {
  id: string;
  code: string;
  applicant: string;
  department: string;
  date: string;
  reason: string;
  items: MaterialLine[];
  status: MaterialRequestStatus;
  comment?: string;
}
export interface MaterialStock {
  id: string;
  name: string;
  unit: string;
  stock: number;
  warningAt: number;
  price: number;
}
export interface MaterialRequestDraft {
  reason: string;
  itemName: string;
  quantity: string;
}
