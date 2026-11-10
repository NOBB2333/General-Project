import type { ApiRequestOptions } from '@/api/client';

export namespace PlatformNotificationApi {
  export interface Item {
    avatar: string;
    avatarText: string;
    creationTime: string;
    date: string;
    id: string;
    isRead: boolean;
    level: string;
    link: string;
    message: string;
    title: string;
    type: string;
  }

  export interface ListInput {
    maxResultCount: number;
    onlyUnread: boolean;
    skipCount: number;
  }

  export interface UnreadCount {
    count: number;
  }
}

export interface PlatformNotificationsApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createPlatformNotificationsApi(client: PlatformNotificationsApiClient) {
  const basePath = '/app/platform/notification';

  return {
    getList(input: PlatformNotificationApi.ListInput, signal?: AbortSignal): Promise<PlatformNotificationApi.Item[]> {
      return client.request<PlatformNotificationApi.Item[]>(`${basePath}/list`, {
        query: {
          maxResultCount: input.maxResultCount,
          onlyUnread: input.onlyUnread,
          skipCount: input.skipCount,
        },
        signal,
      });
    },
    getUnreadCount(signal?: AbortSignal): Promise<PlatformNotificationApi.UnreadCount> {
      return client.request<PlatformNotificationApi.UnreadCount>(`${basePath}/unread-count`, { signal });
    },
    markRead(id: string, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${basePath}/${encodeURIComponent(id)}/read`, { method: 'POST', signal });
    },
    markAllRead(signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${basePath}/read-all`, { method: 'POST', signal });
    },
    remove(id: string, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${basePath}/${encodeURIComponent(id)}`, { method: 'DELETE', signal });
    },
  };
}
