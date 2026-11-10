export interface DrawingOverviewMetric {
  id: string;
  label: string;
  value: string;
  tone: 'neutral' | 'green' | 'orange' | 'purple' | 'blue';
}

export interface DrawingOverviewAction {
  id: string;
  label: string;
  target?: string;
}

export interface DrawingReviewQueueRow {
  id: string;
  drawingNo: string;
  name: string;
  version: string;
  submitter: string;
  submittedAt: string;
  status: string;
}

export interface DrawingChangeQueueRow {
  id: string;
  changeNo: string;
  name: string;
  versions: string;
  reason: string;
  changedAt: string;
  status: string;
}

export interface DrawingOverviewFixture {
  sourceUrl: string;
  capturedAt: string;
  viewport: { width: number; height: number };
  rows: readonly DrawingReviewQueueRow[];
  metrics: readonly DrawingOverviewMetric[];
  actions: readonly DrawingOverviewAction[];
  changes: readonly DrawingChangeQueueRow[];
}
