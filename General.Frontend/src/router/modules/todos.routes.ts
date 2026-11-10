import type { RouteRecordRaw } from 'vue-router';

export const todos_routes: RouteRecordRaw[] = [
  {
    path: 'todos/todos',
    name: 'page.todos.todos',
    component: () => import('@/features/todos/views/TodosPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '待办管理',
      title: '待办管理',
      module: '待办管理',
      pageLabel: '待办管理',
      feature: 'todos',
    },
  },
];
