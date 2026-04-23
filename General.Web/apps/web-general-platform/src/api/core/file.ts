import { requestClient } from '#/api/request';

export namespace FileApi {
  export interface FileItem {
    category: string;
    contentType: string;
    fileKey: string;
    fileName: string;
    parentPath?: null | string;
    size: number;
    storageLocation: string;
    uploadedAt: string;
    uploadedBy?: null | string;
  }
}

export async function getFileListApi() {
  return requestClient.get<FileApi.FileItem[]>('/app/file/list');
}

export async function uploadFileApi(file: File, payload?: { category?: string; parentPath?: null | string }) {
  return requestClient.upload<FileApi.FileItem>('/app/file/upload', {
    category: payload?.category,
    file,
    parentPath: payload?.parentPath,
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
