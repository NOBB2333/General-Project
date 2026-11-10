export type DispatchUrgency = '紧急' | '高' | '中' | '低';

export type DispatchServiceMethod = '远程支持' | '上门服务' | '客户自处理指导' | '备件寄送' | '定期巡检';

export interface DispatchEngineer {
  id: string;
  name: string;
  team: string;
  region: string;
  pendingCount: number;
}

export interface DispatchOrderRow {
  id: string;
  code: string;
  title: string;
  type: string;
  urgency: DispatchUrgency;
  status: '待分派';
  customerName: string;
  contactPerson: string;
  address: string;
  createdAt: string;
  serviceMode: DispatchServiceMethod;
  isShutdown: boolean;
  overdue: boolean;
  source: string;
  recommendedEngineerId: string;
  recommendationReasons: readonly string[];
}

export interface DispatchMetric {
  id: 'total' | 'urgent' | 'overdue' | 'today';
  label: string;
  tone: 'warning' | 'danger' | 'primary';
}
