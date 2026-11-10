export type PerformanceTab = 'mine' | 'flow' | 'rebook' | 'confirm' | 'analysis';
export type PerformancePeriod = '全部' | '本年' | 'Q1' | 'Q2' | 'Q3' | 'Q4' | '本月';
export interface PerformanceRow {
  id: string;
  direction: '原始确认' | '业绩分出' | '业绩分入';
  customer: string;
  amount: number;
  rate: number;
  date: string;
  status: string;
}
