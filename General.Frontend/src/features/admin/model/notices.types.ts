export type NoticeCategory = '政策制度' | '人事变动' | '活动通知' | '紧急通知' | '节假日安排' | '规章制度' | '其他';

export type NoticePriority = '普通' | '重要' | '紧急';
export type NoticeScope = '全公司' | '指定部门' | '指定人员';
export type NoticeStatus = '草稿' | '已发布' | '已撤回' | '已过期';

export interface CompanyNotice {
  id: string;
  code: string;
  title: string;
  summary: string;
  category: NoticeCategory;
  priority: NoticePriority;
  scope: NoticeScope;
  scopeDetail: string;
  department: string;
  publisher: string;
  publisherInitial: string;
  publishedAt: string;
  readCount: number;
  totalReaders: number;
  status: NoticeStatus;
  pinned: boolean;
  attachments: number;
}

export interface NoticeDraft {
  title: string;
  summary: string;
  category: NoticeCategory;
  priority: NoticePriority;
  scope: NoticeScope;
}
