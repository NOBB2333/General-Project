import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { toast } from '@/components/ui';
import type { FeishuMessage } from '@/features/sales/model/feishu-orders.types';
import { makeTransientMessage, toConversationListItem, toConversationMessage } from './conversations.model';
import { routeNames } from '@/router/route.types';

const api = createAiApi(apiClient);
const emptyConversation = {
  id: 'ai-empty',
  name: 'AI 助手',
  preview: '选择或新建一个对话',
  time: '',
  unread: 0,
  pinned: false,
  tone: 'bot' as const,
};

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function conversationTitle(content: string): string {
  const normalized = content.replace(/\s+/gu, ' ').trim();
  return normalized.length > 64 ? `${normalized.slice(0, 61)}…` : normalized || '新对话';
}

function wait(milliseconds: number): Promise<void> {
  return new Promise((resolve) => window.setTimeout(resolve, milliseconds));
}

export function useAiConversations() {
  const route = useRoute();
  const conversations = ref<AiApi.Conversation[]>([]);
  const models = ref<AiApi.Model[]>([]);
  const usableSkills = ref<AiApi.SkillUseOption[]>([]);
  const usableTemplates = ref<AiApi.PromptTemplateUseOption[]>([]);
  const activeConversationId = ref('');
  const detail = ref<AiApi.ConversationDetail | null>(null);
  const selectedModelId = ref('');
  const selectedSkillCode = ref('');
  const selectedTemplateId = ref('');
  const templateInputJson = ref('{}');
  const templateTitle = ref('');
  const newConversationMode = ref('General');
  const modeOptions = [
    { label: '通用对话', value: 'General' },
    { label: '澄清对话', value: 'Clarify' },
    { label: '计划对话', value: 'Plan' },
  ];
  const transientMessages = ref<FeishuMessage[]>([]);
  const busyMessageIds = ref(new Set<string>());
  const loading = ref(false);
  const loadingDetail = ref(false);
  const isSending = ref(false);
  const notice = ref('');
  const error = ref('');
  const modelError = ref('');
  const useCatalogError = ref('');
  const loadingUseCatalog = ref(false);
  const usingTemplate = ref(false);
  const templateDialogOpen = ref(false);
  const deleteDialogOpen = ref(false);
  const deleting = ref(false);
  const stopRequested = ref(false);
  const managementMode = computed(() => route.name === routeNames.aiConversationManagement);
  const canStartFromTemplate = computed(() => !managementMode.value && usableTemplates.value.length > 0);
  const conversationItems = computed(() => conversations.value.map(toConversationListItem));
  const isArchived = computed(() => detail.value?.status.toLowerCase() === 'archived');
  const activeConversation = computed(() => {
    const item = conversations.value.find((conversation) => conversation.id === activeConversationId.value);
    return item ? toConversationListItem(item) : emptyConversation;
  });
  const activeModel = computed(() => models.value.find((model) => model.id === selectedModelId.value));
  const modelOptions = computed(() =>
    models.value.map((model) => ({ label: `${model.displayName} · ${model.providerName}`, value: model.id })),
  );
  const skillOptions = computed(() =>
    usableSkills.value.map((skill) => ({ label: `${skill.name} · ${skill.category}`, value: skill.skillCode })),
  );
  const templateOptions = computed(() =>
    usableTemplates.value.map((template) => ({
      label: `${template.name} · ${template.category}`,
      value: template.id,
    })),
  );
  const readonly = computed(() => Boolean(isArchived.value));
  const displayMessages = computed<FeishuMessage[]>(() => {
    if (!detail.value) return transientMessages.value;
    let previousUserText = '';
    const mapped = detail.value.messages.map((message) => {
      if (message.role.toLowerCase() === 'user') previousUserText = message.content;
      const retryText =
        message.role.toLowerCase() === 'assistant' && message.status.toLowerCase() === 'failed' ? previousUserText : '';
      const display = toConversationMessage(message, retryText);
      display.actionBusy = isSending.value || busyMessageIds.value.has(message.id);
      return display;
    });
    return [...mapped, ...transientMessages.value];
  });
  const canWrite = computed(() => !isArchived.value && !isSending.value && !usingTemplate.value);
  const headerModelName = computed(() => {
    if (isArchived.value)
      return `已归档 · ${detail.value?.modelName || activeModel.value?.displayName || '未指定模型'}`;
    return detail.value?.modelName || activeModel.value?.displayName || '';
  });

  let listController: AbortController | null = null;
  let detailController: AbortController | null = null;
  let sendController: AbortController | null = null;
  let disposed = false;

  async function loadModels(): Promise<void> {
    try {
      models.value = (await api.getModels()).filter((model) => model.isEnabled);
      if (!selectedModelId.value) {
        selectedModelId.value = models.value.find((model) => model.isDefault)?.id ?? models.value[0]?.id ?? '';
      }
      modelError.value = models.value.length ? '' : '当前没有可用模型。';
    } catch (requestError) {
      modelError.value = errorText(requestError);
    }
  }

  async function loadUseCatalog(): Promise<void> {
    loadingUseCatalog.value = true;
    useCatalogError.value = '';
    const [templateResult, skillResult] = await Promise.allSettled([
      api.getUsablePromptTemplates(),
      api.getUsableSkills(),
    ]);
    if (templateResult.status === 'fulfilled') {
      usableTemplates.value = templateResult.value;
      if (!selectedTemplateId.value || !templateResult.value.some((item) => item.id === selectedTemplateId.value)) {
        selectedTemplateId.value = templateResult.value[0]?.id ?? '';
      }
    } else {
      useCatalogError.value = errorText(templateResult.reason);
    }
    if (skillResult.status === 'fulfilled') {
      usableSkills.value = skillResult.value;
      if (!skillResult.value.some((item) => item.skillCode === selectedSkillCode.value)) selectedSkillCode.value = '';
    } else {
      useCatalogError.value = [useCatalogError.value, errorText(skillResult.reason)].filter(Boolean).join(' ');
    }
    loadingUseCatalog.value = false;
  }

  async function loadDetail(id: string): Promise<AiApi.ConversationDetail | null> {
    detailController?.abort();
    const controller = new AbortController();
    detailController = controller;
    loadingDetail.value = true;
    error.value = '';
    try {
      const result = await api.getConversation(id, controller.signal);
      if (disposed || controller.signal.aborted) return null;
      activeConversationId.value = id;
      detail.value = result;
      selectedModelId.value =
        result.modelId ?? models.value.find((model) => model.isDefault)?.id ?? models.value[0]?.id ?? '';
      transientMessages.value = [];
      return result;
    } catch (requestError) {
      if (!controller.signal.aborted && !disposed) error.value = errorText(requestError);
      return null;
    } finally {
      if (detailController === controller) {
        detailController = null;
        loadingDetail.value = false;
      }
    }
  }

  async function loadConversations(options: { refreshDetail?: boolean } = {}): Promise<void> {
    listController?.abort();
    const controller = new AbortController();
    listController = controller;
    loading.value = true;
    error.value = '';
    try {
      const rows = await api.getConversations({ status: managementMode.value ? 'All' : 'Active' }, controller.signal);
      if (disposed || controller.signal.aborted) return;
      conversations.value = rows;
      const nextId = rows.some((conversation) => conversation.id === activeConversationId.value)
        ? activeConversationId.value
        : (rows[0]?.id ?? '');
      if (!nextId) {
        activeConversationId.value = '';
        detail.value = null;
        transientMessages.value = [];
      } else if (options.refreshDetail || detail.value?.id !== nextId) {
        await loadDetail(nextId);
      }
    } catch (requestError) {
      if (!controller.signal.aborted && !disposed) error.value = errorText(requestError);
    } finally {
      if (listController === controller) {
        listController = null;
        loading.value = false;
      }
    }
  }

  async function createConversation(title = '新对话'): Promise<AiApi.ConversationDetail | null> {
    if (isSending.value) {
      toast.warning('请先停止当前生成，再新建会话。');
      return null;
    }
    error.value = '';
    try {
      const created = await api.createConversation({
        contextId: '',
        contextSnapshotJson: '{}',
        contextType: 'None',
        modelId: selectedModelId.value || null,
        mode: newConversationMode.value,
        title,
      });
      conversations.value = [created, ...conversations.value];
      activeConversationId.value = created.id;
      detail.value = created;
      transientMessages.value = [];
      toast.success('AI 会话已创建。');
      return created;
    } catch (requestError) {
      error.value = errorText(requestError);
      toast.error(error.value);
      return null;
    }
  }

  async function selectConversation(id: string): Promise<void> {
    if (isSending.value) {
      toast.warning('请先停止当前生成，再切换会话。');
      return;
    }
    if (id === activeConversationId.value && detail.value?.id === id) return;
    activeConversationId.value = id;
    transientMessages.value = [];
    await loadDetail(id);
  }

  function setNewConversationMode(mode: string): void {
    newConversationMode.value = mode;
  }

  function openTemplateDialog(): void {
    if (isSending.value || usingTemplate.value) return;
    templateInputJson.value = '{}';
    templateTitle.value = '';
    error.value = '';
    templateDialogOpen.value = true;
  }

  async function createFromTemplate(): Promise<void> {
    if (!selectedTemplateId.value || usingTemplate.value || isSending.value) return;
    try {
      JSON.parse(templateInputJson.value);
    } catch {
      error.value = '场景输入必须是合法 JSON。';
      return;
    }
    usingTemplate.value = true;
    error.value = '';
    try {
      const created = await api.createConversationFromTemplate(selectedTemplateId.value, {
        contextId: '',
        contextSnapshotJson: '{}',
        contextType: 'None',
        inputJson: templateInputJson.value,
        modelId: selectedModelId.value || null,
        title: templateTitle.value.trim(),
      });
      activeConversationId.value = created.id;
      detail.value = created;
      selectedModelId.value = created.modelId ?? selectedModelId.value;
      transientMessages.value = [];
      templateDialogOpen.value = false;
      await loadConversations({ refreshDetail: false });
      toast.success('已使用场景创建会话并发送提示词。');
    } catch (requestError) {
      error.value = errorText(requestError);
      toast.error(error.value);
      await loadConversations({ refreshDetail: true });
    } finally {
      usingTemplate.value = false;
    }
  }

  async function updateSelectedModel(nextModelId: string): Promise<void> {
    selectedModelId.value = nextModelId;
    if (!detail.value || detail.value.id !== activeConversationId.value || !nextModelId) return;
    try {
      const updated = await api.updateConversation(detail.value.id, {
        contextId: detail.value.contextId,
        contextSnapshotJson: detail.value.contextSnapshotJson,
        contextType: detail.value.contextType,
        modelId: nextModelId,
        title: detail.value.title,
      });
      detail.value = { ...detail.value, ...updated };
      conversations.value = conversations.value.map((item) =>
        item.id === updated.id ? { ...item, ...updated } : item,
      );
      toast.success('本会话模型已更新。');
    } catch (requestError) {
      error.value = errorText(requestError);
      selectedModelId.value = detail.value.modelId ?? '';
      toast.error(error.value);
    }
  }

  async function reconcileInterruptedSend(
    conversationId: string,
    content: string,
    baselineIds: Set<string>,
    reason: string,
    controller: AbortController,
  ): Promise<void> {
    const getLatest = async () => api.getConversation(conversationId);
    let latest: AiApi.ConversationDetail | null = null;
    for (let attempt = 0; attempt < 2; attempt += 1) {
      if (attempt > 0) await wait(350);
      try {
        latest = await getLatest();
      } catch {
        latest = null;
      }
      const newUser = latest?.messages.find(
        (message) =>
          !baselineIds.has(message.id) && message.role.toLowerCase() === 'user' && message.content === content,
      );
      const newAssistant = newUser
        ? latest?.messages.find(
            (message) =>
              !baselineIds.has(message.id) &&
              message.role.toLowerCase() === 'assistant' &&
              message.sequenceNo > newUser.sequenceNo,
          )
        : undefined;
      if (latest && newUser && newAssistant) break;
    }
    if (disposed || activeConversationId.value !== conversationId || controller.signal.reason?.name === 'UnmountError')
      return;

    if (latest) detail.value = latest;
    transientMessages.value = [];
    const newUser = latest?.messages.find(
      (message) => !baselineIds.has(message.id) && message.role.toLowerCase() === 'user' && message.content === content,
    );
    const newAssistant = newUser
      ? latest?.messages.find(
          (message) =>
            !baselineIds.has(message.id) &&
            message.role.toLowerCase() === 'assistant' &&
            message.sequenceNo > newUser.sequenceNo,
        )
      : undefined;
    if (newUser && newAssistant) {
      if (newAssistant.status.toLowerCase() === 'failed') {
        notice.value = '已重新读取服务端记录；本轮失败，可在消息卡片中重试。';
      } else {
        notice.value = '已重新读取服务端记录，本轮消息已保存。';
      }
    } else {
      const stopped = stopRequested.value;
      transientMessages.value = [
        makeTransientMessage(`recovered-user-${Date.now()}`, 'user', content, '未提交'),
        {
          ...makeTransientMessage(
            `recovered-assistant-${Date.now()}`,
            'assistant',
            reason || (stopped ? '已停止生成，本轮消息未提交到服务端。' : '连接中断，本轮消息未提交到服务端。'),
            stopped ? '已停止 · 未保存' : '未完成 · 未保存',
            content,
          ),
          errorMessage: reason || (stopped ? '已停止生成，本轮消息未保存。' : '连接中断，本轮消息未保存。'),
        },
      ];
      notice.value = '已重新读取会话，当前未发现本轮消息的服务端记录；可从消息卡片手动重试。';
    }
    try {
      conversations.value = await api.getConversations({ status: managementMode.value ? 'All' : 'Active' });
    } catch {
      // 会话详情已成功读取时，保留列表快照，避免用网络抖动清空当前页面。
    }
  }

  async function sendMessage(content: string): Promise<void> {
    const normalized = content.trim();
    if (!normalized || isSending.value || usingTemplate.value) return;
    notice.value = '';
    error.value = '';
    transientMessages.value = [];

    let current = detail.value?.id === activeConversationId.value ? detail.value : null;
    if (!current || !activeConversationId.value) {
      current = await createConversation(conversationTitle(normalized));
      if (!current) return;
    }
    if (current.status.toLowerCase() === 'archived') {
      toast.warning('已归档会话不能发送消息，请先恢复会话。');
      return;
    }
    if (current.messages.length === 0 && current.title === '新对话') {
      try {
        const updated = await api.updateConversation(current.id, {
          contextId: current.contextId,
          contextSnapshotJson: current.contextSnapshotJson,
          contextType: current.contextType,
          modelId: selectedModelId.value || null,
          title: conversationTitle(normalized),
        });
        current = { ...current, ...updated };
        detail.value = current;
      } catch (requestError) {
        error.value = errorText(requestError);
        toast.error(error.value);
        return;
      }
    }

    const conversationId = current.id;
    const baselineIds = new Set(current.messages.map((message) => message.id));
    const controller = new AbortController();
    sendController = controller;
    stopRequested.value = false;
    isSending.value = true;
    const userId = `sending-user-${Date.now()}`;
    const assistantId = `sending-assistant-${Date.now()}`;
    let streamedText = '';
    transientMessages.value = [
      makeTransientMessage(userId, 'user', normalized, '发送中'),
      { ...makeTransientMessage(assistantId, 'assistant', '', '生成中'), transient: false, retryText: undefined },
    ];

    try {
      const result = await api.streamMessage(
        conversationId,
        {
          content: normalized,
          modelId: selectedModelId.value || null,
          skillCode: selectedSkillCode.value || undefined,
        },
        {
          signal: controller.signal,
          onEvent: (event) => {
            if (event.event === 'userMessage' && event.userMessage) {
              transientMessages.value[0] = { ...toConversationMessage(event.userMessage), statusLabel: '发送中' };
            }
            if (event.event === 'delta') {
              streamedText += event.delta;
              transientMessages.value[1] = {
                ...transientMessages.value[1]!,
                text: streamedText,
                statusLabel: '生成中',
              };
            }
            if (event.event === 'error') notice.value = event.errorMessage;
          },
        },
      );
      transientMessages.value = [];
      if (result.messages.some((message) => message.status.toLowerCase() === 'failed')) {
        const failed = result.messages.find(
          (message) => message.role.toLowerCase() === 'assistant' && message.status.toLowerCase() === 'failed',
        );
        notice.value = failed?.errorMessage || 'AI 请求未成功，服务端已保存失败状态。';
      }
      await loadDetail(conversationId);
      await loadConversations();
    } catch (requestError) {
      const reason = controller.signal.aborted
        ? stopRequested.value
          ? '已停止生成，本轮请求已取消。'
          : 'AI 请求已取消或超时。'
        : errorText(requestError);
      await reconcileInterruptedSend(conversationId, normalized, baselineIds, reason, controller);
      if (!controller.signal.aborted && !disposed) error.value = reason;
    } finally {
      if (sendController === controller) {
        sendController = null;
        isSending.value = false;
      }
    }
  }

  function stopMessage(): void {
    if (!sendController || !isSending.value) return;
    stopRequested.value = true;
    sendController.abort();
  }

  async function runPlanMutation(
    messageId: string,
    action: 'confirm' | 'draft' | 'toggle',
    step?: { isDone: boolean; stepId: string },
  ): Promise<void> {
    const conversationId = detail.value?.id;
    if (!conversationId || isSending.value) return;
    busyMessageIds.value = new Set(busyMessageIds.value).add(messageId);
    error.value = '';
    try {
      if (action === 'confirm') await api.confirmPlan(conversationId, { confirmed: true, messageId });
      else if (action === 'toggle' && step) {
        await api.togglePlanStep(conversationId, { isDone: step.isDone, messageId, stepId: step.stepId });
      } else if (action === 'draft') {
        detail.value = await api.createPlanDraft(conversationId, { messageId });
      }
      await loadDetail(conversationId);
      await api
        .getConversations({ status: managementMode.value ? 'All' : 'Active' })
        .then((rows) => (conversations.value = rows));
      toast.success(
        action === 'draft' ? 'AI 任务草稿已生成。' : action === 'toggle' ? '计划步骤状态已更新。' : '计划已确认。',
      );
    } catch (requestError) {
      error.value = errorText(requestError);
      toast.error(error.value);
    } finally {
      const next = new Set(busyMessageIds.value);
      next.delete(messageId);
      busyMessageIds.value = next;
    }
  }

  async function toggleArchive(): Promise<void> {
    const id = detail.value?.id;
    if (!id || isSending.value) return;
    const wasArchived = isArchived.value;
    try {
      if (wasArchived) await api.restoreConversation(id);
      else await api.archiveConversation(id);
      await loadConversations({ refreshDetail: managementMode.value });
      toast.success(wasArchived ? '会话已恢复。' : '会话已归档。');
    } catch (requestError) {
      error.value = errorText(requestError);
      toast.error(error.value);
    }
  }

  async function exportConversation(): Promise<void> {
    const id = detail.value?.id;
    if (!id) return;
    try {
      const exported = await api.exportConversation(id);
      const objectUrl = URL.createObjectURL(new Blob([exported.content], { type: 'text/markdown;charset=utf-8' }));
      const link = document.createElement('a');
      link.href = objectUrl;
      link.download = exported.fileName;
      link.click();
      window.setTimeout(() => URL.revokeObjectURL(objectUrl), 0);
      toast.success('会话 Markdown 已导出。');
    } catch (requestError) {
      error.value = errorText(requestError);
      toast.error(error.value);
    }
  }

  function requestDeleteConversation(): void {
    if (detail.value && !isSending.value) deleteDialogOpen.value = true;
  }

  async function deleteConversation(): Promise<void> {
    const id = detail.value?.id;
    if (!id || deleting.value) return;
    deleting.value = true;
    try {
      await api.deleteConversation(id);
      deleteDialogOpen.value = false;
      activeConversationId.value = '';
      detail.value = null;
      transientMessages.value = [];
      await loadConversations();
      toast.success('会话已删除。');
    } catch (requestError) {
      error.value = errorText(requestError);
      toast.error(error.value);
    } finally {
      deleting.value = false;
    }
  }

  async function refresh(): Promise<void> {
    if (isSending.value) {
      toast.warning('生成过程中请先停止，再刷新会话。');
      return;
    }
    await loadConversations({ refreshDetail: Boolean(activeConversationId.value) });
  }

  watch(managementMode, () => {
    if (!isSending.value) void loadConversations({ refreshDetail: true });
  });

  onBeforeUnmount(() => {
    disposed = true;
    listController?.abort();
    detailController?.abort();
    sendController?.abort(new DOMException('页面已关闭', 'UnmountError'));
  });

  void Promise.all([loadModels(), loadUseCatalog()]).then(() => loadConversations());

  return {
    activeConversation,
    activeConversationId,
    activeModel,
    canWrite,
    canStartFromTemplate,
    conversations,
    conversationItems,
    createConversation,
    createPlanDraft: (messageId: string) => runPlanMutation(messageId, 'draft'),
    deleteConversation,
    deleteDialogOpen,
    deleting,
    detail,
    displayMessages,
    error,
    exportConversation,
    headerModelName,
    isArchived,
    isSending,
    loading,
    loadingDetail,
    loadingUseCatalog,
    managementMode,
    modelError,
    modelOptions,
    models,
    modeOptions,
    notice,
    newConversationMode,
    readonly,
    refresh,
    requestDeleteConversation,
    runPlanMutation,
    selectConversation,
    selectedModelId,
    selectedSkillCode,
    selectedTemplateId,
    setNewConversationMode,
    skillOptions,
    sendMessage,
    stopMessage,
    toggleArchive,
    templateDialogOpen,
    templateInputJson,
    templateOptions,
    templateTitle,
    useCatalogError,
    usableTemplates,
    usingTemplate,
    openTemplateDialog,
    createFromTemplate,
    updateSelectedModel,
  };
}
