import { inject, type InjectionKey, type Ref } from 'vue';

import type { BusinessApi } from '#/api/core';

import type { useBusinessProjectsPage } from './composables/useBusinessProjectsPage';

export type BusinessProjectsProviderContext = ReturnType<
  typeof useBusinessProjectsPage
> & {
  budgetColumns: any[];
  businessColumns: any[];
  contractColumns: any[];
  detailTableScroll: { x: 'max-content' };
  historyColumns: any[];
  procurementColumns: any[];
  receivableColumns: any[];
};

export type BusinessProjectsContext = Omit<
  BusinessProjectsProviderContext,
  'detail'
> & {
  detail: Ref<BusinessApi.ProjectDetail>;
  budgetColumns: any[];
  businessColumns: any[];
  contractColumns: any[];
  detailTableScroll: { x: 'max-content' };
  historyColumns: any[];
  procurementColumns: any[];
  receivableColumns: any[];
};

export const businessProjectsContextKey: InjectionKey<BusinessProjectsProviderContext> =
  Symbol('BusinessProjectsContext');

export function useBusinessProjectsContext() {
  const context = inject(businessProjectsContextKey);
  if (!context) {
    throw new Error('Business projects context is not available.');
  }

  return context as unknown as BusinessProjectsContext;
}
