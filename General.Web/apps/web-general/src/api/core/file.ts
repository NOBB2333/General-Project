import { requestClient } from '#/api/request';

export namespace FileApi {
  export interface FileItem {
    contentType: string;
    fileKey: string;
    fileName: string;
    size: number;
    uploadedAt: string;
  }
}

export async function getFileListApi() {
  return requestClient.get<FileApi.FileItem[]>('/app/file/list');
}

export async function uploadFileApi(file: File) {
  const formData = new FormData();
  formData.append('file', file);
  return requestClient.post<FileApi.FileItem>('/app/file/upload', formData);
}

export async function deleteFileApi(fileKey: string) {
  return requestClient.delete<boolean>(`/app/file/${fileKey}`);
}

export async function downloadFileApi(fileKey: string) {
  return requestClient.get<Blob>(`/app/file/download/${fileKey}`, {
    responseType: 'blob',
  });
}
