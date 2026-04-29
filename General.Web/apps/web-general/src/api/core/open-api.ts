import { requestClient } from '#/api/request';

export namespace OpenApiApplicationApi {
  export interface ApplicationItem {
    appId: string;
    creationTime: string;
    id: string;
    isEnabled: boolean;
    lastModificationTime?: string;
    name: string;
    remark: string;
    scopes: string[];
  }

  export interface ApplicationSecretItem extends ApplicationItem {
    appSecret: string;
  }

  export interface ApplicationSaveInput {
    isEnabled: boolean;
    name: string;
    remark: string;
    scopes: string[];
  }
}

export async function getOpenApiApplicationListApi() {
  return requestClient.get<OpenApiApplicationApi.ApplicationItem[]>('/app/platform/open-api/applications');
}

export async function createOpenApiApplicationApi(data: OpenApiApplicationApi.ApplicationSaveInput) {
  return requestClient.post<OpenApiApplicationApi.ApplicationSecretItem>('/app/platform/open-api/applications', data);
}

export async function updateOpenApiApplicationApi(id: string, data: OpenApiApplicationApi.ApplicationSaveInput) {
  return requestClient.put<OpenApiApplicationApi.ApplicationItem>(`/app/platform/open-api/applications/${id}`, data);
}

export async function resetOpenApiApplicationSecretApi(id: string) {
  return requestClient.post<OpenApiApplicationApi.ApplicationSecretItem>(
    `/app/platform/open-api/applications/${id}/reset-secret`,
  );
}

export async function deleteOpenApiApplicationApi(id: string) {
  return requestClient.delete<boolean>(`/app/platform/open-api/applications/${id}`);
}
