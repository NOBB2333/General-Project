import type { TenantApi } from '@/api/core/tenant';

export interface TenantDraft {
  adminEmail: string;
  adminPassword: string;
  adminUserId: string;
  adminUserName: string;
  defaultConnectionString: string;
  name: string;
  remark: string;
}

export type TenantFormMode = 'create' | 'edit';

export type TenantStatusFilter = 'all' | 'active' | 'inactive';

export function createTenantDraft(item?: TenantApi.ListItem): TenantDraft {
  return {
    adminEmail: item?.adminEmail ?? '',
    adminPassword: '',
    adminUserId: item?.adminUserId ?? '',
    adminUserName: '',
    defaultConnectionString: '',
    name: item?.name ?? '',
    remark: item?.remark ?? '',
  };
}

export function validateTenantDraft(draft: TenantDraft, mode: TenantFormMode): string | null {
  const name = draft.name.trim();
  if (!name) return '请填写租户名称。';
  if (name.length > 64) return '租户名称不能超过64个字符。';
  if (draft.adminEmail.trim().length > 256) return '管理员邮箱不能超过256个字符。';
  if (draft.remark.trim().length > 256) return '备注不能超过256个字符。';
  if (draft.defaultConnectionString.trim().length > 512) return '数据库连接串不能超过512个字符。';

  if (mode === 'create' && !draft.adminUserId.trim()) {
    const username = draft.adminUserName.trim();
    if (!/^[A-Za-z][A-Za-z0-9._-]{2,63}$/u.test(username)) {
      return '管理员账号必须以英文字母开头，只能包含英文、数字、点、下划线或中划线，长度3–64位。';
    }
    if (!draft.adminPassword.trim()) return '请设置租户管理员初始密码。';
    if (draft.adminPassword.length > 128) return '管理员初始密码不能超过128个字符。';
  }

  return null;
}

export function toTenantSaveInput(draft: TenantDraft, mode: TenantFormMode): TenantApi.SaveInput {
  const input: TenantApi.SaveInput = {
    name: draft.name.trim(),
    remark: draft.remark.trim() || null,
  };
  const connectionString = draft.defaultConnectionString.trim();
  if (connectionString) input.defaultConnectionString = connectionString;

  if (mode === 'create') {
    input.adminEmail = draft.adminEmail.trim() || null;
    input.adminUserId = draft.adminUserId.trim() || null;
    if (!input.adminUserId) {
      input.adminPassword = draft.adminPassword;
      input.adminUserName = draft.adminUserName.trim();
    }
  } else {
    input.adminUserId = draft.adminUserId.trim() || null;
  }

  return input;
}

export function filterTenants(
  tenants: readonly TenantApi.ListItem[],
  keyword: string,
  status: TenantStatusFilter,
): TenantApi.ListItem[] {
  const normalizedKeyword = keyword.trim().toLocaleLowerCase();
  return tenants.filter((tenant) => {
    if (status === 'active' && !tenant.isActive) return false;
    if (status === 'inactive' && tenant.isActive) return false;
    if (!normalizedKeyword) return true;
    return [tenant.name, tenant.adminUserName, tenant.adminEmail, tenant.remark].some((value) =>
      value?.toLocaleLowerCase().includes(normalizedKeyword),
    );
  });
}

export function getTenantMetrics(tenants: readonly TenantApi.ListItem[]) {
  return [
    { id: 'total', label: '租户总数', tone: 'primary', value: tenants.length },
    { id: 'active', label: '正常租户', tone: 'success', value: tenants.filter((tenant) => tenant.isActive).length },
    { id: 'inactive', label: '已停用', tone: 'warning', value: tenants.filter((tenant) => !tenant.isActive).length },
    {
      id: 'connection',
      label: '独立连接配置',
      tone: 'neutral',
      value: tenants.filter((tenant) => tenant.hasDefaultConnectionString).length,
    },
  ] as const;
}
