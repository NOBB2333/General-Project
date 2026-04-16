import type { UserInfo } from '@vben/types';

import { requestClient } from '#/api/request';

export namespace UserApi {
  export interface UserSaveInput {
    displayName: string;
    email: string;
    isActive: boolean;
    organizationUnitId?: null | string;
    password?: string;
    roleNames: string[];
    username: string;
  }

  export interface UserListInput {
    keyword?: string;
    organizationUnitId?: string;
  }

  export interface UserListItem {
    displayName: string;
    email: string;
    id: string;
    isActive: boolean;
    organizationUnitNames: string[];
    roles: string[];
    username: string;
  }
}

/**
 * 获取用户信息
 */
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
