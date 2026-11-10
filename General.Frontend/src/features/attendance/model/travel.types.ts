export type TravelStatus = '待审批' | '已批准' | '已驳回' | '已撤回' | '已完成' | '已报销';
export type TravelTransport = '高铁' | '飞机' | '汽车' | '其他';

export interface TravelRecord {
  id: string;
  code: string;
  destination: string;
  transport: TravelTransport;
  reason: string;
  departureDate: string;
  returnDate: string;
  days: number;
  budget: number;
  status: TravelStatus;
}

export interface TravelDraft {
  destination: string;
  transport: TravelTransport;
  reason: string;
  departureDate: string;
  returnDate: string;
  budget: string;
}
