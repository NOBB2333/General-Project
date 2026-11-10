import type { ListPageRow } from '@/components/page/families/list/list-page.types';
import type { PlatformFileStorageSourceApi } from '@/api/core/platform-file-storage-sources';

export interface StorageSourceDraft {
  accessKeyId: string;
  bucketName: string;
  customDomain: string;
  endpoint: string;
  isDefault: boolean;
  isEnabled: boolean;
  isPublic: boolean;
  name: string;
  pathTemplate: string;
  providerName: string;
  region: string;
  remark: string;
  rootPath: string;
  secretKey: string;
  useSsl: boolean;
}

export interface StorageSourceRow extends ListPageRow {
  accessKeyId: string;
  isDefault: boolean;
  name: string;
  providerName: string;
  status: string;
}

const providerNames = new Set<string>(['AliyunOSS', 'Local', 'MinIO']);

export function createStorageSourceDraft(): StorageSourceDraft {
  return {
    accessKeyId: '',
    bucketName: '',
    customDomain: '',
    endpoint: '',
    isDefault: false,
    isEnabled: true,
    isPublic: false,
    name: '',
    pathTemplate: '',
    providerName: 'Local',
    region: '',
    remark: '',
    rootPath: '',
    secretKey: '',
    useSsl: false,
  };
}

export function toStorageSourceDraft(source: PlatformFileStorageSourceApi.Item): StorageSourceDraft {
  return {
    accessKeyId: source.accessKeyId,
    bucketName: source.bucketName ?? '',
    customDomain: source.customDomain ?? '',
    endpoint: source.endpoint ?? '',
    isDefault: source.isDefault,
    isEnabled: source.isEnabled,
    isPublic: source.isPublic,
    name: source.name,
    pathTemplate: source.pathTemplate ?? '',
    providerName: source.providerName,
    region: source.region ?? '',
    remark: source.remark ?? '',
    rootPath: source.rootPath ?? '',
    secretKey: '',
    useSsl: source.useSsl,
  };
}

export function toStorageSourceSaveInput(draft: StorageSourceDraft): PlatformFileStorageSourceApi.SaveInput {
  return {
    accessKeyId: draft.accessKeyId.trim(),
    bucketName: draft.bucketName.trim() || null,
    customDomain: draft.customDomain.trim() || null,
    endpoint: draft.endpoint.trim() || null,
    isDefault: draft.isDefault,
    isEnabled: draft.isEnabled,
    isPublic: draft.isPublic,
    name: draft.name.trim(),
    pathTemplate: draft.pathTemplate.trim() || null,
    providerName: draft.providerName as PlatformFileStorageSourceApi.ProviderName,
    region: draft.region.trim() || null,
    remark: draft.remark.trim() || null,
    rootPath: draft.rootPath.trim() || null,
    secretKey: draft.secretKey.trim(),
    useSsl: draft.useSsl,
  };
}

export function validateStorageSourceDraft(draft: StorageSourceDraft): null | string {
  if (!draft.name.trim()) return '请输入存储源名称。';
  if (draft.name.trim().length > 128) return '存储源名称不能超过128个字符。';
  if (draft.providerName.length > 32) return '存储类型名称不能超过32个字符。';
  if (draft.accessKeyId.length > 256) return 'AccessKeyId不能超过256个字符。';
  if (draft.bucketName.length > 128) return 'Bucket名称不能超过128个字符。';
  if (draft.customDomain.length > 512) return '自定义访问域名不能超过512个字符。';
  if (draft.endpoint.length > 512) return 'Endpoint不能超过512个字符。';
  if (draft.pathTemplate.length > 128) return '对象路径模板不能超过128个字符。';
  if (draft.region.length > 128) return '区域不能超过128个字符。';
  if (draft.remark.length > 256) return '备注不能超过256个字符。';
  if (draft.rootPath.length > 512) return '本地根目录不能超过512个字符。';
  if (draft.secretKey.length > 1024) return 'SecretKey不能超过1024个字符。';
  if (!providerNames.has(draft.providerName)) return '请选择支持的存储类型。';
  if (draft.providerName === 'Local') return null;
  if (!draft.endpoint.trim()) return '请输入云存储Endpoint。';
  if (!draft.accessKeyId.trim()) return '请输入AccessKeyId。';
  if (!draft.bucketName.trim()) return '请输入Bucket名称。';
  if (!draft.secretKey.trim()) return '编辑云存储配置时请重新输入SecretKey；后端不会回传已保存密钥。';
  return null;
}

export function toStorageSourceRow(source: PlatformFileStorageSourceApi.Item): StorageSourceRow {
  return {
    accessKeyId: source.accessKeyId,
    id: source.id,
    isDefault: source.isDefault,
    name: source.name,
    providerName: source.providerName,
    status: source.isEnabled ? '启用' : '停用',
  };
}
