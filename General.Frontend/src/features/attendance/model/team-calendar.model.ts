import type { PageModel } from '@/components/page';

export const teamCalendarModel = {
  key: '考勤假期|团队日历',
  module: '考勤假期',
  moduleSlug: 'attendance',
  label: '团队日历',
  path: '/attendance/team-calendar',
  routeName: 'page.attendance.team-calendar',
  pageSlug: 'team-calendar',
  demoPath: '/hr/team-calendar',
  surface: 'month-calendar',
  description: '查看团队请假/出差排期，识别人力冲突',
  searchPlaceholder: '搜索团队日历编号/名称/负责人...',
  tabs: ['月视图', '列表'],
  tools: ['今天', '全部部门', '全部类型'],
  filters: [
    {
      key: 'dept',
      label: '部门',
      options: ['全部部门', '产品部', '技术部', '市场部', '销售部', '财务部', '人事部'],
    },
    {
      key: 'type',
      label: '类型',
      options: ['全部类型', '年假', '病假', '事假', '婚假', '产假', '调休', '出差'],
    },
    {
      key: 'time',
      label: '时间',
      options: ['全部时间', '本周', '本月', '本季', '本年'],
    },
  ],
  columns: [
    {
      key: 'owner',
      title: '人员',
      width: 100,
    },
    {
      key: 'type',
      title: '类型',
      width: 100,
    },
    {
      key: 'date',
      title: '开始',
      width: 120,
    },
    {
      key: 'endDate',
      title: '结束',
      width: 120,
    },
    {
      key: 'dept',
      title: '部门',
      width: 100,
    },
  ],
  stats: [
    {
      key: 'a',
      label: '请假记录',
      value: '5',
      tone: 'warning',
    },
    {
      key: 'b',
      label: '出差记录',
      value: '5',
      tone: 'primary',
    },
    {
      key: 'c',
      label: '冲突日期',
      value: '0',
      tone: 'success',
    },
    {
      key: 'd',
      label: '总事件数',
      value: '10',
      tone: 'neutral',
    },
  ],
  panels: [],
  rows: [],
  primaryAction: '今天',
  moreTools: ['导入', '打印', '刷新', '列设置', '批量启用', '批量停用'],
  pageSize: 10,
  emptyHint: '当前筛选下没有团队日历，可重置条件或今天',
  rowActions: ['view', 'edit', 'copy', 'delete'],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/hr/team-calendar',
    referenceScreenshot: '.codex-artifacts/replication/attendance__team-calendar/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/attendance__team-calendar/local-default.png',
    evidenceDocument: 'outputs/page-evidence/attendance__team-calendar.md',
    capturedAt: '2026-07-21',
  },
} as const satisfies PageModel;

export type TeamCalendarModel = typeof teamCalendarModel;
