import { apiClient } from '@/api/request';

export namespace DictApi {
  export interface Type {
    code: string;
    dataCount: number;
    enabledDataCount: number;
    id: string;
    name: string;
    remark: string;
    sort: number;
  }

  export interface TypeSaveInput {
    code: string;
    name: string;
    remark: string;
    sort: number;
  }

  export interface Data {
    dictTypeId: string;
    id: string;
    isEnabled: boolean;
    label: string;
    remark: string;
    sort: number;
    tagColor: string;
    value: string;
  }

  export interface DataSaveInput {
    isEnabled: boolean;
    label: string;
    remark: string;
    sort: number;
    tagColor: string;
    value: string;
  }

  export interface Item {
    label: string;
    tagColor: string;
    value: string;
  }
}

export function getDictTypesApi(): Promise<DictApi.Type[]> {
  return apiClient.request('/app/platform/dict/types');
}

export function getDictItemsApi(code: string): Promise<DictApi.Item[]> {
  return apiClient.request(`/app/platform/dict/${encodeURIComponent(code)}/items`);
}

export function getDictItemsBatchApi(codes: readonly string[]): Promise<Record<string, DictApi.Item[]>> {
  const normalizedCodes = [...new Set(codes.map((code) => code.trim().toLowerCase()).filter(Boolean))];
  if (!normalizedCodes.length) return Promise.resolve({});
  return apiClient.request('/app/platform/dict/items', { query: { codes: normalizedCodes.join(',') } });
}

export function getDictDataApi(code: string): Promise<DictApi.Data[]> {
  return apiClient.request(`/app/platform/dict/${encodeURIComponent(code)}/data`);
}

export function createDictTypeApi(input: DictApi.TypeSaveInput): Promise<DictApi.Type> {
  return apiClient.request('/app/platform/dict/types', { body: input, method: 'POST' });
}

export function updateDictTypeApi(id: string, input: DictApi.TypeSaveInput): Promise<DictApi.Type> {
  return apiClient.request(`/app/platform/dict/types/${encodeURIComponent(id)}`, { body: input, method: 'PUT' });
}

export function deleteDictTypeApi(id: string): Promise<boolean> {
  return apiClient.request(`/app/platform/dict/types/${encodeURIComponent(id)}`, { method: 'DELETE' });
}

export function createDictDataApi(code: string, input: DictApi.DataSaveInput): Promise<DictApi.Data> {
  return apiClient.request(`/app/platform/dict/${encodeURIComponent(code)}/data`, { body: input, method: 'POST' });
}

export function updateDictDataApi(id: string, input: DictApi.DataSaveInput): Promise<DictApi.Data> {
  return apiClient.request(`/app/platform/dict/data/${encodeURIComponent(id)}`, { body: input, method: 'PUT' });
}

export function deleteDictDataApi(id: string): Promise<boolean> {
  return apiClient.request(`/app/platform/dict/data/${encodeURIComponent(id)}`, { method: 'DELETE' });
}
