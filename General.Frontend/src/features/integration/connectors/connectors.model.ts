import type { IntegrationConnectorsApi } from '@/api/core/integration-connectors';
import type { ListPageRow } from '@/components/page/families/list/list-page.types';

export interface ConnectorDraft {
  callbackPath: string;
  clearRobotWebhook: boolean;
  configJson: string;
  isEnabled: boolean;
  remark: string;
  robotWebhook: string;
  secret: string;
}

export interface ConnectorRow extends ListPageRow {
  capabilities: string;
  configuration: string;
  health: string;
  lastTest: string;
  provider: string;
  remark: string;
  status: string;
}

const secretLabelMap: Record<string, string> = {
  appSecret: '应用密钥',
  corpSecret: '企业微信应用密钥',
  encodingAesKey: '消息加密密钥',
  encryptKey: '消息加密密钥',
  robotSecret: '机器人签名密钥',
  token: '回调校验令牌',
  verificationToken: '回调校验令牌',
};

const configLabelMap: Record<string, string> = {
  agentId: '应用 Agent ID',
  appId: '应用 ID',
  appKey: '应用 Key',
  baseUrl: '接口地址',
  corpId: '企业 ID',
  robotWebhook: '群机器人 Webhook 或 Key',
};

export function createConnectorDraft(): ConnectorDraft {
  return {
    callbackPath: '',
    clearRobotWebhook: false,
    configJson: '{}',
    isEnabled: true,
    remark: '',
    robotWebhook: '',
    secret: '',
  };
}

export function toConnectorDraft(connector: IntegrationConnectorsApi.Connector): ConnectorDraft {
  return {
    callbackPath: connector.callbackPath,
    clearRobotWebhook: false,
    configJson: connector.configJson || '{}',
    isEnabled: connector.isTenantEnabled,
    remark: connector.remark,
    robotWebhook: '',
    secret: '',
  };
}

export function toConnectorSaveInput(draft: ConnectorDraft): IntegrationConnectorsApi.TenantConfigSaveInput {
  return {
    callbackPath: draft.callbackPath.trim(),
    configJson: draft.configJson.trim() || '{}',
    isEnabled: draft.isEnabled,
    remark: draft.remark.trim(),
    ...(draft.clearRobotWebhook
      ? { robotWebhook: '' }
      : draft.robotWebhook.trim()
        ? { robotWebhook: draft.robotWebhook.trim() }
        : {}),
    secret: draft.secret.trim(),
  };
}

export function validateConnectorDraft(
  draft: ConnectorDraft,
  connector: IntegrationConnectorsApi.Connector,
): string | null {
  if (draft.callbackPath.length > 512) return '回调路径不能超过512个字符。';
  if (draft.remark.length > 512) return '备注不能超过512个字符。';
  if (draft.secret.length > 4096) return '连接密钥不能超过4096个字符。';
  if (draft.robotWebhook.length > 4096) return '机器人Webhook或Key不能超过4096个字符。';
  if (draft.configJson.length > 16_000) return '连接配置不能超过16000个字符。';

  let config: unknown;
  try {
    config = JSON.parse(draft.configJson || '{}');
  } catch {
    return '连接配置必须是合法JSON。';
  }
  if (config === null || Array.isArray(config) || typeof config !== 'object') {
    return '连接配置必须是JSON对象。';
  }

  const secretFields = parseSecretFieldKeys(connector.configSchemaJson);
  const configuredSecretField = [...secretFields].find((field) => {
    const value = (config as Record<string, unknown>)[field];
    return value !== undefined && value !== null && String(value).trim() !== '';
  });
  if (configuredSecretField) {
    const label = secretLabelMap[configuredSecretField] ?? configuredSecretField;
    return `${label}属于密钥字段，请填写在连接密钥输入框，不能放入普通配置JSON。`;
  }
  if (Object.keys(config as Record<string, unknown>).some((key) => key.toLowerCase() === 'robotwebhook')) {
    return '机器人Webhook或Key请填写在单独的凭证输入框。';
  }

  return null;
}

export function parseSecretFieldKeys(configSchemaJson: string): string[] {
  try {
    const schema = JSON.parse(configSchemaJson) as { fields?: unknown };
    if (!Array.isArray(schema.fields)) return [];
    return schema.fields.flatMap((field) => {
      if (!field || typeof field !== 'object') return [];
      const item = field as { Key?: unknown; Secret?: unknown; key?: unknown; secret?: unknown };
      const key = typeof item.Key === 'string' ? item.Key : typeof item.key === 'string' ? item.key : '';
      const secret = item.Secret === true || item.secret === true;
      return key && secret ? [key] : [];
    });
  } catch {
    return [];
  }
}

export function getConnectorSchemaSummary(configSchemaJson: string): string {
  try {
    const schema = JSON.parse(configSchemaJson) as { fields?: unknown };
    if (!Array.isArray(schema.fields)) return '后端未提供配置字段说明。';
    const labels = schema.fields.flatMap((field) => {
      if (!field || typeof field !== 'object') return [];
      const item = field as { Key?: unknown; Secret?: unknown; key?: unknown; secret?: unknown };
      const key = typeof item.Key === 'string' ? item.Key : typeof item.key === 'string' ? item.key : '';
      if (!key) return [];
      return [
        (item.Secret === true || item.secret === true ? secretLabelMap[key] : undefined) ?? configLabelMap[key] ?? key,
      ];
    });
    return labels.length ? `配置项：${labels.join('、')}` : '后端未提供配置字段说明。';
  } catch {
    return '后端配置字段说明不可读取。';
  }
}

export function toConnectorRow(
  connector: IntegrationConnectorsApi.Connector,
  health?: IntegrationConnectorsApi.ConnectorHealth,
): ConnectorRow {
  const configuredStatus = connector.isConfigured ? '已配置' : '未配置';
  const secretStatus = connector.hasSecret ? '密钥已录入' : '缺少密钥';
  const webhookStatus = connector.hasRobotWebhook ? '机器人凭证已录入' : '';
  return {
    capabilities: connector.capabilities.join('、') || '—',
    configuration: [configuredStatus, secretStatus, webhookStatus].filter(Boolean).join(' / '),
    health: health ? healthLabel(health.healthStatus) : '状态未知',
    id: connector.id,
    lastTest: [connector.lastTestStatus || '未测试', formatConnectorDate(connector.lastTestTime)]
      .filter(Boolean)
      .join(' / '),
    name: connector.displayName,
    provider: connector.providerType,
    remark: connector.remark,
    status: connector.isEnabled ? (connector.isTenantEnabled ? '启用' : '停用') : '连接器已停用',
  };
}

function healthLabel(value: string): string {
  switch (value) {
    case 'Healthy':
      return '健康';
    case 'Unconfigured':
      return '未配置';
    case 'Unhealthy':
      return '异常';
    case 'Disabled':
      return '停用';
    case 'Warning':
      return '待检查';
    default:
      return '状态未知';
  }
}

export function formatConnectorDate(value: null | string | undefined): string {
  if (!value) return '';
  const date = new Date(value);
  if (!Number.isFinite(date.getTime())) return '';
  return date.toLocaleString('zh-CN', { dateStyle: 'short', timeStyle: 'short' });
}
