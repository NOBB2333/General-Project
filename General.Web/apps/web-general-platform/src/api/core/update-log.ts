import { requestClient } from '#/api/request';

export namespace UpdateLogApi {
  export interface UpdateLogItem {
    authorName: string;
    creationTime: string;
    id: string;
    impactScope?: null | string;
    publishedAt: string;
    summary: string;
    title: string;
    version: string;
  }

  export interface UpdateLogSaveInput {
    impactScope?: null | string;
    publishedAt: string;
    summary: string;
    title: string;
    version: string;
  }
}

export async function getUpdateLogListApi() {
  return requestClient.get<UpdateLogApi.UpdateLogItem[]>('/app/update-log/list');
}

export async function createUpdateLogApi(data: UpdateLogApi.UpdateLogSaveInput) {
  return requestClient.post<boolean>('/app/update-log', data);
}

export async function updateUpdateLogApi(id: string, data: UpdateLogApi.UpdateLogSaveInput) {
  return requestClient.put<boolean>(`/app/update-log/${id}`, data);
}

export async function deleteUpdateLogApi(id: string) {
  return requestClient.delete<boolean>(`/app/update-log/${id}`);
}
