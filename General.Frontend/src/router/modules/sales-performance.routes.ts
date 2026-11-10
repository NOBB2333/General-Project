import type { RouteRecordRaw } from 'vue-router';

export const sales_performance_routes: RouteRecordRaw[] = [
  {
    path: 'sales-perf/sales-board',
    name: 'page.sales-performance.sales-board',
    component: () => import('@/features/sales-performance/views/SalesBoardPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '销售看板',
      title: '销售看板',
      module: '销售绩效',
      pageLabel: '销售看板',
      feature: 'sales-performance',
    },
  },
  {
    path: 'sales-perf/sales-targets',
    name: 'page.sales-performance.sales-targets',
    component: () => import('@/features/sales-performance/views/SalesTargetsPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '销售目标',
      title: '销售目标',
      module: '销售绩效',
      pageLabel: '销售目标',
      feature: 'sales-performance',
    },
  },
  {
    path: 'sales-perf/perf-plans',
    name: 'page.sales-performance.perf-plans',
    component: () => import('@/features/sales-performance/views/PerfPlansPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '绩效方案配置',
      title: '绩效方案配置',
      module: '销售绩效',
      pageLabel: '绩效方案配置',
      feature: 'sales-performance',
    },
  },
  {
    path: 'sales-perf/my-performance',
    name: 'page.sales-performance.my-performance',
    component: () => import('@/features/sales-performance/views/MyPerformancePage.vue'),
    meta: {
      appView: 'feature',
      workspace: '我的绩效',
      title: '我的绩效',
      module: '销售绩效',
      pageLabel: '我的绩效',
      feature: 'sales-performance',
    },
  },
  {
    path: 'sales-perf/perf-mgmt',
    name: 'page.sales-performance.perf-mgmt',
    component: () => import('@/features/sales-performance/views/PerfMgmtPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '绩效管理',
      title: '绩效管理',
      module: '销售绩效',
      pageLabel: '绩效管理',
      feature: 'sales-performance',
    },
  },
];
