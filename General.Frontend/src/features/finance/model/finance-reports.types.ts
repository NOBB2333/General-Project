export type FinanceReportView =
  '老板驾驶舱' | '收入分析' | '成本费用' | '现金流' | '项目盈利' | '合同签约' | '人力资源支出';

export interface HealthScore {
  label: string;
  value: number;
  status: string;
  detail: string;
  benchmark: string;
  tone: 'primary' | 'success' | 'warning';
}

export interface FinanceReportMetric {
  id: string;
  label: string;
  value: string;
  description: string;
  badge?: string;
  tone: 'green' | 'orange' | 'cyan' | 'purple' | 'red';
  size: 'large' | 'small';
}

export interface AnalysisPanel {
  view: FinanceReportView;
  title: string;
  subtitle: string;
  values: readonly number[];
}
