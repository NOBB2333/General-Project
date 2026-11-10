import type { RouteRecordRaw } from 'vue-router';

export const shared_supply_routes: RouteRecordRaw[] = [
  {
    path: 'shared-supply/supply-market',
    name: 'page.shared-supply.supply-market',
    component: () => import('@/features/shared-supply/views/SupplyMarketPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '供应链市场',
      title: '供应链市场',
      module: '共享供应链',
      pageLabel: '供应链市场',
      feature: 'shared-supply',
    },
  },
  {
    path: 'shared-supply/coop-demand',
    name: 'page.shared-supply.coop-demand',
    component: () => import('@/features/shared-supply/views/CoopDemandPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '合作需求中心',
      title: '合作需求中心',
      module: '共享供应链',
      pageLabel: '合作需求中心',
      feature: 'shared-supply',
    },
  },
];
