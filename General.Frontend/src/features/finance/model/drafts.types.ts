export type DraftSection = 'receivable' | 'warnings' | 'payable' | 'history';
export type DraftPoolStatus = 'all' | 'held' | 'endorsed' | 'discounted';
export type DraftDirection = '应收' | '应付';
export type DraftStatus = '持有中' | '已背书' | '已贴现' | '已解付' | '已退票';
export type AcceptanceKind = '银行承兑' | '商业承兑';
export type DraftMedium = '电票' | '纸票';

export interface DraftRow {
  id: string;
  code: string;
  direction: DraftDirection;
  noteNumber: string;
  bank: string;
  acceptanceKind: AcceptanceKind;
  medium: DraftMedium;
  drawer: string;
  acceptor: string;
  amount: number;
  issueDate: string;
  dueDate: string;
  remainingDays: number;
  status: DraftStatus;
}

export interface DraftMetric {
  id: 'pool' | 'expiring' | 'commercial' | 'returned';
  label: string;
  value: string;
  hint: string;
  tone: 'primary' | 'danger' | 'warning' | 'neutral';
}

export interface DraftRegistration {
  noteNumber: string;
  drawer: string;
  acceptor: string;
  amount: string;
  issueDate: string;
  dueDate: string;
  acceptanceKind: AcceptanceKind;
  medium: DraftMedium;
}

export interface PayableDraftRegistration {
  payee: string;
  amount: string;
  issueDate: string;
  dueDate: string;
}
