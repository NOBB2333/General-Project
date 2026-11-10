import { apiClient } from '@/api/request';

export namespace RoleApi {
  export interface Item {
    accountScopeMode: string;
    accountUserIds: string[];
    apiBlacklist: string[];
    customOrganizationUnitIds: string[];
    dataScopeMode: string;
    description: string;
    homePath: string;
    id: string;
    menuAuthorizationCount: number;
    menuCount: number;
    name: string;
    status: boolean;
    userCount: number;
  }

  export interface Option {
    description: string;
    id: string;
    name: string;
  }

  export interface CreateInput {
    name: string;
  }

  export interface Authorization {
    accountScopeMode: string;
    accountUserIds: string[];
    apiBlacklist: string[];
    customOrganizationUnitIds: string[];
    dataScopeMode: string;
    menuIds: string[];
  }

  export interface AuthorizationSaveInput {
    accountScopeMode: string;
    accountUserIds: string[];
    apiBlacklist: string[];
    customOrganizationUnitIds: string[];
    dataScopeMode: string;
  }
}

export function getRoleListApi(): Promise<RoleApi.Item[]> {
  return apiClient.request('/app/role/list');
}

export function getRoleOptionsApi(): Promise<RoleApi.Option[]> {
  return apiClient.request('/app/role/options');
}

export function createRoleApi(input: RoleApi.CreateInput): Promise<boolean> {
  return apiClient.request('/app/role', { body: input, method: 'POST' });
}

export function deleteRoleApi(id: string): Promise<boolean> {
  return apiClient.request(`/app/role/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function getRoleAuthorizationApi(id: string): Promise<RoleApi.Authorization> {
  return apiClient.request(`/app/role/${encodeURIComponent(id)}/authorization`);
}

export function saveRoleAuthorizationApi(id: string, input: RoleApi.AuthorizationSaveInput): Promise<boolean> {
  return apiClient.request(`/app/role/${encodeURIComponent(id)}/authorization`, { body: input, method: 'PUT' });
}
