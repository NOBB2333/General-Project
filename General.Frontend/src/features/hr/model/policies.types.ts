export type PolicyStatus = '生效中' | '修订中';
export type PolicyCategory = '人事制度' | '财务制度' | '安全管理' | '行政管理' | '保密制度' | '质量管理';
export interface PolicyRecord {
  id: string;
  code: string;
  title: string;
  version: string;
  summary: string;
  category: PolicyCategory;
  status: PolicyStatus;
  effectiveAt: string;
  creator: string;
  audience: string;
}
export interface PolicyDraft {
  title: string;
  code: string;
  category: PolicyCategory;
  version: string;
  summary: string;
}
