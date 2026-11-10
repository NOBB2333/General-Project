import type { ListPageRow } from '@/components/page';

export interface SalesCollectionRow extends ListPageRow {
  scopeGroup?: 'mine' | 'subordinate' | 'other';
  followed?: boolean;
  code: string;
  customer: string;
  owner: string;
  date: string;
  method: string;
  payer: string;
  amount: string;
  allocated: string;
  status: string;
}

export interface SalesDeliveryRow extends ListPageRow {
  scopeGroup?: 'mine' | 'subordinate' | 'other';
  followed?: boolean;
  code: string;
  order: string;
  customer: string;
  receiver: string;
  status: string;
  shipPct: string;
  amount: string;
  outbound: string;
  trackingNo: string;
  date: string;
  owner: string;
}

export interface SalesReturnRow extends ListPageRow {
  scopeGroup?: 'mine' | 'subordinate' | 'other';
  followed?: boolean;
  code: string;
  order: string;
  customer: string;
  reason: string;
  qty: string;
  amount: string;
  status: string;
  attachments: string;
  owner: string;
  date: string;
}
