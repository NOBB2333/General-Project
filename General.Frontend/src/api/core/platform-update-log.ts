import type { ApiRequestOptions } from '@/api/client';

export namespace PlatformUpdateLogApi {
  export interface Item {
    authorName: string;
    creationTime: string;
    id: string;
    impactScope: null | string;
    publishedAt: string;
    summary: string;
    title: string;
    version: string;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface SaveInput {
    impactScope: null | string;
    publishedAt: string;
    summary: string;
    title: string;
    version: string;
  }
}

export interface PlatformUpdateLogApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createPlatformUpdateLogApi(client: PlatformUpdateLogApiClient) {
  const rootPath = '/app/update-log';

  return {
    create(input: PlatformUpdateLogApi.SaveInput, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(rootPath, { body: input, method: 'POST', signal });
    },
    delete(id: string, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${rootPath}/${encodeURIComponent(id)}`, { method: 'DELETE', signal });
    },
    getPaged(
      input: { keyword?: string; maxResultCount: number; skipCount: number },
      signal?: AbortSignal,
    ): Promise<PlatformUpdateLogApi.PagedResult<PlatformUpdateLogApi.Item>> {
      return client.request<PlatformUpdateLogApi.PagedResult<PlatformUpdateLogApi.Item>>(`${rootPath}/paged`, {
        query: {
          keyword: input.keyword,
          maxResultCount: input.maxResultCount,
          skipCount: input.skipCount,
        },
        signal,
      });
    },
    update(id: string, input: PlatformUpdateLogApi.SaveInput, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${rootPath}/${encodeURIComponent(id)}`, { body: input, method: 'PUT', signal });
    },
  };
}
