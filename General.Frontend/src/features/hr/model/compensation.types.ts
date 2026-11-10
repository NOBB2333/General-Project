export type CompensationView = '薪酬概览' | '工资条' | '福利明细';
export type CompensationMode = '我的薪酬' | '管理后台';
export interface CompensationItem {
  id: string;
  label: string;
  value: string;
  note?: string;
}
export interface PayslipRow {
  id: string;
  month: string;
  gross: string;
  deductions: string;
  net: string;
  status: '已发放';
}
