import type { PageModel } from '@/components/page';

export const customersModel = {
  key: 'CRM客户管理|客户管理',
  module: 'CRM客户管理',
  moduleSlug: 'crm',
  label: '客户管理',
  path: '/base/customers',
  routeName: 'page.crm.customers',
  pageSlug: 'customers',
  demoPath: '/base/customers',
  surface: 'list',
  description: '客户档案、联系人、跟进记录与商务往来管理',
  searchPlaceholder: '搜索客户名称、联系人...',
  tabs: ['全部', '终端客户', '集成商', 'EPC', '贸易商', 'OEM'],
  tools: ['新增客户', '批量导入', '扫描名片', '导出', '回收站', '统计分析'],
  filters: [],
  columns: [
    { key: 'name', title: '客户名称', width: 220 },
    { key: 'trafficLight', title: '红绿灯', width: 70 },
    { key: 'type', title: '类型', width: 100 },
    { key: 'level', title: '级别', width: 110 },
    { key: 'owner', title: '负责人', width: 120 },
    { key: 'contact', title: '联系方式', width: 150 },
    { key: 'creditLimit', title: '信用额度', width: 140 },
    { key: 'paymentDays', title: '账期', width: 80 },
    { key: 'totalSales', title: '累计销售', width: 120 },
    { key: 'receivable', title: '应收余额', width: 120 },
    { key: 'action', title: '操作', width: 150 },
  ],
  stats: [
    { key: 'total', label: '客户总数', value: '8', tone: 'neutral' },
    { key: 'sales', label: '累计销售额', value: '¥767万', tone: 'neutral' },
    { key: 'receivable', label: '应收余额', value: '¥124.1万', tone: 'danger' },
    { key: 'terms', label: '平均账期', value: '41天', tone: 'neutral' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新增客户',
  pageSize: 10,
  emptyHint: '当前筛选下没有客户',
  rowActions: ['view', 'edit', 'delete'],
  actions: [
    { id: 'create', label: '新增客户', kind: 'create', tone: 'primary' },
    { id: 'import', label: '批量导入', kind: 'import' },
    { id: 'scan', label: '扫描名片', kind: 'custom' },
    { id: 'export', label: '导出', kind: 'export' },
    { id: 'recycle-bin', label: '回收站', kind: 'custom' },
    { id: 'analytics', label: '统计分析', kind: 'custom' },
  ],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/base/customers',
    referenceScreenshot: '.codex-artifacts/replication/crm__customers/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/crm__customers/local-default.png',
    evidenceDocument: 'outputs/page-evidence/crm__customers.md',
    capturedAt: '2026-08-16',
  },
} as const satisfies PageModel;

export const customerColumns = [
  { key: 'name', label: '客户名称', required: true },
  { key: 'trafficLight', label: '红绿灯' },
  { key: 'type', label: '类型' },
  { key: 'level', label: '级别' },
  { key: 'owner', label: '负责人' },
  { key: 'contact', label: '联系方式' },
  { key: 'creditLimit', label: '信用额度' },
  { key: 'paymentDays', label: '账期' },
  { key: 'totalSales', label: '累计销售' },
  { key: 'receivable', label: '应收余额' },
] as const;

export const customerOwners = ['张伟', '李娜', '王强', '赵敏', '陈晨'] as const;
export const customerTags = [
  'VIP',
  'A级',
  'B级',
  '电力能源',
  '石油化工',
  '冶金钢铁',
  '智能制造',
  '长期合作',
  '新客户',
] as const;

export type CustomersModel = typeof customersModel;
