import { requestClient } from '#/api/request';

export namespace RoleApi {
  export interface RoleItem {
    apiBlacklist: string[];
    accountScopeMode: string;
    accountUserIds: string[];
    customOrganizationUnitIds: string[];
    dataScopeMode: string;
    description: string;
    homePath: string;
    id: string;
    menuCount: number;
    name: string;
    status: boolean;
    userCount: number;
  }

  export interface RoleAuthorization {
    apiBlacklist: string[];
    accountScopeMode: string;
    accountUserIds: string[];
    customOrganizationUnitIds: string[];
    dataScopeMode: string;
    menuIds: string[];
  }

  export interface RoleAuthorizationSaveInput {
    apiBlacklist: string[];
    accountScopeMode: string;
    accountUserIds: string[];
    customOrganizationUnitIds: string[];
    dataScopeMode: string;
  }

  export interface RoleSaveInput {
    name: string;
  }
}

export async function getRoleListApi() {
  return requestClient.get<RoleApi.RoleItem[]>('/app/role/list');
}

export async function createRoleApi(data: RoleApi.RoleSaveInput) {
  return requestClient.post<boolean>('/app/role', data);
}

export async function deleteRoleApi(id: string) {
  return requestClient.delete<boolean>(`/app/role/${id}`);
}

export async function getRoleAuthorizationApi(id: string) {
  return requestClient.get<RoleApi.RoleAuthorization>(`/app/role/${id}/authorization`);
}

export async function saveRoleAuthorizationApi(
  id: string,
  data: RoleApi.RoleAuthorizationSaveInput,
) {
  return requestClient.put<boolean>(`/app/role/${id}/authorization`, data);
}
