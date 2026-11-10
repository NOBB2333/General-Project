import { ApiRequestError, type ApiRequestOptions } from '../../../api/client.ts';

export namespace PlatformFileApi {
  export interface ListInput {
    businessId?: null | string;
    businessKeyword?: null | string;
    businessProjectId?: null | string;
    businessScope?: null | string;
    businessType?: null | string;
    category?: null | string;
    categoryPrefix?: null | string;
    keyword?: null | string;
    maxResultCount: number;
    parentPath?: null | string;
    skipCount: number;
    storageSourceId?: null | string;
    uploadedBy?: null | string;
    uploadedFrom?: null | string;
    uploadedTo?: null | string;
  }

  export interface TreeItem {
    businessType: null | string;
    category: string;
    parentPath: null | string;
  }

  export interface Item {
    businessId: null | string;
    businessJumpPath: null | string;
    businessJumpQuery: Record<string, null | string>;
    businessObjectCode: null | string;
    businessObjectName: null | string;
    businessObjectTypeName: null | string;
    businessProjectId: null | string;
    businessProjectName: null | string;
    businessType: null | string;
    bucketName: null | string;
    category: string;
    contentType: string;
    fileKey: string;
    fileName: string;
    id: string;
    isCurrentVersion: boolean;
    isPublic: boolean;
    parentPath: null | string;
    previousVersionFileId: null | string;
    relativePath: string;
    replacedByFileId: null | string;
    size: number;
    storageLocation: string;
    storageProvider: string;
    storageSourceId: null | string;
    storageSourceName: null | string;
    uploadedAt: string;
    uploadedBy: null | string;
    versionNo: number;
  }

  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
  }

  export interface UploadInput {
    businessId?: string;
    businessType?: string;
    category?: string;
    file: File;
    isPublic?: boolean;
    parentPath?: string;
    storageSourceId?: string;
  }
}

export interface PlatformFileApiClient {
  request<T>(path: string, options?: ApiRequestOptions): Promise<T>;
}

function isJsonRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

async function rejectApplicationErrorBlob(blob: Blob): Promise<Blob> {
  if (!blob.type.toLocaleLowerCase().includes('json')) return blob;
  const text = await blob.text();
  let payload: unknown;
  try {
    payload = JSON.parse(text) as unknown;
  } catch {
    return blob;
  }
  if (!isJsonRecord(payload)) return blob;

  const nestedError = payload.error ?? payload.Error;
  const nestedErrorObject = isJsonRecord(nestedError) ? nestedError : undefined;
  const hasApplicationFailure =
    (typeof payload.code === 'number' && payload.code !== 0 && 'data' in payload) ||
    (nestedErrorObject !== undefined &&
      (typeof nestedErrorObject.code === 'string' || typeof nestedErrorObject.message === 'string'));
  if (!hasApplicationFailure) return blob;

  const message =
    (typeof nestedError === 'string' && nestedError.trim()) ||
    (typeof nestedErrorObject?.message === 'string' && nestedErrorObject.message.trim()) ||
    (typeof payload.message === 'string' && payload.message.trim()) ||
    '服务端未能返回文件内容。';
  const code =
    typeof payload.code === 'number' || typeof payload.code === 'string'
      ? payload.code
      : typeof nestedErrorObject?.code === 'string'
        ? nestedErrorObject.code
        : undefined;
  throw new ApiRequestError(message, { code, status: 200 });
}

export function createPlatformFilesApi(client: PlatformFileApiClient) {
  return {
    deleteFile(fileKey: string, signal?: AbortSignal): Promise<boolean> {
      return client.request<boolean>(`/app/file/${encodeURIComponent(fileKey)}`, { method: 'DELETE', signal });
    },
    async getFileContent(fileKey: string, kind: 'download' | 'preview', signal?: AbortSignal): Promise<Blob> {
      const blob = await client.request<Blob>(`/app/file/${kind}/${encodeURIComponent(fileKey)}`, {
        headers: { Accept: 'application/json' },
        responseType: 'blob',
        signal,
      });
      return rejectApplicationErrorBlob(blob);
    },
    getList(
      input: PlatformFileApi.ListInput,
      signal?: AbortSignal,
    ): Promise<PlatformFileApi.PagedResult<PlatformFileApi.Item>> {
      return client.request<PlatformFileApi.PagedResult<PlatformFileApi.Item>>('/app/file/list', {
        query: {
          businessId: input.businessId,
          businessKeyword: input.businessKeyword,
          businessProjectId: input.businessProjectId,
          businessScope: input.businessScope,
          businessType: input.businessType,
          category: input.category,
          categoryPrefix: input.categoryPrefix,
          keyword: input.keyword,
          maxResultCount: input.maxResultCount,
          parentPath: input.parentPath,
          skipCount: input.skipCount,
          storageSourceId: input.storageSourceId,
          uploadedBy: input.uploadedBy,
          uploadedFrom: input.uploadedFrom,
          uploadedTo: input.uploadedTo,
        },
        signal,
      });
    },
    getTree(
      input: Omit<PlatformFileApi.ListInput, 'maxResultCount' | 'skipCount'> = {},
      signal?: AbortSignal,
    ): Promise<PlatformFileApi.TreeItem[]> {
      return client.request<PlatformFileApi.TreeItem[]>('/app/file/tree', { query: input, signal });
    },
    upload(input: PlatformFileApi.UploadInput, signal?: AbortSignal): Promise<PlatformFileApi.Item> {
      const formData = new FormData();
      formData.set('File', input.file, input.file.name);
      if (input.businessId) formData.set('BusinessId', input.businessId);
      if (input.businessType) formData.set('BusinessType', input.businessType);
      if (input.category) formData.set('Category', input.category);
      if (input.isPublic !== undefined) formData.set('IsPublic', String(input.isPublic));
      if (input.parentPath) formData.set('ParentPath', input.parentPath);
      if (input.storageSourceId) formData.set('StorageSourceId', input.storageSourceId);
      return client.request<PlatformFileApi.Item>('/app/file/upload', { body: formData, method: 'POST', signal });
    },
  };
}
