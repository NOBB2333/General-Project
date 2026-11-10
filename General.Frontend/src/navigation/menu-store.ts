import { reactive } from 'vue';
import { adaptBackendMenuTree, getResolvedPages, resolveAuthorizedHomePath } from './page-registry.ts';
import type { MenuApi } from '@/api/core/menu';
import type { ResolvedMenuNode } from './page-registry.ts';

export type NavigationStatus = 'empty' | 'ready';

export const navigationState = reactive({
  items: [] as ResolvedMenuNode[],
  pages: [] as ResolvedMenuNode[],
  status: 'empty' as NavigationStatus,
});

export function setCurrentNavigation(routes: readonly MenuApi.BackendRoute[], grantedCodes: readonly string[]): void {
  const items = adaptBackendMenuTree(routes, grantedCodes);
  navigationState.items = items;
  navigationState.pages = getResolvedPages(items);
  navigationState.status = 'ready';
}

export function clearCurrentNavigation(): void {
  navigationState.items = [];
  navigationState.pages = [];
  navigationState.status = 'empty';
}

export function isRouteAuthorized(routeName: string): boolean {
  return navigationState.pages.some((page) => page.routeName === routeName);
}

export function getAuthorizedPageByPath(path: string): ResolvedMenuNode | undefined {
  return navigationState.pages.find((page) => page.localPath === path);
}

export function getAuthorizedHomePath(requestedPath: string): null | string {
  return resolveAuthorizedHomePath(requestedPath, navigationState.pages);
}
