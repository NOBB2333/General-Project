import type { ConfigApi } from '@/api/core/config';
import type { ListPageRow } from '@/components/page/families/list/list-page.types';

const knownConfigTypes: Readonly<Record<string, string>> = {
  'Admin.Platform.SystemName': 'string',
  'Admin.Platform.LoginPageTitle': 'string',
  'Admin.Platform.DefaultPageSize': 'number',
  'Admin.Platform.SchedulerRecordKeepLastN': 'number',
  'Admin.Platform.AuditLogRetentionDays': 'number',
  'Admin.Platform.ProjectEvmWorklogHourlyRate': 'number',
  'Admin.Platform.IntegrationWebhookIpWhitelist': 'string',
  'Admin.Platform.IntegrationWebhookRequestLimitPerMinute': 'number',
  'Admin.Platform.IntegrationMessageSendLimitPerMinute': 'number',
};

const groups: Readonly<Record<string, string>> = {
  audit: '审计日志',
  general: '通用',
  integration: '外部系统集成',
  scheduler: '定时任务',
  ui: '界面',
  project: '项目管理',
};

export function isKnownSafeConfig(item: ConfigApi.Item): boolean {
  return Object.hasOwn(knownConfigTypes, item.code) && knownConfigTypes[item.code] === item.valueType;
}

export function canEditConfig(item: ConfigApi.Item): boolean {
  return !item.isReadonly && isKnownSafeConfig(item);
}

export function configGroup(item: ConfigApi.Item): string {
  return groups[item.groupCode] ?? (item.groupName.trim() || '其他配置');
}

export function configValue(item: ConfigApi.Item, field: 'value' | 'defaultValue' = 'value'): string {
  return isKnownSafeConfig(item) ? item[field] || '（空值）' : '未接入（内容不展示）';
}

export function configDescription(item: ConfigApi.Item): string {
  if (item.code === 'Admin.Platform.SystemName' || item.code === 'Admin.Platform.LoginPageTitle') {
    return `${item.description} 新管理端标题目前使用前端部署配置。`;
  }
  if (item.code === 'Admin.Platform.DefaultPageSize') {
    return `${item.description} 新管理端目前使用各页面的分页参数。`;
  }
  return item.description;
}

export function buildConfigSaveInput(
  item: ConfigApi.Item,
  rawValue: string,
  tenantScoped: boolean,
): ConfigApi.SaveInput {
  if (!canEditConfig(item)) throw new Error('该配置只读或尚未接入安全编辑。');
  let value = rawValue.trim();
  if (value.length > 512) throw new Error('配置值不能超过512个字符。');
  if (item.valueType === 'number') {
    if (!/^\d+$/u.test(value) || Number(value) > 2147483647) throw new Error('配置值必须是0至2147483647之间的整数。');
    value = String(Number(value));
  }
  // 租户使用服务端当前上下文，不接收可编辑的目标租户ID。
  return { value, providerName: tenantScoped ? 'T' : 'G', ...(tenantScoped ? {} : { providerKey: null }) };
}

export function expectedConfigValue(item: ConfigApi.Item, input: ConfigApi.SaveInput): string {
  return input.value || item.defaultValue;
}

export interface ConfigRow extends ListPageRow {
  id: string;
  code: string;
  name: string;
  group: string;
  scope: string;
  value: string;
  defaultValue: string;
  type: string;
  description: string;
  editing: string;
}

function configValuePreview(value: string): string {
  const characters = Array.from(value);
  return characters.length > 14 ? `${characters.slice(0, 14).join('')}…` : value;
}

export function toConfigRow(item: ConfigApi.Item, tenantScoped: boolean): ConfigRow {
  return {
    id: item.code,
    code: item.code,
    name: item.name,
    group: configGroup(item),
    scope: tenantScoped ? (item.hasTenantValue ? '当前租户' : '继承全局 / 默认') : '全局 / 默认',
    value: configValuePreview(configValue(item)),
    defaultValue: configValuePreview(configValue(item, 'defaultValue')),
    type: item.valueType === 'number' ? '整数' : item.valueType === 'string' ? '文本' : '未接入',
    description: configDescription(item),
    editing: canEditConfig(item) ? '可编辑' : '只读 / 未接入',
  };
}
