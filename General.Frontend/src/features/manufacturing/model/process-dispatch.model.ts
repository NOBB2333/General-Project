import type { PageModel } from '@/components/page';

export const processDispatchModel = {
  key: '生产制造|工序派工',
  module: '生产制造',
  moduleSlug: 'manufacturing',
  label: '工序派工',
  path: '/mfg/process-dispatch',
  routeName: 'page.manufacturing.process-dispatch',
  pageSlug: 'process-dispatch',
  demoPath: '/mfg/dispatch',
  surface: 'kanban',
  description: '将已下达工单的工序分派到工作中心成员/班组，按交期优先排程',
  searchPlaceholder: '搜索工序派工编号/名称/负责人...',
  tabs: ['看板', '表格'],
  tools: ['看板', '表格', '派工'],
  filters: [
    {
      key: 'status',
      label: '状态',
      options: ['全部状态', '草稿', '进行中', '已完成'],
    },
    {
      key: 'time',
      label: '时间',
      options: ['全部时间', '本周', '本月', '本季', '本年'],
    },
  ],
  columns: [
    {
      key: 'code',
      title: '工单号',
      width: 140,
    },
    {
      key: 'name',
      title: '工序',
      width: 220,
    },
    {
      key: 'status',
      title: '状态',
      width: 100,
    },
    {
      key: 'progress',
      title: '工序号',
      width: 100,
    },
    {
      key: 'owner',
      title: '分派',
      width: 100,
    },
  ],
  stats: [
    {
      key: 'a',
      label: '待派工序',
      value: '14道',
      tone: 'warning',
    },
    {
      key: 'b',
      label: '待派定额工时',
      value: '131h',
      tone: 'primary',
    },
    {
      key: 'c',
      label: '已派/在制工序',
      value: '1道',
      tone: 'success',
    },
    {
      key: 'd',
      label: '超载工作中心',
      value: '0个',
      tone: 'neutral',
    },
  ],
  panels: [
    {
      key: 'pool',
      title: '待派工序池',
      items: [
        'MO-2026-0002 SIS安全仪表系统 · 30 SIL回路验证调试',
        'MO-2026-0002 SIS安全仪表系统 · 40 安全认证检验',
        'MO-2026-0004 DCS分布式控制系统 · 10 机柜组装',
        'MO-2026-0004 DCS分布式控制系统 · 20 模块安装与接线',
        'MO-2026-0004 DCS分布式控制系统 · 30 系统上电调试',
        'MO-2026-0004 DCS分布式控制系统 · 40 出厂检验(FAT)',
      ],
    },
    {
      key: 'assigned',
      title: '按工作中心已派工序',
      items: ['电气班组 · MO-2026-0002 SIS安全仪表系统 · 20 安全回路接线'],
    },
  ],
  rows: [],
  primaryAction: '派工',
  moreTools: ['导入', '打印', '刷新', '列设置', '批量启用', '批量停用'],
  pageSize: 10,
  emptyHint: '当前筛选下没有工序派工，可重置条件或派工',
  rowActions: ['view', 'edit', 'copy', 'delete'],
  replication: {
    status: 'verified',
    referenceUrl: 'https://demo.risemap.cn/mfg/dispatch',
    referenceScreenshot: '.codex-artifacts/replication/mfg__process-dispatch/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/mfg__process-dispatch/local-default.png',
    evidenceDocument: 'outputs/page-evidence/mfg__process-dispatch.md',
    capturedAt: '2026-07-25',
  },
} as const satisfies PageModel;

export type ProcessDispatchModel = typeof processDispatchModel;
