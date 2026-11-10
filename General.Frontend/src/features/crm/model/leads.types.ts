export type LeadStatus = '新线索' | '已联系' | '已确认' | '已转化';
export type LeadSource = '展会' | '网站' | '转介绍' | '电话开发' | '客户推荐';
export type LeadScope = 'mine' | 'public';

export interface LeadRow {
  id: string;
  code: string;
  company: string;
  contact: string;
  role: string;
  status: LeadStatus;
  source: LeadSource;
  score: number;
  amount: number;
  region: string;
  industry: string;
  updatedAt: string;
  owner: string;
  intent?: string;
  scope: LeadScope;
  alsoPublic?: boolean;
}

export interface LeadDraft {
  company: string;
  contact: string;
  role: string;
  source: LeadSource;
  amount: string;
  region: string;
  industry: string;
}
