import type { PageModel } from '@/components/page';

export const serviceConfigModel = {
  key: '服务管理|服务配置',
  module: '服务管理',
  moduleSlug: 'service',
  label: '服务配置',
  path: '/service/service-config',
  routeName: 'page.service.service-config',
  pageSlug: 'service-config',
  demoPath: '/after-sales/config',
  surface: 'settings',
  description: '统一配置工单类型、状态、质保、SLA、费用、人员与编号规则',
  searchPlaceholder: '搜索服务配置',
  tabs: [
    '工单类型',
    '状态与紧急度',
    '质保规则',
    'SLA 规则',
    '费用类型',
    '付款模板',
    '服务人员',
    '编号与表单',
    '数据管理',
  ],
  tools: ['新增类型', '服务工单'],
  filters: [],
  columns: [
    { key: 'name', title: '配置名称', width: 180 },
    { key: 'description', title: '说明', width: 320 },
    { key: 'enabled', title: '启用状态', width: 100 },
  ],
  stats: [],
  panels: [
    {
      key: 'types',
      title: '工单类型',
      items: ['售后报修', '安装调试', '巡检保养', '备件更换', '有偿服务', '远程支持', '客户投诉', '其他服务'],
    },
  ],
  rows: [
    { id: 'type-repair', name: '售后报修', description: '客户设备故障、系统异常、无法正常使用', enabled: true },
    { id: 'type-install', name: '安装调试', description: '新设备安装、现场调试、交付验收', enabled: true },
    { id: 'type-inspection', name: '巡检保养', description: '定期维护、设备点检、预防性服务', enabled: true },
    { id: 'type-parts', name: '备件更换', description: '更换配件、耗材、零部件', enabled: true },
    { id: 'type-paid', name: '有偿服务', description: '过保维修、上门服务、专项技术支持', enabled: true },
    { id: 'type-remote', name: '远程支持', description: '电话、微信、远程桌面等非现场支持', enabled: true },
    { id: 'type-complaint', name: '客户投诉', description: '服务态度、响应不及时、质量不满等投诉', enabled: true },
    { id: 'type-other', name: '其他服务', description: '未归类的其它服务需求', enabled: true },
  ],
  primaryAction: '新增类型',
  emptyHint: '当前配置分类下没有项目',
  rowActions: ['edit', 'status'],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/after-sales/config',
    referenceScreenshot: '.codex-artifacts/replication/service__service-config/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/service__service-config/local-default.png',
    evidenceDocument: 'outputs/page-evidence/service__service-config.md',
    capturedAt: '2026-09-16',
  },
} satisfies PageModel;

export type ServiceConfigModel = typeof serviceConfigModel;
