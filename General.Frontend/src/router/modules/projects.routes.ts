import type { RouteRecordRaw } from 'vue-router';

export const projects_routes: RouteRecordRaw[] = [
  {
    path: 'projects/project-center',
    name: 'page.projects.project-center',
    component: () => import('@/features/projects/views/ProjectCenterPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '项目中心',
      title: '项目中心',
      module: '项目管理',
      pageLabel: '项目中心',
      feature: 'projects',
    },
  },
  {
    path: 'projects/project-analytics',
    name: 'page.projects.project-analytics',
    component: () => import('@/features/projects/views/ProjectAnalyticsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '项目分析中心',
      title: '项目分析中心',
      module: '项目管理',
      pageLabel: '项目分析中心',
      feature: 'projects',
    },
  },
  {
    path: 'projects/tasks',
    name: 'page.projects.tasks',
    component: () => import('@/features/projects/views/TasksPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '任务管理',
      title: '任务管理',
      module: '项目管理',
      pageLabel: '任务管理',
      feature: 'projects',
    },
  },
  {
    path: 'projects/timesheets',
    name: 'page.projects.timesheets',
    component: () => import('@/features/projects/views/TimesheetsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '工时管理',
      title: '工时管理',
      module: '项目管理',
      pageLabel: '工时管理',
      feature: 'projects',
    },
  },
  {
    path: 'projects/delivery-packages',
    name: 'page.projects.delivery-packages',
    component: () => import('@/features/projects/views/DeliveryPackagesPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '项目交付包',
      title: '项目交付包',
      module: '项目管理',
      pageLabel: '项目交付包',
      feature: 'projects',
    },
  },
  {
    path: 'projects/project-config',
    name: 'page.projects.project-config',
    component: () => import('@/features/projects/views/ProjectConfigPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '项目配置中心',
      title: '项目配置中心',
      module: '项目管理',
      pageLabel: '项目配置中心',
      feature: 'projects',
    },
  },
];
