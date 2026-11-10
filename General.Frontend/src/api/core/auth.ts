import { apiClient } from '@/api/request';

export namespace AuthApi {
  export interface LoginInput {
    password: string;
    username: string;
  }

  export interface LoginResult {
    accessToken: string;
    homePath: string;
    isHostTenantOperation: boolean;
    operationTenantId: null | string;
    operationTenantName: null | string;
    realName: string;
    roles: string[];
    username: string;
  }

  export interface CurrentUserInfo {
    avatar: null | string;
    desc: string;
    homePath: string;
    id: string;
    isHostTenantOperation: boolean;
    lastLoginTime: null | string;
    operationTenantId: null | string;
    operationTenantName: null | string;
    organizationUnitNames: string[];
    realName: string;
    roles: string[];
    token?: string;
    username: string;
  }

  export interface PasswordChangeInput {
    currentPassword: string;
    newPassword: string;
  }
}

export type SafeCurrentUserInfo = Omit<AuthApi.CurrentUserInfo, 'token'>;

export function loginApi(input: AuthApi.LoginInput): Promise<AuthApi.LoginResult> {
  return apiClient.request('app/auth/login', {
    anonymous: true,
    body: { ...input, username: input.username.trim() },
    method: 'POST',
  });
}

export function getCurrentUserInfoApi(): Promise<AuthApi.CurrentUserInfo> {
  return apiClient.request('app/user/info');
}

export function getAccessCodesApi(): Promise<string[]> {
  return apiClient.request('app/auth/codes');
}

export function logoutApi(): Promise<boolean> {
  return apiClient.request('app/auth/logout', { method: 'POST' });
}

export function enterTenantOperationApi(tenantId: string): Promise<AuthApi.LoginResult> {
  return apiClient.request('app/auth/tenant-operation/enter', {
    body: { tenantId },
    method: 'POST',
  });
}

export function exitTenantOperationApi(): Promise<AuthApi.LoginResult> {
  return apiClient.request('app/auth/tenant-operation/exit', { method: 'POST' });
}

export function changeCurrentPasswordApi(input: AuthApi.PasswordChangeInput): Promise<boolean> {
  return apiClient.request('app/user/password', { method: 'PUT', body: input });
}
