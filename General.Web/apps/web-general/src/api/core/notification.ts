import { requestClient } from '#/api/request';

export namespace NotificationApi {
  export interface NotificationItem {
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

  export interface NotificationListInput {
    maxResultCount?: number;
    onlyUnread?: boolean;
    skipCount?: number;
  }

  export interface UnreadCount {
    count: number;
  }
}

export async function getNotificationListApi(
  params?: NotificationApi.NotificationListInput,
) {
  return requestClient.get<NotificationApi.NotificationItem[]>(
    '/app/platform/notification/list',
    { params },
  );
}

export async function getNotificationUnreadCountApi() {
  return requestClient.get<NotificationApi.UnreadCount>(
    '/app/platform/notification/unread-count',
  );
}

export async function markNotificationReadApi(id: string) {
  return requestClient.post<boolean>(`/app/platform/notification/${id}/read`);
}

export async function markAllNotificationsReadApi() {
  return requestClient.post<boolean>('/app/platform/notification/read-all');
}

export async function removeNotificationApi(id: string) {
  return requestClient.delete<boolean>(`/app/platform/notification/${id}`);
}

export async function clearNotificationsApi() {
  return requestClient.delete<boolean>('/app/platform/notification/clear');
}
