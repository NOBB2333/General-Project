export type TenderStage = '报名中' | '资料准备中' | '文件审核中' | '已开标' | '评标中';
export type TenderResult = '中标' | '待结果' | '未中标';
export type TenderDepositStatus = '待申请' | '待缴纳' | '已缴纳' | '待退还' | '已退还';
export type TenderPriority = '高' | '中' | '低';
export type TenderTab = 'bids' | 'templates' | 'analytics' | 'workbench';

export interface TenderRow {
  id: string;
  code: string;
  name: string;
  customer: string;
  customerShort: string;
  owner: string;
  stage: TenderStage;
  deadline: string;
  openingDate: string;
  depositStatus: TenderDepositStatus;
  depositAmount: number;
  result: TenderResult;
  risks: readonly string[];
  updatedAt: string;
  priority: TenderPriority;
  method: '公开招标' | '邀请招标' | '竞争性谈判';
  active: boolean;
}

export interface TenderTask {
  id: string;
  title: string;
  project: string;
  deadline: string;
  tone: 'document' | 'review' | 'deposit';
}

export interface TenderDraft {
  name: string;
  customer: string;
  owner: string;
  stage: TenderStage;
  deadline: string;
  openingDate: string;
  depositAmount: string;
  priority: TenderPriority;
  method: TenderRow['method'];
}
