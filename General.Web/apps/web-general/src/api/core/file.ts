import { requestClient } from '#/api/request';

export namespace FileApi {
  export interface FileListQuery {
    businessType?: string;
    category?: string;
    keyword?: string;
    storageSourceId?: string;
    uploadedBy?: string;
    uploadedFrom?: string;
    uploadedTo?: string;
  }

  export interface FileItem {
    businessId?: null | string;
    businessType?: null | string;
    bucketName?: null | string;
    category: string;
    contentType: string;
    fileKey: string;
    fileName: string;
    isPublic: boolean;
    parentPath?: null | string;
    relativePath: string;
    size: number;
    storageLocation: string;
    storageProvider: string;
    storageSourceId?: null | string;
    storageSourceName?: null | string;
    uploadedAt: string;
    uploadedBy?: null | string;
  }

  export interface StorageSourceItem {
    accessKeyId: string;
    bucketName?: null | string;
    customDomain?: null | string;
    endpoint?: null | string;
    id: string;
    isDefault: boolean;
    isEnabled: boolean;
    isPublic: boolean;
    name: string;
    pathTemplate?: null | string;
    providerName: string;
    region?: null | string;
    remark?: null | string;
    rootPath?: null | string;
    useSsl: boolean;
  }

  export interface StorageSourceSaveInput {
    accessKeyId?: string;
    bucketName?: null | string;
    customDomain?: null | string;
    endpoint?: null | string;
    isDefault: boolean;
    isEnabled: boolean;
    isPublic: boolean;
    name: string;
    pathTemplate?: null | string;
    providerName: string;
    region?: null | string;
    remark?: null | string;
    rootPath?: null | string;
    secretKey?: null | string;
    useSsl: boolean;
  }
}

export async function getFileListApi(params: FileApi.FileListQuery = {}) {
  return requestClient.get<FileApi.FileItem[]>('/app/file/list', { params });
}

export async function uploadFileApi(
  file: File,
  payload?: {
    businessId?: null | string;
    businessType?: null | string;
    category?: string;
    isPublic?: boolean;
    parentPath?: null | string;
    storageSourceId?: null | string;
  },
) {
  return requestClient.upload<FileApi.FileItem>('/app/file/upload', {
    businessId: payload?.businessId,
    businessType: payload?.businessType,
    category: payload?.category,
    file,
    isPublic: payload?.isPublic,
    parentPath: payload?.parentPath,
    storageSourceId: payload?.storageSourceId,
  });
}

export async function deleteFileApi(fileKey: string) {
  return requestClient.delete<boolean>(`/app/file/${fileKey}`);
}

export async function downloadFileApi(fileKey: string) {
  return requestClient.get<Blob>(`/app/file/download/${fileKey}`, {
    responseType: 'blob',
  });
}

export async function getFileStorageSourcesApi(enabledOnly = false) {
  return requestClient.get<FileApi.StorageSourceItem[]>('/app/platform/file-storage-sources', {
    params: { enabledOnly },
  });
}

export async function createFileStorageSourceApi(data: FileApi.StorageSourceSaveInput) {
  return requestClient.post<FileApi.StorageSourceItem>('/app/platform/file-storage-sources', data);
}

export async function updateFileStorageSourceApi(id: string, data: FileApi.StorageSourceSaveInput) {
  return requestClient.put<FileApi.StorageSourceItem>(`/app/platform/file-storage-sources/${id}`, data);
}

export async function toggleFileStorageSourceApi(id: string, isEnabled: boolean) {
  return requestClient.post<boolean>(`/app/platform/file-storage-sources/${id}/toggle`, { isEnabled });
}

export async function setDefaultFileStorageSourceApi(id: string) {
  return requestClient.post<boolean>(`/app/platform/file-storage-sources/${id}/default`);
}

export async function deleteFileStorageSourceApi(id: string) {
  return requestClient.delete<boolean>(`/app/platform/file-storage-sources/${id}`);
}
