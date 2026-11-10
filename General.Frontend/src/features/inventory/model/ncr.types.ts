export type NcrStatus = '待提交' | '待审批' | '待执行' | '执行中' | '已完成' | '已关闭';
export type NcrUrgency = '普通' | '紧急' | '重大';
export type NcrResponsibility = '供应商' | '内部' | '运输' | '待判定';

export interface NcrRecord {
  id: string;
  inspectionCode: string;
  supplier: string;
  failedQuantity: number;
  urgency: NcrUrgency;
  responsibility: NcrResponsibility;
  resolution: string;
  status: NcrStatus;
  createdAt: string;
}

export interface NcrDraft {
  inspectionCode: string;
  supplier: string;
  failedQuantity: string;
  urgency: NcrUrgency;
  responsibility: NcrResponsibility;
  resolution: string;
}
