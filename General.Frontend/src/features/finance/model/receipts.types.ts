export type ReceiptSection = 'receipts' | 'writeoffs' | 'advances' | 'auto-match' | 'unmatched' | 'dashboard';

export type ReceiptDashboardView = 'board' | 'details';
export type ReceiptRisk = '高风险' | '中风险' | '低风险';
export type ReceiptScheduleStatus = '已逾期' | '即将到期' | '未来待回' | '已到账';

export interface ReceiptScheduleRow {
  id: string;
  kind: 'schedule';
  risk: ReceiptRisk;
  orderCode: string;
  customer: string;
  planned: number;
  received: number;
  overdueAmount: number;
  overdueCount: number;
  upcomingCount: number;
  longestOverdueDays: number;
  nextDate: string;
  countdownDays: number;
  status: ReceiptScheduleStatus;
}

export interface ReceiptMonthlyRow {
  id: string;
  kind: 'monthly';
  month: string;
  planned: number;
  actual: number;
}

export interface ReceiptStatusRow {
  id: string;
  kind: 'status';
  label: ReceiptScheduleStatus;
  amount: number;
  count: number;
  tone: 'success' | 'danger' | 'warning' | 'neutral';
}

export type ReceiptsReferenceRow = ReceiptScheduleRow | ReceiptMonthlyRow | ReceiptStatusRow;

export interface ReceiptOverviewMetric {
  id: string;
  label: string;
  value: string;
  suffix?: string;
  tone: 'success' | 'warning' | 'primary' | 'purple';
}

export interface ReceiptPlanMetric {
  id: string;
  label: string;
  value: string;
  hint: string;
  tone: 'default' | 'success' | 'primary' | 'danger';
}

export interface ReceiptFutureMetric {
  id: string;
  label: string;
  value: string;
  hint: string;
}
