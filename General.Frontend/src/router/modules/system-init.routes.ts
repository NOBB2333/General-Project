import type { RouteRecordRaw } from 'vue-router';

export const system_init_routes: RouteRecordRaw[] = [
  {
    path: 'system-init/system-init',
    name: 'page.system-init.system-init',
    component: () => import('@/features/system-init/views/OnboardingPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '系统初始化',
      title: '系统初始化',
      module: '系统初始化',
      pageLabel: '系统初始化',
      feature: 'system-init',
    },
  },
];
