import type { RouteRecordRaw } from 'vue-router';

export const dashboard_routes: RouteRecordRaw[] = [
  {
    path: 'dashboard',
    name: 'page.dashboard.dashboard',
    component: () => import('@/features/dashboard/views/WorkbenchPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '工作台',
      title: '工作台',
      module: '工作台',
      pageLabel: '工作台',
      feature: 'dashboard',
    },
  },
];
