import type { PageModel } from '@/components/page';

export const followUpsModel = {
  key: 'CRM客户管理|跟进记录',
  module: 'CRM客户管理',
  moduleSlug: 'crm',
  label: '跟进记录',
  path: '/crm/follow-ups',
  routeName: 'page.crm.follow-ups',
  pageSlug: 'follow-ups',
  demoPath: '/sales/follow-ups',
  surface: 'list',
  description: '记录和管理所有客户及商机的跟进活动',
  searchPlaceholder: '搜索客户、内容、商机...',
  tabs: [],
  tools: ['所有', '我负责的', '下属负责的', '我关注的', '新增跟进'],
  filters: [],
  columns: [],
  stats: [
    { key: 'total', label: '总跟进记录', value: '36', tone: 'primary' },
    { key: 'week', label: '本周跟进', value: '0', tone: 'primary' },
    { key: 'pending', label: '待跟进', value: '36', tone: 'danger' },
    { key: 'customers', label: '覆盖客户', value: '8', tone: 'primary' },
  ],
  panels: [],
  rows: [],
  primaryAction: '新增跟进',
  pageSize: 20,
  emptyHint: '当前条件下没有跟进记录',
  actions: [{ id: 'create', label: '新增跟进', kind: 'create', tone: 'primary' }],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/sales/follow-ups',
    referenceScreenshot: '.codex-artifacts/replication/crm__follow-ups/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/crm__follow-ups/local-default.png',
    evidenceDocument: 'outputs/page-evidence/crm__follow-ups.md',
    capturedAt: '2026-09-16',
  },
} as const satisfies PageModel;

export type FollowUpsModel = typeof followUpsModel;
