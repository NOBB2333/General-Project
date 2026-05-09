import { requestClient } from '#/api/request';

export namespace ConfigApi {
  export interface ConfigItem {
    code: string;
    defaultValue: string;
    description: string;
    groupCode: string;
    groupName: string;
    hasTenantValue: boolean;
    isReadonly: boolean;
    name: string;
    providerKey?: null | string;
    providerName: string;
    value: string;
    valueType: string;
  }

  export interface ConfigSaveInput {
    providerKey?: null | string;
    providerName?: string;
    value: string;
  }
}

export async function getPlatformConfigListApi() {
  return requestClient.get<ConfigApi.ConfigItem[]>('/app/platform/config/list');
}

export async function updatePlatformConfigApi(code: string, data: ConfigApi.ConfigSaveInput) {
  return requestClient.put<boolean>(`/app/platform/config/${code}`, data);
}
