import type { ListPageRow } from '@/components/page/families/list/list-page.types';
import type { PlatformUpdateLogApi } from '@/api/core/platform-update-log';

export interface UpdateLogDraft {
  impactScope: string;
  publishedAt: string;
  summary: string;
  title: string;
  version: string;
}

export interface UpdateLogRow extends ListPageRow {
  authorName: string;
  impactScope: string;
  publishedAt: string;
  summary: string;
  title: string;
  version: string;
}

const pad = (part: number) => String(part).padStart(2, '0');

export function parseUpdateLogTime(value: string): Date {
  // General.Admin按UTC保存发布时间；SQLite回读可能丢失DateTime.Kind和时区后缀。
  const utcValue = /^\d{4}-\d{2}-\d{2}T/u.test(value) && !/(Z|[+-]\d{2}:\d{2})$/iu.test(value) ? `${value}Z` : value;
  return new Date(utcValue);
}

function toLocalDateTimeInput(value: string): string {
  const date = parseUpdateLogTime(value);
  if (Number.isNaN(date.getTime())) return '';
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function createUpdateLogDraft(now = new Date()): UpdateLogDraft {
  return {
    impactScope: '',
    publishedAt: toLocalDateTimeInput(now.toISOString()),
    summary: '',
    title: '',
    version: '',
  };
}

export function toUpdateLogDraft(item: PlatformUpdateLogApi.Item): UpdateLogDraft {
  return {
    impactScope: item.impactScope ?? '',
    publishedAt: toLocalDateTimeInput(item.publishedAt),
    summary: item.summary,
    title: item.title,
    version: item.version,
  };
}

export function toUpdateLogSaveInput(draft: UpdateLogDraft): PlatformUpdateLogApi.SaveInput {
  const publishedAt = new Date(draft.publishedAt);
  if (Number.isNaN(publishedAt.getTime())) throw new Error('请选择有效的发布时间。');
  return {
    impactScope: draft.impactScope.trim() || null,
    publishedAt: publishedAt.toISOString(),
    summary: draft.summary.trim(),
    title: draft.title.trim(),
    version: draft.version.trim(),
  };
}

export function validateUpdateLogDraft(draft: UpdateLogDraft): null | string {
  if (!draft.version.trim()) return '请输入版本号。';
  if (draft.version.trim().length > 64) return '版本号不能超过64个字符。';
  if (!draft.title.trim()) return '请输入更新标题。';
  if (draft.title.trim().length > 128) return '更新标题不能超过128个字符。';
  if (!draft.summary.trim()) return '请输入更新内容。';
  if (draft.summary.trim().length > 2048) return '更新内容不能超过2048个字符。';
  if (!draft.publishedAt || Number.isNaN(new Date(draft.publishedAt).getTime())) return '请选择有效的发布时间。';
  return null;
}

export function formatUpdateLogDate(value: string): string {
  const date = parseUpdateLogTime(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat('zh-CN', { dateStyle: 'medium', timeStyle: 'short' }).format(date);
}

export function toUpdateLogRow(item: PlatformUpdateLogApi.Item): UpdateLogRow {
  return {
    authorName: item.authorName,
    id: item.id,
    impactScope: item.impactScope ?? '',
    publishedAt: formatUpdateLogDate(item.publishedAt),
    summary: item.summary,
    title: item.title,
    version: item.version,
  };
}
