export type GiftApplicationStatus = '待审核' | '审核中' | '已通过' | '已驳回' | '已发放';
export type GiftApplicationScope = 'mine' | 'team';
export type GiftCategory = '商务礼品' | '节日礼品' | '员工福利' | '活动礼品';

export interface GiftApplication {
  id: string;
  code: string;
  title: string;
  project: string;
  category: GiftCategory;
  recipientType: '客户' | '员工';
  recipient: string;
  amount: number;
  appliedAt: string;
  status: GiftApplicationStatus;
  approvalStep: number;
  scope: GiftApplicationScope;
}

export interface GiftApplicationDraft {
  title: string;
  project: string;
  category: GiftCategory;
  recipient: string;
  amount: string;
}

export interface GiftSectionRecord {
  id: string;
  title: string;
  detail: string;
  value: string;
  status: string;
}
