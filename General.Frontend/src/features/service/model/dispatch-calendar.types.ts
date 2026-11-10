export type DispatchCalendarMode = 'week' | 'biweek' | 'month';

export type DispatchPriority = '低' | '中' | '高';

export interface DispatchEngineerRow {
  id: string;
  role: 'engineer';
  code: string;
  name: string;
  team: string;
  region: string;
  scheduleCount: number;
}

export interface DispatchUnscheduledRow {
  id: string;
  role: 'unscheduled';
  code: string;
  title: string;
  status: '已派工·待约时间';
  priority: DispatchPriority;
  serviceType: string;
  detailStatus: '待预约' | '服务中';
  serviceMethod: '远程支持' | '上门服务';
  engineerId: string;
  engineerName: string;
  engineerTeam: string;
  region: string;
  customer: string;
  contactName: string;
  contactPhone: string;
  address?: string;
  product: string;
  model: string;
  serialNumber: string;
  warrantyStatus: '质保内' | '已过保';
  warrantyEnd: string;
  faultDescription: string;
  impactScope: string;
  source: string;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
}

export type DispatchCalendarReferenceRow = DispatchEngineerRow | DispatchUnscheduledRow;

export interface DispatchCalendarDay {
  id: string;
  iso: string;
  dateLabel: string;
  weekdayLabel: string;
  isToday: boolean;
}
