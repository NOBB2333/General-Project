import type { ListPageRow } from '@/components/page';

export type SupplierLevel = 'A' | 'B' | 'C' | 'D';
export type SupplierStatus = '已审批' | '待审批' | '草稿' | '已驳回';
export type SupplierOwnerRole = 'self' | 'subordinate';
export type SupplierScope = 'all' | 'mine' | 'subordinate' | 'starred';
export type SupplierView = 'list' | 'qualification';
export type QualificationView = 'supplier' | 'certificate';
export type SupplierBusinessStatus = '启用' | '停用' | '暂停' | '拉黑';
export type QualificationStatus = '有效' | '即将到期' | '已过期';

export interface SupplierRow extends ListPageRow {
  id: string;
  name: string;
  contact: string;
  phone: string;
  payMethod: string;
  paymentDays: number;
  totalPurchase: number;
  payable: number;
  level: SupplierLevel;
  owner: string;
  ownerRole: SupplierOwnerRole;
  status: SupplierStatus;
}

export interface SupplierQualificationSummary {
  supplierId: string;
  supplierName: string;
  businessStatus: SupplierBusinessStatus;
  responsible: string;
  score: number;
  qualificationCount: number;
  validCount: number;
  expiringCount: number;
  expiredCount: number;
}

export interface SupplierCertificate {
  id: string;
  supplierId: string;
  supplierName: string;
  name: string;
  certificateNumber: string;
  issueDate: string;
  expiryDate: string;
  reminderDays: number;
  responsible: string;
  status: QualificationStatus;
}

export interface SupplierDraft {
  name: string;
  category: string;
  owner: string;
  level: SupplierLevel;
  payMethod: string;
  contact: string;
  phone: string;
  email: string;
  paymentDays: string;
  address: string;
  remark: string;
}
