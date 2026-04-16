import { requestClient } from '#/api/request';

export namespace TenantApi {
  export interface TenantItem {
    defaultConnectionString?: null | string;
    id: string;
    name: string;
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
