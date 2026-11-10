export type QualificationCategory =
  '体系认证' | '行业资质' | '安全资质' | '知识产权证书' | '产品认证' | '企业荣誉资质' | '政府认定类资质';
export type QualificationStatus = '有效' | '即将到期' | '年审中' | '复审中' | '已过期' | '已注销';

export interface Qualification {
  id: string;
  name: string;
  archiveCode: string;
  category: QualificationCategory;
  certificateNo: string;
  issuer: string;
  validFrom: string;
  validTo: string;
  expiryHint?: string;
  owner: string;
  status: QualificationStatus;
}
export interface QualificationDraft {
  name: string;
  archiveCode: string;
  category: QualificationCategory;
  certificateNo: string;
  issuer: string;
  validTo: string;
  owner: string;
}
