import type { UserInfo } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace UserApi {
  export interface ExternalAccountMapping {
    boundAt: string;
    externalSource: string;
    externalUserId: string;
    id: string;
    lastSyncedAt?: null | string;
    remark?: null | string;
    status: string;
    userId: string;
  }

  export interface PasswordChangeInput {
    currentPassword: string;
    newPassword: string;
  }

  export interface UserSaveInput {
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

  export interface UserListInput {
    isActive?: boolean;
    keyword?: string;
    organizationUnitId?: string;
    roleName?: string;
  }

  export interface UserListItem {
    displayName: string;
    email: string;
    employeeNo?: null | string;
    externalAccounts: ExternalAccountMapping[];
    externalSource?: null | string;
    externalUserId?: null | string;
    id: string;
    isActive: boolean;
    isOnline: boolean;
    lastLoginTime?: null | string;
    organizationUnitNames: string[];
    phoneNumber?: null | string;
    roles: string[];
    tenantName?: null | string;
    username: string;
  }
}

export async function getUserInfoApi() {
  return requestClient.get<UserInfo>('/app/user/info');
}

export async function getUserListApi(params: UserApi.UserListInput = {}) {
  return requestClient.get<UserApi.UserListItem[]>('/app/user/list', { params });
}

export async function createUserApi(data: UserApi.UserSaveInput) {
  return requestClient.post<boolean>('/app/user', data);
}

export async function updateUserApi(id: string, data: UserApi.UserSaveInput) {
  return requestClient.put<boolean>(`/app/user/${id}`, data);
}

export async function deleteUserApi(id: string) {
  return requestClient.delete<boolean>(`/app/user/${id}`);
}

export async function changePasswordApi(data: UserApi.PasswordChangeInput) {
  return requestClient.put<boolean>('/app/user/password', data);
}
