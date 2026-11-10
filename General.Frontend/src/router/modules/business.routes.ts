import type { RouteRecordRaw } from 'vue-router';
import { routeNames } from '../route.types';

export const business_routes: RouteRecordRaw[] = [
  {
    path: 'business/overview',
    name: routeNames.businessOverview,
    component: () => import('@/features/business/views/BusinessOverviewPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '经营总览',
      title: '经营总览',
      module: '经营管理',
      pageLabel: '经营总览',
      feature: 'business',
    },
  },
  {
    path: 'business/projects',
    name: routeNames.businessProjects,
    component: () => import('@/features/business/views/BusinessProjectsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '项目经营',
      title: '项目经营',
      module: '经营管理',
      pageLabel: '项目经营',
      feature: 'business',
    },
  },
  {
    path: 'business/reports',
    name: routeNames.businessReports,
    component: () => import('@/features/business/views/BusinessReportsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '报表中心',
      title: '报表中心',
      module: '经营管理',
      pageLabel: '报表中心',
      feature: 'business',
    },
  },
];
