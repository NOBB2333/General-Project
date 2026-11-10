export type SalesContractStatus = '草稿' | '待审批' | '执行中' | '已完成' | '已终止';

export type SalesContractTraffic = 'warning' | 'success' | 'neutral';

export interface SalesContractRow {
  [key: string]: string | number | boolean | null | undefined;
  id: string;
  code: string;
  name: string;
  subtitle: string;
  customer: string;
  source: string;
  amount: string;
  amountValue: number;
  invoicePct: string;
  shipPct: string;
  collectPct: string;
  status: SalesContractStatus;
  orderStatus: string;
  traffic: SalesContractTraffic;
  signDate: string;
  owner: string;
  followed: boolean;
}

export type SalesContractCreateStatus = Extract<SalesContractStatus, '草稿' | '待审批'>;

export interface SalesContractCreatePayload {
  title: string;
  category: string;
  customer: string;
  amount: string;
  signDate: string;
  paymentTerms: string;
  deliveryDays: string;
  warrantyMonths: string;
  note: string;
  limitOrderAmount: boolean;
  allowAffiliates: boolean;
  allowOffCatalog: boolean;
  requireApproval: boolean;
  status: SalesContractCreateStatus;
}

export type SalesContractScopeId = 'all' | 'mine' | 'subordinates' | 'followed';

export type SalesContractStatusFilter = '全部' | SalesContractStatus;

export type SalesContractProgressFilter = 'all' | 'invoice-pending' | 'shipping-pending' | 'collection-pending';
