import type { ApiRequestOptions } from '@/api/client';

export namespace PlatformOpenApiApi {
  export interface Application {
    appId: string;
    creationTime: string;
    id: string;
    isEnabled: boolean;
    lastModificationTime: null | string;
    name: string;
    remark: string;
    scopes: string[];
  }

  export interface ApplicationSecret extends Application {
    appSecret: string;
  }

  export interface ApplicationSaveInput {
    isEnabled: boolean;
    name: string;
    remark: string;
    scopes: string[];
  }

  export interface PagedQuery {
    keyword?: string;
    maxResultCount: number;
    skipCount: number;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }
}

interface PlatformOpenApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

const basePath = '/app/platform/open-api/applications';
const encodeId = (id: string) => encodeURIComponent(id);

export function createPlatformOpenApiApi(client: PlatformOpenApiClient) {
  return {
    getApplications(
      query: PlatformOpenApiApi.PagedQuery,
      signal?: AbortSignal,
    ): Promise<PlatformOpenApiApi.PagedResult<PlatformOpenApiApi.Application>> {
      return client.request(`${basePath}/paged`, {
        query: {
          keyword: query.keyword,
          maxResultCount: query.maxResultCount,
          skipCount: query.skipCount,
        },
        signal,
      });
    },
    createApplication(
      input: PlatformOpenApiApi.ApplicationSaveInput,
      signal?: AbortSignal,
    ): Promise<PlatformOpenApiApi.ApplicationSecret> {
      return client.request(basePath, { body: input, method: 'POST', signal });
    },
    updateApplication(
      id: string,
      input: PlatformOpenApiApi.ApplicationSaveInput,
      signal?: AbortSignal,
    ): Promise<PlatformOpenApiApi.Application> {
      return client.request(`${basePath}/${encodeId(id)}`, { body: input, method: 'PUT', signal });
    },
    resetApplicationSecret(id: string, signal?: AbortSignal): Promise<PlatformOpenApiApi.ApplicationSecret> {
      return client.request(`${basePath}/${encodeId(id)}/reset-secret`, { method: 'POST', signal });
    },
    deleteApplication(id: string, signal?: AbortSignal): Promise<boolean> {
      return client.request(`${basePath}/${encodeId(id)}`, { method: 'DELETE', signal });
    },
  };
}
