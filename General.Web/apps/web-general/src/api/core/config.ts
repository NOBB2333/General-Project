import { requestClient } from '#/api/request';

export namespace ConfigApi {
  export interface ConfigItem {
    code: string;
    description: string;
    groupCode: string;
    isReadonly: boolean;
    name: string;
    value: string;
    valueType: string;
  }
}

export async function getPlatformConfigListApi() {
  return requestClient.get<ConfigApi.ConfigItem[]>('/app/platform/config/list');
}

export async function updatePlatformConfigApi(code: string, value: string) {
  return requestClient.put<boolean>(`/app/platform/config/${code}`, { value });
}
