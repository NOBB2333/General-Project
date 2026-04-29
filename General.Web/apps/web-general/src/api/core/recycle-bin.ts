import { requestClient } from '#/api/request';

export namespace RecycleBinApi {
  export interface RecycleBinItem {
    deletionTime?: string;
    displayName: string;
    entityType: string;
    id: string;
  }
}

export async function getRecycleBinItemsApi(entityType?: string) {
  return requestClient.get<RecycleBinApi.RecycleBinItem[]>('/app/platform/recycle-bin/items', {
    params: { entityType },
  });
}

export async function restoreRecycleBinItemApi(entityType: string, id: string) {
  return requestClient.post<boolean>(`/app/platform/recycle-bin/${entityType}/${id}/restore`);
}
