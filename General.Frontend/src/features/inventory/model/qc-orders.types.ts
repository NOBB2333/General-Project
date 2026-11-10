export type QcOrderStatus = '待检验' | '检验中' | '待审核' | '已完成' | '已关闭';
export type QcResult = '合格' | '不合格' | '-';

export interface QcOrderRecord {
  id: string;
  material: string;
  specification: string;
  quantity: number;
  supplier: string;
  sourceCode: string;
  arrivalDate: string;
  status: QcOrderStatus;
  result: QcResult;
  inspector: string;
}

export interface QcOrderDraft {
  material: string;
  specification: string;
  quantity: string;
  supplier: string;
  sourceCode: string;
}
