import type { RouteRecordRaw } from 'vue-router';
import { routeNames } from '@/router/route.types';

export const system_settings_routes: RouteRecordRaw[] = [
  {
    path: 'system/users-roles',
    name: 'page.system-settings.users-roles',
    redirect: { name: routeNames.platformUsers },
  },
  {
    path: 'system/departments',
    name: 'page.system-settings.departments',
    redirect: { name: routeNames.platformOrganization },
  },
  {
    path: 'system/service',
    name: 'page.system-settings.service',
    redirect: { name: routeNames.platformOrganization },
  },
  {
    path: 'system/custom-forms',
    name: 'page.system-settings.custom-forms',
    component: () => import('@/features/system-settings/views/CustomFormsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '自定义表单',
      title: '自定义表单',
      module: '系统设置',
      pageLabel: '自定义表单',
      feature: 'system-settings',
    },
  },
  {
    path: 'system/user-sessions',
    name: 'page.system-settings.user-sessions',
    redirect: { name: routeNames.platformUsers, query: { tab: 'online' } },
  },
];
