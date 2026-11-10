import type { ApiRequestOptions } from '@/api/client';

export namespace PlatformFileStorageSourceApi {
  export type ProviderName = 'AliyunOSS' | 'Local' | 'MinIO';

  export interface Item {
    accessKeyId: string;
    bucketName: null | string;
    customDomain: null | string;
    endpoint: null | string;
    id: string;
    isDefault: boolean;
    isEnabled: boolean;
    isPublic: boolean;
    name: string;
    pathTemplate: null | string;
    providerName: string;
    region: null | string;
    remark: null | string;
    rootPath: null | string;
    useSsl: boolean;
  }

  export interface SaveInput {
    accessKeyId: string;
    bucketName: null | string;
    customDomain: null | string;
    endpoint: null | string;
    isDefault: boolean;
    isEnabled: boolean;
    isPublic: boolean;
    name: string;
    pathTemplate: null | string;
    providerName: ProviderName;
    region: null | string;
    remark: null | string;
    rootPath: null | string;
    secretKey: string;
    useSsl: boolean;
  }
}

export interface PlatformFileStorageSourceApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createPlatformFileStorageSourcesApi(client: PlatformFileStorageSourceApiClient) {
  const rootPath = '/app/platform/file-storage-sources';

  return {
    create(
      input: PlatformFileStorageSourceApi.SaveInput,
      signal?: AbortSignal,
    ): Promise<PlatformFileStorageSourceApi.Item> {
      return client.request<PlatformFileStorageSourceApi.Item>(rootPath, { body: input, method: 'POST', signal });
    },
    delete(id: string, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${rootPath}/${encodeURIComponent(id)}`, { method: 'DELETE', signal });
    },
    getList(enabledOnly = false, signal?: AbortSignal): Promise<PlatformFileStorageSourceApi.Item[]> {
      return client.request<PlatformFileStorageSourceApi.Item[]>(rootPath, {
        query: { enabledOnly },
        signal,
      });
    },
    setDefault(id: string, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${rootPath}/${encodeURIComponent(id)}/default`, { method: 'POST', signal });
    },
    toggle(id: string, isEnabled: boolean, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`${rootPath}/${encodeURIComponent(id)}/toggle`, {
        body: { isEnabled },
        method: 'POST',
        signal,
      });
    },
    update(
      id: string,
      input: PlatformFileStorageSourceApi.SaveInput,
      signal?: AbortSignal,
    ): Promise<PlatformFileStorageSourceApi.Item> {
      return client.request<PlatformFileStorageSourceApi.Item>(`${rootPath}/${encodeURIComponent(id)}`, {
        body: input,
        method: 'PUT',
        signal,
      });
    },
  };
}
