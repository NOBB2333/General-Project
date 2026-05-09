import { requestClient } from '#/api/request';

export namespace RecycleBinApi {
  export interface RecycleBinItem {
    deleterId?: null | string;
    deleterName?: null | string;
    deletionTime?: string;
    displayName: string;
    entityType: string;
    id: string;
    originalLocation: string;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }
}

export async function getRecycleBinItemsApi(params?: {
  entityType?: string;
  maxResultCount?: number;
  skipCount?: number;
}) {
  return requestClient.get<RecycleBinApi.PagedResult<RecycleBinApi.RecycleBinItem>>('/app/platform/recycle-bin/items', {
    params,
  });
}

export async function restoreRecycleBinItemApi(entityType: string, id: string) {
  return requestClient.post<boolean>(`/app/platform/recycle-bin/${entityType}/${id}/restore`);
}

export async function deleteRecycleBinItemPermanentlyApi(entityType: string, id: string) {
  return requestClient.delete<boolean>(`/app/platform/recycle-bin/${entityType}/${id}`);
}
