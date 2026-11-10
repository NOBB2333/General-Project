export type ServiceCardStatus = 'active' | 'expiring' | 'expired';
export type ServiceCardTone = 'diamond' | 'platinum' | 'gold' | 'silver';
export type ServiceCardFilter = 'all' | ServiceCardStatus;

export interface ServiceCardRow {
  id: string;
  serviceName: string;
  tierLabel: string;
  cardNumber: string;
  customer: string;
  validUntil: string;
  validityLabel: string;
  amount: number;
  status: ServiceCardStatus;
  remainingText: string;
  progress: number;
  tone: ServiceCardTone;
}

export interface ServiceCardDraft {
  serviceName: string;
  customer: string;
  tierLabel: string;
  cardNumber: string;
  validUntil: string;
  validityLabel: string;
  amount: string;
  tone: ServiceCardTone;
}
