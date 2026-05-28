import { inject, type InjectionKey } from 'vue';

import type { useProjectDetailPage } from './composables/useProjectDetailPage';

export type ProjectDetailContext = ReturnType<typeof useProjectDetailPage>;

export const projectDetailContextKey: InjectionKey<ProjectDetailContext> = Symbol('ProjectDetailContext');

export function useProjectDetailContext() {
  const context = inject(projectDetailContextKey);
  if (!context) {
    throw new Error('Project detail context is not available.');
  }

  return context;
}
