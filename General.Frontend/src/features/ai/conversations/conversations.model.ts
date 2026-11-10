import type { AiApi } from '@/api/core/ai';
import type {
  AiPlanStepDisplay,
  AiRunStepDisplay,
  AiToolInvocationDisplay,
  FeishuConversation,
  FeishuMessage,
} from '@/features/sales/model/feishu-orders.types';

type JsonObject = Record<string, unknown>;

function asObject(value: unknown): JsonObject | undefined {
  return typeof value === 'object' && value !== null && !Array.isArray(value) ? (value as JsonObject) : undefined;
}

function asText(value: unknown): string {
  return typeof value === 'string' ? value : '';
}

function asNumber(value: unknown): number {
  return typeof value === 'number' && Number.isFinite(value) ? value : 0;
}

function statusLabel(status: string): string {
  const normalized = status.toLowerCase();
  if (normalized === 'completed') return '已完成';
  if (normalized === 'failed') return '失败';
  if (normalized === 'draft') return '草稿';
  if (normalized === 'running') return '执行中';
  if (normalized === 'timeout') return '超时';
  if (normalized === 'skipped') return '已跳过';
  return status || '未知';
}

function parseAiPlan(value: unknown) {
  const plan = asObject(value);
  if (!plan || asText(plan.mode).toLowerCase() !== 'plan' || asText(plan.type).toLowerCase() !== 'plan')
    return undefined;
  const rawSteps = Array.isArray(plan.steps) ? plan.steps : [];
  const steps = rawSteps.flatMap((rawStep, index): AiPlanStepDisplay[] => {
    const step = asObject(rawStep);
    if (!step) return [];
    return [
      {
        expectedOutput: asText(step.expectedOutput),
        id: asText(step.id) || `step-${index + 1}`,
        isDone: step.isDone === true,
        title: asText(step.title) || `步骤 ${index + 1}`,
      },
    ];
  });
  return { confirmed: plan.confirmed === true, goal: asText(plan.goal) || 'AI 计划', steps };
}

function parseRunSteps(value: unknown): AiRunStepDisplay[] {
  if (!Array.isArray(value)) return [];
  return value.flatMap((rawStep): AiRunStepDisplay[] => {
    const step = asObject(rawStep);
    if (!step) return [];
    return [
      {
        code: asText(step.code),
        status: statusLabel(asText(step.status)),
        summary: asText(step.summary),
        title: asText(step.title),
      },
    ];
  });
}

function parseToolInvocations(value: unknown): AiToolInvocationDisplay[] {
  if (!Array.isArray(value)) return [];
  return value.flatMap((rawInvocation): AiToolInvocationDisplay[] => {
    const invocation = asObject(rawInvocation);
    if (!invocation) return [];
    return [
      {
        elapsedMilliseconds: asNumber(invocation.elapsedMilliseconds),
        errorMessage: asText(invocation.errorMessage),
        invocationId: asText(invocation.invocationId),
        status: statusLabel(asText(invocation.status)),
        toolCode: asText(invocation.toolCode),
      },
    ];
  });
}

export function toConversationMessage(message: AiApi.Message, retryText = ''): FeishuMessage {
  let metadata: JsonObject = {};
  try {
    metadata = asObject(JSON.parse(message.metadataJson)) ?? {};
  } catch {
    metadata = {};
  }

  const plan = parseAiPlan(metadata.structured);
  const tokens =
    message.totalTokens > 0
      ? `Token（服务端记录）: 输入 ${message.promptTokens} · 输出 ${message.completionTokens} · 合计 ${message.totalTokens}`
      : '';
  return {
    id: message.id,
    sender: message.role.toLowerCase() === 'user' ? 'user' : 'bot',
    kind: plan ? 'plan' : 'text',
    time: new Date(message.creationTime).toLocaleString('zh-CN', {
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    }),
    text: message.content || message.errorMessage,
    aiPlan: plan,
    aiRunSteps: parseRunSteps(metadata.runSteps),
    aiToolInvocations: parseToolInvocations(metadata.toolInvocations),
    citations: message.citations,
    errorMessage: message.errorMessage,
    modelName: message.modelName,
    retryText: retryText || undefined,
    statusLabel: statusLabel(message.status),
    tokenSummary: tokens,
  };
}

export function toConversationListItem(conversation: AiApi.Conversation): FeishuConversation {
  return {
    id: conversation.id,
    name: conversation.title || '新对话',
    preview: conversation.lastMessageSummary || '暂无消息',
    time: new Date(conversation.lastMessageTime ?? conversation.creationTime).toLocaleDateString('zh-CN', {
      month: '2-digit',
      day: '2-digit',
    }),
    unread: 0,
    pinned: false,
    tone: 'bot',
    status: conversation.status,
  };
}

export function makeTransientMessage(
  id: string,
  role: 'assistant' | 'user',
  content: string,
  status: string,
  retryText = '',
): FeishuMessage {
  return {
    id,
    sender: role === 'user' ? 'user' : 'bot',
    kind: 'text',
    time: new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit', hour12: false }),
    text: content,
    statusLabel: status,
    transient: role === 'assistant',
    retryText: retryText || undefined,
  };
}

export function statusLabelForConversation(status: string): string {
  return statusLabel(status);
}
