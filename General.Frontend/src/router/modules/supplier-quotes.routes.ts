import type { RouteRecordRaw } from 'vue-router';

export const supplier_quotes_routes: RouteRecordRaw[] = [
  {
    path: 'supplier-quotes/supplier-quotes',
    name: 'page.supplier-quotes.supplier-quotes',
    component: () => import('@/features/supplier-quotes/views/SupplierQuotesPage.vue'),
    meta: {
      appView: 'feature',
      workspace: '供应商报价',
      title: '供应商报价',
      module: '供应商报价',
      pageLabel: '供应商报价',
      feature: 'supplier-quotes',
    },
  },
];
