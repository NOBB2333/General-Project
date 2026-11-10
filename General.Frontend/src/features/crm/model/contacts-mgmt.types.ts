export type ContactStatus = '在职' | '已跳槽' | '已离职' | '停用';
export type ContactDecision = '第一决策人' | '关键决策人' | '技术决策人' | '财务决策人' | '影响者' | '无决策能力';

export interface ContactRow {
  id: string;
  name: string;
  position: string;
  department: string;
  company: string;
  companyExtra?: string;
  status: ContactStatus;
  decision: ContactDecision;
  phone: string;
  email: string;
  owner: string;
  ownerInitial: string;
  isPrimary?: boolean;
}

export interface ContactDraft {
  name: string;
  position: string;
  department: string;
  company: string;
  companyExtra: string;
  phone: string;
  landline: string;
  email: string;
  status: ContactStatus;
  decision: ContactDecision;
  owner: string;
  isPrimary: boolean;
  note: string;
}
