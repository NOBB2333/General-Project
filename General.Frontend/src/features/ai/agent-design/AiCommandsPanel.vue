<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Command } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import { toast, UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiSwitch, UiTextarea } from '@/components/ui';

const api = createAiApi(apiClient);
const items = ref<AiApi.Command[]>([]);
const busy = ref(false);
const saving = ref(false);
const deleting = ref(false);
const loadError = ref('');
const actionError = ref('');
const formError = ref('');
const dialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const editing = ref<AiApi.Command | null>(null);
const deleteTarget = ref<AiApi.Command | null>(null);
const canManage = computed(() => canAccess('Ai.Agent.Manage'));

function blankCommand(): AiApi.CommandSaveInput {
  return {
    agentCode: '',
    commandCode: '',
    configJson: '{}',
    description: '',
    exampleInput: '',
    inputPlaceholder: '',
    isEnabled: true,
    mode: 'General',
    name: '',
    promptTemplateCode: '',
    skillCode: '',
    sort: 0,
    slashName: '',
  };
}

const draft = ref<AiApi.CommandSaveInput>(blankCommand());

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

const rows = computed<ListPageRow[]>(() =>
  items.value.map((item) => ({
    agentCode: item.agentCode || '—',
    commandCode: item.commandCode,
    id: item.id,
    mode: item.mode,
    name: item.name,
    slashName: item.slashName,
    status: item.isEnabled ? '启用' : '停用',
    type: item.isBuiltin ? '内置' : '自定义',
  })),
);

const pageModel = computed<ListPageModel>(() => {
  const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' }];
  if (canManage.value) {
    actions.push(
      { id: 'create', kind: 'create', label: '新增命令' },
      { id: 'edit', kind: 'custom', label: '编辑命令', requiresSelection: true },
      { id: 'delete', kind: 'delete', label: '删除命令', requiresSelection: true },
    );
  }
  return {
    actions,
    columns: [
      { key: 'name', title: '命令名称', width: 220, sticky: true },
      { key: 'slashName', title: '快捷入口', width: 160 },
      { key: 'commandCode', title: '命令编码', width: 220 },
      { key: 'agentCode', title: 'Agent编码', width: 180 },
      { key: 'mode', title: '模式', width: 120 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
      { key: 'type', title: '来源', width: 100 },
    ],
    demoPath: '/ai/agent-design',
    description: '配置在对话中可用的快捷命令；本页只维护定义，不直接执行命令。',
    emptyHint: busy.value ? '正在读取命令…' : loadError.value ? '命令读取失败，请刷新重试。' : '暂无命令定义。',
    filters: [],
    key: 'AI管理|快捷命令',
    label: '快捷命令',
    listScopes: [{ id: 'all', label: '全部命令', count: items.value.length }],
    listTabs: [{ id: 'all', label: '全部命令' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-commands',
    pageSize: 20,
    panels: [],
    path: '/ai/agent-design',
    primaryAction: '新增命令',
    replication: { status: 'implemented', referenceUrl: '/ai/conversations' },
    routeName: 'ai-agent-design',
    rows: rows.value,
    searchPlaceholder: '搜索命令名称、编码或快捷入口',
    stats: [
      { key: 'total', label: '全部命令', value: String(items.value.length), tone: 'primary' },
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
    items.value = await api.getCommands({ includeDisabled: true });
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    busy.value = false;
  }
}

function openForm(item: AiApi.Command | null): void {
  editing.value = item;
  formError.value = '';
  draft.value = item
    ? {
        agentCode: item.agentCode,
        commandCode: item.commandCode,
        configJson: item.configJson,
        description: item.description,
        exampleInput: item.exampleInput,
        inputPlaceholder: item.inputPlaceholder,
        isEnabled: item.isEnabled,
        mode: item.mode,
        name: item.name,
        promptTemplateCode: item.promptTemplateCode,
        skillCode: item.skillCode,
        sort: item.sort,
        slashName: item.slashName,
      }
    : blankCommand();
  dialogOpen.value = true;
}

function selected(selection: ListPageRow[]): AiApi.Command | null {
  const item = selection.length === 1 ? items.value.find((command) => command.id === selection[0]?.id) : null;
  actionError.value = item ? '' : '请选择一条命令记录。';
  return item ?? null;
}

function handleAction(action: PageAction, selection: ListPageRow[]): void {
  if (action.id === 'reload') return void load();
  if (action.id === 'create') return openForm(null);
  const item = selected(selection);
  if (!item) return;
  if (action.id === 'edit') openForm(item);
  if (action.id === 'delete') {
    if (item.isBuiltin) return void toast.warning('内置命令不能删除，可停用或调整配置。');
    deleteTarget.value = item;
    deleteDialogOpen.value = true;
  }
}

async function save(): Promise<void> {
  if (saving.value) return;
  formError.value = '';
  const input = draft.value;
  if (!input.commandCode.trim() || !input.name.trim() || !input.slashName.trim()) {
    formError.value = '命令编码、名称和快捷入口为必填项。';
    return;
  }
  if (!validJson(input.configJson)) {
    formError.value = '命令配置必须是合法 JSON。';
    return;
  }
  saving.value = true;
  try {
    const payload: AiApi.CommandSaveInput = {
      ...input,
      agentCode: input.agentCode.trim(),
      commandCode: input.commandCode.trim(),
      name: input.name.trim(),
      promptTemplateCode: input.promptTemplateCode.trim(),
      skillCode: input.skillCode.trim(),
      slashName: input.slashName.trim(),
    };
    if (editing.value) await api.updateCommand(editing.value.id, payload);
    else await api.createCommand(payload);
    dialogOpen.value = false;
    await load();
    toast.success('命令配置已保存。');
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
    await api.deleteCommand(deleteTarget.value.id);
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    await load();
    toast.success('命令已删除。');
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
      ><Command
        :size="22"
        aria-hidden="true"
    /></template>
    <template #notice>
      <p>快捷命令由对话流程解析并执行；当前后端没有独立命令执行或审批端点。</p>
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
    </template>
  </ListPageSurface>

  <UiDialog
    :open="dialogOpen"
    :title="editing ? `编辑命令：${editing.name}` : '新增快捷命令'"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form @submit.prevent="save">
      <UiFormField label="命令名称">
        <UiInput
          v-model="draft.name"
          maxlength="128"
          required
        />
      </UiFormField>
      <UiFormField label="命令编码">
        <UiInput
          v-model="draft.commandCode"
          maxlength="128"
          required
          :readonly="Boolean(editing)"
        />
      </UiFormField>
      <UiFormField label="快捷入口">
        <UiInput
          v-model="draft.slashName"
          maxlength="64"
          required
          placeholder="例如：/周报"
        />
      </UiFormField>
      <UiFormField label="运行模式">
        <UiInput
          v-model="draft.mode"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField label="关联 Agent 编码">
        <UiInput
          v-model="draft.agentCode"
          maxlength="128"
          placeholder="可留空"
        />
      </UiFormField>
      <UiFormField label="关联技能编码">
        <UiInput
          v-model="draft.skillCode"
          maxlength="128"
          placeholder="可留空"
        />
      </UiFormField>
      <UiFormField label="提示词模板编码">
        <UiInput
          v-model="draft.promptTemplateCode"
          maxlength="128"
          placeholder="可留空"
        />
      </UiFormField>
      <UiFormField label="输入占位提示">
        <UiInput
          v-model="draft.inputPlaceholder"
          maxlength="256"
        />
      </UiFormField>
      <UiFormField label="示例输入">
        <UiInput
          v-model="draft.exampleInput"
          maxlength="512"
        />
      </UiFormField>
      <UiFormField label="排序">
        <UiInput
          :model-value="String(draft.sort)"
          type="number"
          @update:model-value="draft.sort = Number($event)"
        />
      </UiFormField>
      <UiFormField label="命令状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用命令"
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
        label="配置 JSON"
        wide
      >
        <UiTextarea
          v-model="draft.configJson"
          rows="5"
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
      >
        {{ saving ? '保存中…' : '保存配置' }}
      </UiButton>
    </template>
  </UiDialog>

  <UiAlertDialog
    :open="deleteDialogOpen"
    title="确认删除命令"
    :description="deleteTarget ? `删除“${deleteTarget.name}”？此操作无法撤销。` : ''"
    confirm-label="删除"
    @update:open="deleteDialogOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />
</template>
