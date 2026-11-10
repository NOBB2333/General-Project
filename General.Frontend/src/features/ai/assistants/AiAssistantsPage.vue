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
const assistants = ref<AiApi.Assistant[]>([]);
const loading = ref(false);
const saving = ref(false);
const loadError = ref('');
const formError = ref('');
const actionError = ref('');
const dialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const editing = ref<AiApi.Assistant | null>(null);
const deleteTarget = ref<AiApi.Assistant | null>(null);
const canManage = computed(() => canAccess('Ai.Assistant.Manage'));
const draft = ref<AiApi.AssistantSaveInput>(blankDraft());

function blankDraft(): AiApi.AssistantSaveInput {
  return {
    assistantCode: '',
    configJson: '{}',
    defaultMode: 'General',
    defaultModelId: null,
    description: '',
    isEnabled: true,
    name: '',
    roleType: 'General',
    sort: 0,
    systemPrompt: '',
    toolCodes: [],
  };
}

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
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
  assistants.value.map((item) => ({
    assistantCode: item.assistantCode,
    defaultModel: item.defaultModelName || '未指定',
    defaultMode: item.defaultMode,
    id: item.id,
    name: item.name,
    roleType: item.roleType,
    status: item.isEnabled ? '启用' : '停用',
    type: item.isBuiltin ? '内置' : '自定义',
  })),
);

const pageModel = computed<ListPageModel>(() => {
  const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }];
  if (canManage.value) {
    actions.push(
      { id: 'create', kind: 'create', label: '新增助手' },
      { id: 'edit', kind: 'custom', label: '编辑助手', requiresSelection: true },
      { id: 'delete', kind: 'delete', label: '删除助手', requiresSelection: true },
    );
  }
  const enabled = assistants.value.filter((item) => item.isEnabled).length;
  return {
    actions,
    columns: [
      { key: 'name', title: '助手名称', width: 220, sticky: true },
      { key: 'assistantCode', title: '助手编码', width: 220 },
      { key: 'roleType', title: '角色类型', width: 140 },
      { key: 'defaultMode', title: '默认模式', width: 120 },
      { key: 'defaultModel', title: '默认模型', width: 180 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
      { key: 'type', title: '来源', width: 100 },
    ],
    demoPath: '/ai/assistants',
    description: '维护场景模板关联的助手角色、系统提示词和默认模型。',
    emptyHint: loading.value ? '正在读取助手…' : loadError.value ? '助手读取失败，请刷新重试。' : '暂无助手配置。',
    filters: [],
    key: 'AI管理|助手角色',
    label: '助手角色',
    listScopes: [{ id: 'all', label: '全部助手', count: assistants.value.length }],
    listTabs: [{ id: 'all', label: '全部助手' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-assistants',
    pageSize: 20,
    panels: [],
    path: '/ai/assistants',
    primaryAction: '新增助手',
    replication: { status: 'implemented', referenceUrl: '/system-settings/users-roles' },
    routeName: 'ai-assistants',
    rows: rows.value,
    searchPlaceholder: '搜索助手名称、编码或角色类型',
    stats: [
      { key: 'total', label: '全部助手', value: String(assistants.value.length), tone: 'primary' },
      { key: 'enabled', label: '启用', value: String(enabled), tone: 'success' },
      {
        key: 'builtin',
        label: '内置',
        value: String(assistants.value.filter((item) => item.isBuiltin).length),
        tone: 'warning',
      },
      {
        key: 'custom',
        label: '自定义',
        value: String(assistants.value.filter((item) => !item.isBuiltin).length),
        tone: 'neutral',
      },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
});

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = '';
  try {
    assistants.value = await api.getAssistants(true);
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    loading.value = false;
  }
}

function openForm(item: AiApi.Assistant | null): void {
  editing.value = item;
  formError.value = '';
  draft.value = item
    ? {
        assistantCode: item.assistantCode,
        configJson: item.configJson,
        defaultMode: item.defaultMode,
        defaultModelId: item.defaultModelId,
        description: item.description,
        isEnabled: item.isEnabled,
        name: item.name,
        roleType: item.roleType,
        sort: item.sort,
        systemPrompt: item.systemPrompt,
        toolCodes: [...item.toolCodes],
      }
    : blankDraft();
  dialogOpen.value = true;
}

function selectedAssistant(selection: ListPageRow[]): AiApi.Assistant | null {
  const selected = selection.length === 1 ? assistants.value.find((item) => item.id === selection[0]?.id) : null;
  if (!selected) actionError.value = '请选择一条助手记录。';
  else actionError.value = '';
  return selected ?? null;
}

function handleAction(action: PageAction, selection: ListPageRow[]): void {
  if (action.id === 'reload') {
    void load();
    return;
  }
  if (action.id === 'create') {
    openForm(null);
    return;
  }
  const selected = selectedAssistant(selection);
  if (!selected) return;
  if (action.id === 'edit') openForm(selected);
  if (action.id === 'delete') {
    if (selected.isBuiltin) {
      toast.warning('内置助手不能删除，可停用或调整提示词。');
      return;
    }
    deleteTarget.value = selected;
    deleteDialogOpen.value = true;
  }
}

function openRow(row: ListPageRow): void {
  const assistant = assistants.value.find((item) => item.id === row.id);
  if (assistant && canManage.value) openForm(assistant);
}

async function save(): Promise<void> {
  if (saving.value) return;
  if (!draft.value.name.trim() || !draft.value.assistantCode.trim() || !draft.value.systemPrompt.trim()) {
    formError.value = '助手名称、编码和系统提示词为必填项。';
    return;
  }
  try {
    JSON.parse(draft.value.configJson);
  } catch {
    formError.value = '配置 JSON 格式无效。';
    return;
  }
  saving.value = true;
  formError.value = '';
  const input = { ...draft.value, assistantCode: draft.value.assistantCode.trim(), name: draft.value.name.trim() };
  try {
    if (editing.value) await api.updateAssistant(editing.value.id, input);
    else await api.createAssistant(input);
    dialogOpen.value = false;
    await load();
    toast.success('助手配置已保存。');
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
    await api.deleteAssistant(deleteTarget.value.id);
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    await load();
    toast.success('助手配置已删除。');
  } catch (error) {
    actionError.value = errorText(error);
    toast.error(actionError.value);
  } finally {
    deleting.value = false;
  }
}

const deleting = ref(false);

onMounted(() => void load());
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="handleAction"
    @row="openRow"
  >
    <template #hero-icon>
      <Bot
        :size="22"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <p>系统提示词只通过助手管理权限读取；普通会话使用者通过场景模板间接使用助手配置。</p>
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
        当前账号没有助手管理权限。
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    :open="dialogOpen"
    :title="editing ? `编辑助手：${editing.name}` : '新增助手'"
    description="助手参数由场景模板在会话中引用；保存后才会生效。"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form @submit.prevent="save">
      <UiFormField label="助手名称">
        <UiInput
          v-model="draft.name"
          maxlength="128"
          required
        />
      </UiFormField>
      <UiFormField label="助手编码">
        <UiInput
          v-model="draft.assistantCode"
          maxlength="64"
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
      <UiFormField label="默认模式">
        <UiInput
          v-model="draft.defaultMode"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField
        label="默认模型 ID"
        hint="可填写模型 ID；留空时由会话使用端选择模型。"
      >
        <UiInput
          :model-value="draft.defaultModelId ?? ''"
          maxlength="36"
          placeholder="模型 GUID，可留空"
          @update:model-value="draft.defaultModelId = $event || null"
        />
      </UiFormField>
      <UiFormField label="排序">
        <UiInput
          :model-value="String(draft.sort)"
          type="number"
          @update:model-value="draft.sort = Number($event)"
        />
      </UiFormField>
      <UiFormField label="助手状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用助手"
        />
      </UiFormField>
      <UiFormField
        label="描述"
        wide
      >
        <UiTextarea
          v-model="draft.description"
          rows="2"
          maxlength="512"
        />
      </UiFormField>
      <UiFormField
        label="系统提示词"
        hint="此内容仅对助手管理权限开放。"
        wide
      >
        <UiTextarea
          v-model="draft.systemPrompt"
          rows="8"
          maxlength="16000"
          required
        />
      </UiFormField>
      <UiFormField
        label="工具编码"
        hint="多个编码用逗号或换行分隔。"
        wide
      >
        <UiTextarea
          :model-value="draft.toolCodes.join('\n')"
          rows="3"
          @update:model-value="draft.toolCodes = splitCodes($event)"
        />
      </UiFormField>
      <UiFormField
        label="配置 JSON"
        wide
      >
        <UiTextarea
          v-model="draft.configJson"
          rows="4"
          maxlength="8000"
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
        >{{ saving ? '保存中…' : '保存助手' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiAlertDialog
    :open="deleteDialogOpen"
    title="确认删除助手"
    :description="deleteTarget ? `删除“${deleteTarget.name}”？此操作无法撤销。` : ''"
    confirm-label="删除助手"
    @update:open="deleteDialogOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />
</template>
