export type ServiceTicketUrgency = '紧急' | '高' | '中' | '低';

export type ServiceTicketStatus =
  | '待受理'
  | '待分派'
  | '待接单'
  | '待预约'
  | '处理中'
  | '待客户确认'
  | '待回访'
  | '已完成'
  | '已关闭'
  | '已驳回'
  | '需报价';

export interface ServiceTicketRow {
  id: string;
  code: string;
  title: string;
  aiRisk: boolean;
  type: string;
  customer: string;
  contact: string;
  product: string;
  serialNumber: string;
  warranty: '质保内' | '已过保' | '无质保';
  urgency: ServiceTicketUrgency;
  status: ServiceTicketStatus;
  overdue: boolean;
  billable: boolean;
  owner: string;
  createdAt: string;
  response: string;
  description: string;
}

export type ServiceTicketBatchAction = 'dispatch' | 'transfer' | 'force-close';

export interface ServiceTicketDraft {
  title: string;
  type: string;
  customer: string;
  contact: string;
  product: string;
  serialNumber: string;
  urgency: ServiceTicketUrgency;
  description: string;
}
