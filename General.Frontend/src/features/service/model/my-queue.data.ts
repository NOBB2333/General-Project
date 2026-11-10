import { defineReferenceFixture } from '@/components/page';
import type { MyQueueMetric, MyQueueTab, MyQueueTicketRow } from './my-queue.types';

export const myQueueTabs: readonly MyQueueTab[] = [
  { id: '待接单', label: '待接单' },
  { id: '待预约', label: '待预约' },
  { id: '服务中', label: '服务中' },
  { id: '我已完成', label: '我已完成' },
];

export const myQueueMetrics: readonly MyQueueMetric[] = [
  { id: '待接单', label: '待接单', tone: 'primary' },
  { id: '待预约', label: '待预约', tone: 'cyan' },
  { id: '服务中', label: '服务中', tone: 'primary' },
  { id: '我已完成', label: '已完成 / 待确认', tone: 'success' },
];

export const myQueueAssignees = ['张伟', '王工', '李工', '赵工', '陈工', '周工'] as const;
export const myQueueTypes = [
  '售后报修',
  '安装调试',
  '巡检保养',
  '备件更换',
  '有偿服务',
  '远程支持',
  '客户投诉',
  '其他服务',
] as const;
export const myQueueUrgencies = ['紧急', '高', '中', '低'] as const;
export const myQueueServiceModes = ['上门服务', '远程支持', '返厂维修'] as const;

const referenceRows: readonly MyQueueTicketRow[] = [];

export const myQueueReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/after-sales/my-tasks',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  defaults: {
    activeTab: '待接单',
    assignee: '张伟',
    query: '',
    customer: '',
    type: '',
    urgency: '',
    serviceMode: '',
    startDate: '',
    endDate: '',
    totalCount: 0,
  },
  rows: referenceRows,
});
