export type CheckinStatus = '待拜访' | '进行中' | '已完成';
export interface SalesVisit {
  id: string;
  customer: string;
  time: string;
  address: string;
  purpose: string;
  priority: 'A' | 'B';
  status: CheckinStatus;
  distance?: string;
  duration?: string;
  summary?: string;
}
