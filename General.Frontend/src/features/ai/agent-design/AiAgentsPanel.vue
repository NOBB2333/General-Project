<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Bot } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import { toast, UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiSwitch, UiTextarea } from '@/components/ui';

const api = createAiApi(apiClient);
const items = ref<AiApi.Agent[]>([]);
const busy = ref(false);
const saving = ref(false);
const deleting = ref(false);
const loadError = ref('');
const actionError = ref('');
const formError = ref('');
const dialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const previewDialogOpen = ref(false);
const editing = ref<AiApi.Agent | null>(null);
const deleteTarget = ref<AiApi.Agent | null>(null);
const previewAgent = ref<AiApi.Agent | null>(null);
const previewInputJson = ref('{}');
const previewError = ref('');
const previewBusy = ref(false);
const preview = ref<AiApi.AgentRunPlan | null>(null);
const canManage = computed(() => canAccess('Ai.Agent.Manage'));

function blankAgent(): AiApi.AgentSaveInput {
  return {
    agentCode: '',
    defaultModelId: null,
    description: '',
    isEnabled: true,
    name: '',
    personaCode: '',
    roleType: 'General',
    runtimeConfigJson: '{}',
    skillCodes: [],
    sort: 0,
    systemPrompt: '',
    toolCodes: [],
  };
}

const draft = ref<AiApi.AgentSaveInput>(blankAgent());
const defaultModelIdText = computed({
  get: () => draft.value.defaultModelId ?? '',
  set: (value: string) => {
    draft.value.defaultModelId = value || null;
  },
});

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function validJson(value: string): boolean {
  try {
    JSON.parse(value);
    return true;
  } catch {
    return false;
  }
}

function splitCodes(value: string): string[] {
  return [
    ...new Set(
      value
        .split(/[，,\n]/u)
        .map((item) => item.trim())
        .filter(Boolean),
    ),
  ];
}

const rows = computed<ListPageRow[]>(() =>
  items.value.map((item) => ({
    agentCode: item.agentCode,
    defaultModel: item.defaultModelName || '运行时默认',
    id: item.id,
    name: item.name,
    personaCode: item.personaCode || '—',
    roleType: item.roleType,
    status: item.isEnabled ? '启用' : '停用',
    type: item.isBuiltin ? '内置' : '自定义',
  })),
);

const pageModel = computed<ListPageModel>(() => {
  const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' }];
  if (canManage.value) {
    actions.push(
      { id: 'create', kind: 'create', label: '新增Agent' },
      { id: 'edit', kind: 'custom', label: '编辑Agent', requiresSelection: true },
      { id: 'preview', kind: 'custom', label: '运行计划预览', requiresSelection: true },
      { id: 'delete', kind: 'delete', label: '删除Agent', requiresSelection: true },
    );
  }
  return {
    actions,
    columns: [
      { key: 'name', title: 'Agent名称', width: 220, sticky: true },
      { key: 'agentCode', title: 'Agent编码', width: 220 },
      { key: 'roleType', title: '角色类型', width: 140 },
      { key: 'personaCode', title: '角色画像', width: 180 },
      { key: 'defaultModel', title: '默认模型', width: 180 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
      { key: 'type', title: '来源', width: 100 },
    ],
    demoPath: '/ai/agent-design',
    description: '维护可复用的Agent定义，并在保存前查看服务端装配计划。',
    emptyHint: busy.value ? '正在读取Agent…' : loadError.value ? 'Agent读取失败，请刷新重试。' : '暂无Agent定义。',
    filters: [],
    key: 'AI管理|Agent',
    label: 'Agent',
    listScopes: [{ id: 'all', label: '全部Agent', count: items.value.length }],
    listTabs: [{ id: 'all', label: '全部Agent' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-agents',
    pageSize: 20,
    panels: [],
    path: '/ai/agent-design',
    primaryAction: '新增Agent',
    replication: { status: 'implemented', referenceUrl: '/ai/conversations' },
    routeName: 'ai-agent-design',
    rows: rows.value,
    searchPlaceholder: '搜索Agent名称、编码或说明',
    stats: [
      { key: 'total', label: '全部Agent', value: String(items.value.length), tone: 'primary' },
      {
        key: 'enabled',
        label: '启用',
        value: String(items.value.filter((item) => item.isEnabled).length),
        tone: 'success',
      },
      {
        key: 'builtin',
        label: '内置',
        value: String(items.value.filter((item) => item.isBuiltin).length),
        tone: 'warning',
      },
      {
        key: 'custom',
        label: '自定义',
        value: String(items.value.filter((item) => !item.isBuiltin).length),
        tone: 'neutral',
      },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
});

async function load(): Promise<void> {
  busy.value = true;
  loadError.value = '';
  try {
    items.value = await api.getAgents({ includeDisabled: true });
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    busy.value = false;
  }
}

function openForm(item: AiApi.Agent | null): void {
  editing.value = item;
  formError.value = '';
  draft.value = item
    ? {
        agentCode: item.agentCode,
        defaultModelId: item.defaultModelId,
        description: item.description,
        isEnabled: item.isEnabled,
        name: item.name,
        personaCode: item.personaCode,
        roleType: item.roleType,
        runtimeConfigJson: item.runtimeConfigJson,
        skillCodes: [...item.skillCodes],
        sort: item.sort,
        systemPrompt: item.systemPrompt,
        toolCodes: [...item.toolCodes],
      }
    : blankAgent();
  dialogOpen.value = true;
}

function selected(selection: ListPageRow[]): AiApi.Agent | null {
  const item = selection.length === 1 ? items.value.find((agent) => agent.id === selection[0]?.id) : null;
  actionError.value = item ? '' : '请选择一条Agent记录。';
  return item ?? null;
}

function handleAction(action: PageAction, selection: ListPageRow[]): void {
  if (action.id === 'reload') return void load();
  if (action.id === 'create') return openForm(null);
  const item = selected(selection);
  if (!item) return;
  if (action.id === 'edit') openForm(item);
  if (action.id === 'preview') openPreview(item);
  if (action.id === 'delete') {
    if (item.isBuiltin) return void toast.warning('内置Agent不能删除，可停用或调整配置。');
    deleteTarget.value = item;
    deleteDialogOpen.value = true;
  }
}

function openPreview(item: AiApi.Agent): void {
  previewAgent.value = item;
  previewInputJson.value = '{}';
  preview.value = null;
  previewError.value = '';
  previewDialogOpen.value = true;
}

async function buildRunPlan(): Promise<void> {
  if (!previewAgent.value || previewBusy.value) return;
  previewError.value = '';
  preview.value = null;
  if (!validJson(previewInputJson.value)) {
    previewError.value = '输入内容必须是合法 JSON。';
    return;
  }
  previewBusy.value = true;
  try {
    preview.value = await api.buildAgentRunPlan(previewAgent.value.id, { inputJson: previewInputJson.value });
  } catch (error) {
    previewError.value = errorText(error);
  } finally {
    previewBusy.value = false;
  }
}

async function save(): Promise<void> {
  if (saving.value) return;
  formError.value = '';
  const input = draft.value;
  if (!input.agentCode.trim() || !input.name.trim() || !input.systemPrompt.trim()) {
    formError.value = 'Agent编码、名称和系统提示词为必填项。';
    return;
  }
  if (!validJson(input.runtimeConfigJson)) {
    formError.value = '运行配置必须是合法 JSON。';
    return;
  }
  const modelId = input.defaultModelId?.trim() ?? '';
  if (modelId && !/^[\da-f]{8}-(?:[\da-f]{4}-){3}[\da-f]{12}$/iu.test(modelId)) {
    formError.value = '默认模型 ID 必须是有效 GUID，或留空使用运行时默认模型。';
    return;
  }
  saving.value = true;
  try {
    const payload: AiApi.AgentSaveInput = {
      ...input,
      agentCode: input.agentCode.trim(),
      defaultModelId: modelId || null,
      name: input.name.trim(),
      personaCode: input.personaCode.trim(),
      roleType: input.roleType.trim() || 'General',
      skillCodes: [...input.skillCodes],
      systemPrompt: input.systemPrompt.trim(),
      toolCodes: [...input.toolCodes],
    };
    if (editing.value) await api.updateAgent(editing.value.id, payload);
    else await api.createAgent(payload);
    dialogOpen.value = false;
    await load();
    toast.success('Agent配置已保存。');
  } catch (error) {
    formError.value = errorText(error);
  } finally {
    saving.value = false;
  }
}

async function confirmDelete(): Promise<void> {
  if (!deleteTarget.value || deleting.value) return;
  deleting.value = true;
  try {
    await api.deleteAgent(deleteTarget.value.id);
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    await load();
    toast.success('Agent已删除。');
  } catch (error) {
    actionError.value = errorText(error);
    toast.error(actionError.value);
  } finally {
    deleting.value = false;
  }
}

onMounted(() => void load());
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="handleAction"
    @row="(row) => canManage && openForm(items.find((item) => item.id === row.id) ?? null)"
  >
    <template #hero-icon
      ><Bot
        :size="22"
        aria-hidden="true"
    /></template>
    <template #notice>
      <p>运行计划预览只装配Agent配置，不会调用模型、执行工具或创建会话。</p>
      <p
        v-if="loadError"
        role="alert"
      >
        {{ loadError }}
      </p>
      <p
        v-if="actionError"
        role="alert"
      >
        {{ actionError }}
      </p>
      <p
        v-if="!canManage"
        role="alert"
      >
        当前账号没有Agent管理权限。
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    :open="dialogOpen"
    :title="editing ? `编辑Agent：${editing.name}` : '新增Agent'"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form @submit.prevent="save">
      <UiFormField label="Agent名称">
        <UiInput
          v-model="draft.name"
          maxlength="128"
          required
        />
      </UiFormField>
      <UiFormField label="Agent编码">
        <UiInput
          v-model="draft.agentCode"
          maxlength="128"
          required
          :readonly="Boolean(editing)"
        />
      </UiFormField>
      <UiFormField label="角色类型">
        <UiInput
          v-model="draft.roleType"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField label="角色画像编码">
        <UiInput
          v-model="draft.personaCode"
          maxlength="128"
          placeholder="可留空"
        />
      </UiFormField>
      <UiFormField
        label="默认模型 ID"
        hint="填写模型 GUID；留空时由运行时选择默认模型。"
      >
        <UiInput
          v-model="defaultModelIdText"
          maxlength="36"
          placeholder="可留空"
        />
      </UiFormField>
      <UiFormField label="排序">
        <UiInput
          :model-value="String(draft.sort)"
          type="number"
          @update:model-value="draft.sort = Number($event)"
        />
      </UiFormField>
      <UiFormField label="Agent状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用Agent"
        />
      </UiFormField>
      <UiFormField
        label="说明"
        wide
      >
        <UiTextarea
          v-model="draft.description"
          rows="2"
          maxlength="512"
        />
      </UiFormField>
      <UiFormField
        label="技能编码"
        hint="多个编码用逗号或换行分隔。"
        wide
      >
        <UiTextarea
          :model-value="draft.skillCodes.join('\n')"
          rows="3"
          @update:model-value="draft.skillCodes = splitCodes($event)"
        />
      </UiFormField>
      <UiFormField
        label="工具编码"
        hint="多个编码用逗号或换行分隔；工具运行仍受工具权限限制。"
        wide
      >
        <UiTextarea
          :model-value="draft.toolCodes.join('\n')"
          rows="3"
          @update:model-value="draft.toolCodes = splitCodes($event)"
        />
      </UiFormField>
      <UiFormField
        label="运行配置 JSON"
        wide
      >
        <UiTextarea
          v-model="draft.runtimeConfigJson"
          rows="4"
          maxlength="8000"
        />
      </UiFormField>
      <UiFormField
        label="系统提示词"
        wide
      >
        <UiTextarea
          v-model="draft.systemPrompt"
          rows="8"
          maxlength="16000"
          required
        />
      </UiFormField>
      <p
        v-if="formError"
        role="alert"
      >
        {{ formError }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="dialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="saving"
        @click="save"
      >
        {{ saving ? '保存中…' : '保存配置' }}
      </UiButton>
    </template>
  </UiDialog>

  <UiDialog
    :open="previewDialogOpen"
    :title="previewAgent ? `运行计划预览：${previewAgent.name}` : '运行计划预览'"
    size="lg"
    @update:open="previewDialogOpen = $event"
  >
    <UiFormField
      label="输入 JSON"
      wide
    >
      <UiTextarea
        v-model="previewInputJson"
        rows="4"
        maxlength="8000"
      />
    </UiFormField>
    <p>此接口只返回Agent、模型、角色画像、技能、工具及系统提示词的装配结果，不执行模型或工具调用。</p>
    <p
      v-if="previewError"
      role="alert"
    >
      {{ previewError }}
    </p>
    <template v-if="preview">
      <UiFormField label="Agent / 模型">
        <UiInput
          :model-value="`${preview.agentName}（${preview.modelName || '运行时默认'}）`"
          readonly
        />
      </UiFormField>
      <UiFormField
        label="角色画像 / 技能 / 工具"
        wide
      >
        <UiTextarea
          :model-value="
            [
              `角色画像：${preview.personaCode || '无'}`,
              `技能：${preview.skillCodes.join('、') || '无'}`,
              `工具：${preview.toolCodes.join('、') || '无'}`,
            ].join('\n')
          "
          rows="4"
          readonly
        />
      </UiFormField>
      <UiFormField
        label="系统提示词"
        wide
      >
        <UiTextarea
          :model-value="preview.systemPrompt"
          rows="8"
          readonly
        />
      </UiFormField>
    </template>
    <template #footer>
      <UiButton
        :disabled="previewBusy"
        @click="previewDialogOpen = false"
        >关闭</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="previewBusy"
        @click="buildRunPlan"
      >
        {{ previewBusy ? '生成中…' : '生成计划预览' }}
      </UiButton>
    </template>
  </UiDialog>

  <UiAlertDialog
    :open="deleteDialogOpen"
    title="确认删除Agent"
    :description="deleteTarget ? `删除“${deleteTarget.name}”？此操作无法撤销。` : ''"
    confirm-label="删除"
    @update:open="deleteDialogOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />
</template>
