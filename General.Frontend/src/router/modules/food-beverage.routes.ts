import type { RouteRecordRaw } from 'vue-router';

export const food_beverage_routes: RouteRecordRaw[] = [
  {
    path: 'industry/food/shelf-life',
    name: 'page.food-beverage.shelf-life',
    component: () => import('@/features/food-beverage/views/ShelfLifePage.vue'),
    meta: {
      appView: 'feature',
      workspace: '效期管理',
      title: '效期管理',
      module: '食品饮料',
      pageLabel: '效期管理',
      feature: 'food-beverage',
    },
  },
];
