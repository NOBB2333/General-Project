import { requestClient } from '#/api/request';

export namespace DictApi {
  export interface DictTypeItem {
    code: string;
    id: string;
    name: string;
    remark: string;
    sort: number;
  }

  export interface DictTypeSaveInput {
    code: string;
    name: string;
    remark: string;
    sort: number;
  }

  export interface DictDataItem {
    dictTypeId: string;
    id: string;
    isEnabled: boolean;
    label: string;
    remark: string;
    sort: number;
    tagColor: string;
    value: string;
  }

  export interface DictDataSaveInput {
    isEnabled: boolean;
    label: string;
    remark: string;
    sort: number;
    tagColor: string;
    value: string;
  }

  export interface DictOptionItem {
    label: string;
    tagColor: string;
    value: string;
  }
}

export async function getDictTypesApi() {
  return requestClient.get<DictApi.DictTypeItem[]>('/app/platform/dict/types');
}

export async function createDictTypeApi(data: DictApi.DictTypeSaveInput) {
  return requestClient.post<DictApi.DictTypeItem>('/app/platform/dict/types', data);
}

export async function updateDictTypeApi(id: string, data: DictApi.DictTypeSaveInput) {
  return requestClient.put<DictApi.DictTypeItem>(`/app/platform/dict/types/${id}`, data);
}

export async function deleteDictTypeApi(id: string) {
  return requestClient.delete<boolean>(`/app/platform/dict/types/${id}`);
}

export async function getDictDataApi(dictTypeCode: string) {
  return requestClient.get<DictApi.DictDataItem[]>(`/app/platform/dict/${dictTypeCode}/data`);
}

export async function createDictDataApi(dictTypeCode: string, data: DictApi.DictDataSaveInput) {
  return requestClient.post<DictApi.DictDataItem>(`/app/platform/dict/${dictTypeCode}/data`, data);
}

export async function updateDictDataApi(id: string, data: DictApi.DictDataSaveInput) {
  return requestClient.put<DictApi.DictDataItem>(`/app/platform/dict/data/${id}`, data);
}

export async function deleteDictDataApi(id: string) {
  return requestClient.delete<boolean>(`/app/platform/dict/data/${id}`);
}

export async function getDictItemsApi(dictTypeCode: string) {
  return requestClient.get<DictApi.DictOptionItem[]>(`/app/platform/dict/${dictTypeCode}/items`);
}
