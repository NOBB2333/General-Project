export type CertificateView = '证书模板' | '已发证书' | '我的证书';
export interface CertificateTemplate {
  id: string;
  code: string;
  title: string;
  issuer: string;
  tone: 'blue' | 'cyan' | 'green' | 'purple' | 'orange';
  issued: number;
  valid: number;
  expiring: number;
  validity: string;
  rule: string;
}
export interface IssuedCertificate {
  id: string;
  templateTitle: string;
  holder: string;
  department: string;
  issuedAt: string;
  expiresAt: string;
  status: '有效' | '即将到期';
}
