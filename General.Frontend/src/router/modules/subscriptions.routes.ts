import type { RouteRecordRaw } from 'vue-router';

export const subscriptions_routes: RouteRecordRaw[] = [
  {
    path: 'subscriptions/subscriptions',
    name: 'page.subscriptions.subscriptions',
    component: () => import('@/features/subscriptions/views/SubscriptionsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '服务订阅管理',
      title: '服务订阅管理',
      module: '服务订阅管理',
      pageLabel: '服务订阅管理',
      feature: 'subscriptions',
    },
  },
];
