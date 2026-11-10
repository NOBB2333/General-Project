<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { LibraryBig } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import { toast, UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiSwitch, UiTextarea } from '@/components/ui';

const api = createAiApi(apiClient);
const templates = ref<AiApi.PromptTemplate[]>([]);
const loading = ref(false);
const saving = ref(false);
const previewing = ref(false);
const deleting = ref(false);
const loadError = ref('');
const formError = ref('');
const actionError = ref('');
const previewError = ref('');
const dialogOpen = ref(false);
const previewDialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const editing = ref<AiApi.PromptTemplate | null>(null);
const previewTarget = ref<AiApi.PromptTemplate | null>(null);
const deleteTarget = ref<AiApi.PromptTemplate | null>(null);
const previewResult = ref<AiApi.PromptTemplateDryRunResult | null>(null);
const previewInputJson = ref('{}');
const canManage = computed(() => canAccess('Ai.Prompt.Manage'));
const draft = ref<AiApi.PromptTemplateSaveInput>(blankDraft());

function blankDraft(): AiApi.PromptTemplateSaveInput {
  return {
    assistantCode: '',
    category: '项目管理',
    description: '',
    inputSchemaJson: '{}',
    isEnabled: true,
    mode: 'General',
    name: '',
    outputSchemaJson: '{}',
    promptContent: '',
    scenario: 'General',
    sort: 0,
    templateCode: '',
    toolCodes: [],
    version: 1,
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
  templates.value.map((item) => ({
    assistant: item.assistantName || item.assistantCode || '通用',
    category: item.category,
    id: item.id,
    mode: item.mode,
    name: item.name,
    scenario: item.scenario,
    status: item.isEnabled ? '启用' : '停用',
    templateCode: item.templateCode,
    type: item.isBuiltin ? '内置' : '自定义',
    version: item.version,
  })),
);

const pageModel = computed<ListPageModel>(() => {
  const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }];
  if (canManage.value) {
    actions.push(
      { id: 'create', kind: 'create', label: '新增场景' },
      { id: 'edit', kind: 'custom', label: '编辑场景', requiresSelection: true },
      { id: 'preview', kind: 'custom', label: '预览模板', requiresSelection: true },
      { id: 'delete', kind: 'delete', label: '删除场景', requiresSelection: true },
    );
  }
  return {
    actions,
    columns: [
      { key: 'name', title: '场景名称', width: 220, sticky: true },
      { key: 'templateCode', title: '场景编码', width: 220 },
      { key: 'category', title: '分类', width: 140 },
      { key: 'scenario', title: '场景', width: 140 },
      { key: 'assistant', title: '关联助手', width: 180 },
      { key: 'mode', title: '模式', width: 110 },
      { key: 'version', title: '版本', width: 90 },
      { key: 'status', title: '状态', width: 100, presentation: 'status' },
      { key: 'type', title: '来源', width: 100 },
    ],
    demoPath: '/ai/prompts',
    description: '维护可由AI会话直接使用的场景模板，并在保存后预览服务端渲染结果。',
    emptyHint: loading.value ? '正在读取场景…' : loadError.value ? '场景读取失败，请刷新重试。' : '暂无场景模板。',
    filters: [],
    key: 'AI管理|场景模板',
    label: '场景模板',
    listScopes: [{ id: 'all', label: '全部场景', count: templates.value.length }],
    listTabs: [{ id: 'all', label: '全部场景' }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-prompts',
    pageSize: 20,
    panels: [],
    path: '/ai/prompts',
    primaryAction: '新增场景',
    replication: { status: 'implemented', referenceUrl: '/ai/conversations' },
    routeName: 'ai-prompts',
    rows: rows.value,
    searchPlaceholder: '搜索场景名称、编码、分类或说明',
    stats: [
      { key: 'total', label: '全部场景', value: String(templates.value.length), tone: 'primary' },
      {
        key: 'enabled',
        label: '启用',
        value: String(templates.value.filter((item) => item.isEnabled).length),
        tone: 'success',
      },
      {
        key: 'builtin',
        label: '内置',
        value: String(templates.value.filter((item) => item.isBuiltin).length),
        tone: 'warning',
      },
      {
        key: 'custom',
        label: '自定义',
        value: String(templates.value.filter((item) => !item.isBuiltin).length),
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
    templates.value = await api.getPromptTemplates({ includeDisabled: true });
  } catch (error) {
    loadError.value = errorText(error);
  } finally {
    loading.value = false;
  }
}

function openForm(item: AiApi.PromptTemplate | null): void {
  editing.value = item;
  formError.value = '';
  draft.value = item
    ? {
        assistantCode: item.assistantCode,
        category: item.category,
        description: item.description,
        inputSchemaJson: item.inputSchemaJson,
        isEnabled: item.isEnabled,
        mode: item.mode,
        name: item.name,
        outputSchemaJson: item.outputSchemaJson,
        promptContent: item.promptContent,
        scenario: item.scenario,
        sort: item.sort,
        templateCode: item.templateCode,
        toolCodes: [...item.toolCodes],
        version: item.version,
      }
    : blankDraft();
  dialogOpen.value = true;
}

function selectedTemplate(selection: ListPageRow[]): AiApi.PromptTemplate | null {
  const selected = selection.length === 1 ? templates.value.find((item) => item.id === selection[0]?.id) : null;
  if (!selected) actionError.value = '请选择一条场景模板。';
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
  const selected = selectedTemplate(selection);
  if (!selected) return;
  if (action.id === 'edit') openForm(selected);
  if (action.id === 'preview') openPreview(selected);
  if (action.id === 'delete') {
    if (selected.isBuiltin) {
      toast.warning('内置场景不能删除，可停用或调整提示词。');
      return;
    }
    deleteTarget.value = selected;
    deleteDialogOpen.value = true;
  }
}

function openRow(row: ListPageRow): void {
  const item = templates.value.find((template) => template.id === row.id);
  if (item && canManage.value) openForm(item);
}

function openPreview(template: AiApi.PromptTemplate): void {
  previewTarget.value = template;
  previewInputJson.value = '{}';
  previewResult.value = null;
  previewError.value = '';
  previewDialogOpen.value = true;
}

async function runPreview(): Promise<void> {
  if (!previewTarget.value || previewing.value) return;
  try {
    JSON.parse(previewInputJson.value);
  } catch {
    previewError.value = '输入 JSON 格式无效。';
    return;
  }
  previewing.value = true;
  previewError.value = '';
  try {
    previewResult.value = await api.dryRunPromptTemplate(previewTarget.value.id, {
      contextId: '',
      contextSnapshotJson: '{}',
      contextType: 'None',
      inputJson: previewInputJson.value,
    });
  } catch (error) {
    previewError.value = errorText(error);
  } finally {
    previewing.value = false;
  }
}

function validJson(value: string): boolean {
  try {
    JSON.parse(value);
    return true;
  } catch {
    return false;
  }
}

async function save(): Promise<void> {
  if (saving.value) return;
  if (!draft.value.name.trim() || !draft.value.templateCode.trim() || !draft.value.promptContent.trim()) {
    formError.value = '场景名称、编码和提示词正文为必填项。';
    return;
  }
  if (!validJson(draft.value.inputSchemaJson) || !validJson(draft.value.outputSchemaJson)) {
    formError.value = '输入和输出 Schema 必须是合法 JSON。';
    return;
  }
  saving.value = true;
  formError.value = '';
  const input = { ...draft.value, name: draft.value.name.trim(), templateCode: draft.value.templateCode.trim() };
  try {
    if (editing.value) await api.updatePromptTemplate(editing.value.id, input);
    else await api.createPromptTemplate(input);
    dialogOpen.value = false;
    await load();
    toast.success('场景模板已保存。');
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
    await api.deletePromptTemplate(deleteTarget.value.id);
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    await load();
    toast.success('场景模板已删除。');
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
    @row="openRow"
  >
    <template #hero-icon>
      <LibraryBig
        :size="22"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <p>预览读取当前已保存版本，不创建会话，也不会调用模型；使用场景会在AI对话中创建真实会话并发送渲染后的提示词。</p>
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
        当前账号没有场景模板管理权限。
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    :open="dialogOpen"
    :title="editing ? `编辑场景：${editing.name}` : '新增场景模板'"
    description="提示词正文只对场景模板管理权限开放；修改只有保存后才会生效。"
    size="lg"
    @update:open="dialogOpen = $event"
  >
    <form @submit.prevent="save">
      <UiFormField label="场景名称">
        <UiInput
          v-model="draft.name"
          maxlength="128"
          required
        />
      </UiFormField>
      <UiFormField label="场景编码">
        <UiInput
          v-model="draft.templateCode"
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
      <UiFormField label="场景">
        <UiInput
          v-model="draft.scenario"
          maxlength="64"
        />
      </UiFormField>
      <UiFormField
        label="关联助手编码"
        hint="留空使用通用模板角色。"
      >
        <UiInput
          v-model="draft.assistantCode"
          maxlength="64"
        />
      </UiFormField>
      <UiFormField label="模式">
        <UiInput
          v-model="draft.mode"
          maxlength="32"
        />
      </UiFormField>
      <UiFormField label="版本">
        <UiInput
          :model-value="String(draft.version)"
          type="number"
          @update:model-value="draft.version = Number($event)"
        />
      </UiFormField>
      <UiFormField label="排序">
        <UiInput
          :model-value="String(draft.sort)"
          type="number"
          @update:model-value="draft.sort = Number($event)"
        />
      </UiFormField>
      <UiFormField label="场景状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用场景"
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
        label="提示词正文"
        hint="使用场景时，输入 JSON 会作为模板参数附加到提示词中；当前不会替换提示词里的占位符。"
        wide
      >
        <UiTextarea
          v-model="draft.promptContent"
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
        label="输入 Schema JSON"
        wide
      >
        <UiTextarea
          v-model="draft.inputSchemaJson"
          rows="4"
          maxlength="8000"
        />
      </UiFormField>
      <UiFormField
        label="输出 Schema JSON"
        wide
      >
        <UiTextarea
          v-model="draft.outputSchemaJson"
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
        >{{ saving ? '保存中…' : '保存场景' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="previewDialogOpen"
    :title="previewTarget ? `${previewTarget.name} · 预览` : '场景预览'"
    description="预览使用已保存的服务端模板，只渲染结果，不创建会话或调用模型。"
    size="lg"
    @update:open="previewDialogOpen = $event"
  >
    <UiFormField label="输入 JSON">
      <UiTextarea
        v-model="previewInputJson"
        rows="5"
        maxlength="8000"
      />
    </UiFormField>
    <p
      v-if="previewError"
      role="alert"
    >
      {{ previewError }}
    </p>
    <template v-if="previewResult">
      <p role="status">
        {{
          previewResult.isModelCallSkipped ? '未调用模型；未创建会话。' : '服务端返回了模型调用状态，请检查接口配置。'
        }}
      </p>
      <p
        v-for="error in previewResult.errors"
        :key="error"
        role="alert"
      >
        {{ error }}
      </p>
      <p
        v-for="warning in previewResult.warnings"
        :key="warning"
      >
        {{ warning }}
      </p>
      <UiFormField label="渲染后的提示词">
        <UiTextarea
          :model-value="previewResult.renderedPrompt"
          rows="8"
          readonly
        />
      </UiFormField>
      <UiFormField label="结构化预览结果">
        <UiTextarea
          :model-value="previewResult.displayContent"
          rows="5"
          readonly
        />
      </UiFormField>
    </template>
    <template #footer>
      <UiButton
        :disabled="previewing"
        @click="previewDialogOpen = false"
        >关闭</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="previewing"
        @click="runPreview"
        >{{ previewing ? '预览中…' : '运行预览' }}</UiButton
      >
    </template>
  </UiDialog>

  <UiAlertDialog
    :open="deleteDialogOpen"
    title="确认删除场景模板"
    :description="deleteTarget ? `删除“${deleteTarget.name}”？此操作无法撤销。` : ''"
    confirm-label="删除场景"
    @update:open="deleteDialogOpen = $event"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />
</template>
