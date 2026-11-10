export type WorkReportNewSection = 'reports' | 'statistics' | 'templates';
export type WorkReportNewMode = 'person' | 'summary';

export interface WorkReportNewTemplate {
  id: string;
  name: string;
  frequency: string;
  state: 'active' | 'retired';
  tone: 'purple' | 'teal' | 'green' | 'muted';
}

export interface WorkReportNewSubmission {
  id: string;
  person: string;
  submitted: boolean;
  keyResults: string;
  details: string;
  nextPlan: string;
  risks: string;
  score?: number;
}

export interface WorkReportNewDraft {
  keyResults: string;
  details: string;
  nextPlan: string;
  risks: string;
}
