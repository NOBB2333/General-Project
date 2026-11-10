export type QuoteStatus = '草稿' | '待审核' | '已发送' | '已接受' | '已拒绝' | '已过期';
export type QuoteOwnerScope = 'mine' | 'subordinate' | 'other';
export type QuoteScope = 'all' | 'mine' | 'subordinate' | 'following';
export type QuoteView = 'list' | 'analysis';

export interface SalesQuoteRow {
  id: string;
  code: string;
  customer: string;
  project: string;
  owner: string;
  ownerInitial: string;
  ownerScope: QuoteOwnerScope;
  productCount: number;
  amount: number;
  discount: number | null;
  status: QuoteStatus;
  validUntil: string;
  date: string;
  followed?: boolean;
}

export interface SalesQuoteDraft {
  customer: string;
  project: string;
  owner: string;
  productCount: string;
  amount: string;
  discount: string;
  validUntil: string;
  status: QuoteStatus;
}
