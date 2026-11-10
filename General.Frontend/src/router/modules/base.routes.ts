import type { RouteRecordRaw } from 'vue-router';

export const base_routes: RouteRecordRaw[] = [
  {
    path: 'base/product-bundles',
    name: 'page.base.product-bundles',
    component: () => import('@/features/base/views/ProductBundlesPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '商品组合',
      title: '商品组合',
      module: '基础资料',
      pageLabel: '商品组合',
      feature: 'base',
    },
  },
  {
    path: 'base/bom',
    name: 'page.base.bom',
    component: () => import('@/features/base/views/BomPage.vue'),
    meta: {
      appView: 'feature',
      workspace: 'BOM管理',
      title: 'BOM管理',
      module: '基础资料',
      pageLabel: 'BOM管理',
      feature: 'base',
    },
  },
  {
    path: 'base/material-search',
    name: 'page.base.material-search',
    component: () => import('@/features/base/views/MaterialSearchPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '综合物料搜索',
      title: '综合物料搜索',
      module: '基础资料',
      pageLabel: '综合物料搜索',
      feature: 'base',
    },
  },
  {
    path: 'base/product-trace',
    name: 'page.base.product-trace',
    component: () => import('@/features/base/views/ProductTracePage.vue'),
    meta: {
      appView: 'feature',
      workspace: '产品实例追溯',
      title: '产品实例追溯',
      module: '基础资料',
      pageLabel: '产品实例追溯',
      feature: 'base',
    },
  },
  {
    path: 'base/suppliers',
    name: 'page.base.suppliers',
    component: () => import('@/features/base/views/SuppliersPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '供应商管理',
      title: '供应商管理',
      module: '基础资料',
      pageLabel: '供应商管理',
      feature: 'base',
    },
  },
  {
    path: 'base/warehouses',
    name: 'page.base.warehouses',
    component: () => import('@/features/base/views/WarehousesPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '仓库管理',
      title: '仓库管理',
      module: '基础资料',
      pageLabel: '仓库管理',
      feature: 'base',
    },
  },
];
