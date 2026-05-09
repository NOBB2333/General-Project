import { baseRequestClient, requestClient } from '#/api/request';

export namespace AuthApi {
  /** 登录接口参数 */
  export interface LoginParams {
    password?: string;
    username?: string;
  }

  /** 登录接口返回值 */
  export interface LoginResult {
    accessToken: string;
    isHostTenantOperation?: boolean;
    operationTenantId?: null | string;
    operationTenantName?: null | string;
  }

  export interface RefreshTokenResult {
    data: string;
    status: number;
  }
}

/**
 * 登录
 */
export async function loginApi(data: AuthApi.LoginParams) {
  return requestClient.post<AuthApi.LoginResult>('/app/auth/login', data);
}

export async function enterTenantOperationApi(tenantId: string) {
  return requestClient.post<AuthApi.LoginResult>('/app/auth/tenant-operation/enter', {
    tenantId,
  });
}

export async function exitTenantOperationApi() {
  return requestClient.post<AuthApi.LoginResult>('/app/auth/tenant-operation/exit');
}

/**
 * 刷新accessToken
 */
export async function refreshTokenApi() {
  return baseRequestClient.post<AuthApi.RefreshTokenResult>('/app/auth/refresh', {
    withCredentials: true,
  });
}

/**
 * 退出登录
 */
export async function logoutApi() {
  return baseRequestClient.post('/app/auth/logout', {
    withCredentials: true,
  });
}

/**
 * 获取用户权限码
 */
export async function getAccessCodesApi() {
  return requestClient.get<string[]>('/app/auth/codes');
}
