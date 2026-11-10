import type { ApiRequestOptions } from '@/api/client';

export namespace PlatformCacheApi {
  export interface Area {
    area: string;
    description: string;
    version: string;
  }

  export interface RefreshInput {
    area: string;
  }
}

interface PlatformCacheClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createPlatformCacheApi(client: PlatformCacheClient) {
  return {
    getAreas(signal?: AbortSignal): Promise<PlatformCacheApi.Area[]> {
      return client.request('/app/platform/cache/areas', { signal });
    },
    refresh(area: string, signal?: AbortSignal): Promise<boolean> {
      const input: PlatformCacheApi.RefreshInput = { area };
      return client.request('/app/platform/cache/refresh', { body: input, method: 'POST', signal });
    },
  };
}
