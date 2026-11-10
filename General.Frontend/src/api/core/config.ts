import type { ApiRequestOptions } from '@/api/client';

export namespace ConfigApi {
  export interface Item {
    code: string;
    defaultValue: string;
    description: string;
    groupCode: string;
    groupName: string;
    hasTenantValue: boolean;
    isReadonly: boolean;
    name: string;
    providerKey: string | null;
    providerName: string;
    value: string;
    valueType: string;
  }

  export interface SaveInput {
    value: string;
    providerName: 'G' | 'T';
    providerKey?: string | null;
  }
}

interface ConfigClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

export function createConfigApi(client: ConfigClient) {
  return {
    getList(signal?: AbortSignal): Promise<ConfigApi.Item[]> {
      return client.request('/app/platform/config/list', { signal });
    },
    update(code: string, input: ConfigApi.SaveInput, signal?: AbortSignal): Promise<boolean> {
      return client.request(`/app/platform/config/${encodeURIComponent(code)}`, { method: 'PUT', body: input, signal });
    },
  };
}
