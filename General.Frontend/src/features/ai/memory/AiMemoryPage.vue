<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { DatabaseZap } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { PageColumn } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { getProjectListApi, type ProjectApi } from '@/api/core/project';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import {
  toast,
  UiAlertDialog,
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiSwitch,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  UiTextarea,
  type UiSelectOption,
} from '@/components/ui';

const api = createAiApi(apiClient);
const memories = ref<AiApi.Memory[]>([]);
const sources = ref<AiApi.KnowledgeSource[]>([]);
const projects = ref<ProjectApi.ListItem[]>([]);
const activeTab = ref('memories');
const busy = ref(false);
const saving = ref(false);
const loadError = ref('');
const actionBusy = ref(false);
const memoryDialogOpen = ref(false);
const sourceDialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const importDialogOpen = ref(false);
const importConfirmOpen = ref(false);
const editing = ref<AiApi.Memory | null>(null);
const selectedMemory = ref<AiApi.Memory | null>(null);
const selectedSource = ref<AiApi.KnowledgeSource | null>(null);
const importProjectId = ref('');
const importKind = ref<'documents' | 'meetings' | 'risks'>('documents');
const formError = ref('');
const canManage = computed(() => canAccess('Ai.Memory.Manage'));

function blankMemory(): AiApi.MemorySaveInput {
  return {
    content: '',
    contextId: null,
    contextType: null,
    importance: 50,
    isEnabled: true,
    knowledgeSourceId: null,
    memoryType: 'Project',
    ownerUserId: null,
    sourceReferencesJson: '[]',
    summary: '',
    tagsJson: '[]',
    title: '',
  };
}

const draft = ref<AiApi.MemorySaveInput>(blankMemory());
const projectOptions = computed<UiSelectOption[]>(() =>
  projects.value.map((item) => ({
    label: `${item.projectCode} · ${item.name}`,
    value: item.id,
  })),
);

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function validArray(value: string): boolean {
  try {
    return Array.isArray(JSON.parse(value));
  } catch {
    return false;
  }
}

function formatTime(value: string | null): string {
  return value ? new Date(value).toLocaleString('zh-CN') : '—';
}

const memoryRows = computed<ListPageRow[]>(() =>
  memories.value.map((item) => ({
    creationTime: formatTime(item.creationTime),
    id: item.id,
    importance: item.importance,
    memoryType: item.memoryType,
    ownerUserId: item.ownerUserId ?? '当前上下文',
    status: item.isEnabled ? '启用' : '停用',
    summary: item.summary || item.content.slice(0, 90),
    title: item.title,
    useCount: item.useCount,
  })),
);

const sourceRows = computed<ListPageRow[]>(() =>
  sources.value.map((item) => ({
    id: item.id,
    lastIndexedTime: formatTime(item.lastIndexedTime),
    projectId: item.projectId ?? '—',
    sourceType: item.sourceType,
    status: item.status,
    summary: item.summary,
    title: item.title,
  })),
);

function makeModel(
  rows: ListPageRow[],
  label: string,
  path: string,
  actions: PageAction[],
  columns: PageColumn[],
): ListPageModel {
  return {
    actions,
    columns,
    demoPath: path,
    description:
      label === '记忆'
        ? '按租户范围管理 AI 记忆，并保留类型、上下文、来源与启用状态。'
        : '浏览 AI 知识来源，并从当前账号可访问的项目导入文档、会议和风险记录。',
    emptyHint: busy.value ? '正在读取数据…' : loadError.value || `暂无${label}。`,
    filters: [],
    key: `AI管理|记忆库|${label}`,
    label: `${label}库`,
    listScopes: [{ id: 'all', label: `全部${label}`, count: rows.length }],
    listTabs: [{ id: 'all', label: `全部${label}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-memory',
    pageSize: 20,
    panels: [],
    path,
    primaryAction: actions.find((action) => action.kind === 'create')?.label ?? '导入项目资料',
    replication: { status: 'implemented', referenceUrl: '/ai/memory' },
    routeName: 'ai-memory',
    rows,
    searchPlaceholder: `搜索${label}名称、摘要或内容`,
    stats: [
      { key: 'total', label: `全部${label}`, value: String(rows.length), tone: 'primary' },
      {
        key: 'enabled',
        label: '启用',
        value: String(rows.filter((row) => row.status === '启用').length),
        tone: 'success',
      },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
}

const memoriesModel = computed(() =>
  makeModel(
    memoryRows.value,
    '记忆',
    '/ai/memory',
    [
      { id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' },
      ...(canManage.value
        ? [
            { id: 'create', kind: 'create' as const, label: '新增记忆' },
            { id: 'edit', kind: 'custom' as const, label: '编辑记忆', requiresSelection: true },
            { id: 'toggle', kind: 'custom' as const, label: '切换启用状态', requiresSelection: true },
            { id: 'delete', kind: 'delete' as const, label: '删除记忆', requiresSelection: true },
          ]
        : []),
    ],
    [
      { key: 'title', title: '标题', width: 210 },
      { key: 'memoryType', title: '类型', width: 110 },
      { key: 'summary', title: '摘要', width: 340 },
      { key: 'importance', title: '重要度', width: 100 },
      { key: 'useCount', title: '引用次数', width: 100 },
      { key: 'ownerUserId', title: '所有人', width: 160 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
    ],
  ),
);

const sourcesModel = computed(() =>
  makeModel(
    sourceRows.value,
    '知识来源',
    '/ai/memory',
    [
      { id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' },
      ...(canManage.value ? [{ id: 'import', kind: 'custom' as const, label: '导入项目资料' }] : []),
    ],
    [
      { key: 'title', title: '来源标题', width: 250 },
      { key: 'sourceType', title: '来源类型', width: 170 },
      { key: 'projectId', title: '项目 ID', width: 240 },
      { key: 'summary', title: '摘要', width: 360 },
      { key: 'lastIndexedTime', title: '最后索引', width: 190 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
    ],
  ),
);

async function load(): Promise<void> {
  busy.value = true;
  loadError.value = '';
  try {
    const [memoryResult, sourceResult] = await Promise.all([
      api.getMemories({ includeDisabled: true, maxResultCount: 500 }),
      api.getKnowledgeSources({ maxResultCount: 500 }),
    ]);
    memories.value = memoryResult;
    sources.value = sourceResult;
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    busy.value = false;
  }
}

function openMemoryForm(item: AiApi.Memory | null): void {
  editing.value = item;
  draft.value = item
    ? {
        content: item.content,
        contextId: item.contextId || null,
        contextType: item.contextType || null,
        importance: item.importance,
        isEnabled: item.isEnabled,
        knowledgeSourceId: item.knowledgeSourceId,
        memoryType: item.memoryType,
        ownerUserId: item.ownerUserId,
        sourceReferencesJson: item.sourceReferencesJson,
        summary: item.summary,
        tagsJson: item.tagsJson,
        title: item.title,
      }
    : blankMemory();
  formError.value = '';
  memoryDialogOpen.value = true;
}

async function saveMemory(): Promise<void> {
  if (!canManage.value) return;
  if (!draft.value.title.trim() || !draft.value.content.trim()) {
    formError.value = '记忆标题和内容不能为空。';
    return;
  }
  if (!validArray(draft.value.tagsJson) || !validArray(draft.value.sourceReferencesJson)) {
    formError.value = '标签和来源引用必须是合法的 JSON 数组。';
    return;
  }
  saving.value = true;
  formError.value = '';
  try {
    const input = { ...draft.value };
    if (input.memoryType === 'User' && !editing.value) input.ownerUserId = null;
    if (editing.value) await api.updateMemory(editing.value.id, input);
    else await api.createMemory(input);
    memoryDialogOpen.value = false;
    await load();
    toast.success(editing.value ? 'AI 记忆已更新。' : 'AI 记忆已创建。');
  } catch (error) {
    formError.value = errorText(error);
  } finally {
    saving.value = false;
  }
}

function selectedMemoryFrom(rows: ListPageRow[]): AiApi.Memory | null {
  const id = rows[0]?.id;
  return id ? (memories.value.find((item) => item.id === id) ?? null) : null;
}

async function handleMemoryAction(action: PageAction, rows: ListPageRow[]): Promise<void> {
  if (action.id === 'reload') return void load();
  if (action.id === 'create' && canManage.value) return openMemoryForm(null);
  const item = selectedMemoryFrom(rows);
  if (!item || !canManage.value) return;
  if (action.id === 'edit') openMemoryForm(item);
  if (action.id === 'delete') {
    selectedMemory.value = item;
    deleteDialogOpen.value = true;
  }
  if (action.id === 'toggle') {
    actionBusy.value = true;
    try {
      await api.toggleMemory(item.id, { isEnabled: !item.isEnabled });
      await load();
      toast.success(item.isEnabled ? 'AI 记忆已停用。' : 'AI 记忆已启用。');
    } catch (error) {
      toast.error(errorText(error));
    } finally {
      actionBusy.value = false;
    }
  }
}

async function confirmDelete(): Promise<void> {
  if (!selectedMemory.value || !canManage.value) return;
  actionBusy.value = true;
  try {
    await api.deleteMemory(selectedMemory.value.id);
    deleteDialogOpen.value = false;
    selectedMemory.value = null;
    await load();
    toast.success('AI 记忆已删除。');
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
  }
}

async function openImport(): Promise<void> {
  if (!canManage.value) return;
  actionBusy.value = true;
  try {
    projects.value = await getProjectListApi({});
    importProjectId.value = projects.value[0]?.id ?? '';
    importKind.value = 'documents';
    importDialogOpen.value = true;
  } catch (error) {
    toast.error(`项目列表读取失败：${errorText(error)}`);
  } finally {
    actionBusy.value = false;
  }
}

function handleSourceAction(action: PageAction): void {
  if (action.id === 'reload') void load();
  if (action.id === 'import') void openImport();
}

function openSource(row: ListPageRow): void {
  const source = sources.value.find((item) => item.id === row.id);
  if (!source) return;
  selectedSource.value = source;
  sourceDialogOpen.value = true;
}

async function importProjectData(): Promise<void> {
  if (!importProjectId.value || !canManage.value) return;
  actionBusy.value = true;
  try {
    const result =
      importKind.value === 'documents'
        ? await api.importProjectDocuments(importProjectId.value)
        : importKind.value === 'meetings'
          ? await api.importProjectMeetings(importProjectId.value)
          : await api.importProjectRisks(importProjectId.value);
    importConfirmOpen.value = false;
    importDialogOpen.value = false;
    await load();
    toast.success(`导入完成：写入 ${result.importedCount} 条，跳过 ${result.skippedCount} 条。`);
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
  }
}

onMounted(() => void load());
</script>

<template>
  <UiTabs v-model="activeTab">
    <UiTabsList label="AI 记忆库">
      <UiTabsTrigger value="memories">记忆</UiTabsTrigger>
      <UiTabsTrigger value="sources">知识来源</UiTabsTrigger>
    </UiTabsList>
    <UiTabPanel value="memories">
      <ListPageSurface
        :model="memoriesModel"
        @action="handleMemoryAction"
      >
        <template #hero-icon
          ><DatabaseZap
            :size="22"
            aria-hidden="true"
        /></template>
        <template #notice>
          <p>User 类型的新记忆默认归当前账号；项目、场景记忆按当前租户和项目上下文管理。</p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
          <p
            v-if="!canManage"
            role="alert"
          >
            当前账号没有记忆库管理权限。
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
    <UiTabPanel value="sources">
      <ListPageSurface
        :model="sourcesModel"
        @action="handleSourceAction"
        @row="openSource"
      >
        <template #notice>
          <p>知识来源只能通过当前后端支持的项目文档、会议纪要和风险复盘导入；导入权限仍受项目数据范围校验。</p>
          <p
            v-if="loadError"
            role="alert"
          >
            {{ loadError }}
          </p>
        </template>
      </ListPageSurface>
    </UiTabPanel>
  </UiTabs>

  <UiDialog
    :open="memoryDialogOpen"
    :title="editing ? `编辑记忆：${editing.title}` : '新增 AI 记忆'"
    description="记忆内容可能被用于后续 AI 会话，请只保存必要且有权限使用的信息。"
    size="lg"
    @update:open="memoryDialogOpen = $event"
  >
    <form @submit.prevent="saveMemory">
      <UiFormField label="标题"
        ><UiInput
          v-model="draft.title"
          maxlength="256"
          required
      /></UiFormField>
      <UiFormField label="记忆类型"
        ><UiSelect
          v-model="draft.memoryType"
          label="记忆类型"
          :options="[
            { label: '项目', value: 'Project' },
            { label: '场景', value: 'Scenario' },
            { label: '个人', value: 'User' },
          ]"
      /></UiFormField>
      <UiFormField label="上下文类型"
        ><UiInput
          :model-value="draft.contextType ?? ''"
          maxlength="64"
          @update:model-value="draft.contextType = $event || null"
      /></UiFormField>
      <UiFormField label="上下文 ID"
        ><UiInput
          :model-value="draft.contextId ?? ''"
          maxlength="128"
          @update:model-value="draft.contextId = $event || null"
      /></UiFormField>
      <UiFormField label="知识来源 ID"
        ><UiInput
          :model-value="draft.knowledgeSourceId ?? ''"
          maxlength="36"
          @update:model-value="draft.knowledgeSourceId = $event || null"
      /></UiFormField>
      <UiFormField label="重要度"
        ><UiInput
          :model-value="String(draft.importance)"
          type="number"
          min="0"
          max="100"
          @update:model-value="draft.importance = Number($event)"
      /></UiFormField>
      <UiFormField
        label="摘要"
        wide
        ><UiTextarea
          v-model="draft.summary"
          rows="3"
          maxlength="2000"
      /></UiFormField>
      <UiFormField
        label="内容"
        wide
        ><UiTextarea
          v-model="draft.content"
          rows="7"
          maxlength="8000"
          required
      /></UiFormField>
      <UiFormField
        label="标签 JSON 数组"
        wide
        ><UiTextarea
          v-model="draft.tagsJson"
          rows="2"
          maxlength="16000"
      /></UiFormField>
      <UiFormField
        label="来源引用 JSON 数组"
        wide
        ><UiTextarea
          v-model="draft.sourceReferencesJson"
          rows="3"
          maxlength="16000"
      /></UiFormField>
      <UiFormField label="记忆状态"
        ><UiSwitch
          v-model="draft.isEnabled"
          label="启用记忆"
      /></UiFormField>
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
        @click="memoryDialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="saving"
        @click="saveMemory"
        >{{ saving ? '保存中…' : '保存记忆' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="sourceDialogOpen"
    :title="selectedSource?.title ?? '知识来源详情'"
    description="来源内容作为不可信文本展示，不会按 HTML 执行。"
    size="lg"
    @update:open="sourceDialogOpen = $event"
  >
    <UiFormField label="来源类型"
      ><UiInput
        :model-value="selectedSource?.sourceType ?? ''"
        readonly
    /></UiFormField>
    <UiFormField label="来源 ID"
      ><UiInput
        :model-value="selectedSource?.sourceId ?? ''"
        readonly
    /></UiFormField>
    <UiFormField label="摘要"
      ><UiTextarea
        :model-value="selectedSource?.summary ?? ''"
        rows="3"
        readonly
    /></UiFormField>
    <UiFormField
      label="完整内容"
      wide
      ><UiTextarea
        :model-value="selectedSource?.contentText ?? ''"
        rows="12"
        readonly
    /></UiFormField>
  </UiDialog>

  <UiDialog
    :open="importDialogOpen"
    title="导入项目资料"
    description="将从已授权项目读取内容并写入 AI 知识来源和记忆。请选择来源类型与项目。"
    @update:open="importDialogOpen = $event"
  >
    <UiFormField label="项目"
      ><UiSelect
        v-model="importProjectId"
        label="项目"
        placeholder="选择项目"
        :options="projectOptions"
        :disabled="actionBusy || projectOptions.length === 0"
    /></UiFormField>
    <UiFormField label="资料类型"
      ><UiSelect
        v-model="importKind"
        label="资料类型"
        :options="[
          { label: '项目文档', value: 'documents' },
          { label: '会议纪要', value: 'meetings' },
          { label: '风险复盘', value: 'risks' },
        ]"
    /></UiFormField>
    <p v-if="projects.length === 0">当前账号的数据范围内没有可导入项目。</p>
    <template #footer>
      <UiButton
        :disabled="actionBusy"
        @click="importDialogOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="actionBusy || !importProjectId"
        @click="importConfirmOpen = true"
        >下一步</UiButton
      >
    </template>
  </UiDialog>

  <UiAlertDialog
    :open="importConfirmOpen"
    title="确认导入项目资料"
    description="这会读取所选项目的真实数据并更新 AI 记忆。导入结果会返回新增、更新和跳过的数量。"
    confirm-label="确认导入"
    tone="warning"
    @update:open="importConfirmOpen = $event"
    @confirm="importProjectData"
  />
  <UiAlertDialog
    :open="deleteDialogOpen"
    title="确认删除 AI 记忆"
    :description="selectedMemory ? `永久删除“${selectedMemory.title}”？关联知识来源不会因此删除。` : ''"
    confirm-label="删除记忆"
    @update:open="deleteDialogOpen = $event"
    @confirm="confirmDelete"
    @cancel="selectedMemory = null"
  />
</template>
