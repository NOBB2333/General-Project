import type { createApiClient } from '../client';

export namespace OnlineUserApi {
  export interface Item {
    browser: string;
    canForceLogout: boolean;
    currentTenantId: null | string;
    device: string;
    id: string;
    ipAddress: string;
    lastAccessedAt: null | string;
    signedInAt: null | string;
    tenantName: null | string;
    userId: string;
    userName: string;
  }
}

export function createOnlineUserApi(client: ReturnType<typeof createApiClient>) {
  return {
    getList: (signal?: AbortSignal) => client.request<OnlineUserApi.Item[]>('/app/online-user/list', { signal }),
    forceLogout: (userId: string, signal?: AbortSignal) => {
      if (!/^[\da-f]{8}(?:-[\da-f]{4}){3}-[\da-f]{12}$/iu.test(userId)) throw new Error('请选择有效账号。');
      return client.request<boolean>(`/app/online-user/${encodeURIComponent(userId)}/force-logout`, {
        method: 'POST',
        signal,
      });
    },
  };
}
