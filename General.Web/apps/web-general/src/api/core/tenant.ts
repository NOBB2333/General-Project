import { requestClient } from '#/api/request';

export namespace TenantApi {
  export interface TenantAuthorization {
    adminUserId?: null | string;
    apiBlacklist: string[];
    isActive: boolean;
    menuIds: string[];
    remark?: null | string;
  }

  export interface TenantItem {
    adminEmail?: null | string;
    adminUserId?: null | string;
    adminUserName?: null | string;
    apiBlacklist: string[];
    creationTime: string;
    defaultConnectionStringDisplay?: null | string;
    hasExplicitAuthorization: boolean;
    hasDefaultConnectionString: boolean;
    id: string;
    isActive: boolean;
    name: string;
    remark?: null | string;
  }

  export interface TenantSaveInput {
    adminUserId?: null | string;
    defaultConnectionString?: null | string;
    name: string;
    remark?: null | string;
  }

  export interface TenantUserItem {
    displayName: string;
    email: string;
    id: string;
    isActive: boolean;
    username: string;
  }
}

export async function getTenantListApi() {
  return requestClient.get<TenantApi.TenantItem[]>('/app/tenant/list');
}

export async function createTenantApi(data: TenantApi.TenantSaveInput) {
  return requestClient.post<boolean>('/app/tenant', data);
}

export async function deleteTenantApi(id: string) {
  return requestClient.delete<boolean>(`/app/tenant/${id}`);
}

export async function getTenantAuthorizationApi(id: string) {
  return requestClient.get<TenantApi.TenantAuthorization>(`/app/tenant/${id}/authorization`);
}

export async function saveTenantAuthorizationApi(
  id: string,
  data: TenantApi.TenantAuthorization,
) {
  return requestClient.put<boolean>(`/app/tenant/${id}/authorization`, data);
}

export async function setTenantStatusApi(id: string, isActive: boolean) {
  return requestClient.put<boolean>(`/app/tenant/${id}/status`, undefined, {
    params: { isActive },
  });
}

export async function getTenantUsersApi(id: string) {
  return requestClient.get<TenantApi.TenantUserItem[]>(`/app/tenant/${id}/users`);
}
