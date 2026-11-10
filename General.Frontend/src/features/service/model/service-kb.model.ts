import type { PageModel } from '@/components/page';

export const serviceKbModel = {
  key: '服务管理|服务知识库',
  module: '服务管理',
  moduleSlug: 'service',
  label: '服务知识库',
  path: '/service/service-kb',
  routeName: 'page.service.service-kb',
  pageSlug: 'service-kb',
  demoPath: '/after-sales/kb',
  surface: 'list',
  description: '把已解决工单沉淀为可检索、可复用的服务知识',
  searchPlaceholder: '搜索标题 / 现象 / 步骤 / 标签…',
  tabs: ['概览', '排查步骤', '备件清单', '关联工单', '编辑历史'],
  tools: ['从工单沉淀', '新建条目', 'AI 解答'],
  filters: [
    {
      key: 'category',
      label: '分类',
      options: ['全部', '通讯问题', '电气问题', 'HMI问题', '巡检保养', '收费说明', '客户沟通话术'],
    },
    {
      key: 'status',
      label: '状态',
      options: ['全部状态', '已发布', '草稿'],
    },
  ],
  columns: [
    { key: 'code', title: '编号', width: 90 },
    { key: 'name', title: '标题', width: 260 },
    { key: 'category', title: '分类', width: 120 },
    { key: 'productLine', title: '产品线', width: 140 },
    { key: 'status', title: '状态', width: 90 },
  ],
  stats: [
    { key: 'total', label: '知识库条目', value: '6', tone: 'primary' },
    { key: 'monthly', label: '本月新增', value: '0', tone: 'success' },
    { key: 'references', label: '本月被引用', value: '0', tone: 'primary' },
    { key: 'drafts', label: '草稿待发布', value: '0', tone: 'primary' },
  ],
  panels: [
    {
      key: 'categories',
      title: '知识分类',
      items: ['通讯问题', '电气问题', 'HMI问题', '巡检保养', '收费说明', '客户沟通话术'],
    },
  ],
  rows: [
    { id: 'kb006', code: 'KB-006', name: '报修受理标准话术', category: '客户沟通话术', status: '已发布' },
    { id: 'kb002', code: 'KB-002', name: 'PLC 上电后无法通讯排查', category: '通讯问题', status: '已发布' },
    { id: 'kb004', code: 'KB-004', name: 'DCS / 控制柜定期巡检标准作业', category: '巡检保养', status: '已发布' },
    { id: 'kb001', code: 'KB-001', name: 'HMI 黑屏与触控失灵处理', category: 'HMI问题', status: '已发布' },
    { id: 'kb003', code: 'KB-003', name: '变频器过热告警快速处理', category: '电气问题', status: '已发布' },
    { id: 'kb005', code: 'KB-005', name: '有偿服务收费说明', category: '收费说明', status: '已发布' },
  ],
  primaryAction: '新建条目',
  emptyHint: '当前条件下没有知识条目',
  rowActions: ['view', 'edit'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/after-sales/kb',
    referenceScreenshot: '.codex-artifacts/replication/service__service-kb/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/service__service-kb/local-default.png',
    evidenceDocument: 'outputs/page-evidence/service__service-kb.md',
    capturedAt: '2026-09-16',
  },
} satisfies PageModel;

export type ServiceKbModel = typeof serviceKbModel;
