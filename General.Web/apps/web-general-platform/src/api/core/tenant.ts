import { requestClient } from '#/api/request';

export namespace TenantApi {
  export interface TenantAuthorization {
    apiBlacklist: string[];
    isActive: boolean;
    menuIds: string[];
  }

  export interface TenantItem {
    adminEmail?: null | string;
    adminUserId?: null | string;
    adminUserName?: null | string;
    apiBlacklist: string[];
    creationTime: string;
    defaultConnectionString?: null | string;
    id: string;
    isActive: boolean;
    name: string;
    remark?: null | string;
  }

  export interface TenantSaveInput {
    defaultConnectionString?: null | string;
    name: string;
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
