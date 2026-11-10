export type WorkbenchTabId = 'today' | 'orders' | 'performance' | 'parts' | 'expenses';

export type WorkbenchMetricTone = 'primary' | 'cyan' | 'success' | 'purple' | 'warning';

export interface WorkbenchMetric {
  id: string;
  label: string;
  value: string;
  hint?: string;
  tone: WorkbenchMetricTone;
}

export interface WorkbenchScheduleSection {
  id: string;
  kind: 'schedule';
  title: string;
  count: number;
  emptyText: string;
  tone: 'primary' | 'cyan' | 'danger';
  compact: boolean;
}

export interface WorkbenchSummaryCard {
  id: string;
  kind: 'summary';
  title: string;
  count: number;
  description: string;
  actionLabel: string;
  targetTab: Extract<WorkbenchTabId, 'parts' | 'expenses'>;
  tone: 'purple' | 'warning';
}

export interface WorkbenchQuickLink {
  id: string;
  kind: 'quick-link';
  label: string;
  route: string;
  icon: 'send' | 'book' | 'calendar' | 'scan';
}

export type MyWorkbenchReferenceRow = WorkbenchScheduleSection | WorkbenchSummaryCard | WorkbenchQuickLink;
