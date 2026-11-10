import type { ApiRequestOptions } from '@/api/client';

export namespace PlatformRecycleBinApi {
  export interface Item {
    deletionTime: null | string;
    deleterId: null | string;
    deleterName: null | string;
    displayName: string;
    entityType: string;
    id: string;
    originalLocation: string;
  }

  export interface ListInput {
    entityType?: string;
    maxResultCount: number;
    skipCount: number;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }
}

export interface PlatformRecycleBinApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createPlatformRecycleBinApi(client: PlatformRecycleBinApiClient) {
  const rootPath = '/app/platform/recycle-bin';

  return {
    deletePermanently(entityType: string, id: string, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${rootPath}/${encodeURIComponent(entityType)}/${encodeURIComponent(id)}`, {
        method: 'DELETE',
        signal,
      });
    },
    getItems(
      input: PlatformRecycleBinApi.ListInput,
      signal?: AbortSignal,
    ): Promise<PlatformRecycleBinApi.PagedResult<PlatformRecycleBinApi.Item>> {
      return client.request<PlatformRecycleBinApi.PagedResult<PlatformRecycleBinApi.Item>>(`${rootPath}/items`, {
        query: {
          entityType: input.entityType,
          maxResultCount: input.maxResultCount,
          skipCount: input.skipCount,
        },
        signal,
      });
    },
    restore(entityType: string, id: string, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(
        `${rootPath}/${encodeURIComponent(entityType)}/${encodeURIComponent(id)}/restore`,
        { method: 'POST', signal },
      );
    },
  };
}
