import { requestClient } from '#/api/request';

export namespace CacheApi {
  export interface CacheAreaItem {
    area: string;
    description: string;
    version: string;
  }
}

export async function getPlatformCacheAreasApi() {
  return requestClient.get<CacheApi.CacheAreaItem[]>('/app/platform/cache/areas');
}

export async function refreshPlatformCacheAreaApi(area: string) {
  return requestClient.post<boolean>('/app/platform/cache/refresh', { area });
}
