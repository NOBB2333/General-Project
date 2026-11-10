import type { PageModel } from '@/components/page';

export const dispatchCalendarModel = {
  key: '服务管理|派工日历',
  module: '服务管理',
  moduleSlug: 'service',
  label: '派工日历',
  path: '/service/dispatch-calendar',
  routeName: 'page.service.dispatch-calendar',
  pageSlug: 'dispatch-calendar',
  demoPath: '/after-sales/dispatch-calendar',
  surface: 'schedule',
  description: '工程师 × 日期视图 · 一眼看清排程冲突、待预约和负载分布',
  searchPlaceholder: '搜索派工日历编号/名称/负责人...',
  tabs: ['周', '双周', '月'],
  tools: ['去派工台', '周', '双周', '月', '回到本周'],
  filters: [
    {
      key: 'region',
      label: '地区',
      options: ['全部地区', '上海', '北京', '广州', '远程', '其他'],
    },
    {
      key: 'owner',
      label: '工程师',
      options: ['全部工程师', '王工', '李工', '赵工', '陈工', '周工'],
    },
  ],
  columns: [],
  stats: [],
  panels: [],
  rows: [
    { id: 'resource-wang', role: 'resource', name: '王工', type: '华东服务组', status: '0 排程' },
    { id: 'resource-zhao', role: 'resource', name: '赵工', type: '华北服务组', status: '0 排程' },
    { id: 'resource-chen', role: 'resource', name: '陈工', type: '远程支持组', status: '0 排程' },
    {
      id: 'unscheduled-reference',
      role: 'unscheduled',
      code: 'SO-20260522-028',
      status: '未排期',
      type: '已派工·待约时间',
    },
  ],
  primaryAction: '去派工台',
  emptyHint: '当前筛选下没有可见工程师，可重置筛选条件',
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/after-sales/dispatch-calendar',
    referenceScreenshot: '.codex-artifacts/replication/service__dispatch-calendar/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/service__dispatch-calendar/local-default.png',
    evidenceDocument: 'outputs/page-evidence/service__dispatch-calendar.md',
    capturedAt: '2026-07-15',
  },
} as const satisfies PageModel;

export type DispatchCalendarModel = typeof dispatchCalendarModel;
