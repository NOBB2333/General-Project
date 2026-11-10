export type BizInquiryPriority = '紧急' | '普通' | '低';
export type BizInquiryStatus = '待分配' | '已分配' | '报价中' | '已报价' | '已完成' | '已取消';
export type BizInquiryResult = '待定' | '中标' | '未中';
export type BizInquirySortKey = 'id' | 'title' | 'estimatedAmount' | 'deadline';

export interface BizInquiryRow {
  id: string;
  title: string;
  customerName: string;
  salesPerson: string;
  salesDept: string;
  priority: BizInquiryPriority;
  assignedBuyer: string;
  status: BizInquiryStatus;
  result: BizInquiryResult;
  materialCount: number;
  estimatedAmount: number;
  createdAt: string;
  deadline: string;
  note: string;
  quoteMin: number | null;
  quoteMax: number | null;
  quoteCount: number;
}

export interface BizInquiryDraft {
  title: string;
  customerName: string;
  salesPerson: string;
  priority: BizInquiryPriority;
  assignedBuyer: string;
  materialCount: string;
  estimatedAmount: string;
  deadline: string;
  note: string;
}
