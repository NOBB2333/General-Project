import type { PageModel } from '@/components/page';

export const serviceCardsModel = {
  key: 'CRM客户管理|服务权益卡',
  module: 'CRM客户管理',
  moduleSlug: 'crm',
  label: '服务权益卡',
  path: '/crm/service-cards',
  routeName: 'page.crm.service-cards',
  pageSlug: 'service-cards',
  demoPath: '/base/service-cards',
  surface: 'cards',
  description: '客户服务周期与会员权益管理 · 录入服务、设定期限、生成卡面、到期倒计时',
  searchPlaceholder: '搜索客户、服务名称、卡号...',
  tabs: ['全部', '生效中', '即将到期', '已过期'],
  tools: ['录入服务'],
  filters: [{ key: 'customer', label: '按客户筛选', options: ['全部客户'] }],
  columns: [],
  stats: [
    { key: 'total', label: '服务卡总数', value: '6', tone: 'neutral' },
    { key: 'active', label: '生效中', value: '3', tone: 'success' },
    { key: 'expiring', label: '即将到期', value: '1', tone: 'warning' },
    { key: 'expired', label: '已过期', value: '2', tone: 'danger' },
  ],
  panels: [],
  rows: [],
  primaryAction: '录入服务',
  pageSize: 12,
  emptyHint: '当前条件下没有服务权益卡',
  actions: [{ id: 'create', label: '录入服务', kind: 'create', tone: 'primary' }],
  replication: {
    status: 'implemented',
    referenceUrl: 'https://demo.risemap.cn/base/service-cards',
    referenceScreenshot: '.codex-artifacts/replication/crm__service-cards/reference-default.png',
    localScreenshot: '.codex-artifacts/replication/crm__service-cards/local-default.png',
    evidenceDocument: 'outputs/page-evidence/crm__service-cards.md',
    capturedAt: '2026-09-16',
  },
} as const satisfies PageModel;

export type ServiceCardsModel = typeof serviceCardsModel;
