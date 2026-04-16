import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

// 示例路由，实际开发中可以删除，当前先保留为业务占位示例。
const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ic:baseline-view-in-ar',
      keepAlive: true,
      order: 1000,
      title: $t('demos.title'),
    },
    name: 'Demos',
    path: '/demos',
    children: [
      {
        meta: {
          title: $t('demos.general'),
        },
        name: 'GeneralDemos',
        path: '/demos/general-demos',
        component: () => import('#/views/demos/antd/index.vue'),
      },
    ],
  },
];

export default routes;
