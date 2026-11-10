import type { DictApi } from '@/api/core/dict';

export type DictTagTone = 'danger' | 'neutral' | 'primary' | 'success' | 'warning';

export interface DictTypeDraft {
  code: string;
  name: string;
  remark: string;
  sort: string;
}

export interface DictDataDraft {
  isEnabled: boolean;
  label: string;
  remark: string;
  sort: string;
  tagColor: string;
  value: string;
}

export function createDictTypeDraft(type?: DictApi.Type): DictTypeDraft {
  return {
    code: type?.code ?? '',
    name: type?.name ?? '',
    remark: type?.remark ?? '',
    sort: String(type?.sort ?? 0),
  };
}

export function createDictDataDraft(data?: DictApi.Data): DictDataDraft {
  return {
    isEnabled: data?.isEnabled ?? true,
    label: data?.label ?? '',
    remark: data?.remark ?? '',
    sort: String(data?.sort ?? 0),
    tagColor: data?.tagColor ?? 'default',
    value: data?.value ?? '',
  };
}

export function normalizeDictCode(code: string): string {
  return code.trim().toLowerCase();
}

export function validateDictTypeDraft(
  draft: DictTypeDraft,
  types: readonly DictApi.Type[],
  editingId?: string,
): string | null {
  const code = normalizeDictCode(draft.code);
  const name = draft.name.trim();
  if (!code) return '请输入字典编码。';
  if (code.length > 64) return '字典编码不能超过64个字符。';
  if (!/^[a-z0-9][a-z0-9._-]*$/u.test(code)) return '字典编码仅支持字母、数字、点、下划线和短横线。';
  if (types.some((type) => type.id !== editingId && normalizeDictCode(type.code) === code)) {
    return '字典编码已存在。';
  }
  if (!name) return '请输入字典名称。';
  if (name.length > 128) return '字典名称不能超过128个字符。';
  if (draft.remark.trim().length > 256) return '备注不能超过256个字符。';
  return validateSort(draft.sort);
}

export function validateDictDataDraft(
  draft: DictDataDraft,
  rows: readonly DictApi.Data[],
  editingId?: string,
): string | null {
  const label = draft.label.trim();
  const value = draft.value.trim();
  const tagColor = draft.tagColor.trim();
  if (!label) return '请输入显示名称。';
  if (label.length > 128) return '显示名称不能超过128个字符。';
  if (!value) return '请输入字典值。';
  if (value.length > 128) return '字典值不能超过128个字符。';
  if (rows.some((row) => row.id !== editingId && row.value.trim() === value)) return '该字典值已存在。';
  if (tagColor.length > 32) return '标签颜色标识不能超过32个字符。';
  if (draft.remark.trim().length > 256) return '备注不能超过256个字符。';
  return validateSort(draft.sort);
}

export function toDictTypeSaveInput(draft: DictTypeDraft, immutableCode?: string): DictApi.TypeSaveInput {
  return {
    code: normalizeDictCode(immutableCode ?? draft.code),
    name: draft.name.trim(),
    remark: draft.remark.trim(),
    sort: Number(draft.sort),
  };
}

export function toDictDataSaveInput(draft: DictDataDraft): DictApi.DataSaveInput {
  return {
    isEnabled: draft.isEnabled,
    label: draft.label.trim(),
    remark: draft.remark.trim(),
    sort: Number(draft.sort),
    tagColor: draft.tagColor.trim(),
    value: draft.value.trim(),
  };
}

export function getDictTagTone(tagColor: null | string | undefined): DictTagTone {
  switch (tagColor?.trim().toLowerCase()) {
    case 'blue':
    case 'processing':
      return 'primary';
    case 'success':
      return 'success';
    case 'error':
      return 'danger';
    case 'gold':
    case 'warning':
      return 'warning';
    default:
      return 'neutral';
  }
}

export function getDictTagLabel(tagColor: null | string | undefined): string {
  return tagColor?.trim() || 'default';
}

export function resolveDictLabel(items: readonly DictApi.Item[], value: null | string | undefined): string {
  const normalizedValue = value?.trim();
  if (!normalizedValue) return '—';
  const item = items.find((option) => option.value === normalizedValue);
  return item?.label?.trim() || normalizedValue;
}

export function displayDictText(value: null | string | undefined): string {
  return value?.trim() || '—';
}

function validateSort(value: string): string | null {
  if (!value.trim()) return '排序值必须是0到2147483647之间的整数。';
  const sort = Number(value);
  if (!Number.isInteger(sort) || sort < 0 || sort > 2_147_483_647) {
    return '排序值必须是0到2147483647之间的整数。';
  }
  return null;
}
