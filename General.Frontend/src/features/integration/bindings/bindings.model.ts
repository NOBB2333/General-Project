import type { IntegrationBindingsApi } from '@/api/core/integration-bindings';
import type { ListPageRow } from '@/components/page/families/list/list-page.types';

export type BindingTab = 'accounts' | 'users' | 'departments' | 'groups' | 'mappings';

export interface BindingRow extends ListPageRow {
  connector: string;
  externalId: string;
  name: string;
  related: string;
  status: string;
}

export interface ImportParseResult {
  accounts: IntegrationBindingsApi.ExternalAccountSaveInput[];
  error: null | string;
}

const statusLabels: Record<string, string> = {
  Bound: '已绑定',
  Conflict: '待处理冲突',
  Disabled: '已停用',
  Pending: '待绑定',
};

export const bindingStatusOptions = [
  { label: '待绑定', value: 'Pending' },
  { label: '已绑定', value: 'Bound' },
  { label: '存在冲突', value: 'Conflict' },
  { label: '已停用', value: 'Disabled' },
] as const;

export function bindingStatusLabel(status: string): string {
  return statusLabels[status] ?? (status || '未知');
}

export function bindingStatusFilterValue(label: string): string {
  return bindingStatusOptions.find((option) => option.label === label)?.value ?? 'All';
}

export function toAccountRow(account: IntegrationBindingsApi.ExternalAccount, connectorLabel: string): BindingRow {
  return {
    connector: connectorLabel,
    externalId: account.externalUserId,
    id: account.id,
    name: account.displayName || account.externalUserId,
    related: [account.email, account.mobile, account.departmentExternalId].filter(Boolean).join(' · ') || '—',
    status: bindingStatusLabel(account.status),
  };
}

export function toUserBindingRow(binding: IntegrationBindingsApi.UserBinding, connectorLabel: string): BindingRow {
  return {
    connector: connectorLabel,
    externalId: binding.externalUserId,
    id: binding.id,
    name: binding.externalDisplayName || binding.externalUserId,
    related: binding.internalUserName || binding.conflictReason || '未绑定内部用户',
    status: bindingStatusLabel(binding.status),
  };
}

export function toDepartmentBindingRow(
  binding: IntegrationBindingsApi.DepartmentBinding,
  connectorLabel: string,
): BindingRow {
  return {
    connector: connectorLabel,
    externalId: binding.externalDepartmentId,
    id: binding.id,
    name: binding.externalDepartmentName || binding.externalDepartmentId,
    related: binding.internalOrganizationUnitName || '未绑定内部组织',
    status: bindingStatusLabel(binding.status),
  };
}

export function toGroupBindingRow(binding: IntegrationBindingsApi.GroupBinding, connectorLabel: string): BindingRow {
  return {
    connector: connectorLabel,
    externalId: binding.externalGroupId,
    id: binding.id,
    name: binding.externalGroupName || binding.externalGroupId,
    related:
      [binding.internalObjectType, binding.internalObjectName || binding.internalObjectId]
        .filter(Boolean)
        .join(' · ') || '未绑定内部对象',
    status: bindingStatusLabel(binding.status),
  };
}

export function toIdMappingRow(mapping: IntegrationBindingsApi.IdMapping, connectorLabel: string): BindingRow {
  return {
    connector: connectorLabel,
    externalId: `${mapping.externalObjectType} · ${mapping.externalObjectId}`,
    id: mapping.id,
    name: `${mapping.internalObjectType} · ${mapping.internalObjectId}`,
    related: mapping.source || '—',
    status: bindingStatusLabel(mapping.status),
  };
}

function isObject(value: unknown): value is Record<string, unknown> {
  return value !== null && typeof value === 'object' && !Array.isArray(value);
}

const importFieldLimits: Record<string, number> = {
  avatarUrl: 512,
  departmentExternalId: 256,
  displayName: 128,
  email: 256,
  externalUnionId: 256,
  externalUserId: 256,
  mobile: 64,
  status: 32,
};

export function parseExternalAccountImportJson(value: string, connectorId: string): ImportParseResult {
  if (!connectorId) return { accounts: [], error: '请先选择连接器。' };
  let parsed: unknown;
  try {
    parsed = JSON.parse(value);
  } catch {
    return { accounts: [], error: '批量数据必须是合法JSON数组。' };
  }
  if (!Array.isArray(parsed) || parsed.length === 0) {
    return { accounts: [], error: '请提供至少一条外部账号记录。' };
  }
  if (parsed.length > 500) return { accounts: [], error: '单次最多导入500条账号。' };

  const accounts: IntegrationBindingsApi.ExternalAccountSaveInput[] = [];
  const externalIds = new Set<string>();
  for (const [index, entry] of parsed.entries()) {
    if (!isObject(entry)) return { accounts: [], error: `第${index + 1}条必须是JSON对象。` };
    const unsupportedField = Object.keys(entry).find((key) => !(key in importFieldLimits));
    if (unsupportedField) {
      return { accounts: [], error: `第${index + 1}条包含不支持的字段“${unsupportedField}”，已拒绝导入。` };
    }
    const externalUserId = typeof entry.externalUserId === 'string' ? entry.externalUserId.trim() : '';
    if (!externalUserId) return { accounts: [], error: `第${index + 1}条缺少外部用户ID。` };
    if (externalIds.has(externalUserId)) {
      return { accounts: [], error: `外部用户 ID“${externalUserId}”在本次数据中重复，请先合并记录。` };
    }
    externalIds.add(externalUserId);
    const invalidField = Object.entries(entry).find(
      ([key, fieldValue]) => typeof fieldValue !== 'string' || fieldValue.length > (importFieldLimits[key] ?? 0),
    );
    if (invalidField) {
      return { accounts: [], error: `第${index + 1}条的“${invalidField[0]}”必须是符合长度限制的文本。` };
    }
    accounts.push({
      connectorId,
      externalUserId,
      ...(typeof entry.avatarUrl === 'string' && entry.avatarUrl.trim() ? { avatarUrl: entry.avatarUrl.trim() } : {}),
      ...(typeof entry.departmentExternalId === 'string' && entry.departmentExternalId.trim()
        ? { departmentExternalId: entry.departmentExternalId.trim() }
        : {}),
      ...(typeof entry.displayName === 'string' && entry.displayName.trim()
        ? { displayName: entry.displayName.trim() }
        : {}),
      ...(typeof entry.email === 'string' && entry.email.trim() ? { email: entry.email.trim() } : {}),
      ...(typeof entry.externalUnionId === 'string' && entry.externalUnionId.trim()
        ? { externalUnionId: entry.externalUnionId.trim() }
        : {}),
      ...(typeof entry.mobile === 'string' && entry.mobile.trim() ? { mobile: entry.mobile.trim() } : {}),
      status: typeof entry.status === 'string' && entry.status.trim() ? entry.status.trim() : 'Pending',
    });
  }

  return { accounts, error: null };
}
