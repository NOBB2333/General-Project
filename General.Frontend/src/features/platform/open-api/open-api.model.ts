import type { PlatformOpenApiApi } from '@/api/core/platform-open-api';
import type { ListPageRow } from '@/components/page/families/list/list-page.types';

export const openApiScopeOptions = [
  { code: 'dict:read', description: '只读取已启用的平台字典项。', label: '平台字典读取' },
] as const;

export const openApiAllScope = '*';

export interface OpenApiApplicationDraft {
  isEnabled: boolean;
  name: string;
  remark: string;
  scopes: string[];
}

export interface OpenApiApplicationRow extends ListPageRow {
  appId: string;
  createdAt: string;
  name: string;
  remark: string;
  scopes: string;
  status: string;
}

export function createOpenApiApplicationDraft(): OpenApiApplicationDraft {
  return { isEnabled: true, name: '', remark: '', scopes: ['dict:read'] };
}

export function toOpenApiApplicationDraft(application: PlatformOpenApiApi.Application): OpenApiApplicationDraft {
  return {
    isEnabled: application.isEnabled,
    name: application.name,
    remark: application.remark,
    scopes: [...application.scopes],
  };
}

export function toggleOpenApiScope(scopes: readonly string[], scope: string, checked: boolean): string[] {
  if (scope === openApiAllScope) return checked ? [openApiAllScope] : scopes.filter((item) => item !== openApiAllScope);

  const scopeIsKnown = openApiScopeOptions.some((option) => option.code === scope);
  if (!scopeIsKnown && scopes.includes(openApiAllScope)) {
    return checked ? [...scopes] : scopes.filter((item) => item !== scope);
  }

  const current = scopes.includes(openApiAllScope) ? [] : [...scopes];
  const next = new Set(current);
  if (checked) next.add(scope);
  else next.delete(scope);
  return [...next];
}

export function validateOpenApiApplicationDraft(draft: OpenApiApplicationDraft): string | null {
  const name = draft.name.trim();
  if (!name) return '请输入应用名称。';
  if (name.length > 128) return '应用名称不能超过128个字符。';
  if (draft.remark.length > 256) return '备注不能超过256个字符。';
  if (!draft.scopes.length) return '至少选择一个授权范围。';
  if (draft.scopes.includes(openApiAllScope) && draft.scopes.length > 1) {
    return '全部开放接口范围不能与单项范围同时选择。';
  }
  return null;
}

export function toOpenApiApplicationSaveInput(draft: OpenApiApplicationDraft): PlatformOpenApiApi.ApplicationSaveInput {
  return {
    isEnabled: draft.isEnabled,
    name: draft.name.trim(),
    remark: draft.remark.trim(),
    scopes: [...new Set(draft.scopes.map((scope) => scope.trim()).filter(Boolean))],
  };
}

export function toOpenApiApplicationRow(application: PlatformOpenApiApi.Application): OpenApiApplicationRow {
  const scopeLabel = application.scopes
    .map((scope) => (scope === openApiAllScope ? '全部开放接口（*）' : scope))
    .join('、');
  return {
    appId: application.appId,
    createdAt: formatOpenApiDate(application.creationTime),
    id: application.id,
    name: application.name,
    remark: application.remark,
    scopes: scopeLabel || '—',
    status: application.isEnabled ? '启用' : '停用',
  };
}

export function formatOpenApiDate(value: string): string {
  const time = new Date(value);
  if (!Number.isFinite(time.getTime())) return '—';
  return time.toLocaleString('zh-CN', { dateStyle: 'short', timeStyle: 'short' });
}
