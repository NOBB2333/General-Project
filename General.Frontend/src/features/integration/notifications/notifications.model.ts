import type { IntegrationNotificationsApi } from '@/api/core/integration-notifications';
import type { ListPageRow } from '@/components/page/families/list/list-page.types';

export type NotificationTab = 'rules' | 'templates';

export type TemplateDraft = IntegrationNotificationsApi.TemplateSaveInput;

export interface TemplateRow extends ListPageRow {
  code: string;
  messageType: string;
  name: string;
  remark: string;
  status: string;
}

export interface RuleDraft {
  channelType: string;
  conditionJson: string;
  connectorId: string;
  eventCode: string;
  eventName: string;
  id: string;
  name: string;
  quietEndTime: string;
  quietStartTime: string;
  rateLimitMaxCount: string;
  rateLimitWindowMinutes: string;
  recipientRuleJson: string;
  remark: string;
  ruleCode: string;
  status: string;
  targetType: string;
  templateId: string;
}

export interface RuleRow extends ListPageRow {
  channel: string;
  event: string;
  lastTriggered: string;
  name: string;
  status: string;
  template: string;
}

const templateTypeLabels: Record<string, string> = {
  Card: '卡片',
  Markdown: 'Markdown',
  Text: '文本',
};

const templateStatusLabels: Record<string, string> = {
  Active: '启用',
  Disabled: '停用',
  Draft: '草稿',
};

const ruleStatusLabels: Record<string, string> = {
  Active: '启用',
  Disabled: '停用',
  Paused: '暂停',
};

const channelLabels: Record<string, string> = {
  Group: '群消息',
  Mixed: '个人与群',
  Personal: '个人消息',
};

function isJson(value: string): boolean {
  try {
    JSON.parse(value);
    return true;
  } catch {
    return false;
  }
}

function isJsonObject(value: string): boolean {
  try {
    const parsed: unknown = JSON.parse(value);
    return parsed !== null && typeof parsed === 'object' && !Array.isArray(parsed);
  } catch {
    return false;
  }
}

function isGuid(value: string): boolean {
  return /^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$/iu.test(value);
}

export function createTemplateDraft(): TemplateDraft {
  return {
    bodyTemplate: '',
    cardJson: '{}',
    messageType: 'Text',
    name: '',
    previewVariablesJson: '{}',
    remark: '',
    status: 'Draft',
    templateCode: '',
    titleTemplate: '',
    variableSchemaJson: '{}',
  };
}

export function toTemplateDraft(template?: IntegrationNotificationsApi.Template): TemplateDraft {
  if (!template) return createTemplateDraft();
  return {
    bodyTemplate: template.bodyTemplate,
    cardJson: template.cardJson,
    id: template.id,
    messageType: template.messageType,
    name: template.name,
    previewVariablesJson: template.previewVariablesJson,
    remark: template.remark,
    status: template.status,
    templateCode: template.templateCode,
    titleTemplate: template.titleTemplate,
    variableSchemaJson: template.variableSchemaJson,
  };
}

export function validateTemplateDraft(draft: TemplateDraft): string | null {
  if (!draft.templateCode.trim()) return '请填写模板编码。';
  if (!draft.name.trim()) return '请填写模板名称。';
  if (draft.templateCode.length > 64) return '模板编码不能超过64个字符。';
  if (draft.name.length > 128) return '模板名称不能超过128个字符。';
  if (draft.titleTemplate.length > 256) return '消息标题不能超过256个字符。';
  if (draft.remark.length > 512) return '备注不能超过512个字符。';
  if (![draft.cardJson, draft.previewVariablesJson, draft.variableSchemaJson].every(isJson)) {
    return '卡片、变量说明和预览变量都必须是合法JSON。';
  }
  if (!isJsonObject(draft.previewVariablesJson) || !isJsonObject(draft.variableSchemaJson)) {
    return '变量说明和预览变量的根节点都必须是JSON对象。';
  }
  if (!['Text', 'Markdown', 'Card'].includes(draft.messageType)) return '请选择有效的消息类型。';
  if (!['Draft', 'Active', 'Disabled'].includes(draft.status)) return '请选择有效的模板状态。';
  return null;
}

export function toTemplateRow(template: IntegrationNotificationsApi.Template): TemplateRow {
  return {
    code: template.templateCode,
    id: template.id,
    messageType: templateTypeLabels[template.messageType] ?? template.messageType,
    name: template.name,
    remark: template.remark || '—',
    status: templateStatusLabels[template.status] ?? template.status,
  };
}

export function templateStatusLabel(status: string): string {
  return templateStatusLabels[status] ?? status;
}

export function createRuleDraft(): RuleDraft {
  return {
    channelType: 'Personal',
    conditionJson: '{}',
    connectorId: '',
    eventCode: '',
    eventName: '',
    id: '',
    name: '',
    quietEndTime: '',
    quietStartTime: '',
    rateLimitMaxCount: '0',
    rateLimitWindowMinutes: '0',
    recipientRuleJson: '{"mode":"Direct","targetType":"OpenId","targetIds":[]}',
    remark: '',
    ruleCode: '',
    status: 'Active',
    targetType: 'OpenId',
    templateId: '',
  };
}

export function toRuleDraft(rule?: IntegrationNotificationsApi.Rule): RuleDraft {
  if (!rule) return createRuleDraft();
  return {
    channelType: rule.channelType,
    conditionJson: rule.conditionJson,
    connectorId: rule.connectorId ?? '',
    eventCode: rule.eventCode,
    eventName: rule.eventName,
    id: rule.id,
    name: rule.name,
    quietEndTime: rule.quietEndTime,
    quietStartTime: rule.quietStartTime,
    rateLimitMaxCount: String(rule.rateLimitMaxCount),
    rateLimitWindowMinutes: String(rule.rateLimitWindowMinutes),
    recipientRuleJson: rule.recipientRuleJson,
    remark: rule.remark,
    ruleCode: rule.ruleCode,
    status: rule.status,
    targetType: rule.targetType,
    templateId: rule.templateId,
  };
}

export function validateRuleDraft(draft: RuleDraft): string | null {
  if (!draft.ruleCode.trim()) return '请填写规则编码。';
  if (!draft.name.trim()) return '请填写规则名称。';
  if (!draft.eventCode.trim()) return '请填写事件编码。';
  if (!isGuid(draft.templateId.trim())) return '请填写有效的消息模板 ID。';
  if (draft.connectorId.trim() && !isGuid(draft.connectorId.trim())) return '连接器 ID 必须是有效的 GUID。';
  if (!isJson(draft.conditionJson) || !isJson(draft.recipientRuleJson)) return '触发条件和接收人规则都必须是合法JSON。';
  if (
    ![draft.conditionJson, draft.recipientRuleJson].every((value) => {
      try {
        const parsed: unknown = JSON.parse(value);
        return parsed !== null && typeof parsed === 'object' && !Array.isArray(parsed);
      } catch {
        return false;
      }
    })
  )
    return '触发条件和接收人规则的根节点都必须是JSON对象。';
  if (draft.name.length > 128 || draft.eventCode.length > 128) return '规则名称和事件编码不能超过128个字符。';
  if (draft.ruleCode.length > 64) return '规则编码不能超过64个字符。';
  if (draft.remark.length > 512) return '备注不能超过512个字符。';

  const quietStartTime = draft.quietStartTime.trim();
  const quietEndTime = draft.quietEndTime.trim();
  const clockTimePattern = /^(?:\d|[01]\d|2[0-3]):[0-5]\d(?::[0-5]\d)?$/u;
  if (Boolean(quietStartTime) !== Boolean(quietEndTime)) return '静默开始和结束时间需要同时填写，或同时留空。';
  if ([quietStartTime, quietEndTime].some((time) => time && !clockTimePattern.test(time))) {
    return '静默时间请使用 22:00 或 22:00:00 格式。';
  }

  const windowMinutes = Number(draft.rateLimitWindowMinutes);
  const maxCount = Number(draft.rateLimitMaxCount);
  if (!Number.isInteger(windowMinutes) || windowMinutes < 0) return '频控窗口必须是非负整数。';
  if (![0, 1].includes(maxCount)) return '当前后端仅支持关闭频控（0）或窗口内限制一次（1）。';
  if (!['Personal', 'Group', 'Mixed'].includes(draft.channelType)) return '请选择有效的消息渠道。';
  if (!['Active', 'Disabled', 'Paused'].includes(draft.status)) return '请选择有效的规则状态。';
  return null;
}

export function toRuleSaveInput(draft: RuleDraft): IntegrationNotificationsApi.RuleSaveInput {
  return {
    channelType: draft.channelType,
    conditionJson: draft.conditionJson.trim() || '{}',
    connectorId: draft.connectorId.trim() || null,
    eventCode: draft.eventCode.trim(),
    eventName: draft.eventName.trim(),
    ...(draft.id ? { id: draft.id } : {}),
    name: draft.name.trim(),
    quietEndTime: draft.quietEndTime,
    quietStartTime: draft.quietStartTime,
    rateLimitMaxCount: Number(draft.rateLimitMaxCount),
    rateLimitWindowMinutes: Number(draft.rateLimitWindowMinutes),
    recipientRuleJson: draft.recipientRuleJson.trim() || '{}',
    remark: draft.remark.trim(),
    ruleCode: draft.ruleCode.trim(),
    status: draft.status,
    targetType: draft.targetType.trim() || 'OpenId',
    templateId: draft.templateId.trim(),
  };
}

export function ruleStatusLabel(status: string): string {
  return ruleStatusLabels[status] ?? status;
}

export function toRuleRow(rule: IntegrationNotificationsApi.Rule): RuleRow {
  return {
    channel: channelLabels[rule.channelType] ?? rule.channelType,
    event: [rule.eventName, rule.eventCode].filter(Boolean).join(' · '),
    id: rule.id,
    lastTriggered: rule.lastTriggeredTime ? new Date(rule.lastTriggeredTime).toLocaleString('zh-CN') : '从未触发',
    name: rule.name,
    status: ruleStatusLabels[rule.status] ?? rule.status,
    template: rule.templateName || rule.templateCode,
  };
}
