export type ProductionOrderStageState = 'complete' | 'active' | 'pending';
export type ProductionOperationState = 'completed' | 'active' | 'pending';

export interface ProductionOrderStage {
  id: string;
  label: string;
  step: number;
  state: ProductionOrderStageState;
}

export interface ProductionOrderMetric {
  id: string;
  label: string;
  value: string;
  hint: string;
  tone: 'primary' | 'danger' | 'warning' | 'success';
}

export interface ProductionOperationNode {
  id: string;
  operationNo: string;
  name: string;
  center: string;
  progress: string;
  owner: string;
  state: ProductionOperationState;
}

export interface ProductionMaterialRow {
  id: string;
  material: string;
  code: string;
  specification: string;
  required: string;
  issued: string;
  shortage: string;
  stock: string;
  tags: string[];
  amount: string;
}

export interface ProductionWorkReportRow {
  id: string;
  code: string;
  operation: string;
  reporter: string;
  qualified: number;
  defective: number;
  hours: string;
  time: string;
  status: string;
}

export interface ProductionMaterialSlipRow {
  id: string;
  code: string;
  type: string;
  materialCount: number;
  quantity: string;
  amount: string;
  note: string;
  owner: string;
  date: string;
}

export interface ProductionOrderLog {
  id: string;
  action: string;
  owner: string;
  time: string;
  description: string;
}

export interface ProductionOrderDetail {
  code: string;
  product: string;
  quantity: string;
  bom: string;
  status: string;
  priority: string;
  source: string;
  project: string;
  stages: ProductionOrderStage[];
  metrics: ProductionOrderMetric[];
  operations: ProductionOperationNode[];
  materials: ProductionMaterialRow[];
  workReports: ProductionWorkReportRow[];
  materialSlips: ProductionMaterialSlipRow[];
  costs: { label: string; value: string; emphasis?: boolean }[];
  relatedDocuments: { label: string; value: string }[];
  logs: ProductionOrderLog[];
}
