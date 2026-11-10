export type FollowUpType = '电话' | '拜访' | '微信' | '邮件' | '会议' | '其他';
export type FollowUpScope = 'all' | 'mine' | 'subordinate' | 'following';
export type FollowUpRole = 'admin' | 'manager' | 'sales';
export type FollowUpView = 'list' | 'calendar';

export interface FollowUpRow {
  id: string;
  date: string;
  type: FollowUpType;
  customer: string;
  contact: string;
  owner: string;
  content: string;
  result: string;
  nextDate?: string;
  nextPlan?: string;
  opportunity?: string;
  ownerScope: Exclude<FollowUpScope, 'all' | 'following'>;
  followed?: boolean;
}

export interface FollowUpDraft {
  customer: string;
  contact: string;
  owner: string;
  type: FollowUpType;
  content: string;
  result: string;
  nextDate: string;
  nextPlan: string;
}
