<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { PackageCheck } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import { toast, UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiSwitch, UiTextarea } from '@/components/ui';

const api = createAiApi(apiClient);
const items = ref<AiApi.Artifact[]>([]);
const busy = ref(false);
const saving = ref(false);
const deleting = ref(false);
const loadError = ref('');
const actionError = ref('');
const formError = ref('');
const dialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const editing = ref<AiApi.Artifact | null>(null);
const deleteTarget = ref<AiApi.Artifact | null>(null);
const canManage = computed(() => canAccess('Ai.Artifact.Manage'));

function blankArtifact(): AiApi.ArtifactSaveInput {
  return {
    artifactCode: '',
    category: 'Chart',
    description: '',
    examplePayloadJson: '{}',
    isEnabled: true,
    jsonSchema: '{}',
    name: '',
    rendererType: 'metric-card',
    sort: 0,
    systemHint: '',
  };
}

const draft = ref<AiApi.ArtifactSaveInput>(blankArtifact());

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
    artifactCode: item.artifactCode,
    category: item.category,
    id: item.id,
    name: item.name,
    rendererType: item.rendererType,
    status: item.isEnabled ? '启用' : '停用',
    type: item.isBuiltin ? '内置' : '自定义',
  })),
);

const pageModel = computed<ListPageModel>(() => {
  const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: busy.value ? '读取中' : '刷新' }];
  if (canManage.value) {
    actions.push(
      { id: 'create', kind: 'create', label: '新增制品模板' },
      { id: 'edit', kind: 'custom', label: '编辑制品模板', requiresSelection: true },
      { id: 'delete', kind: 'delete', label: '删除制品模板', requiresSelection: true },
    );
  }
  return {
    actions,
    columns: [
      { key: 'name', title: '制品名称', width: 220, sticky: true },
      { key: 'artifactCode', title: '制品编码', width: 220 },
      { key: 'category', title: '分类', width: 140 },
      { key: 'rendererType', title: '渲染器', width: 180 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
      { key: 'type', title: '来源', width: 100 },
    ],
    demoPath: '/ai/artifacts',
    description: '维护Agent可生成的结构化制品定义、Schema和示例负载。',
    emptyHint: busy.value ? '正在读取制品模板…' : loadError.value ? '制品模板读取失败，请刷新重试。' : '暂无制品模板。',
    filters: [],
    key: 'AI管理|制品模板',
    label: '制品模板',
    listScopes: [{ id: 'all', label: '全部制品模板', count: items.value.length }],
    listTabs: [{ id: 'all', label: '全部制品模板' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-artifacts',
    pageSize: 20,
    panels: [],
    path: '/ai/artifacts',
    primaryAction: '新增制品模板',
    replication: { status: 'implemented', referenceUrl: '/ai/conversations' },
    routeName: 'ai-artifacts',
    rows: rows.value,
    searchPlaceholder: '搜索制品名称、编码或分类',
    stats: [
      { key: 'total', label: '全部制品', value: String(items.value.length), tone: 'primary' },
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
    items.value = await api.getArtifacts({ includeDisabled: true });
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    busy.value = false;
  }
}

function openForm(item: AiApi.Artifact | null): void {
  editing.value = item;
  formError.value = '';
  draft.value = item
    ? {
        artifactCode: item.artifactCode,
        category: item.category,
        description: item.description,
        examplePayloadJson: item.examplePayloadJson,
        isEnabled: item.isEnabled,
        jsonSchema: item.jsonSchema,
        name: item.name,
        rendererType: item.rendererType,
        sort: item.sort,
        systemHint: item.systemHint,
      }
    : blankArtifact();
  dialogOpen.value = true;
}

function selected(selection: ListPageRow[]): AiApi.Artifact | null {
  const item = selection.length === 1 ? items.value.find((artifact) => artifact.id === selection[0]?.id) : null;
  actionError.value = item ? '' : '请选择一条制品模板记录。';
  return item ?? null;
}

function handleAction(action: PageAction, selection: ListPageRow[]): void {
  if (action.id === 'reload') return void load();
  if (action.id === 'create') return openForm(null);
  const item = selected(selection);
  if (!item) return;
  if (action.id === 'edit') openForm(item);
  if (action.id === 'delete') {
    if (item.isBuiltin) return void toast.warning('内置制品模板不能删除，可停用或调整配置。');
    deleteTarget.value = item;
    deleteDialogOpen.value = true;
  }
}

async function save(): Promise<void> {
  if (saving.value) return;
  formError.value = '';
  const input = draft.value;
  if (!input.artifactCode.trim() || !input.name.trim() || !input.rendererType.trim()) {
    formError.value = '制品编码、名称和渲染器类型为必填项。';
    return;
  }
  if (!validJson(input.jsonSchema) || !validJson(input.examplePayloadJson)) {
    formError.value = 'Schema 和示例负载必须是合法 JSON。';
    return;
  }
  saving.value = true;
  try {
    const payload: AiApi.ArtifactSaveInput = {
      ...input,
      artifactCode: input.artifactCode.trim(),
      category: input.category.trim(),
      name: input.name.trim(),
      rendererType: input.rendererType.trim(),
    };
    if (editing.value) await api.updateArtifact(editing.value.id, payload);
    else await api.createArtifact(payload);
    dialogOpen.value = false;
    await load();
    toast.success('制品模板已保存。');
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
    await api.deleteArtifact(deleteTarget.value.id);
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    await load();
    toast.success('制品模板已删除。');
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
      ><PackageCheck
        :size="22"
        aria-hidden="true"
    /></template>
    <template #notice>
      <p>制品模板由Agent或会话生成；当前页面维护真实Schema与渲染器配置。</p>
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
        当前账号没有AI制品管理权限。
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    :open="dialogOpen"
    :title="editing ? `编辑制品模板：${editing.name}` : '新增制品模板'"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form @submit.prevent="save">
      <UiFormField label="制品名称">
        <UiInput
          v-model="draft.name"
          maxlength="128"
          required
        />
      </UiFormField>
      <UiFormField label="制品编码">
        <UiInput
          v-model="draft.artifactCode"
          maxlength="128"
          required
          :readonly="Boolean(editing)"
        />
      </UiFormField>
      <UiFormField label="分类">
        <UiInput
          v-model="draft.category"
          maxlength="64"
        />
      </UiFormField>
      <UiFormField label="渲染器类型">
        <UiInput
          v-model="draft.rendererType"
          maxlength="64"
          required
        />
      </UiFormField>
      <UiFormField label="排序">
        <UiInput
          :model-value="String(draft.sort)"
          type="number"
          @update:model-value="draft.sort = Number($event)"
        />
      </UiFormField>
      <UiFormField label="模板状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用制品模板"
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
        label="JSON Schema"
        wide
      >
        <UiTextarea
          v-model="draft.jsonSchema"
          rows="7"
          maxlength="16000"
          required
        />
      </UiFormField>
      <UiFormField
        label="示例负载 JSON"
        wide
      >
        <UiTextarea
          v-model="draft.examplePayloadJson"
          rows="5"
          maxlength="16000"
        />
      </UiFormField>
      <UiFormField
        label="系统提示"
        wide
      >
        <UiTextarea
          v-model="draft.systemHint"
          rows="4"
          maxlength="4000"
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
    title="确认删除制品模板"
    :description="deleteTarget ? `删除“${deleteTarget.name}”？此操作无法撤销。` : ''"
    confirm-label="删除"
    @update:open="deleteDialogOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />
</template>
