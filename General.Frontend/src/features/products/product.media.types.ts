export type MediaRole = 'main' | 'gallery' | 'attachment';
export type MediaType = 'image' | 'pdf' | 'doc' | 'video' | 'other';
export type MediaMatchStatus = 'matched' | 'unmatched' | 'duplicate' | 'ignored';

export interface MediaAsset {
  id: string;
  productId: string;
  role: MediaRole;
  mediaType: MediaType;
  fileName: string;
  url: string;
  size: number;
  uploadedAt: string;
  uploadedBy: string;
}

export interface ParsedMediaFile {
  id: string;
  fileName: string;
  size: number;
  mediaType: MediaType;
  previewUrl: string;
  parsedIdentifier: string;
  parsedSequence: string;
  role: MediaRole;
  matchedProductId: string;
  matchConfidence: number;
  status: MediaMatchStatus;
  conflictNote: string;
}

export interface MediaRule {
  name: string;
  separator: '_' | '-' | ' ';
  primary: 'code' | 'model' | 'either';
  classifier: 'sequence' | 'role';
}

export const roleMeta: Record<MediaRole, { label: string; bg: string; color: string }> = {
  main: { label: '主图', bg: '#DEEBFF', color: '#0052CC' },
  gallery: { label: '副图', bg: '#EAE6FF', color: '#5243AA' },
  attachment: { label: '附件', bg: '#E3FCEF', color: '#00875A' },
};

export function detectMediaType(fileName: string): MediaType {
  const extension = fileName.split('.').pop()?.toLowerCase() ?? '';
  if (['jpg', 'jpeg', 'png', 'webp', 'heic', 'gif'].includes(extension)) return 'image';
  if (extension === 'pdf') return 'pdf';
  if (['doc', 'docx', 'xls', 'xlsx', 'txt'].includes(extension)) return 'doc';
  if (['mp4', 'mov', 'webm'].includes(extension)) return 'video';
  return 'other';
}

export function formatFileSize(size: number) {
  if (size < 1024) return `${size} B`;
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`;
  return `${(size / 1024 / 1024).toFixed(1)} MB`;
}
