export type MyQueueTabId = '待接单' | '待预约' | '服务中' | '我已完成';

export type MyQueueMetricTone = 'primary' | 'cyan' | 'success';

export interface MyQueueTab {
  id: MyQueueTabId;
  label: string;
}

export interface MyQueueMetric {
  id: MyQueueTabId;
  label: string;
  tone: MyQueueMetricTone;
}

export interface MyQueueTicketRow {
  id: string;
  code: string;
  type: string;
  title: string;
  customerName: string;
  contactPerson: string;
  contactPhone: string;
  productName: string;
  productModel: string;
  serialNumber: string;
  urgency: string;
  status: string;
  planStart: string;
  createdAt: string;
  serviceMode: string;
  assignee: string;
}
