export type ProcessDispatchView = 'kanban' | 'table';
export type ProcessDispatchTableStatus = '已完工' | '进行中' | '待派工' | '委外中';
export type ProcessDispatchTableAction = '改派' | '派工' | '外协';

export interface ProcessDispatchOperation {
  id: string;
  code: string;
  name: string;
  title: string;
  product: string;
  operationNo: string;
  operationName: string;
  quantity: number;
  unit: string;
  standardHours: number;
  dueDays: number;
  dueDate: string;
  predecessorPending: boolean;
  status: string;
  type: string;
  progress: string;
  owner: string;
  metric: string;
  value: string;
  lane: 'pending' | 'assigned';
  center?: string;
  assignee?: string;
}

export interface ProcessDispatchTableRow {
  id: string;
  linkedOperationId?: string;
  code: string;
  product: string;
  operationNo: string;
  operationName: string;
  center: string;
  quantity: number;
  standardHours: number;
  dueLabel: string;
  dueDate: string;
  predecessorPending: boolean;
  assignee: string;
  candidateAssignees: string[];
  status: ProcessDispatchTableStatus;
  action: ProcessDispatchTableAction;
}

export interface ProcessDispatchMemberGroup {
  center: string;
  members: string[];
}

export interface ProcessDispatchNavigationRequest {
  path: string;
  workOrderCode: string;
  operationId: string;
}

export interface ProcessDispatchWorkCenter {
  id: string;
  name: string;
  capacityHours: number;
  baseHours: number;
}

export interface ProcessDispatchLane extends ProcessDispatchWorkCenter {
  scheduledHours: number;
  loadPercent: number;
  operations: ProcessDispatchOperation[];
}
