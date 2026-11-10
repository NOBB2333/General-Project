import { apiClient } from '@/api/request';

export namespace MenuApi {
  export interface PermissionTreeItem {
    affixTab: boolean;
    appCode: string;
    children: PermissionTreeItem[];
    component: null | string;
    hideInBreadcrumb: boolean;
    hideInMenu: boolean;
    hideInTab: boolean;
    icon: null | string;
    id: string;
    isEnabled: boolean;
    keepAlive: boolean;
    link: null | string;
    menuVisibleWithForbidden: boolean;
    name: string;
    order: number;
    parentId: null | string;
    path: string;
    permissionCode: null | string;
    redirect: null | string;
    title: string;
    type: 1 | 2 | 3;
  }

  export interface SaveInput {
    affixTab: boolean;
    appCode: string;
    component: null | string;
    hideInBreadcrumb: boolean;
    hideInMenu: boolean;
    hideInTab: boolean;
    icon: null | string;
    isEnabled: boolean;
    keepAlive: boolean;
    link: null | string;
    menuVisibleWithForbidden: boolean;
    name: string;
    order: number;
    parentId: null | string;
    path: string;
    permissionCode: null | string;
    redirect: null | string;
    title: string;
    type: 1 | 2 | 3;
  }

  export interface RoleMenuGrantInput {
    menuIds: string[];
  }

  export interface EndpointOption {
    capabilityKey: null | string;
    key: string;
    label: string;
  }

  export interface EndpointGroup {
    groupName: string;
    items: EndpointOption[];
  }

  export interface RouteMeta {
    affixTab?: boolean | null;
    hideInBreadcrumb?: boolean | null;
    hideInMenu?: boolean | null;
    hideInTab?: boolean | null;
    icon?: null | string;
    keepAlive?: boolean | null;
    link?: null | string;
    menuVisibleWithForbidden?: boolean | null;
    order?: null | number;
    title: string;
  }

  export interface BackendRoute {
    children: BackendRoute[];
    component: null | string;
    meta: RouteMeta;
    name: string;
    path: string;
    redirect: null | string;
  }
}

export function getCurrentMenusApi(
  appCodes: readonly string[] = ['platform', 'integration', 'project', 'business', 'ai'],
): Promise<MenuApi.BackendRoute[]> {
  return apiClient.request('/app/menu/all', {
    query: { appCodes: appCodes.join(',') },
  });
}

export function getMenuPermissionTreeApi(appCodes?: readonly string[]): Promise<MenuApi.PermissionTreeItem[]> {
  return apiClient.request('/app/menu/tree', {
    query: appCodes?.length ? { appCodes: appCodes.join(',') } : undefined,
  });
}

export function createMenuApi(input: MenuApi.SaveInput): Promise<boolean> {
  return apiClient.request('/app/menu', { body: input, method: 'POST' });
}

export function updateMenuApi(id: string, input: MenuApi.SaveInput): Promise<boolean> {
  return apiClient.request(`/app/menu/${encodeURIComponent(id)}`, { body: input, method: 'PUT' });
}

export function setMenuEnabledApi(id: string, isEnabled: boolean): Promise<boolean> {
  return apiClient.request(`/app/menu/${encodeURIComponent(id)}/enabled`, {
    method: 'PUT',
    query: { isEnabled },
  });
}

export function deleteMenuApi(id: string): Promise<boolean> {
  return apiClient.request(`/app/menu/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function getRoleMenuIdsApi(roleId: string): Promise<string[]> {
  return apiClient.request(`/app/menu/role/${encodeURIComponent(roleId)}`);
}

export function saveRoleMenusApi(roleId: string, input: MenuApi.RoleMenuGrantInput): Promise<boolean> {
  return apiClient.request(`/app/menu/role/${encodeURIComponent(roleId)}`, { body: input, method: 'POST' });
}

export function getPlatformEndpointOptionsApi(): Promise<MenuApi.EndpointGroup[]> {
  return apiClient.request('/app/platform/endpoint/options');
}
