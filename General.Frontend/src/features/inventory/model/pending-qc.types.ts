export type PendingQcStatus = '草稿' | '已提交' | '检验中' | '已通过' | '部分合格' | '不合格' | '已入库' | '已退回';

export interface PendingQcRecord {
  id: string;
  stage: string;
  attachmentCount: number;
  material: string;
  specification: string;
  quantity: string;
  batchCode: string;
  supplier: string;
  contractCode: string;
  arrivalDate: string;
  status: PendingQcStatus;
  inspectionCode: string;
  result: readonly string[];
}

export interface PendingQcDraft {
  material: string;
  specification: string;
  quantity: string;
  batchCode: string;
  supplier: string;
}
