import type { ListPageRow } from '@/components/page/families/list/list-page.types';
import type { IntegrationOperationsApi } from '@/api/core/integration-operations';

export type IntegrationOperationsSection = 'delivery' | 'sync' | 'webhook';
export type SyncListMode = 'jobs' | 'records';

export interface DeliveryRow extends ListPageRow {
  attempt: string;
  connector: string;
  createdAt: string;
  duration: string;
  messageType: string;
  nextRetryAt: string;
  sourceType: string;
  status: string;
  targetType: string;
}

export interface SyncJobDraft extends IntegrationOperationsApi.SyncJobSaveInput {
  id: string;
}

export interface SyncJobRow extends ListPageRow {
  connector: string;
  cron: string;
  direction: string;
  enabled: string;
  lastRun: string;
  lastRunStatus: string;
  name: string;
  syncType: string;
}

export interface SyncRecordRow extends ListPageRow {
  counts: string;
  connector: string;
  createdAt: string;
  duration: string;
  mode: string;
  retryCount: number;
  status: string;
  syncType: string;
}

export interface WebhookEventRow extends ListPageRow {
  connector: string;
  eventType: string;
  provider: string;
  receivedAt: string;
  retryCount: number;
  signature: string;
  sourceIp: string;
  status: string;
}

const deliveryStatusLabels: Record<string, string> = {
  DeadLetter: '死信',
  Failed: '失败',
  Pending: '待发送',
  Sending: '发送中',
  Skipped: '已跳过',
  Succeeded: '成功',
};

const syncRecordStatusLabels: Record<string, string> = {
  Failed: '失败',
  Pending: '待执行',
  Running: '执行中',
  Skipped: '已跳过',
  Succeeded: '成功',
};

const webhookStatusLabels: Record<string, string> = {
  Failed: '失败',
  Parsed: '已解析',
  Processed: '已处理',
  Received: '已接收',
  Skipped: '已跳过',
};

const syncTypeLabels: Record<string, string> = {
  Custom: '自定义',
  Department: '组织',
  Group: '群',
  Meeting: '会议',
  User: '用户',
};

const syncDirectionLabels: Record<string, string> = {
  Bidirectional: '双向',
  Inbound: '导入',
  Outbound: '导出',
};

const syncModeLabels: Record<string, string> = {
  Full: '全量',
  Incremental: '增量',
  Manual: '手动',
};

const providerTypeLabels: Record<string, string> = {
  DingTalk: '钉钉',
  Feishu: '飞书',
  Sandbox: '集成沙箱',
  WeCom: '企业微信',
};

const messageTypeLabels: Record<string, string> = {
  Card: '卡片',
  Markdown: 'Markdown',
  Text: '文本',
};

export function deliveryStatusLabel(value: string): string {
  return deliveryStatusLabels[value] ?? value;
}

export function syncJobStatusLabel(enabled: boolean): string {
  return enabled ? '启用' : '停用';
}

export function syncRecordStatusLabel(value: string): string {
  return syncRecordStatusLabels[value] ?? value;
}

export function webhookStatusLabel(value: string): string {
  return webhookStatusLabels[value] ?? value;
}

export function syncTypeLabel(value: string): string {
  return syncTypeLabels[value] ?? value;
}

export function syncDirectionLabel(value: string): string {
  return syncDirectionLabels[value] ?? value;
}

export function syncModeLabel(value: string): string {
  return syncModeLabels[value] ?? value;
}

export function providerTypeLabel(value: string): string {
  return providerTypeLabels[value] ?? value;
}

export function messageTypeLabel(value: string): string {
  return messageTypeLabels[value] ?? value;
}

export function isDeliveryRetryable(record: IntegrationOperationsApi.DeliveryRecord): boolean {
  return record.status === 'Failed' || record.status === 'DeadLetter';
}

export function isSyncRecordRetryable(record: IntegrationOperationsApi.SyncRecord): boolean {
  return record.status === 'Failed';
}

export function isWebhookRetryable(event: IntegrationOperationsApi.WebhookEvent): boolean {
  return event.status === 'Failed';
}

export function toDeliveryRow(record: IntegrationOperationsApi.DeliveryRecord): DeliveryRow {
  return {
    attempt: `${record.attemptCount} / ${record.maxRetryCount}`,
    connector: record.connectorCode,
    createdAt: formatOperationsDate(record.creationTime),
    duration: `${record.durationMilliseconds} ms`,
    id: record.id,
    messageType: messageTypeLabel(record.messageType),
    nextRetryAt: formatOperationsDate(record.nextRetryTime),
    sourceType: record.sourceType,
    status: deliveryStatusLabel(record.status),
    targetType: record.targetType,
  };
}

export function createSyncJobDraft(): SyncJobDraft {
  return {
    configJson: '{}',
    connectorId: '',
    cronExpression: '',
    direction: 'Inbound',
    id: '',
    isEnabled: true,
    jobCode: '',
    mode: 'Incremental',
    name: '',
    remark: '',
    syncType: 'User',
  };
}

export function toSyncJobDraft(job?: IntegrationOperationsApi.SyncJob): SyncJobDraft {
  if (!job) return createSyncJobDraft();
  return {
    configJson: job.configJson,
    connectorId: job.connectorId,
    cronExpression: job.cronExpression,
    direction: job.direction,
    id: job.id,
    isEnabled: job.isEnabled,
    jobCode: job.jobCode,
    mode: job.mode,
    name: job.name,
    remark: job.remark,
    syncType: job.syncType,
  };
}

export function validateSyncJobDraft(draft: SyncJobDraft): string | null {
  if (!draft.jobCode.trim()) return '请填写任务编码。';
  if (!draft.name.trim()) return '请填写任务名称。';
  if (!isGuid(draft.connectorId.trim())) return '请选择连接器或填写有效的连接器 ID。';
  if (draft.jobCode.trim().length > 64) return '任务编码不能超过64个字符。';
  if (draft.name.trim().length > 128) return '任务名称不能超过128个字符。';
  if (draft.cronExpression.length > 128) return 'Cron表达式不能超过128个字符。';
  if (draft.configJson.length > 16_000) return '同步配置不能超过16000个字符。';
  if (draft.remark.length > 512) return '备注不能超过512个字符。';
  if (!['Department', 'Group', 'Meeting', 'User', 'Custom'].includes(draft.syncType)) return '同步类型无效。';
  if (!['Inbound', 'Outbound', 'Bidirectional'].includes(draft.direction)) return '同步方向无效。';
  if (!['Full', 'Incremental', 'Manual'].includes(draft.mode)) return '同步模式无效。';

  const parsed = parseJsonObject(draft.configJson);
  if (!parsed) return '同步配置必须是合法JSON对象。';
  if (hasSensitiveKey(parsed)) return '同步映射配置不能包含密码、Token或连接器凭证字段。';
  return null;
}

export function toSyncJobSaveInput(draft: SyncJobDraft): IntegrationOperationsApi.SyncJobSaveInput {
  return {
    configJson: draft.configJson.trim(),
    connectorId: draft.connectorId.trim(),
    cronExpression: draft.cronExpression.trim(),
    direction: draft.direction,
    ...(draft.id ? { id: draft.id } : {}),
    isEnabled: draft.isEnabled,
    jobCode: draft.jobCode.trim(),
    mode: draft.mode,
    name: draft.name.trim(),
    remark: draft.remark.trim(),
    syncType: draft.syncType,
  };
}

export function toSyncJobRow(job: IntegrationOperationsApi.SyncJob): SyncJobRow {
  return {
    connector: job.connectorCode,
    cron: job.cronExpression || '仅手动',
    direction: syncDirectionLabel(job.direction),
    enabled: syncJobStatusLabel(job.isEnabled),
    id: job.id,
    lastRun: formatOperationsDate(job.lastRunTime),
    lastRunStatus: job.lastRunStatus ? syncRecordStatusLabel(job.lastRunStatus) : '未执行',
    name: job.name,
    syncType: syncTypeLabel(job.syncType),
  };
}

export function toSyncRecordRow(record: IntegrationOperationsApi.SyncRecord): SyncRecordRow {
  return {
    counts: `新增 ${record.createdCount} · 更新 ${record.updatedCount} · 跳过 ${record.skippedCount} · 失败 ${record.failedCount}`,
    connector: record.connectorCode,
    createdAt: formatOperationsDate(record.creationTime),
    duration: `${record.durationMilliseconds} ms`,
    id: record.id,
    mode: syncModeLabel(record.mode),
    retryCount: record.retryCount,
    status: syncRecordStatusLabel(record.status),
    syncType: syncTypeLabel(record.syncType),
  };
}

export function toWebhookEventRow(event: IntegrationOperationsApi.WebhookEvent): WebhookEventRow {
  return {
    connector: event.connectorCode || '—',
    eventType: event.eventType,
    id: event.id,
    provider: providerTypeLabel(event.providerType),
    receivedAt: formatOperationsDate(event.creationTime),
    retryCount: event.retryCount,
    signature: signatureStatusLabel(event.signatureStatus),
    sourceIp: event.sourceIp || '—',
    status: webhookStatusLabel(event.status),
  };
}

export function formatOperationsDate(value: null | string | undefined): string {
  if (!value) return '';
  const date = new Date(value);
  if (!Number.isFinite(date.getTime())) return '';
  return date.toLocaleString('zh-CN', { dateStyle: 'short', timeStyle: 'short' });
}

export function containsSensitiveSyncConfig(value: string): boolean {
  const parsed = parseJsonObject(value);
  return parsed ? hasSensitiveKey(parsed) : true;
}

function isGuid(value: string): boolean {
  return /^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$/iu.test(value);
}

function parseJsonObject(value: string): Record<string, unknown> | null {
  try {
    const parsed: unknown = JSON.parse(value);
    return parsed !== null && typeof parsed === 'object' && !Array.isArray(parsed)
      ? (parsed as Record<string, unknown>)
      : null;
  } catch {
    return null;
  }
}

function hasSensitiveKey(value: unknown): boolean {
  if (Array.isArray(value)) return value.some(hasSensitiveKey);
  if (!value || typeof value !== 'object') return false;
  return Object.entries(value).some(
    ([key, nested]) =>
      /secret|token|password|credential|authorization|webhook|private.?key|api.?key|app.?key|access.?key|signing.?key/iu.test(
        key,
      ) || hasSensitiveKey(nested),
  );
}

export function signatureStatusLabel(value: string): string {
  switch (value) {
    case 'Valid':
      return '通过';
    case 'Invalid':
      return '未通过';
    case 'Unchecked':
      return '未检查';
    default:
      return value || '未知';
  }
}
