import type { AiApi } from '@/api/core/ai';
import type { ListPageRow } from '@/components/page/families/list/list-page.types';

export interface ProviderRow extends ListPageRow {
  defaultStatus: string;
  endpoint: string;
  keyStatus: string;
  modelCount: number;
  name: string;
  providerType: string;
  status: string;
}

export interface ModelRow extends ListPageRow {
  capabilities: string;
  defaultStatus: string;
  displayName: string;
  modelCode: string;
  modelType: string;
  providerName: string;
  status: string;
}

export interface ProviderDraft {
  apiKey: string;
  endpoint: string;
  isDefault: boolean;
  isEnabled: boolean;
  name: string;
  organizationId: string;
  providerType: string;
  remark: string;
}

export interface ModelDraft {
  capabilitiesText: string;
  contextWindow: string;
  currency: string;
  displayName: string;
  fallbackModelId: string;
  inputTokenPrice: string;
  isDefault: boolean;
  isEnabled: boolean;
  maxOutputTokens: string;
  modelCode: string;
  modelType: string;
  outputTokenPrice: string;
  remark: string;
  sort: string;
}

export function createProviderDraft(): ProviderDraft {
  return {
    apiKey: '',
    endpoint: '',
    isDefault: false,
    isEnabled: true,
    name: '',
    organizationId: '',
    providerType: 'OpenAIResponses',
    remark: '',
  };
}

export function toProviderDraft(provider?: AiApi.Provider | null): ProviderDraft {
  if (!provider) return createProviderDraft();
  return {
    ...createProviderDraft(),
    endpoint: provider.endpoint,
    isDefault: provider.isDefault,
    isEnabled: provider.isEnabled,
    name: provider.name,
    organizationId: provider.organizationId,
    providerType: provider.providerType,
    remark: provider.remark,
  };
}

export function toProviderSaveInput(draft: ProviderDraft): AiApi.ProviderSaveInput {
  return {
    apiKey: draft.apiKey.trim(),
    endpoint: draft.endpoint.trim(),
    isDefault: draft.isDefault,
    isEnabled: draft.isEnabled,
    name: draft.name.trim(),
    organizationId: draft.organizationId.trim(),
    providerType: draft.providerType,
    remark: draft.remark.trim(),
  };
}

export function toProviderRow(provider: AiApi.Provider): ProviderRow {
  return {
    defaultStatus: provider.isDefault ? '默认供应商' : '—',
    endpoint: provider.endpoint || '—',
    id: provider.id,
    keyStatus: provider.hasApiKey ? '已配置' : '未配置',
    modelCount: provider.modelCount,
    name: provider.name,
    providerType: provider.providerType,
    status: provider.isEnabled ? '启用' : '停用',
  };
}

export function createModelDraft(): ModelDraft {
  return {
    capabilitiesText: 'textGeneration\nstreaming\ntoolCalling',
    contextWindow: '0',
    currency: 'USD',
    displayName: '',
    fallbackModelId: '',
    inputTokenPrice: '0',
    isDefault: false,
    isEnabled: true,
    maxOutputTokens: '0',
    modelCode: '',
    modelType: 'standard',
    outputTokenPrice: '0',
    remark: '',
    sort: '0',
  };
}

export function toModelDraft(model?: AiApi.Model | null): ModelDraft {
  if (!model) return createModelDraft();
  return {
    capabilitiesText: model.capabilities.join('\n'),
    contextWindow: String(model.contextWindow),
    currency: model.currency,
    displayName: model.displayName,
    fallbackModelId: model.fallbackModelId ?? '',
    inputTokenPrice: String(model.inputTokenPrice),
    isDefault: model.isDefault,
    isEnabled: model.isEnabled,
    maxOutputTokens: String(model.maxOutputTokens),
    modelCode: model.modelCode,
    modelType: model.modelType,
    outputTokenPrice: String(model.outputTokenPrice),
    remark: model.remark,
    sort: String(model.sort),
  };
}

export function validateModelDraft(draft: ModelDraft): string | null {
  if (!draft.modelCode.trim()) return '请输入模型编码。';
  if (!draft.displayName.trim()) return '请输入模型名称。';
  if (!draft.modelType.trim()) return '请输入模型类型。';
  const integerFields = [
    ['上下文窗口', draft.contextWindow],
    ['最大输出 Token 数', draft.maxOutputTokens],
    ['排序值', draft.sort],
  ] as const;
  for (const [label, raw] of integerFields) {
    const value = Number(raw);
    if (!Number.isInteger(value) || value < 0) return `${label}必须是非负整数。`;
  }
  const priceFields = [
    ['输入 Token 单价', draft.inputTokenPrice],
    ['输出 Token 单价', draft.outputTokenPrice],
  ] as const;
  for (const [label, raw] of priceFields) {
    const value = Number(raw);
    if (!Number.isFinite(value) || value < 0) return `${label}必须是非负数字。`;
  }
  return null;
}

export function toModelSaveInput(draft: ModelDraft, providerId: string): AiApi.ModelSaveInput {
  return {
    capabilities: draft.capabilitiesText
      .split(/[\n,，]/u)
      .map((capability) => capability.trim())
      .filter(Boolean),
    contextWindow: Number(draft.contextWindow),
    currency: draft.currency.trim().toUpperCase() || 'USD',
    displayName: draft.displayName.trim(),
    fallbackModelId: draft.fallbackModelId || null,
    inputTokenPrice: Number(draft.inputTokenPrice),
    isDefault: draft.isDefault,
    isEnabled: draft.isEnabled,
    maxOutputTokens: Number(draft.maxOutputTokens),
    modelCode: draft.modelCode.trim(),
    modelType: draft.modelType.trim(),
    outputTokenPrice: Number(draft.outputTokenPrice),
    providerId,
    remark: draft.remark.trim(),
    sort: Number(draft.sort),
  };
}

export function toModelRow(model: AiApi.Model): ModelRow {
  return {
    capabilities: model.capabilities.join('、') || '—',
    defaultStatus: model.isDefault ? '默认模型' : '—',
    displayName: model.displayName,
    id: model.id,
    modelCode: model.modelCode,
    modelType: model.modelType,
    providerName: model.providerName,
    status: model.isEnabled ? '启用' : '停用',
  };
}
