<script lang="ts" setup>
import type { AiApi } from '#/api/core';

import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Page } from '@vben/common-ui';

import { Button, Checkbox, Drawer, Empty, Form, Input, Popconfirm, Select, Space, Spin, Tag, Tooltip, message } from 'ant-design-vue';

import {
  archiveAiConversationApi,
  confirmAiConversationPlanApi,
  createAiConversationApi,
  createAiConversationPlanDraftApi,
  deleteAiConversationApi,
  exportAiConversationApi,
  getAiAgentsApi,
  getAiCommandsApi,
  getAiConversationApi,
  getAiConversationsApi,
  getAiModelsApi,
  getAiSkillsApi,
  getAiToolInvocationsApi,
  restoreAiConversationApi,
  sendAiConversationMessageApi,
  streamAiConversationMessageApi,
  toggleAiConversationPlanStepApi,
  updateAiConversationApi,
} from '#/api/core';
import { useActionLoading } from '#/composables/platform/use-action-loading';

import AiMarkdownMessage from './components/AiMarkdownMessage.vue';

defineOptions({ name: 'AiConversationsPage' });

const route = useRoute();
const router = useRouter();

const modeOptions = [
  { label: '通用会话', value: 'General' },
  { label: '澄清模式', value: 'Clarify' },
  { label: '规划模式', value: 'Plan' },
  { label: '知识问答', value: 'Knowledge' },
  { label: '执行辅助', value: 'Execution' },
];

const contextOptions = [
  { label: '无上下文', value: 'None' },
  { label: '项目上下文', value: 'Project' },
  { label: '经营上下文', value: 'Business' },
  { label: '文档上下文', value: 'Document' },
  { label: '风险上下文', value: 'Risk' },
];

const statusOptions = [
  { label: '进行中', value: 'Active' },
  { label: '已归档', value: 'Archived' },
  { label: '全部', value: 'All' },
];

interface StructuredQuestion {
  answerType?: string;
  id: string;
  required?: boolean;
  text: string;
}

interface StructuredPlanStep {
  description?: string;
  expectedOutput?: string;
  id: string;
  isDone?: boolean;
  ownerRole?: string;
  title: string;
}

interface StructuredPayload {
  assumptions?: string[];
  canComplete?: boolean;
  confirmed?: boolean;
  draftCreated?: boolean;
  goal?: string;
  mode?: string;
  nextActions?: string[];
  questions?: StructuredQuestion[];
  risks?: string[];
  steps?: StructuredPlanStep[];
  summary?: string;
  title?: string;
  type?: string;
}

interface MessageMetadata {
  availableTools?: string[];
  contextEngineering?: ContextEngineeringSummary;
  runSteps?: RunStepSummary[];
  structured?: StructuredPayload;
  toolLoop?: ToolLoopSummary;
  toolInvocations?: ToolInvocationSummary[];
}

interface ContextEngineeringSummary {
  automaticCompression?: boolean;
  citationCount?: number;
  compressedMessageCount?: number;
  contextBudgetTokens?: number;
  estimatedPromptTokens?: number;
  hasInjectedContext?: boolean;
  recentMessageCount?: number;
  selectionPolicy?: string;
  strategy?: string;
}

interface RunStepSummary {
  code?: string;
  status?: string;
  summary?: string;
  title?: string;
}

interface ToolLoopSummary {
  enabled?: boolean;
  invocationCount?: number;
  iterations?: number;
  maxIterations?: number;
}

interface ToolInvocationSummary {
  elapsedMilliseconds?: number;
  errorMessage?: string;
  invocationId?: string;
  status?: string;
  toolCode?: string;
}

const conversations = ref<AiApi.ConversationItem[]>([]);
const detail = ref<AiApi.ConversationDetail>();
const models = ref<AiApi.ModelItem[]>([]);
const agents = ref<AiApi.AgentDefinitionItem[]>([]);
const commands = ref<AiApi.CommandItem[]>([]);
const skills = ref<AiApi.SkillItem[]>([]);
const selectedConversationId = ref('');
const currentModelId = ref<string>();
const selectedAgentCode = ref<string>();
const selectedCommandCode = ref<string>();
const selectedSkillCode = ref<string>();
const keyword = ref('');
const status = ref('Active');
const loadingConversations = ref(false);
const loadingDetail = ref(false);
const sending = ref(false);
const conversationDrawerVisible = ref(false);
const toolDrawerVisible = ref(false);
const recentPanelVisible = ref(false);
const runtimePanelVisible = ref(false);
const loadingToolInvocations = ref(false);
const savingConversation = ref(false);
const editingConversationId = ref<null | string>(null);
const messageContent = ref('');
const toolInvocationRecords = ref<AiApi.ToolInvocationItem[]>([]);
const activeToolMessage = ref<AiApi.MessageItem>();
const { actionLoadingKey, runAction } = useActionLoading();

const conversationForm = reactive<AiApi.ConversationCreateInput>({
  contextId: '',
  contextSnapshotJson: '{}',
  contextType: 'None',
  mode: 'General',
  modelId: undefined,
  title: '',
});

const modelOptions = computed(() =>
  models.value
    .filter((item) => item.isEnabled)
    .map((item) => ({
      label: `${item.displayName} · ${item.providerName}`,
      value: item.id,
    })),
);

const agentOptions = computed(() =>
  agents.value
    .filter((item) => item.isEnabled)
    .map((item) => ({
      label: `${item.name} · ${item.agentCode}`,
      value: item.agentCode,
    })),
);

const commandOptions = computed(() =>
  commands.value
    .filter((item) => item.isEnabled)
    .map((item) => ({
      label: `${item.slashName} ${item.name}`,
      value: item.commandCode,
    })),
);

const skillOptions = computed(() =>
  skills.value
    .filter((item) => item.isEnabled)
    .map((item) => ({
      label: `${item.name} · ${item.skillCode}`,
      value: item.skillCode,
    })),
);

const defaultModelId = computed(() => {
  return models.value.find((item) => item.isEnabled && item.isDefault)?.id || models.value.find((item) => item.isEnabled)?.id;
});

const activeRuntimeLabel = computed(() => {
  const command = commands.value.find((item) => item.commandCode === selectedCommandCode.value);
  const skill = skills.value.find((item) => item.skillCode === selectedSkillCode.value);
  const agent = agents.value.find((item) => item.agentCode === selectedAgentCode.value);
  return command?.slashName || skill?.name || agent?.name || '自动';
});

const isArchived = computed(() => detail.value?.status === 'Archived');

async function loadModels() {
  models.value = await getAiModelsApi();
}

async function loadAgentDesignOptions() {
  const [agentResult, commandResult, skillResult] = await Promise.all([
    getAiAgentsApi({ includeDisabled: false }),
    getAiCommandsApi({ includeDisabled: false }),
    getAiSkillsApi({ includeDisabled: false }),
  ]);
  agents.value = agentResult;
  commands.value = commandResult;
  skills.value = skillResult;
}

async function loadConversations(selectId?: string) {
  loadingConversations.value = true;
  try {
    conversations.value = await getAiConversationsApi({
      keyword: keyword.value.trim() || undefined,
      status: status.value,
    });

    const queryId = typeof route.query.id === 'string' ? route.query.id : '';
    const nextId = selectId || queryId || selectedConversationId.value || conversations.value[0]?.id || '';
    if (nextId && conversations.value.some((item) => item.id === nextId)) {
      await loadDetail(nextId);
    } else {
      selectedConversationId.value = '';
      detail.value = undefined;
    }
  } finally {
    loadingConversations.value = false;
  }
}

function upsertConversation(conversation: AiApi.ConversationItem) {
  const index = conversations.value.findIndex((item) => item.id === conversation.id);
  if (index >= 0) {
    conversations.value[index] = conversation;
  } else if (status.value === 'All' || conversation.status === status.value) {
    conversations.value.unshift(conversation);
  }

  conversations.value = conversations.value
    .filter((item) => status.value === 'All' || item.status === status.value)
    .sort((left, right) => {
      const leftTime = new Date(left.lastMessageTime || left.creationTime).getTime();
      const rightTime = new Date(right.lastMessageTime || right.creationTime).getTime();
      return rightTime - leftTime;
    });
}

async function loadDetail(id: string) {
  selectedConversationId.value = id;
  loadingDetail.value = true;
  try {
    detail.value = await getAiConversationApi(id);
    currentModelId.value = detail.value.modelId || undefined;
  } finally {
    loadingDetail.value = false;
  }
}

function applyConversationDetail(nextDetail: AiApi.ConversationDetail) {
  detail.value = nextDetail;
  selectedConversationId.value = nextDetail.id;
  currentModelId.value = nextDetail.modelId || undefined;
  upsertConversation(nextDetail);
}

function formatDateTime(value?: string) {
  return value ? new Date(value).toLocaleString() : '-';
}

function getModeLabel(value?: string) {
  return modeOptions.find((item) => item.value === value)?.label || value || '-';
}

function getContextLabel(value?: string) {
  return contextOptions.find((item) => item.value === value)?.label || value || '-';
}

function getStatusLabel(value?: string) {
  return value === 'Archived' ? '已归档' : '进行中';
}

function getStatusColor(value?: string) {
  return value === 'Archived' ? 'default' : 'green';
}

function getRoleLabel(value: string) {
  if (value === 'assistant') {
    return 'AI';
  }

  if (value === 'system') {
    return '系统';
  }

  return '用户';
}

function parseMessageMetadata(item: AiApi.MessageItem): MessageMetadata {
  if (!item.metadataJson) {
    return {};
  }

  try {
    return JSON.parse(item.metadataJson) as MessageMetadata;
  } catch {
    return {};
  }
}

function resolveStructured(item: AiApi.MessageItem) {
  return parseMessageMetadata(item).structured;
}

function resolveClarification(item: AiApi.MessageItem) {
  const structured = resolveStructured(item);
  return structured?.mode === 'Clarify' ? structured : undefined;
}

function resolvePlan(item: AiApi.MessageItem) {
  const structured = resolveStructured(item);
  return structured?.mode === 'Plan' && structured.type === 'plan' ? structured : undefined;
}

function resolveToolInvocations(item: AiApi.MessageItem) {
  return parseMessageMetadata(item).toolInvocations || [];
}

function resolveAvailableTools(item: AiApi.MessageItem) {
  return parseMessageMetadata(item).availableTools || [];
}

function resolveContextEngineering(item: AiApi.MessageItem) {
  return parseMessageMetadata(item).contextEngineering;
}

function resolveRunSteps(item: AiApi.MessageItem) {
  return parseMessageMetadata(item).runSteps || [];
}

function resolveToolLoop(item: AiApi.MessageItem) {
  return parseMessageMetadata(item).toolLoop;
}

function hasRunTrace(item: AiApi.MessageItem) {
  const metadata = parseMessageMetadata(item);
  return item.role === 'assistant' && (
    !!metadata.contextEngineering ||
    !!metadata.toolLoop ||
    (metadata.availableTools?.length || 0) > 0 ||
    (metadata.runSteps?.length || 0) > 0
  );
}

function getToolInvocationColor(status?: string) {
  return status === 'Completed' ? 'green' : status === 'Timeout' ? 'orange' : status === 'Failed' ? 'red' : 'blue';
}

function getRunStepColor(status?: string) {
  return status === 'Completed' ? 'green' : status === 'Skipped' ? 'default' : status === 'Failed' ? 'red' : 'blue';
}

function formatTokenCount(value?: number) {
  const count = Number(value || 0);
  if (count >= 1_000_000) {
    return `${(count / 1_000_000).toFixed(count % 1_000_000 === 0 ? 0 : 1)}M`;
  }

  if (count >= 1_000) {
    return `${Math.round(count / 100) / 10}K`;
  }

  return `${count}`;
}

function normalizeSlashName(value?: string) {
  const text = (value || '').trim();
  return text.startsWith('/') ? text.toLowerCase() : `/${text.toLowerCase()}`;
}

function findCommandBySlashName(value: string) {
  const normalized = normalizeSlashName(value);
  return commands.value.find((item) => (
    item.isEnabled &&
    (normalizeSlashName(item.slashName) === normalized || item.commandCode.toLowerCase() === normalized.slice(1))
  ));
}

function parseSlashInput(content: string) {
  const text = content.trim();
  if (!text.startsWith('/')) {
    return undefined;
  }

  const match = /^\/([^\s]+)(?:\s+([\s\S]*))?$/.exec(text);
  if (!match) {
    return undefined;
  }

  const name = match[1] || '';
  return {
    args: (match[2] || '').trim(),
    name: name.toLowerCase(),
    slashName: `/${name.toLowerCase()}`,
  };
}

function buildSlashHelpContent() {
  const configuredCommands = commands.value
    .filter((item) => item.isEnabled)
    .sort((left, right) => left.sort - right.sort)
    .map((item) => `- \`${item.slashName}\` ${item.name}${item.description ? `：${item.description}` : ''}`);

  return [
    '可用快捷命令',
    '',
    '- `/help` 查看命令',
    '- `/new` 新建一个空会话',
    '- `/status` 查看当前会话状态',
    ...configuredCommands,
    '',
    '输入格式：`/命令 参数`。如果命令已绑定 Agent 或 Skill，本次发送会自动带上对应运行设置。',
  ].join('\n');
}

function appendLocalSystemMessage(content: string) {
  if (!detail.value) {
    message.info(content);
    return;
  }

  const lastSequenceNo = Math.max(0, ...detail.value.messages.map((item) => item.sequenceNo || 0));
  detail.value.messages = [
    ...detail.value.messages,
    {
      citations: [],
      completionTokens: 0,
      content,
      conversationId: detail.value.id,
      creationTime: new Date().toISOString(),
      errorMessage: '',
      id: `local:${Date.now()}`,
      metadataJson: '{}',
      modelId: currentModelId.value,
      modelName: '',
      promptTokens: 0,
      providerId: undefined,
      providerName: '',
      role: 'system',
      sequenceNo: lastSequenceNo + 1,
      status: 'Completed',
      totalTokens: 0,
    },
  ];
}

async function prepareSlashCommandContent(content: string) {
  const slash = parseSlashInput(content);
  if (!slash) {
    return content;
  }

  if (slash.name === 'help') {
    appendLocalSystemMessage(buildSlashHelpContent());
    messageContent.value = '';
    return undefined;
  }

  if (slash.name === 'new') {
    await createQuickConversation();
    messageContent.value = '';
    return undefined;
  }

  if (slash.name === 'status') {
    appendLocalSystemMessage([
      `当前会话：${detail.value?.title || '-'}`,
      `模型：${models.value.find((item) => item.id === currentModelId.value)?.displayName || detail.value?.modelName || '未选择'}`,
      `运行：${activeRuntimeLabel.value}`,
      `上下文：${getContextLabel(detail.value?.contextType)} ${detail.value?.contextId || '-'}`,
      `Token：${formatTokenCount(detail.value?.totalTokens)}`,
      `最近更新：${formatDateTime(detail.value?.lastMessageTime || detail.value?.creationTime)}`,
    ].join('\n'));
    messageContent.value = '';
    return undefined;
  }

  const command = findCommandBySlashName(slash.slashName);
  if (!command) {
    message.warning(`未找到快捷命令 ${slash.slashName}，输入 /help 查看可用命令`);
    return undefined;
  }

  selectedCommandCode.value = command.commandCode;
  selectedAgentCode.value = command.agentCode || selectedAgentCode.value;
  selectedSkillCode.value = command.skillCode || selectedSkillCode.value;
  return slash.args || command.exampleInput || command.inputPlaceholder || `${command.slashName} ${command.name}`;
}

function getToolDisplayName(toolCode: string) {
  if (toolCode.startsWith('mcp__')) {
    const [, serverCode = 'server', toolId = 'tool'] = toolCode.split('__');
    return `MCP/${serverCode}/${toolId.slice(0, 8)}`;
  }

  const builtinToolNames: Record<string, string> = {
    'business.contract.budget.receivable.read': '经营合同/预算/回款',
    'business.overview.read': '经营总览',
    'project.change.read': '项目变更',
    'project.documents.read': '项目文档',
    'project.evm.read': 'EVM 指标',
    'project.meeting.read': '会议记录',
    'project.overview.read': '项目概览',
    'project.quality.read': '质量记录',
    'project.raid.read': 'RAID',
    'project.schedule.read': '排期里程碑',
    'project.wbs.tasks.read': 'WBS/任务',
    'web.search': 'Web Search',
  };
  return builtinToolNames[toolCode] || toolCode;
}

function openUsageRecords(item: AiApi.MessageItem) {
  router.push({
    path: '/ai/usage',
    query: {
      conversationId: item.conversationId,
      source: 'ModelChat',
    },
  });
}

async function openToolInvocations(item: AiApi.MessageItem) {
  activeToolMessage.value = item;
  toolDrawerVisible.value = true;
  loadingToolInvocations.value = true;
  try {
    const summaryIds = new Set(resolveToolInvocations(item).map((invocation) => invocation.invocationId).filter(Boolean));
    let records = await getAiToolInvocationsApi({
      conversationId: item.conversationId,
      maxResultCount: 50,
      messageId: item.id,
    });
    if (records.length === 0 && summaryIds.size > 0) {
      records = await getAiToolInvocationsApi({
        conversationId: item.conversationId,
        maxResultCount: 200,
      });
      records = records.filter((record) => summaryIds.has(record.invocationId));
    }
    toolInvocationRecords.value = records;
  } finally {
    loadingToolInvocations.value = false;
  }
}

function replaceMessage(updated: AiApi.MessageItem) {
  if (!detail.value) {
    return;
  }

  detail.value.messages = detail.value.messages.map((item) => (item.id === updated.id ? updated : item));
}

async function handlePlanConfirm(item: AiApi.MessageItem, confirmed: boolean) {
  if (!detail.value) {
    return;
  }

  await runAction(`plan-confirm:${item.id}`, async () => {
    const updated = await confirmAiConversationPlanApi(detail.value!.id, {
      confirmed,
      messageId: item.id,
    });
    replaceMessage(updated);
    message.success(confirmed ? '计划已确认' : '计划已取消确认');
  });
}

async function handlePlanStepToggle(item: AiApi.MessageItem, step: StructuredPlanStep) {
  if (!detail.value) {
    return;
  }

  await runAction(`plan-step:${item.id}:${step.id}`, async () => {
    const updated = await toggleAiConversationPlanStepApi(detail.value!.id, {
      isDone: !step.isDone,
      messageId: item.id,
      stepId: step.id,
    });
    replaceMessage(updated);
  });
}

async function handlePlanDraft(item: AiApi.MessageItem) {
  if (!detail.value) {
    return;
  }

  await runAction(`plan-draft:${item.id}`, async () => {
    const updated = await createAiConversationPlanDraftApi(detail.value!.id, {
      messageId: item.id,
      title: `${detail.value!.title} AI 任务草稿`,
    });
    applyConversationDetail(updated);
    message.success('AI 任务草稿已生成');
  });
}

function normalizeJson(value: string) {
  const text = value.trim() || '{}';
  JSON.parse(text);
  return text;
}

function openEditConversation() {
  if (!detail.value) {
    return;
  }

  editingConversationId.value = detail.value.id;
  Object.assign(conversationForm, {
    contextId: detail.value.contextId,
    contextSnapshotJson: detail.value.contextSnapshotJson || '{}',
    contextType: detail.value.contextType || 'None',
    mode: detail.value.mode || 'General',
    modelId: detail.value.modelId || undefined,
    title: detail.value.title,
  });
  conversationDrawerVisible.value = true;
}

async function createQuickConversation() {
  savingConversation.value = true;
  try {
    const created = await createAiConversationApi({
      contextId: '',
      contextSnapshotJson: '{}',
      contextType: 'None',
      mode: 'General',
      modelId: defaultModelId.value,
      title: '新的 AI 会话',
    });
    message.success('会话已创建');
    applyConversationDetail(created);
  } finally {
    savingConversation.value = false;
  }
}

async function handleConversationSubmit() {
  if (!conversationForm.title.trim()) {
    message.warning('请输入会话标题');
    return;
  }

  let contextSnapshotJson = '{}';
  try {
    contextSnapshotJson = normalizeJson(conversationForm.contextSnapshotJson);
  } catch {
    message.warning('上下文快照必须是合法 JSON');
    return;
  }

  savingConversation.value = true;
  try {
    const payload = {
      ...conversationForm,
      contextId: conversationForm.contextId.trim(),
      contextSnapshotJson,
      title: conversationForm.title.trim(),
    };

    if (editingConversationId.value) {
      const updated = await updateAiConversationApi(editingConversationId.value, payload);
      if (detail.value?.id === updated.id) {
        Object.assign(detail.value, updated);
      }
      upsertConversation(updated);
      message.success('会话已保存');
    } else {
      const created = await createAiConversationApi(payload);
      message.success('会话已创建');
      applyConversationDetail(created);
    }

    conversationDrawerVisible.value = false;
  } finally {
    savingConversation.value = false;
  }
}

async function handleModelChange(value: unknown) {
  if (!detail.value) {
    return;
  }

  const modelId = typeof value === 'string' ? value : undefined;
  currentModelId.value = modelId;
  const updated = await updateAiConversationApi(detail.value.id, {
    contextId: detail.value.contextId,
    contextSnapshotJson: detail.value.contextSnapshotJson || '{}',
    contextType: detail.value.contextType || 'None',
    modelId,
    title: detail.value.title,
  });
  Object.assign(detail.value, updated);
  upsertConversation(updated);
}

function handleCommandChange(value: unknown) {
  const commandCode = typeof value === 'string' ? value : undefined;
  selectedCommandCode.value = commandCode;
  const command = commands.value.find((item) => item.commandCode === commandCode);
  if (!command) {
    return;
  }

  selectedAgentCode.value = command.agentCode || selectedAgentCode.value;
  selectedSkillCode.value = command.skillCode || selectedSkillCode.value;
  if (!messageContent.value.trim()) {
    messageContent.value = command.exampleInput || command.inputPlaceholder || `${command.slashName} `;
  }
}

async function sendMessage() {
  const content = messageContent.value.trim();
  if (!detail.value) {
    message.warning('请先选择或创建会话');
    return;
  }

  if (!content) {
    message.warning('请输入消息内容');
    return;
  }

  if (isArchived.value) {
    message.warning('归档会话不能发送消息');
    return;
  }

  const preparedContent = await prepareSlashCommandContent(content);
  if (!preparedContent) {
    return;
  }

  sending.value = true;
  try {
    await streamAiConversationMessageApi(detail.value.id, {
      agentCode: selectedAgentCode.value,
      commandCode: selectedCommandCode.value,
      content: preparedContent,
      modelId: currentModelId.value,
      skillCode: selectedSkillCode.value,
    }, (event) => {
      handleStreamEvent(event);
    });
    messageContent.value = '';
  } catch (error) {
    await sendMessageWithoutStream(preparedContent);
    if (error instanceof Error && error.message) {
      message.warning(`流式输出失败，已切换为普通响应：${error.message}`);
    }
  } finally {
    sending.value = false;
  }
}

async function sendMessageWithoutStream(content: string) {
  if (!detail.value) {
    return;
  }

  const result = await sendAiConversationMessageApi(detail.value.id, {
    agentCode: selectedAgentCode.value,
    commandCode: selectedCommandCode.value,
    content,
    modelId: currentModelId.value,
    skillCode: selectedSkillCode.value,
  });
  Object.assign(detail.value, result.conversation);
  detail.value.messages = [...detail.value.messages, ...result.messages];
  upsertConversation(result.conversation);
  messageContent.value = '';
}

function handleStreamEvent(event: AiApi.ConversationStreamEvent) {
  if (!detail.value) {
    return;
  }

  if (event.event === 'userMessage' && event.userMessage) {
    detail.value.messages = [...detail.value.messages, event.userMessage, createStreamingAssistantMessage(event.userMessage)];
    messageContent.value = '';
    return;
  }

  if (event.event === 'delta') {
    appendStreamingDelta(event.delta);
    return;
  }

  if (event.event === 'error') {
    appendStreamingError(event.errorMessage);
    return;
  }

  if (event.event === 'completed' && event.result) {
    Object.assign(detail.value, event.result.conversation);
    detail.value.messages = replaceStreamingMessages(detail.value.messages, event.result.messages);
    upsertConversation(event.result.conversation);
  }
}

function createStreamingAssistantMessage(userMessage: AiApi.MessageItem): AiApi.MessageItem {
  return {
    citations: [],
    completionTokens: 0,
    content: '',
    conversationId: userMessage.conversationId,
    creationTime: new Date().toISOString(),
    errorMessage: '',
    id: `streaming:${userMessage.id}`,
    metadataJson: '{}',
    modelId: currentModelId.value,
    modelName: '',
    promptTokens: 0,
    providerId: undefined,
    providerName: '',
    role: 'assistant',
    sequenceNo: userMessage.sequenceNo + 1,
    status: 'Streaming',
    totalTokens: 0,
  };
}

function appendStreamingDelta(delta: string) {
  const current = findStreamingMessage();
  if (current) {
    current.content += delta;
  }
}

function appendStreamingError(errorMessage: string) {
  const current = findStreamingMessage();
  if (current) {
    current.errorMessage = errorMessage;
    current.status = 'Failed';
  }
}

function findStreamingMessage() {
  return detail.value?.messages.find((item) => item.id.startsWith('streaming:'));
}

function replaceStreamingMessages(messages: AiApi.MessageItem[], finalMessages: AiApi.MessageItem[]) {
  const userMessage = finalMessages[0];
  const assistantMessage = finalMessages[1];
  if (!userMessage || !assistantMessage) {
    return messages;
  }

  const withoutStreaming = messages.filter((item) => !item.id.startsWith('streaming:') && item.id !== userMessage.id);
  return [...withoutStreaming, userMessage, assistantMessage].sort((left, right) => left.sequenceNo - right.sequenceNo);
}

async function archiveConversation() {
  if (!detail.value) {
    return;
  }

  await runAction(`archive:${detail.value.id}`, async () => {
    const updated = await archiveAiConversationApi(detail.value!.id);
    Object.assign(detail.value!, updated);
    message.success('会话已归档');
    upsertConversation(updated);
  });
}

async function restoreConversation() {
  if (!detail.value) {
    return;
  }

  await runAction(`restore:${detail.value.id}`, async () => {
    const updated = await restoreAiConversationApi(detail.value!.id);
    Object.assign(detail.value!, updated);
    message.success('会话已恢复');
    status.value = 'Active';
    upsertConversation(updated);
  });
}

async function deleteConversation() {
  if (!detail.value) {
    return;
  }

  await runAction(`delete:${detail.value.id}`, async () => {
    const deletedId = detail.value!.id;
    await deleteAiConversationApi(deletedId);
    message.success('会话已删除');
    selectedConversationId.value = '';
    detail.value = undefined;
    const nextQuery = { ...route.query };
    delete nextQuery.id;
    await router.replace({ query: nextQuery });
    await loadConversations();
  });
}

async function exportConversation() {
  if (!detail.value) {
    return;
  }

  await runAction(`export:${detail.value.id}`, async () => {
    const result = await exportAiConversationApi(detail.value!.id);
    const blob = new Blob([result.content], { type: 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = result.fileName || `${detail.value!.title}.md`;
    link.click();
    URL.revokeObjectURL(url);
  });
}

async function handleFilterChange() {
  await loadConversations();
}

onMounted(async () => {
  await Promise.all([loadModels(), loadAgentDesignOptions()]);
  await loadConversations();
});
</script>

<template>
  <Page content-class="ai-conversation-page__content">
    <section class="ai-conversation-page">
      <div
        :class="[
          'ai-conversation-page__shell',
          { 'ai-conversation-page__shell--with-sidebar': recentPanelVisible },
        ]"
      >
        <section v-if="recentPanelVisible" class="ai-conversation-page__sidebar rounded-2xl">
          <header class="ai-conversation-page__panel-head">
            <h3>最近会话</h3>
            <Space>
              <Button size="small" @click="recentPanelVisible = false">收起</Button>
              <Button
                :loading="savingConversation && !conversationDrawerVisible"
                size="small"
                type="primary"
                @click="createQuickConversation"
              >
                新会话
              </Button>
            </Space>
          </header>

          <div class="ai-conversation-page__sidebar-body">
            <div class="ai-conversation-page__filters">
              <Input.Search
                v-model:value="keyword"
                allow-clear
                placeholder="搜索标题或摘要"
                @search="handleFilterChange"
              />
              <Select v-model:value="status" :options="statusOptions" @change="handleFilterChange" />
            </div>

            <div v-if="loadingConversations" class="ai-conversation-page__empty">
              <Empty description="正在加载会话" />
            </div>
            <div v-else-if="conversations.length === 0" class="ai-conversation-page__empty">
              <Empty description="暂无会话" />
            </div>
            <div v-else class="ai-conversation-page__conversation-list">
              <button
                v-for="item in conversations"
                :key="item.id"
                :class="[
                  'ai-conversation-page__conversation-item',
                  { 'ai-conversation-page__conversation-item--active': item.id === selectedConversationId },
                ]"
                type="button"
                @click="loadDetail(item.id)"
              >
                <span>
                  <strong>{{ item.title }}</strong>
                  <Tag :color="getStatusColor(item.status)">{{ getStatusLabel(item.status) }}</Tag>
                </span>
                <em>{{ item.lastMessageSummary || '暂无消息' }}</em>
                <small>
                  {{ getModeLabel(item.mode) }} · {{ item.modelName || '未选择模型' }} · {{ item.messageCount }} 条
                </small>
              </button>
            </div>
          </div>
        </section>

        <section class="ai-conversation-page__workspace rounded-2xl">
          <header class="ai-conversation-page__panel-head ai-conversation-page__workspace-head">
            <h3>{{ detail?.title || '会话详情' }}</h3>
            <Space wrap>
              <Tooltip :title="recentPanelVisible ? '收起最近会话' : '展开最近会话'">
                <Button @click="recentPanelVisible = !recentPanelVisible">
                  {{ recentPanelVisible ? '收起最近' : '最近' }}
                </Button>
              </Tooltip>
              <Button
                :loading="savingConversation && !conversationDrawerVisible"
                type="primary"
                @click="createQuickConversation"
              >
                新会话
              </Button>
              <Select
                v-if="detail"
                v-model:value="currentModelId"
                allow-clear
                class="ai-conversation-page__model-select"
                :options="modelOptions"
                placeholder="选择模型"
                @change="handleModelChange"
              />
              <Button v-if="detail" @click="openEditConversation">设置</Button>
              <Button
                v-if="detail && isArchived"
                :loading="actionLoadingKey === `restore:${detail.id}`"
                @click="restoreConversation"
              >
                恢复
              </Button>
              <Button
                v-else-if="detail"
                :loading="actionLoadingKey === `archive:${detail.id}`"
                @click="archiveConversation"
              >
                归档
              </Button>
              <Button v-if="detail" :loading="actionLoadingKey === `export:${detail.id}`" @click="exportConversation">导出</Button>
              <Popconfirm v-if="detail" title="确认删除当前会话？" @confirm="deleteConversation">
                <Button :loading="actionLoadingKey === `delete:${detail.id}`" danger>删除</Button>
              </Popconfirm>
            </Space>
          </header>

          <Spin :spinning="loadingDetail">
            <div class="ai-conversation-page__workspace-body">
              <div v-if="!detail" class="ai-conversation-page__empty ai-conversation-page__empty--workspace">
                <Empty description="请选择或新建会话" />
              </div>

              <div v-else class="ai-conversation-page__detail">
            <div class="ai-conversation-page__messages">
              <div v-if="detail.messages.length === 0" class="ai-conversation-page__empty">
                <Empty description="暂无消息" />
              </div>
              <article
                v-for="item in detail.messages"
                v-else
                :key="item.id"
                :class="['ai-conversation-page__message', `ai-conversation-page__message--${item.role}`]"
              >
                <div class="ai-conversation-page__message-avatar">
                  {{ getRoleLabel(item.role).slice(0, 1) }}
                </div>
                <div class="ai-conversation-page__message-stack">
                  <div class="ai-conversation-page__message-head">
                    <strong>{{ getRoleLabel(item.role) }}</strong>
                    <span>{{ item.modelName || item.providerName || '-' }}</span>
                    <em>{{ formatDateTime(item.creationTime) }}</em>
                  </div>
                  <div class="ai-conversation-page__message-bubble">
                    <p
                      v-if="item.role === 'user'"
                      class="ai-conversation-page__user-text"
                      v-text="item.errorMessage || item.content"
                    ></p>
                    <AiMarkdownMessage
                      v-else
                      :content="item.errorMessage || item.content"
                      :final="item.status !== 'Streaming'"
                    />
                  </div>
                  <div v-if="item.citations.length > 0" class="ai-conversation-page__citations">
                    <strong>引用来源</strong>
                    <Tag v-for="citation in item.citations" :key="citation" color="blue">{{ citation }}</Tag>
                  </div>
                  <div v-if="hasRunTrace(item)" class="ai-conversation-page__runtime-trace">
                    <div class="ai-conversation-page__runtime-head">
                      <strong>运行轨迹</strong>
                      <span>
                        Context {{ resolveContextEngineering(item)?.estimatedPromptTokens || item.promptTokens || 0 }} tokens
                      </span>
                    </div>
                    <div class="ai-conversation-page__runtime-metrics">
                      <span>
                        <em>上下文</em>
                        <strong>{{ resolveContextEngineering(item)?.hasInjectedContext ? '已注入' : '基础会话' }}</strong>
                      </span>
                      <span>
                        <em>压缩</em>
                        <strong>{{ resolveContextEngineering(item)?.automaticCompression ? '自动' : '未启用' }}</strong>
                      </span>
                      <span>
                        <em>可用工具</em>
                        <strong>{{ resolveAvailableTools(item).length }}</strong>
                      </span>
                      <span>
                        <em>工具轮次</em>
                        <strong>{{ resolveToolLoop(item)?.iterations || 0 }}/{{ resolveToolLoop(item)?.maxIterations || 0 }}</strong>
                      </span>
                      <span>
                        <em>预算</em>
                        <strong>{{ formatTokenCount(resolveContextEngineering(item)?.contextBudgetTokens) }}</strong>
                      </span>
                      <span>
                        <em>消息窗口</em>
                        <strong>
                          近 {{ resolveContextEngineering(item)?.recentMessageCount || 0 }} / 压缩
                          {{ resolveContextEngineering(item)?.compressedMessageCount || 0 }}
                        </strong>
                      </span>
                    </div>
                    <div v-if="resolveAvailableTools(item).length > 0" class="ai-conversation-page__runtime-tools">
                      <Tag v-for="toolCode in resolveAvailableTools(item)" :key="toolCode" color="blue">
                        {{ getToolDisplayName(toolCode) }}
                      </Tag>
                    </div>
                    <div class="ai-conversation-page__runtime-steps">
                      <div
                        v-for="step in resolveRunSteps(item)"
                        :key="step.code || step.title"
                        class="ai-conversation-page__runtime-step"
                      >
                        <Tag :color="getRunStepColor(step.status)">{{ step.status || '-' }}</Tag>
                        <span>
                          <strong>{{ step.title || step.code }}</strong>
                          <em>{{ step.summary || '-' }}</em>
                        </span>
                      </div>
                    </div>
                  </div>
                  <div v-if="resolveToolInvocations(item).length > 0" class="ai-conversation-page__tool-invocations">
                    <div class="ai-conversation-page__tool-head">
                      <strong>工具调用</strong>
                      <Button size="small" type="link" @click="openToolInvocations(item)">查看调用记录</Button>
                    </div>
                    <div class="ai-conversation-page__tool-list">
                      <span
                        v-for="invocation in resolveToolInvocations(item)"
                        :key="invocation.invocationId || invocation.toolCode"
                        class="ai-conversation-page__tool-item"
                      >
                        <Tag :color="getToolInvocationColor(invocation.status)">{{ invocation.status || '-' }}</Tag>
                        <span>{{ invocation.toolCode || '-' }}</span>
                        <em>{{ invocation.elapsedMilliseconds || 0 }} ms</em>
                        <small v-if="invocation.errorMessage">{{ invocation.errorMessage }}</small>
                      </span>
                    </div>
                  </div>
                  <div v-if="resolveClarification(item)" class="ai-conversation-page__structured">
                    <div
                      v-if="resolveClarification(item)?.questions?.length"
                      class="ai-conversation-page__structured-block"
                    >
                      <strong>澄清问题</strong>
                      <ol>
                        <li v-for="question in resolveClarification(item)?.questions" :key="question.id">
                          <span>{{ question.text }}</span>
                          <Tag v-if="question.required" color="orange">必填</Tag>
                        </li>
                      </ol>
                    </div>
                    <div v-else class="ai-conversation-page__structured-block">
                      <strong>{{ resolveClarification(item)?.title || '澄清完成' }}</strong>
                      <p v-if="resolveClarification(item)?.summary">{{ resolveClarification(item)?.summary }}</p>
                      <ul v-if="resolveClarification(item)?.nextActions?.length">
                        <li v-for="action in resolveClarification(item)?.nextActions" :key="action">{{ action }}</li>
                      </ul>
                    </div>
                  </div>
                  <div v-if="resolvePlan(item)" class="ai-conversation-page__structured">
                    <div class="ai-conversation-page__plan-head">
                      <strong>{{ resolvePlan(item)?.goal || '执行计划' }}</strong>
                      <Space>
                        <Button
                          :loading="actionLoadingKey === `plan-confirm:${item.id}`"
                          size="small"
                          @click="handlePlanConfirm(item, !resolvePlan(item)?.confirmed)"
                        >
                          {{ resolvePlan(item)?.confirmed ? '取消确认' : '确认计划' }}
                        </Button>
                        <Button
                          :disabled="resolvePlan(item)?.draftCreated"
                          :loading="actionLoadingKey === `plan-draft:${item.id}`"
                          size="small"
                          type="primary"
                          @click="handlePlanDraft(item)"
                        >
                          {{ resolvePlan(item)?.draftCreated ? '已转草稿' : '转任务草稿' }}
                        </Button>
                      </Space>
                    </div>
                    <div class="ai-conversation-page__plan-list">
                      <label
                        v-for="step in resolvePlan(item)?.steps || []"
                        :key="step.id"
                        class="ai-conversation-page__plan-step"
                      >
                        <Checkbox
                          :checked="!!step.isDone"
                          :disabled="actionLoadingKey === `plan-step:${item.id}:${step.id}`"
                          @click.prevent="handlePlanStepToggle(item, step)"
                        />
                        <span>
                          <strong>{{ step.title }}</strong>
                          <em>{{ step.description || step.expectedOutput || '-' }}</em>
                        </span>
                      </label>
                    </div>
                    <div v-if="resolvePlan(item)?.risks?.length" class="ai-conversation-page__plan-risks">
                      <Tag v-for="risk in resolvePlan(item)?.risks" :key="risk" color="orange">{{ risk }}</Tag>
                    </div>
                  </div>
                  <div class="ai-conversation-page__message-foot">
                    <span>输入 {{ item.promptTokens }}</span>
                    <span>输出 {{ item.completionTokens }}</span>
                    <span>合计 {{ item.totalTokens }}</span>
                    <Button v-if="item.role === 'assistant'" size="small" type="link" @click="openUsageRecords(item)">用量审计</Button>
                    <Tag v-if="item.errorMessage" color="red">错误记录</Tag>
                  </div>
                </div>
              </article>
            </div>

            <div class="ai-conversation-page__composer">
              <div v-if="runtimePanelVisible" class="ai-conversation-page__runtime-controls">
                <Select
                  v-model:value="selectedAgentCode"
                  allow-clear
                  :disabled="isArchived"
                  :options="agentOptions"
                  placeholder="选择 Agent"
                />
                <Select
                  v-model:value="selectedSkillCode"
                  allow-clear
                  :disabled="isArchived"
                  :options="skillOptions"
                  placeholder="固定技能"
                />
                <Select
                  v-model:value="selectedCommandCode"
                  allow-clear
                  :disabled="isArchived"
                  :options="commandOptions"
                  placeholder="快捷命令"
                  @change="handleCommandChange"
                />
              </div>
              <Input.TextArea
                v-model:value="messageContent"
                :auto-size="{ minRows: 3, maxRows: 7 }"
                :disabled="isArchived"
                :maxlength="16_000"
                placeholder="输入问题，或输入 /help 查看快捷命令"
              />
              <div class="ai-conversation-page__composer-actions">
                <div class="ai-conversation-page__status-bar">
                  <span>{{ getModeLabel(detail.mode) }}</span>
                  <span>{{ getContextLabel(detail.contextType) }} {{ detail.contextId || '-' }}</span>
                  <span>Token {{ formatTokenCount(detail.totalTokens) }}</span>
                  <span>更新 {{ formatDateTime(detail.lastMessageTime || detail.creationTime) }}</span>
                </div>
                <div class="ai-conversation-page__composer-tools">
                  <Tooltip title="Agent、Skill 与快捷命令">
                    <Button
                      :type="runtimePanelVisible ? 'primary' : 'default'"
                      @click="runtimePanelVisible = !runtimePanelVisible"
                    >
                      运行 {{ activeRuntimeLabel }}
                    </Button>
                  </Tooltip>
                  <Tooltip title="输入快捷命令">
                    <Button :disabled="isArchived" @click="messageContent = messageContent.trim() ? messageContent : '/'">
                      /
                    </Button>
                  </Tooltip>
                  <span>{{ isArchived ? '归档会话不可发送消息' : `${messageContent.length}/16000` }}</span>
                  <Button :disabled="isArchived" :loading="sending" type="primary" @click="sendMessage">发送</Button>
                </div>
              </div>
            </div>
              </div>
            </div>
          </Spin>
        </section>
      </div>
    </section>

    <Drawer
      v-model:open="conversationDrawerVisible"
      :title="editingConversationId ? '编辑会话' : '新建会话'"
      width="760px"
    >
      <Form :label-col="{ span: 5 }" :model="conversationForm" :wrapper-col="{ span: 18 }" layout="horizontal">
        <Form.Item label="会话标题" required>
          <Input v-model:value="conversationForm.title" :maxlength="128" placeholder="例如 项目周报澄清" />
        </Form.Item>
        <Form.Item label="会话模式">
          <Select v-model:value="conversationForm.mode" :disabled="!!editingConversationId" :options="modeOptions" />
        </Form.Item>
        <Form.Item label="模型">
          <Select
            v-model:value="conversationForm.modelId"
            allow-clear
            :options="modelOptions"
            placeholder="可先不选择模型"
          />
        </Form.Item>
        <Form.Item label="上下文类型">
          <Select v-model:value="conversationForm.contextType" :options="contextOptions" />
        </Form.Item>
        <Form.Item label="上下文编号">
          <Input v-model:value="conversationForm.contextId" :maxlength="128" placeholder="项目、经营、文档或风险编号" />
        </Form.Item>
        <Form.Item label="上下文快照">
          <Input.TextArea
            v-model:value="conversationForm.contextSnapshotJson"
            :auto-size="{ minRows: 4, maxRows: 8 }"
            :maxlength="8000"
          />
        </Form.Item>
      </Form>
      <template #footer>
        <Space>
          <Button @click="conversationDrawerVisible = false">取消</Button>
          <Button :loading="savingConversation" type="primary" @click="handleConversationSubmit">保存</Button>
        </Space>
      </template>
    </Drawer>

    <Drawer v-model:open="toolDrawerVisible" title="AI 工具调用记录" width="780px">
      <Spin :spinning="loadingToolInvocations">
        <div class="ai-conversation-page__tool-drawer">
          <div class="ai-conversation-page__tool-drawer-meta">
            <span>
              <em>会话</em>
              <strong>{{ activeToolMessage?.conversationId || '-' }}</strong>
            </span>
            <span>
              <em>消息</em>
              <strong>{{ activeToolMessage?.id || '-' }}</strong>
            </span>
          </div>
          <Empty v-if="toolInvocationRecords.length === 0" description="暂无工具调用记录" />
          <article
            v-for="record in toolInvocationRecords"
            v-else
            :key="record.invocationId"
            class="ai-conversation-page__tool-record"
          >
            <div class="ai-conversation-page__tool-record-head">
              <strong>{{ record.toolCode }}</strong>
              <Tag :color="getToolInvocationColor(record.status)">{{ record.status || '-' }}</Tag>
              <em>{{ record.elapsedMilliseconds }} ms</em>
            </div>
            <p v-if="record.errorMessage">{{ record.errorMessage }}</p>
            <div v-if="record.sourceReferences.length > 0" class="ai-conversation-page__tool-record-refs">
              <Tag v-for="source in record.sourceReferences" :key="`${source.sourceType}:${source.sourceId}`" color="blue">
                {{ source.title || source.sourceType }}
              </Tag>
            </div>
            <h3>输入</h3>
            <pre>{{ record.inputJson || '{}' }}</pre>
            <h3>输出</h3>
            <pre>{{ record.outputJson || '{}' }}</pre>
          </article>
        </div>
      </Spin>
    </Drawer>
  </Page>
</template>

<style scoped>
@reference "@vben/tailwind-config/theme";
:deep(.ai-conversation-page__content) {
  height: calc(100vh - 72px);
  @apply px-2.5 pb-2.5 pt-2;
  overflow: hidden;
}

.ai-conversation-page {
  display: flex;
  flex-direction: column;
  min-height: 100%;
}

.ai-conversation-page__shell {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  @apply gap-3;
  min-height: calc(100vh - 90px);
  transition: grid-template-columns 0.18s ease;
}

.ai-conversation-page__shell--with-sidebar {
  grid-template-columns: minmax(280px, 340px) minmax(0, 1fr);
}

.ai-conversation-page__sidebar,
.ai-conversation-page__workspace {
  display: flex;
  flex-direction: column;
  min-height: calc(100vh - 90px);
  overflow: hidden;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  box-shadow: 0 8px 24px rgb(15 23 42 / 0.06);
}

.ai-conversation-page__panel-head {
  display: flex;
  @apply gap-3;
  align-items: center;
  justify-content: space-between;
  min-height: 48px;
  @apply px-4 py-3;
  border-bottom: 1px solid var(--ant-color-border-secondary);
}

.ai-conversation-page__panel-head h3 {
  @apply m-0;
  color: var(--ant-color-text);
  font-size: 16px;
  font-weight: 600;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-conversation-page__sidebar-body {
  height: calc(100vh - 148px);
  min-height: 0;
  overflow: hidden;
  @apply p-4;
}

.ai-conversation-page__workspace {
  min-width: 0;
}

.ai-conversation-page__workspace :deep(.ant-spin-nested-loading),
.ai-conversation-page__workspace :deep(.ant-spin-container) {
  display: flex;
  flex: 1;
  min-height: 0;
}

.ai-conversation-page__workspace-body {
  display: flex;
  flex-direction: column;
  flex: 1;
  height: calc(100vh - 138px);
  min-height: 0;
  @apply p-3.5;
}

.ai-conversation-page__filters {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 108px;
  @apply gap-2;
  @apply mb-3;
}

.ai-conversation-page__conversation-list,
.ai-conversation-page__detail,
.ai-conversation-page__messages {
  display: flex;
  flex-direction: column;
}

.ai-conversation-page__conversation-list {
  @apply gap-2;
  max-height: calc(100vh - 238px);
  @apply pr-0.5;
  overflow-y: auto;
}

.ai-conversation-page__conversation-item {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
  width: 100%;
  @apply px-3 py-2.5;
  text-align: left;
  cursor: pointer;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.ai-conversation-page__conversation-item:hover,
.ai-conversation-page__conversation-item--active {
  border-color: var(--ant-color-primary);
}

.ai-conversation-page__conversation-item span {
  display: flex;
  @apply gap-1.5;
  align-items: center;
  justify-content: space-between;
}

.ai-conversation-page__conversation-item strong,
.ai-conversation-page__conversation-item em,
.ai-conversation-page__conversation-item small {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-conversation-page__conversation-item em {
  color: var(--ant-color-text-secondary);
  font-style: normal;
}

.ai-conversation-page__conversation-item small {
  color: var(--ant-color-text-tertiary);
}

.ai-conversation-page__model-select {
  width: 260px;
}

.ai-conversation-page__detail {
  @apply gap-3;
  flex: 1;
  min-height: 0;
}

.ai-conversation-page__messages {
  flex: 1;
  @apply gap-2.5;
  min-height: 0;
  max-height: none;
  @apply p-4;
  overflow-y: auto;
  background: var(--ant-color-bg-layout);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.ai-conversation-page__message {
  display: grid;
  grid-template-columns: 34px minmax(0, 1fr);
  @apply gap-2.5;
  width: min(92%, 920px);
  max-width: 100%;
}

.ai-conversation-page__message--user {
  align-self: flex-end;
  grid-template-columns: minmax(0, 1fr) 34px;
  width: fit-content;
  max-width: min(78%, 680px);
}

.ai-conversation-page__message--assistant,
.ai-conversation-page__message--system {
  align-self: flex-start;
}

.ai-conversation-page__message-avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  color: var(--ant-color-text-secondary);
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-full;
  box-shadow: 0 4px 12px rgb(15 23 42 / 6%);
}

.ai-conversation-page__message--user .ai-conversation-page__message-avatar {
  grid-column: 2;
  color: #fff;
  background: var(--ant-color-primary);
  border-color: var(--ant-color-primary);
}

.ai-conversation-page__message-stack {
  display: flex;
  flex-direction: column;
  @apply gap-1.5;
  min-width: 0;
}

.ai-conversation-page__message--user .ai-conversation-page__message-stack {
  grid-column: 1;
  grid-row: 1;
  align-items: flex-end;
}

.ai-conversation-page__message-head {
  display: flex;
  @apply gap-2;
  align-items: center;
  max-width: 100%;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.ai-conversation-page__message-head strong {
  color: var(--ant-color-text);
  font-weight: 600;
}

.ai-conversation-page__message-head span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-conversation-page__message-head em {
  color: var(--ant-color-text-tertiary);
  font-style: normal;
  white-space: nowrap;
}

.ai-conversation-page__message-bubble {
  position: relative;
  max-width: 100%;
  @apply px-4 py-3;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-xl rounded-bl;
  box-shadow: 0 8px 22px rgb(15 23 42 / 6%);
}

.ai-conversation-page__message--user .ai-conversation-page__message-bubble {
  color: var(--ant-color-text);
  background: var(--ant-color-primary-bg);
  border-color: var(--ant-color-primary-border);
  @apply rounded-xl rounded-br;
  box-shadow: 0 8px 22px color-mix(in srgb, var(--ant-color-primary), transparent 76%);
}

.ai-conversation-page__message--system .ai-conversation-page__message-bubble {
  background: var(--ant-color-warning-bg);
  border-color: var(--ant-color-warning-border);
}

.ai-conversation-page__message--assistant .ai-conversation-page__message-bubble {
  width: 100%;
}

.ai-conversation-page__message-foot,
.ai-conversation-page__composer-actions {
  display: flex;
  @apply gap-2;
  align-items: center;
}

.ai-conversation-page__message p {
  @apply m-0;
  line-height: 1.7;
  white-space: pre-wrap;
}

.ai-conversation-page__user-text {
  max-width: 100%;
  color: inherit;
  overflow-wrap: anywhere;
}

.ai-conversation-page__citations {
  display: flex;
  flex-wrap: wrap;
  @apply gap-1.5;
  align-items: center;
  @apply mt-2.5;
}

.ai-conversation-page__citations strong {
  @apply mr-0.5;
  color: var(--ant-color-text-secondary);
  font-weight: 600;
}

.ai-conversation-page__runtime-trace {
  display: grid;
  @apply gap-2;
  @apply mt-2.5;
  @apply pt-2.5;
  border-top: 1px solid var(--ant-color-border-secondary);
}

.ai-conversation-page__runtime-head {
  display: flex;
  @apply gap-2;
  align-items: center;
  justify-content: space-between;
  color: var(--ant-color-text-secondary);
  font-size: 12px;
}

.ai-conversation-page__runtime-head strong {
  color: var(--ant-color-text);
  font-weight: 600;
}

.ai-conversation-page__runtime-metrics {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(118px, 1fr));
  @apply gap-2;
}

.ai-conversation-page__runtime-metrics span {
  display: grid;
  @apply gap-0.5;
  min-width: 0;
  @apply px-2.5 py-2;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
}

.ai-conversation-page__runtime-metrics em,
.ai-conversation-page__runtime-step em {
  overflow: hidden;
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
  font-style: normal;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-conversation-page__runtime-metrics strong,
.ai-conversation-page__runtime-step strong {
  overflow: hidden;
  color: var(--ant-color-text);
  font-size: 13px;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-conversation-page__runtime-tools {
  display: flex;
  flex-wrap: wrap;
  @apply gap-1.5;
}

.ai-conversation-page__runtime-steps {
  display: grid;
  @apply gap-1.5;
}

.ai-conversation-page__runtime-step {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  @apply gap-2;
  align-items: start;
}

.ai-conversation-page__runtime-step span {
  display: grid;
  @apply gap-0.5;
  min-width: 0;
}

.ai-conversation-page__tool-invocations {
  display: grid;
  @apply gap-1.5;
  @apply mt-2.5;
  @apply p-2;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
}

.ai-conversation-page__tool-head {
  display: flex;
  @apply gap-2;
  align-items: center;
  justify-content: space-between;
}

.ai-conversation-page__tool-head > strong {
  color: var(--ant-color-text-secondary);
  font-weight: 600;
}

.ai-conversation-page__tool-list {
  display: grid;
  @apply gap-1.5;
}

.ai-conversation-page__tool-item {
  display: flex;
  flex-wrap: wrap;
  @apply gap-1.5;
  align-items: center;
  color: var(--ant-color-text-secondary);
}

.ai-conversation-page__tool-item em {
  color: var(--ant-color-text-tertiary);
  font-style: normal;
}

.ai-conversation-page__tool-item small {
  color: var(--ant-color-error);
}

.ai-conversation-page__tool-drawer {
  display: flex;
  flex-direction: column;
  @apply gap-3.5;
}

.ai-conversation-page__tool-drawer-meta {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  @apply gap-2.5;
}

.ai-conversation-page__tool-drawer-meta span {
  display: flex;
  flex-direction: column;
  @apply gap-1;
  min-width: 0;
  @apply px-3 py-2.5;
  background: var(--ant-color-fill-quaternary);
  @apply rounded-md;
}

.ai-conversation-page__tool-drawer-meta em,
.ai-conversation-page__tool-record-head em {
  color: var(--ant-color-text-tertiary);
  font-style: normal;
}

.ai-conversation-page__tool-drawer-meta strong {
  overflow: hidden;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.ai-conversation-page__tool-record {
  display: flex;
  flex-direction: column;
  @apply gap-2.5;
  @apply pb-3.5;
  border-bottom: 1px solid var(--ant-color-border-secondary);
}

.ai-conversation-page__tool-record:last-child {
  @apply pb-0;
  border-bottom: 0;
}

.ai-conversation-page__tool-record-head {
  display: flex;
  flex-wrap: wrap;
  @apply gap-2;
  align-items: center;
}

.ai-conversation-page__tool-record-head strong {
  font-weight: 600;
}

.ai-conversation-page__tool-record p {
  color: var(--ant-color-error);
}

.ai-conversation-page__tool-record h3 {
  @apply m-0;
  color: var(--ant-color-text-secondary);
  font-size: 13px;
  font-weight: 600;
}

.ai-conversation-page__tool-record pre {
  max-height: 280px;
  @apply px-3 py-2.5;
  @apply m-0;
  overflow: auto;
  font-size: 12px;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
}

.ai-conversation-page__tool-record-refs {
  display: flex;
  flex-wrap: wrap;
  @apply gap-1.5;
}

.ai-conversation-page__message-foot {
  @apply mt-2.5;
  color: var(--ant-color-text-tertiary);
}

.ai-conversation-page__structured {
  display: flex;
  flex-direction: column;
  @apply gap-2.5;
  @apply mt-3;
  @apply p-2.5;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.ai-conversation-page__structured-block {
  display: flex;
  flex-direction: column;
  @apply gap-2;
}

.ai-conversation-page__structured ol,
.ai-conversation-page__structured ul {
  @apply pl-4;
  @apply m-0;
}

.ai-conversation-page__structured li {
  @apply mb-1.5;
}

.ai-conversation-page__plan-head,
.ai-conversation-page__plan-step {
  display: flex;
  @apply gap-2.5;
}

.ai-conversation-page__plan-head {
  align-items: center;
  justify-content: space-between;
}

.ai-conversation-page__plan-list {
  display: flex;
  flex-direction: column;
  @apply gap-2;
}

.ai-conversation-page__plan-step {
  align-items: flex-start;
  @apply p-2;
  cursor: pointer;
  background: var(--ant-color-bg-container);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-md;
}

.ai-conversation-page__plan-step span {
  display: flex;
  flex-direction: column;
  @apply gap-0.5;
}

.ai-conversation-page__plan-step em {
  color: var(--ant-color-text-secondary);
  font-style: normal;
}

.ai-conversation-page__plan-risks {
  display: flex;
  flex-wrap: wrap;
  @apply gap-1.5;
}

.ai-conversation-page__composer {
  display: flex;
  flex-direction: column;
  @apply gap-2;
  flex-shrink: 0;
}

.ai-conversation-page__runtime-controls {
  display: grid;
  grid-template-columns: repeat(3, minmax(180px, 1fr));
  @apply gap-2;
  @apply p-2.5;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-lg;
}

.ai-conversation-page__composer-actions {
  justify-content: space-between;
  min-height: 32px;
  color: var(--ant-color-text-secondary);
}

.ai-conversation-page__status-bar {
  display: flex;
  flex: 1;
  flex-wrap: wrap;
  @apply gap-1.5;
  align-items: center;
  min-width: 0;
  font-size: 12px;
}

.ai-conversation-page__status-bar span {
  max-width: 220px;
  @apply px-2 py-0.5;
  overflow: hidden;
  color: var(--ant-color-text-tertiary);
  text-overflow: ellipsis;
  white-space: nowrap;
  background: var(--ant-color-fill-quaternary);
  border: 1px solid var(--ant-color-border-secondary);
  @apply rounded-full;
}

.ai-conversation-page__composer-tools {
  display: flex;
  flex-wrap: wrap;
  @apply gap-2;
  align-items: center;
  justify-content: flex-end;
}

.ai-conversation-page__composer-tools > span {
  color: var(--ant-color-text-tertiary);
  font-size: 12px;
  white-space: nowrap;
}

.ai-conversation-page__empty {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 260px;
}

.ai-conversation-page__empty--workspace {
  min-height: 560px;
}

@media (max-width: 1200px) {
  .ai-conversation-page__runtime-metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .ai-conversation-page__runtime-controls {
    grid-template-columns: 1fr;
  }

  .ai-conversation-page__shell,
  .ai-conversation-page__shell--with-sidebar {
    grid-template-columns: 1fr;
  }

  .ai-conversation-page__sidebar,
  .ai-conversation-page__workspace {
    min-height: auto;
  }

  .ai-conversation-page__sidebar-body,
  .ai-conversation-page__workspace-body {
    height: auto;
  }

  .ai-conversation-page__messages {
    min-height: 420px;
  }

  .ai-conversation-page__composer-actions {
    align-items: flex-start;
  }
}
</style>
