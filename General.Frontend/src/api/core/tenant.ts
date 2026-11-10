import { apiClient } from '@/api/request';

export namespace TenantApi {
  export interface ListItem {
    adminEmail: null | string;
    adminUserId: null | string;
    adminUserName: null | string;
    apiBlacklist: string[];
    creationTime: string;
    defaultConnectionStringDisplay: null | string;
    hasDefaultConnectionString: boolean;
    hasExplicitAuthorization: boolean;
    id: string;
    isActive: boolean;
    name: string;
    remark: null | string;
  }

  export interface PagedQuery {
    maxResultCount: number;
    skipCount: number;
  }

  export interface PagedUserQuery extends PagedQuery {
    keyword?: string;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface Authorization {
    adminUserId: null | string;
    apiBlacklist: string[];
    isActive: boolean;
    menuIds: string[];
    remark: null | string;
  }

  export interface SaveInput {
    adminEmail?: null | string;
    adminPassword?: null | string;
    adminUserId?: null | string;
    adminUserName?: null | string;
    defaultConnectionString?: null | string;
    name: string;
    remark?: null | string;
  }

  export interface UserItem {
    displayName: string;
    email: string;
    id: string;
    isActive: boolean;
    username: string;
  }
}

export function getTenantListApi(): Promise<TenantApi.ListItem[]> {
  return apiClient.request('/app/tenant/list');
}

export function getTenantPagedApi(input: TenantApi.PagedQuery): Promise<TenantApi.PagedResult<TenantApi.ListItem>> {
  return apiClient.request('/app/tenant/paged', {
    query: { maxResultCount: input.maxResultCount, skipCount: input.skipCount },
  });
}

export function createTenantApi(input: TenantApi.SaveInput): Promise<boolean> {
  return apiClient.request('/app/tenant', { body: input, method: 'POST' });
}

export function updateTenantApi(id: string, input: TenantApi.SaveInput): Promise<boolean> {
  return apiClient.request(`/app/tenant/${encodeURIComponent(id)}`, { body: input, method: 'PUT' });
}

export function deleteTenantApi(id: string): Promise<boolean> {
  return apiClient.request(`/app/tenant/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function getTenantAuthorizationApi(id: string): Promise<TenantApi.Authorization> {
  return apiClient.request(`/app/tenant/${encodeURIComponent(id)}/authorization`);
}

export function saveTenantAuthorizationApi(id: string, input: TenantApi.Authorization): Promise<boolean> {
  return apiClient.request(`/app/tenant/${encodeURIComponent(id)}/authorization`, {
    body: input,
    method: 'PUT',
  });
}

export function setTenantStatusApi(id: string, isActive: boolean): Promise<boolean> {
  return apiClient.request(`/app/tenant/${encodeURIComponent(id)}/status`, {
    method: 'PUT',
    query: { isActive },
  });
}

export function getTenantUsersApi(id: string): Promise<TenantApi.UserItem[]> {
  return apiClient.request(`/app/tenant/${encodeURIComponent(id)}/users`);
}

export function getTenantUsersPagedApi(
  id: string,
  input: TenantApi.PagedUserQuery,
): Promise<TenantApi.PagedResult<TenantApi.UserItem>> {
  return apiClient.request(`/app/tenant/${encodeURIComponent(id)}/users/paged`, {
    query: {
      keyword: input.keyword,
      maxResultCount: input.maxResultCount,
      skipCount: input.skipCount,
    },
  });
}
