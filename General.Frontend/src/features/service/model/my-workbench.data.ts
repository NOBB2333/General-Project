import { defineReferenceFixture } from '@/components/page';
import type { WorkbenchMetric, MyWorkbenchReferenceRow } from './my-workbench.types';

export const myWorkbenchMetrics: readonly WorkbenchMetric[] = [
  {
    id: 'today',
    label: '今日待办',
    value: '0',
    hint: '按时间排序的今日行程',
    tone: 'primary',
  },
  {
    id: 'week',
    label: '本周排程',
    value: '0',
    hint: '08-10—08-16',
    tone: 'cyan',
  },
  { id: 'completion', label: '本月完成率', value: '0%', tone: 'success' },
  { id: 'response', label: '平均响应', value: '0 分', tone: 'purple' },
  { id: 'rating', label: '客户评分', value: '—', tone: 'warning' },
];

const referenceRows: readonly MyWorkbenchReferenceRow[] = [
  {
    id: 'schedule-today',
    kind: 'schedule',
    title: '今日行程（08-16）',
    count: 0,
    emptyText: '今日无排程。可去「派工日历」查看本周。',
    tone: 'primary',
    compact: false,
  },
  {
    id: 'schedule-tomorrow',
    kind: 'schedule',
    title: '明日预告（08-17）',
    count: 0,
    emptyText: '明日暂无排程',
    tone: 'cyan',
    compact: true,
  },
  {
    id: 'schedule-urgent',
    kind: 'schedule',
    title: '急单聚焦',
    count: 0,
    emptyText: '无急单 ✓',
    tone: 'danger',
    compact: true,
  },
  {
    id: 'summary-parts',
    kind: 'summary',
    title: '我的备件申请',
    count: 0,
    description: '无进行中备件申请',
    actionLabel: '查看明细',
    targetTab: 'parts',
    tone: 'purple',
  },
  {
    id: 'summary-expenses',
    kind: 'summary',
    title: '我开的费用单',
    count: 0,
    description: '无进行中费用单',
    actionLabel: '查看明细',
    targetTab: 'expenses',
    tone: 'warning',
  },
  {
    id: 'quick-my-queue',
    kind: 'quick-link',
    label: '待我处理',
    route: '/service/my-queue',
    icon: 'send',
  },
  {
    id: 'quick-knowledge',
    kind: 'quick-link',
    label: '服务知识库',
    route: '/service/service-kb',
    icon: 'book',
  },
  {
    id: 'quick-calendar',
    kind: 'quick-link',
    label: '派工日历',
    route: '/service/dispatch-calendar',
    icon: 'calendar',
  },
  {
    id: 'quick-scan',
    kind: 'quick-link',
    label: '扫码报修入口',
    route: '/service/scan-repair',
    icon: 'scan',
  },
];

export const myWorkbenchReferenceFixture = defineReferenceFixture({
  sourceUrl: 'https://demo.risemap.cn/after-sales/my-workspace',
  capturedAt: '2026-08-16',
  viewport: { width: 1618, height: 899 },
  defaults: {
    activeTab: 'today',
    referenceDate: '2026-08-16',
    todayCount: 0,
    weekCount: 0,
  },
  rows: referenceRows,
});
