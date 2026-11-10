export type InquiryStatus = '待发布' | '报价收集中' | '已完成比价' | '已转采购单' | '失效关闭';
export type InquiryScope = 'all' | 'mine' | 'team' | 'watched';
export type InquirySortKey = 'id' | 'title' | 'supplierCount' | 'totalBudget' | 'status' | 'deadline';

export interface InquiryRow {
  id: string;
  title: string;
  projectName: string;
  sourceType: 'PTO合同' | '采购申请' | '销售合同' | '手动创建';
  sourceId: string;
  supplierCount: number;
  quotedCount: number;
  materialCount: number;
  totalBudget: number;
  status: InquiryStatus;
  createdAt: string;
  deadline: string;
  createdBy: string;
  owner: string;
  assignedBuyer: string;
  watched: boolean;
  convertedPOs?: readonly string[];
  closeReason?: string;
}

export interface InquiryDraft {
  title: string;
  projectName: string;
  sourceType: InquiryRow['sourceType'];
  sourceId: string;
  supplierCount: string;
  materialCount: string;
  totalBudget: string;
  deadline: string;
  assignedBuyer: string;
}

export interface BusinessInquiryTask {
  id: string;
  title: string;
  customerName: string;
  priority: '紧急' | '普通' | '低';
  assignedBuyer: string;
  materialCount: number;
  estimatedAmount: number;
  deadline: string;
  status: '待分配' | '已分配' | '报价中' | '已报价' | '已完成';
}
