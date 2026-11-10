import { defineReferenceFixture } from '@/components/page';
import type { DispatchEngineer, DispatchMetric, DispatchOrderRow, DispatchServiceMethod } from './to-dispatch.types';

export const dispatchMetrics: readonly DispatchMetric[] = [
  { id: 'total', label: '待分派工单', tone: 'warning' },
  { id: 'urgent', label: '紧急 / 停机', tone: 'danger' },
  { id: 'overdue', label: '已超时', tone: 'danger' },
  { id: 'today', label: '今日新增', tone: 'primary' },
];

export const dispatchEngineers: readonly DispatchEngineer[] = [
  { id: 'engineer-wang', name: '王工', team: '华东服务组', region: '上海', pendingCount: 2 },
  { id: 'engineer-li', name: '李工', team: '华东服务组', region: '上海', pendingCount: 4 },
  { id: 'engineer-zhao', name: '赵工', team: '华北服务组', region: '北京', pendingCount: 1 },
  { id: 'engineer-chen', name: '陈工', team: '远程支持组', region: '远程', pendingCount: 3 },
  { id: 'engineer-zhou', name: '周工', team: '华南服务组', region: '广州', pendingCount: 2 },
];

export const dispatchRegions = ['全部地区', '上海', '北京', '广州', '远程', '其他'] as const;
export const dispatchTypes = [
  '售后报修',
  '安装调试',
  '巡检保养',
  '备件更换',
  '有偿服务',
  '远程支持',
  '客户投诉',
  '其他服务',
] as const;
export const dispatchUrgencies = ['紧急', '高', '中', '低'] as const;
export const dispatchServiceMethods: readonly DispatchServiceMethod[] = [
  '远程支持',
  '上门服务',
  '客户自处理指导',
  '备件寄送',
  '定期巡检',
];

const referenceRows: readonly DispatchOrderRow[] = [
  {
    id: 'SO-20260518-005',
    code: 'SO-20260518-005',
    title: '新购 HMI 现场安装调试',
    type: '安装调试',
    urgency: '中',
    status: '待分派',
    customerName: '苏州精密机械',
    contactPerson: '孙工',
    address: '江苏省苏州市工业园区精密路 18 号',
    createdAt: '2026-05-18 09:00:00',
    serviceMode: '上门服务',
    isShutdown: false,
    overdue: true,
    source: '客户来电',
    recommendedEngineerId: 'engineer-zhao',
    recommendationReasons: ['技能匹配：DCS、仪表、调试', '当前待处理工单较少（1 单）'],
  },
];

export const toDispatchReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/after-sales/dispatch',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  defaults: {
    region: '全部地区',
    type: '',
    urgency: '',
    query: '',
    referenceToday: '2026-08-16',
    totalCount: 1,
  },
  rows: referenceRows,
});
