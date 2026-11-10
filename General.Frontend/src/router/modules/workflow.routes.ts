import type { RouteRecordRaw } from 'vue-router';

export const workflow_routes: RouteRecordRaw[] = [
  {
    path: 'workflow/todo',
    name: 'page.workflow.my-todos',
    component: () => import('@/features/workflow/views/MyTodosPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '我的待办',
      title: '我的待办',
      module: '流程中心',
      pageLabel: '我的待办',
      feature: 'workflow',
    },
  },
  {
    path: 'workflow/my-done',
    name: 'page.workflow.my-done',
    component: () => import('@/features/workflow/views/MyDonePage.vue'),
    meta: {
      appView: 'feature',
      workspace: '我的已办',
      title: '我的已办',
      module: '流程中心',
      pageLabel: '我的已办',
      feature: 'workflow',
    },
  },
  {
    path: 'workflow/my-started',
    name: 'page.workflow.my-started',
    component: () => import('@/features/workflow/views/MyStartedPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '我发起的',
      title: '我发起的',
      module: '流程中心',
      pageLabel: '我发起的',
      feature: 'workflow',
    },
  },
  {
    path: 'workflow/start-process',
    name: 'page.workflow.start-process',
    component: () => import('@/features/workflow/views/StartProcessPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '发起流程',
      title: '发起流程',
      module: '流程中心',
      pageLabel: '发起流程',
      feature: 'workflow',
    },
  },
  {
    path: 'workflow/process-defs',
    name: 'page.workflow.process-defs',
    component: () => import('@/features/workflow/views/ProcessDefsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '流程定义',
      title: '流程定义',
      module: '流程中心',
      pageLabel: '流程定义',
      feature: 'workflow',
    },
  },
  {
    path: 'workflow/process-categories',
    name: 'page.workflow.process-categories',
    component: () => import('@/features/workflow/views/ProcessCategoriesPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '流程分类',
      title: '流程分类',
      module: '流程中心',
      pageLabel: '流程分类',
      feature: 'workflow',
    },
  },
];
