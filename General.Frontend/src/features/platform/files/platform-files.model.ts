import type { ListPageRow } from '@/components/page/families/list/list-page.types';
import type { PlatformFileApi } from './platform-files.api';

export interface PlatformFileRow extends ListPageRow {
  business: string;
  category: string;
  contentType: string;
  fileKey: string;
  fileName: string;
  relativePath: string;
  size: string;
  status: string;
  uploadedAt: string;
  uploadedBy: string;
  version: string;
}

export function formatFileSize(size: number): string {
  if (!Number.isFinite(size) || size < 0) return '未知';
  if (size < 1024) return `${size} B`;
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`;
  if (size < 1024 * 1024 * 1024) return `${(size / (1024 * 1024)).toFixed(1)} MB`;
  return `${(size / (1024 * 1024 * 1024)).toFixed(2)} GB`;
}

export function formatUploadedAt(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat('zh-CN', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date);
}

const safePreviewContentTypes = new Set([
  'application/json',
  'application/pdf',
  'image/bmp',
  'image/gif',
  'image/jpeg',
  'image/png',
  'image/webp',
  'text/csv',
  'text/markdown',
  'text/plain',
]);

export function isSafePreviewContentType(contentType: string): boolean {
  const normalized = contentType.split(';', 1)[0]?.trim().toLocaleLowerCase() ?? '';
  return safePreviewContentTypes.has(normalized);
}

export function toPlatformFileRow(file: PlatformFileApi.Item): PlatformFileRow {
  return {
    business: file.businessProjectName || file.businessObjectName || '',
    category: file.category,
    contentType: file.contentType,
    fileKey: file.fileKey,
    fileName: file.fileName,
    id: file.id,
    relativePath: file.relativePath,
    size: formatFileSize(file.size),
    status: file.isCurrentVersion ? '当前版本' : '历史版本',
    uploadedAt: formatUploadedAt(file.uploadedAt),
    uploadedBy: file.uploadedBy ?? '',
    version: `第 ${file.versionNo} 版`,
  };
}

export function sanitizeDownloadFileName(fileName: string): string {
  const safeName = [...fileName]
    .map((character) => {
      const codePoint = character.codePointAt(0) ?? 0;
      return character === '/' || character === '\\' || codePoint < 32 || codePoint === 127 ? '_' : character;
    })
    .join('')
    .trim();
  return safeName || '下载文件';
}
