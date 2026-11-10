import type { PageModel } from '@/components/page';

export const toDispatchModel = {
  key: '服务管理|待分派',
  module: '服务管理',
  moduleSlug: 'service',
  label: '待分派',
  path: '/service/to-dispatch',
  routeName: 'page.service.to-dispatch',
  pageSlug: 'to-dispatch',
  demoPath: '/after-sales/dispatch',
  surface: 'list',
  description: '售后主管派工台 · 按地区 / 紧急度 / 类型分派，支持 AI 推荐工程师与批量派工',
  searchPlaceholder: '工单号 / 客户 / 标题',
  tabs: [],
  tools: ['服务工单', '批量派工', '派工'],
  filters: [
    { key: 'region', label: '地区', options: ['全部地区', '上海', '北京', '广州', '远程', '其他'] },
    {
      key: 'type',
      label: '类型',
      options: [
        '全部类型',
        '售后报修',
        '安装调试',
        '巡检保养',
        '备件更换',
        '有偿服务',
        '远程支持',
        '客户投诉',
        '其他服务',
      ],
    },
    { key: 'urgency', label: '紧急度', options: ['全部紧急度', '紧急', '高', '中', '低'] },
  ],
  columns: [],
  stats: [
    { key: 'total', label: '待分派工单', value: '1', tone: 'warning' },
    { key: 'urgent', label: '紧急 / 停机', value: '0', tone: 'danger' },
    { key: 'overdue', label: '已超时', value: '1', tone: 'danger' },
    { key: 'today', label: '今日新增', value: '0', tone: 'primary' },
  ],
  panels: [],
  rows: [],
  primaryAction: '服务工单',
  pageSize: 20,
  emptyHint: '当前筛选下暂无待分派工单',
  rowActions: ['view', 'status'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/after-sales/dispatch',
    referenceScreenshot: '.codex-artifacts/replication/service__to-dispatch/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/service__to-dispatch/local-default.png',
    evidenceDocument: 'outputs/page-evidence/service__to-dispatch.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export type ToDispatchModel = typeof toDispatchModel;
