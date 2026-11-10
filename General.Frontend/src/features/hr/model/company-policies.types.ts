export type CompanyPolicyStatus = '生效中' | '待确认' | '草稿';
export type CompanyPolicyView = '制度中心' | 'AI 智能上传' | '合规看板' | '违规通报';
export interface CompanyPolicyRecord {
  id: string;
  code: string;
  title: string;
  version: string;
  type: string;
  summary: string;
  category: string;
  status: CompanyPolicyStatus;
  clauses: number;
  important: number;
  myProgress: number;
  coverage: number;
  signed: string;
  overdue: number;
}
export interface PolicyChange {
  id: string;
  title: string;
  version: string;
  note: string;
  date: string;
  owner: string;
  tone: 'success' | 'danger';
  tag?: string;
}
