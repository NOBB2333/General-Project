export type SalesReturnScope = 'all' | 'mine' | 'team' | 'followed';
export type SalesReturnStatus = '待审核' | '已审核' | '已退货' | '已退款';
export type SalesReturnStatusFilter = '全部' | SalesReturnStatus;

export interface SalesReturnRow {
  id: string;
  scopeGroup: 'mine' | 'team';
  followed: boolean;
  code: string;
  orderCode: string;
  customer: string;
  reason: string;
  quantity: number;
  amount: number;
  status: SalesReturnStatus;
  attachments: number;
  owner: string;
  requestedAt: string;
}

export interface SalesReturnDraft {
  customer: string;
  orderCode: string;
  reason: string;
  quantity: string;
  amount: string;
}
