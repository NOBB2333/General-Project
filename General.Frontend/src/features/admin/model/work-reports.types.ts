export type WorkReportType = '日报' | '周报' | '月报' | '项目汇报' | '专项汇报';
export type WorkReportStatus = '草稿' | '待审批' | '已通过' | '已驳回' | '已撤回';
export interface WorkReport {
  id: string;
  title: string;
  rule?: string;
  type: WorkReportType;
  period: string;
  approver: string;
  status: WorkReportStatus;
  submittedAt: string;
  score?: number;
  scope: 'mine' | 'team';
  summary: string;
}
export interface WorkReportRule {
  id: string;
  name: string;
  type: WorkReportType;
  department: string;
  frequency: string;
  approver: string;
  enabled: boolean;
}
export interface WorkReportDraft {
  title: string;
  type: WorkReportType;
  period: string;
  approver: string;
  summary: string;
}
