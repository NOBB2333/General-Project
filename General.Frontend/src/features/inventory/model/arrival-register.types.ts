export type ArrivalStatus = '待检' | '检验中' | '免检-已入库' | '已检验-合格' | '已检验-不合格' | '已入库';
export type ArrivalType = '采购到货' | '生产到货' | '退货到货' | '其他到货';

export interface ArrivalRecord {
  id: string;
  type: ArrivalType;
  date: string;
  warehouse: string;
  party: string;
  relatedCode: string;
  quantity: number;
  amount: number;
  status: ArrivalStatus;
  inspectionCode: string;
  inboundCode: string;
}

export interface ArrivalDraft {
  type: ArrivalType;
  date: string;
  warehouse: string;
  party: string;
  relatedCode: string;
  quantity: string;
  amount: string;
}
