import type { ListPageRow } from '@/components/page/families/list/list-page.types';
import type { PlatformRecycleBinApi } from '@/api/core/platform-recycle-bin';

export interface RecycleBinRow extends ListPageRow {
  deletionTime: string;
  displayName: string;
  entityId: string;
  entityType: string;
  entityTypeLabel: string;
  originalLocation: string;
  removedBy: string;
}

export const recycleBinEntityTypes = [
  { label: '全部类型', value: '' },
  { label: '文件', value: 'file' },
  { label: '菜单', value: 'menu' },
  { label: '字典类型', value: 'dict-type' },
  { label: '字典数据', value: 'dict-data' },
  { label: '定时任务', value: 'scheduled-job' },
  { label: '任务触发器', value: 'scheduled-trigger' },
  { label: '更新日志', value: 'update-log' },
  { label: '开放接口应用', value: 'open-api' },
] as const;

const restorableEntityTypes = new Set<string>(
  recycleBinEntityTypes.filter((item) => item.value).map((item) => item.value),
);
const entityLabels = new Map<string, string>(recycleBinEntityTypes.map((item) => [item.value, item.label]));

export function canRestoreRecycleBinEntity(entityType: string): boolean {
  return restorableEntityTypes.has(entityType.toLocaleLowerCase());
}

export function canPermanentlyDeleteRecycleBinEntity(entityType: string): boolean {
  return entityType.toLocaleLowerCase() === 'file';
}

export function formatRecycleBinDate(value: null | string): string {
  if (!value) return '—';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat('zh-CN', { dateStyle: 'medium', timeStyle: 'short' }).format(date);
}

export function toRecycleBinRow(item: PlatformRecycleBinApi.Item): RecycleBinRow {
  return {
    deletionTime: formatRecycleBinDate(item.deletionTime),
    displayName: item.displayName,
    entityId: item.id,
    entityType: item.entityType,
    entityTypeLabel: entityLabels.get(item.entityType.toLocaleLowerCase()) ?? item.entityType,
    id: `${item.entityType}:${item.id}`,
    originalLocation: item.originalLocation,
    removedBy: item.deleterName ?? '—',
  };
}
