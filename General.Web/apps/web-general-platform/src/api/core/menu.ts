import type { RouteRecordStringComponent } from '@vben/types';

import { requestClient } from '#/api/request';

const appCodes = import.meta.env['VITE_GLOB_APP_CODES'];

function createAppCodeParams(appCodesOverride?: null | string) {
  const resolvedAppCodes = appCodesOverride ?? appCodes;
  return resolvedAppCodes ? { appCodes: resolvedAppCodes } : undefined;
}

export namespace MenuApi {
  export interface PermissionTreeItem {
    appCode: string;
    children: PermissionTreeItem[];
    component?: null | string;
    icon?: null | string;
    id: string;
    isEnabled: boolean;
    name: string;
    order: number;
    parentId?: null | string;
    path: string;
    permissionCode?: null | string;
    title: string;
    type: 1 | 2 | 3;
  }

  export interface MenuSaveInput {
    appCode: string;
    component?: null | string;
    icon?: null | string;
    isEnabled: boolean;
    name: string;
    order: number;
    parentId?: null | string;
    path: string;
    permissionCode?: null | string;
    title: string;
    type: 1 | 2 | 3;
  }
}

/**
 * 获取用户所有菜单
 */
export async function getAllMenusApi(appCodesOverride?: null | string) {
  return requestClient.get<RouteRecordStringComponent[]>('/app/menu/all', {
    params: createAppCodeParams(appCodesOverride),
  });
}

export async function getMenuPermissionTreeApi(appCodesOverride?: null | string) {
  return requestClient.get<MenuApi.PermissionTreeItem[]>('/app/menu/tree', {
    params: createAppCodeParams(appCodesOverride),
  });
}

export async function getRoleMenuIdsApi(roleId: string) {
  return requestClient.get<string[]>(`/app/menu/role/${roleId}`);
}

export async function saveRoleMenusApi(roleId: string, menuIds: string[]) {
  return requestClient.post<boolean>(`/app/menu/role/${roleId}`, { menuIds });
}

export async function createMenuApi(data: MenuApi.MenuSaveInput) {
  return requestClient.post<boolean>('/app/menu', data);
}

export async function updateMenuApi(id: string, data: MenuApi.MenuSaveInput) {
  return requestClient.put<boolean>(`/app/menu/${id}`, data);
}

export async function deleteMenuApi(id: string) {
  return requestClient.delete<boolean>(`/app/menu/${id}`);
}
