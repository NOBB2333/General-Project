import { apiClient } from '@/api/request';

export namespace UserApi {
  export interface ExternalAccountMapping {
    boundAt: string;
    externalSource: string;
    externalUserId: string;
    id: string;
    lastSyncedAt: null | string;
    remark: null | string;
    status: string;
    userId: string;
  }

  export interface ListInput {
    displayName?: string;
    email?: string;
    employeeNo?: string;
    isActive?: boolean;
    keyword?: string;
    maxResultCount: number;
    organizationUnitId?: string;
    phoneNumber?: string;
    roleName?: string;
    skipCount: number;
    userName?: string;
  }

  export interface ListItem {
    displayName: string;
    email: string;
    employeeNo: null | string;
    externalAccounts: ExternalAccountMapping[];
    externalSource: null | string;
    externalUserId: null | string;
    id: string;
    isActive: boolean;
    isOnline: boolean;
    lastLoginTime: null | string;
    organizationUnitNames: string[];
    phoneNumber: null | string;
    roles: string[];
    tenantName: null | string;
    username: string;
  }

  export interface SaveInput {
    displayName: string;
    email: string;
    employeeNo?: null | string;
    externalSource?: null | string;
    externalUserId?: null | string;
    isActive: boolean;
    organizationUnitId?: null | string;
    password?: string;
    phoneNumber?: null | string;
    roleNames: string[];
    username: string;
  }

  export interface ResetPasswordInput {
    newPassword: string;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }
}

export function getUserListApi(input: UserApi.ListInput): Promise<UserApi.PagedResult<UserApi.ListItem>> {
  return apiClient.request('/app/user/list', { query: { ...input } });
}

export function createUserApi(input: UserApi.SaveInput): Promise<boolean> {
  return apiClient.request('/app/user', { body: input, method: 'POST' });
}

export function updateUserApi(id: string, input: UserApi.SaveInput): Promise<boolean> {
  return apiClient.request(`/app/user/${encodeURIComponent(id)}`, { body: input, method: 'PUT' });
}

export function deleteUserApi(id: string): Promise<boolean> {
  return apiClient.request(`/app/user/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function resetUserPasswordApi(id: string, input: UserApi.ResetPasswordInput): Promise<boolean> {
  return apiClient.request(`/app/user/${encodeURIComponent(id)}/reset-password`, { body: input, method: 'PUT' });
}
