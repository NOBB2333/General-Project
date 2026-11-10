import type { RouteRecordRaw } from 'vue-router';

export const promotion_routes: RouteRecordRaw[] = [
  {
    path: 'promotion/promotion',
    name: 'page.promotion.promotion',
    component: () => import('@/features/promotion/views/PromotionPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '推广奖励',
      title: '推广奖励',
      module: '推广奖励',
      pageLabel: '推广奖励',
      feature: 'promotion',
    },
  },
];
