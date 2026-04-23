import { requestClient } from '#/api/request';

export namespace OnlineUserApi {
  export interface OnlineUserItem {
    browser: string;
    canForceLogout: boolean;
    currentTenantId?: null | string;
    device: string;
    id: string;
    ipAddress: string;
    lastAccessedAt?: null | string;
    tenantName?: null | string;
    userId: string;
    userName: string;
  }
}

export async function getOnlineUserListApi() {
  return requestClient.get<OnlineUserApi.OnlineUserItem[]>('/app/online-user/list');
}

export async function forceLogoutOnlineUserApi(userId: string) {
  return requestClient.post<boolean>(`/app/online-user/${userId}/force-logout`);
}
