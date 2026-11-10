export type SealType = '公章' | '合同专用章' | '财务专用章' | '法人章' | '发票专用章' | '部门章';
export type SealApplicationStatus = '待审批' | '审批中' | '已通过' | '已驳回' | '已盖章' | '已归还';

export interface SealApplication {
  id: string;
  code: string;
  urgency?: '加急' | '特急';
  company: string;
  applicant: string;
  department: string;
  sealType: SealType;
  fileName: string;
  fileType: string;
  copies: number;
  purpose: string;
  status: SealApplicationStatus;
  appliedAt: string;
}

export interface SealLedgerItem {
  id: string;
  name: string;
  type: SealType;
  company: string;
  custodian: string;
  department: string;
  location: string;
  status: '正常' | '借出' | '作废';
}

export interface SealApplicationDraft {
  company: string;
  sealType: SealType;
  fileName: string;
  purpose: string;
  copies: string;
  urgency: '普通' | '加急' | '特急';
}
