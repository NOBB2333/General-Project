<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { ApiRequestError } from '@/api/client';
import { createConfigApi, type ConfigApi } from '@/api/core/config';
import { createProjectPlanTemplateApi, type ProjectPlanTemplateApi } from '@/api/core/project-plan-templates';
import { apiClient } from '@/api/request';
import { ReferenceBoardPageSurface, type PageModel, type PageRow } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import { buildConfigSaveInput, canEditConfig, expectedConfigValue } from '@/features/platform/configs/config.model';
import ConfigEditorDialog from '@/features/platform/configs/ConfigEditorDialog.vue';
import { hasCurrentTenantScope } from '@/session/token-storage';
import { sessionState } from '@/session/session-store';
import { projectConfigModel } from '@/features/projects/model/project-config.model';

interface NodeDraft {
  clientKey: string;
  deliveryDescription: string;
  estimateSummary: string;
  id?: string;
  name: string;
  parentClientKey: string;
  scopeDescription: string;
  sortOrder: string;
  type: string;
  wbsCode: string;
}

const projectPlanApi = createProjectPlanTemplateApi(apiClient);
const configApi = createConfigApi(apiClient);
const pageSize = 10;
const projectPlanTemplates = ref<ProjectPlanTemplateApi.Item[]>([]);
const page = ref(1);
const totalCount = ref(0);
const searchQuery = ref('');
const loading = ref(false);
const loadError = ref('');
const selectedTemplate = ref<ProjectPlanTemplateApi.Detail | null>(null);
const detailOpen = ref(false);
const detailLoading = ref(false);
const detailError = ref('');
const editorOpen = ref(false);
const editorMode = ref<'create' | 'edit'>('create');
const editingId = ref('');
const saving = ref(false);
const editorError = ref('');
const deleteOpen = ref(false);
const deleteTarget = ref<ProjectPlanTemplateApi.Detail | null>(null);
const deleting = ref(false);
const deleteError = ref('');
const rateConfig = ref<ConfigApi.Item | null>(null);
const rateError = ref('');
const rateEditorOpen = ref(false);
const rateSaving = ref(false);
const rateSaveError = ref('');
const tenantScoped = computed(() => Boolean(sessionState.user && hasCurrentTenantScope()));
const canViewRateConfig = computed(() => canAccess('Platform.Config.Manage'));
const canManageRateConfig = computed(() =>
  Boolean(rateConfig.value && canAccess('Platform.Config.Manage') && canEditConfig(rateConfig.value)),
);
const canManageTemplates = computed(() => canAccess('Project.PlanTemplate.Manage'));
const rateEditorItem = computed(() => rateConfig.value);
const rateDisplay = computed(() => {
  const value = rateConfig.value?.value || rateConfig.value?.defaultValue;
  return value ? `${value} / 小时` : '未配置';
});

const templateForm = reactive({
  description: '',
  isEnabled: true,
  name: '',
  nodes: [] as NodeDraft[],
});

const templateStatusOptions: UiSelectOption[] = [
  { label: '启用', value: 'enabled' },
  { label: '停用', value: 'disabled' },
];
const nodeTypeOptions: UiSelectOption[] = [
  { label: '阶段', value: '阶段' },
  { label: '规划包', value: '规划包' },
  { label: '工作包', value: '工作包' },
  { label: '交付物', value: '交付物' },
  { label: '里程碑', value: '里程碑' },
];

const remoteRows = computed<PageRow[]>(() =>
  projectPlanTemplates.value.map((item) => ({
    code: item.code,
    id: item.id,
    name: item.name,
    nodeCount: item.nodeCount,
    status: item.isEnabled ? '启用' : '停用',
    type: '项目计划模板',
    updatedAt: formatDate(item.lastModificationTime ?? item.creationTime),
    value: `${item.nodeCount} 个WBS条目`,
  })),
);

const pageModel = computed<PageModel>(() => ({
  ...projectConfigModel,
  description: '维护项目计划模板；项目工时费率复用平台配置参数。',
  emptyHint: loading.value
    ? '正在读取项目计划模板…'
    : loadError.value
      ? '模板加载失败，请刷新重试。'
      : '当前条件下没有项目计划模板。',
  moreTools: [],
  rows: remoteRows.value,
  stats: [
    { key: 'total', label: '项目计划模板', value: String(totalCount.value), tone: 'primary' },
    {
      key: 'hourly-rate',
      label: '项目工时费率',
      value: canViewRateConfig.value && rateConfig.value ? rateDisplay.value : '—',
      tone: 'neutral',
    },
  ],
  tools: canManageTemplates.value ? ['新建模板', '刷新'] : ['刷新'],
}));

const detailNodes = computed(() =>
  [...(selectedTemplate.value?.nodes ?? [])].sort(
    (left, right) => left.sortOrder - right.sortOrder || left.wbsCode.localeCompare(right.wbsCode),
  ),
);

let pageController: AbortController | undefined;
let detailController: AbortController | undefined;
let rateController: AbortController | undefined;
let saveController: AbortController | undefined;
let requestVersion = 0;
let detailRequestVersion = 0;
let rateRequestVersion = 0;
let searchTimer: ReturnType<typeof setTimeout> | undefined;

function formatDate(value: null | string) {
  if (!value) return '—';
  return value.slice(0, 10) || '—';
}

function makeClientKey() {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') return crypto.randomUUID();
  return `node-${Date.now()}-${Math.random().toString(36).slice(2)}`;
}

function createNodeDraft(index: number): NodeDraft {
  return {
    clientKey: makeClientKey(),
    deliveryDescription: '',
    estimateSummary: '',
    name: '',
    parentClientKey: '',
    scopeDescription: '',
    sortOrder: String(index * 10),
    type: '工作包',
    wbsCode: String(index).padStart(2, '0'),
  };
}

function parentOptions(node: NodeDraft): UiSelectOption[] {
  return [
    { label: '顶层条目', value: '' },
    ...templateForm.nodes
      .filter((item) => item.clientKey !== node.clientKey)
      .map((item) => ({ label: `${item.wbsCode} ${item.name || '未命名条目'}`, value: item.clientKey })),
  ];
}

function nodeName(clientKey: string) {
  return selectedTemplate.value?.nodes.find((node) => node.clientKey === clientKey)?.name ?? '未找到父级';
}

function resetTemplateForm() {
  templateForm.description = '';
  templateForm.isEnabled = true;
  templateForm.name = '';
  templateForm.nodes = [];
  editingId.value = '';
  editorError.value = '';
}

function beginCreate() {
  if (!canManageTemplates.value) return;
  detailOpen.value = false;
  resetTemplateForm();
  editorMode.value = 'create';
  editorOpen.value = true;
}

function beginEdit() {
  const detail = selectedTemplate.value;
  if (!detail || !canManageTemplates.value) return;
  templateForm.description = detail.description;
  templateForm.isEnabled = detail.isEnabled;
  templateForm.name = detail.name;
  templateForm.nodes = detail.nodes.map((node) => ({
    clientKey: node.clientKey,
    deliveryDescription: node.deliveryDescription,
    estimateSummary: node.estimateSummary,
    id: node.id,
    name: node.name,
    parentClientKey: node.parentClientKey ?? '',
    scopeDescription: node.scopeDescription,
    sortOrder: String(node.sortOrder),
    type: node.type,
    wbsCode: node.wbsCode,
  }));
  editingId.value = detail.id;
  editorError.value = '';
  editorMode.value = 'edit';
  detailOpen.value = false;
  editorOpen.value = true;
}

function addNode() {
  templateForm.nodes.push(createNodeDraft(templateForm.nodes.length + 1));
}

function removeNode(clientKey: string) {
  templateForm.nodes = templateForm.nodes.filter((node) => node.clientKey !== clientKey);
  for (const node of templateForm.nodes) {
    if (node.parentClientKey === clientKey) node.parentClientKey = '';
  }
}

function validateTemplateForm(): string | undefined {
  if (!templateForm.name.trim()) return '请填写模板名称。';
  if (templateForm.name.trim().length > 128) return '模板名称不能超过128个字符。';
  if (templateForm.description.trim().length > 1024) return '模板说明不能超过1024个字符。';
  if (templateForm.nodes.length > 500) return '每个项目计划模板最多包含500个WBS条目。';

  const codes = new Set<string>();
  for (const [index, node] of templateForm.nodes.entries()) {
    const code = node.wbsCode.trim().toUpperCase();
    if (!code || code.length > 64) return `第${index + 1}个WBS条目需要填写不超过64个字符的编码。`;
    if (codes.has(code)) return `WBS编码“${code}”重复。`;
    codes.add(code);
    if (!node.name.trim() || node.name.trim().length > 128)
      return `第${index + 1}个WBS条目需要填写不超过128个字符的名称。`;
    if (!node.type.trim() || node.type.trim().length > 32) return `第${index + 1}个WBS条目类型无效。`;
    if (node.scopeDescription.trim().length > 1024) return `第${index + 1}个WBS条目范围说明不能超过1024个字符。`;
    if (node.deliveryDescription.trim().length > 1024) return `第${index + 1}个WBS条目交付说明不能超过1024个字符。`;
    if (node.estimateSummary.trim().length > 512) return `第${index + 1}个WBS条目估算说明不能超过512个字符。`;
    const sortOrder = Number(node.sortOrder);
    if (!Number.isInteger(sortOrder) || sortOrder < 0) return `第${index + 1}个WBS条目顺序必须是大于或等于0的整数。`;
  }

  return undefined;
}

function toSaveInput(): ProjectPlanTemplateApi.SaveInput {
  return {
    description: templateForm.description.trim(),
    isEnabled: templateForm.isEnabled,
    name: templateForm.name.trim(),
    nodes: templateForm.nodes.map((node) => ({
      clientKey: node.clientKey,
      deliveryDescription: node.deliveryDescription.trim(),
      estimateSummary: node.estimateSummary.trim(),
      ...(node.id ? { id: node.id } : {}),
      name: node.name.trim(),
      parentClientKey: node.parentClientKey || null,
      scopeDescription: node.scopeDescription.trim(),
      sortOrder: Number(node.sortOrder),
      type: node.type.trim(),
      wbsCode: node.wbsCode.trim().toUpperCase(),
    })),
  };
}

async function loadPage(): Promise<boolean> {
  pageController?.abort();
  const controller = new AbortController();
  pageController = controller;
  const currentVersion = ++requestVersion;
  loading.value = true;
  loadError.value = '';

  try {
    const result = await projectPlanApi.getList(
      {
        filter: searchQuery.value.trim() || undefined,
        maxResultCount: pageSize,
        skipCount: (page.value - 1) * pageSize,
      },
      controller.signal,
    );
    if (currentVersion !== requestVersion) return false;
    projectPlanTemplates.value = result.items;
    totalCount.value = result.totalCount;
    if (result.items.length === 0 && page.value > 1 && (page.value - 1) * pageSize >= result.totalCount) {
      page.value = Math.max(1, Math.ceil(result.totalCount / pageSize));
      return await loadPage();
    }
    return true;
  } catch (error) {
    if (currentVersion === requestVersion && !(error instanceof Error && error.name === 'AbortError')) {
      loadError.value = error instanceof Error ? error.message : '模板加载失败，请刷新重试。';
      projectPlanTemplates.value = [];
      totalCount.value = 0;
    }
    return false;
  } finally {
    if (currentVersion === requestVersion) loading.value = false;
  }
}

async function loadRateConfig() {
  if (!canViewRateConfig.value) {
    rateConfig.value = null;
    rateError.value = '';
    return;
  }
  rateController?.abort();
  const controller = new AbortController();
  rateController = controller;
  const currentVersion = ++rateRequestVersion;
  rateError.value = '';
  try {
    const items = await configApi.getList(controller.signal);
    if (currentVersion !== rateRequestVersion) return;
    rateConfig.value = items.find((item) => item.code === 'Admin.Platform.ProjectEvmWorklogHourlyRate') ?? null;
    if (!rateConfig.value) rateError.value = '平台配置中未找到项目工时费率参数。';
  } catch (error) {
    if (currentVersion === rateRequestVersion && !(error instanceof Error && error.name === 'AbortError')) {
      rateError.value = error instanceof Error ? error.message : '工时费率读取失败，请重新加载。';
    }
  }
}

async function loadDetail(id: string) {
  detailController?.abort();
  const controller = new AbortController();
  detailController = controller;
  const currentVersion = ++detailRequestVersion;
  detailLoading.value = true;
  detailError.value = '';
  selectedTemplate.value = null;
  detailOpen.value = true;
  try {
    const result = await projectPlanApi.get(id, controller.signal);
    if (currentVersion === detailRequestVersion) selectedTemplate.value = result;
  } catch (error) {
    if (currentVersion === detailRequestVersion && !(error instanceof Error && error.name === 'AbortError')) {
      detailError.value = error instanceof Error ? error.message : '模板详情加载失败，请重试。';
    }
  } finally {
    if (currentVersion === detailRequestVersion) detailLoading.value = false;
  }
}

function handleSearchChange(value: string) {
  searchQuery.value = value;
  page.value = 1;
  if (searchTimer) clearTimeout(searchTimer);
  searchTimer = setTimeout(() => void loadPage(), 250);
}

function handlePageChange(nextPage: number) {
  page.value = nextPage;
  void loadPage();
}

function handleTool(tool: string) {
  if (tool === '新建模板') beginCreate();
  else if (tool === '刷新') void refreshPage();
}

async function refreshPage() {
  await Promise.all([loadPage(), loadRateConfig()]);
}

async function openTemplate(row: PageRow) {
  await loadDetail(row.id);
}

async function saveTemplate() {
  if (!canManageTemplates.value || saving.value) return;
  const validationMessage = validateTemplateForm();
  if (validationMessage) {
    editorError.value = validationMessage;
    return;
  }

  saveController?.abort();
  const controller = new AbortController();
  saveController = controller;
  saving.value = true;
  editorError.value = '';
  let writeConfirmed = false;
  try {
    const input = toSaveInput();
    let savedId = editingId.value;
    if (editorMode.value === 'create') {
      savedId = await projectPlanApi.create(input, controller.signal);
    } else if (!(await projectPlanApi.update(savedId, input, controller.signal))) {
      throw new Error('服务未确认模板更新。');
    }
    writeConfirmed = true;
    const readback = await projectPlanApi.get(savedId, controller.signal);
    const expectedCodes = input.nodes.map((node) => node.wbsCode).sort();
    const actualCodes = readback.nodes.map((node) => node.wbsCode.toUpperCase()).sort();
    if (
      readback.name !== input.name ||
      readback.isEnabled !== input.isEnabled ||
      readback.nodes.length !== input.nodes.length ||
      expectedCodes.join('|') !== actualCodes.join('|')
    ) {
      throw new Error('模板读回内容与提交内容不一致，请刷新核对。');
    }

    editorOpen.value = false;
    const refreshed = await loadPage();
    if (refreshed) toast.success('项目计划模板已保存并重新读取确认。');
    else toast.info('模板已保存并读回确认；列表刷新失败，请点击刷新。');
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') return;
    const message = error instanceof Error ? error.message : '模板保存失败，请重试。';
    editorError.value = writeConfirmed ? `保存已完成，但读回确认失败。请刷新核对。${message}` : message;
  } finally {
    saving.value = false;
  }
}

function askDeleteTemplate() {
  if (!selectedTemplate.value || !canManageTemplates.value) return;
  deleteTarget.value = selectedTemplate.value;
  deleteError.value = '';
  detailOpen.value = false;
  deleteOpen.value = true;
}

async function deleteTemplate() {
  const target = deleteTarget.value;
  if (!target || !canManageTemplates.value || deleting.value) return;
  deleting.value = true;
  deleteError.value = '';
  const controller = new AbortController();
  try {
    if (!(await projectPlanApi.delete(target.id, controller.signal))) throw new Error('服务未确认模板删除。');
    try {
      await projectPlanApi.get(target.id, controller.signal);
      throw new Error('删除后仍能读取到该模板，请刷新核对。');
    } catch (error) {
      if (!(error instanceof ApiRequestError && error.status === 404)) throw error;
    }
    deleteOpen.value = false;
    deleteTarget.value = null;
    if (await loadPage()) toast.success('项目计划模板已删除并确认不可读取。');
    else toast.info('模板已删除；列表刷新失败，请点击刷新。');
  } catch (error) {
    deleteError.value = error instanceof Error ? error.message : '模板删除失败，请重试。';
  } finally {
    deleting.value = false;
  }
}

function openRateEditor() {
  if (!canManageRateConfig.value) return;
  rateSaveError.value = '';
  rateEditorOpen.value = true;
}

async function saveRateConfig(rawValue: string) {
  const target = rateConfig.value;
  if (!target || !canManageRateConfig.value || rateSaving.value) return;
  rateSaving.value = true;
  rateSaveError.value = '';
  let writeConfirmed = false;
  try {
    const input = buildConfigSaveInput(target, rawValue, tenantScoped.value);
    if (!(await configApi.update(target.code, input))) throw new Error('服务未确认费率保存。');
    writeConfirmed = true;
    const refreshed = await configApi.getList();
    const readback = refreshed.find((item) => item.code === target.code);
    if (!readback || readback.value !== expectedConfigValue(target, input)) {
      throw new Error('费率读回值与保存值不一致，请刷新核对。');
    }
    rateConfig.value = readback;
    rateEditorOpen.value = false;
    toast.success('项目工时费率已保存并重新读取确认。');
  } catch (error) {
    rateSaveError.value = error instanceof Error ? error.message : '费率保存失败，请重试。';
    if (writeConfirmed) rateSaveError.value = `费率保存请求已完成，但未确认读回结果。${rateSaveError.value}`;
  } finally {
    rateSaving.value = false;
  }
}

function handleTemplateStatus(value: string) {
  templateForm.isEnabled = value === 'enabled';
}

function cancelEditor() {
  if (saving.value) return;
  editorOpen.value = false;
  editorError.value = '';
}

onMounted(() => {
  void loadPage();
  if (canViewRateConfig.value) void loadRateConfig();
});

onBeforeUnmount(() => {
  pageController?.abort();
  detailController?.abort();
  rateController?.abort();
  saveController?.abort();
  if (searchTimer) clearTimeout(searchTimer);
});
</script>

<template>
  <ReferenceBoardPageSurface
    :model="pageModel"
    :loading="loading"
    remote-mode
    :remote-rows="remoteRows"
    :remote-search="searchQuery"
    :remote-pagination="{ currentPage: page, pageSize, totalCount }"
    @page-change="handlePageChange"
    @row="openTemplate"
    @search-change="handleSearchChange"
    @tool="handleTool"
  >
    <template #notice>
      <p v-if="canViewRateConfig && rateConfig">
        项目工时费率：{{ rateDisplay }}。此参数与平台配置中心共用同一配置项。
        <UiButton
          v-if="canManageRateConfig"
          variant="secondary"
          size="sm"
          @click="openRateEditor"
          >调整费率</UiButton
        >
      </p>
      <p
        v-else-if="canViewRateConfig && rateError"
        role="alert"
      >
        {{ rateError }}
      </p>
      <p v-else>工时费率由平台配置参数维护；当前账号没有配置参数访问权限。</p>
      <p
        v-if="loadError"
        role="alert"
      >
        {{ loadError }}
      </p>
    </template>
  </ReferenceBoardPageSurface>

  <UiDialog
    v-model:open="detailOpen"
    :title="selectedTemplate?.name ?? '项目计划模板详情'"
    description="查看模板信息和结构化WBS条目。"
    size="xl"
  >
    <p v-if="detailLoading">正在读取模板详情…</p>
    <p
      v-else-if="detailError"
      role="alert"
    >
      {{ detailError }}
    </p>
    <template v-else-if="selectedTemplate">
      <UiFormField label="模板编码"
        ><span>{{ selectedTemplate.code }}</span></UiFormField
      >
      <UiFormField label="模板状态"
        ><span>{{ selectedTemplate.isEnabled ? '启用' : '停用' }}</span></UiFormField
      >
      <UiFormField label="模板说明"
        ><span>{{ selectedTemplate.description || '—' }}</span></UiFormField
      >
      <UiFormField label="WBS条目数"
        ><span>{{ selectedTemplate.nodes.length }}</span></UiFormField
      >
      <section>
        <h3>WBS结构</h3>
        <p v-if="detailNodes.length === 0">此模板尚未配置WBS条目。</p>
        <ol v-else>
          <li
            v-for="node in detailNodes"
            :key="node.id"
          >
            <strong>{{ node.wbsCode }} {{ node.name }}</strong>
            <span> · {{ node.type }} · 父级：{{ node.parentClientKey ? nodeName(node.parentClientKey) : '顶层' }}</span>
            <p v-if="node.scopeDescription">范围：{{ node.scopeDescription }}</p>
            <p v-if="node.deliveryDescription">交付：{{ node.deliveryDescription }}</p>
            <p v-if="node.estimateSummary">估算：{{ node.estimateSummary }}</p>
          </li>
        </ol>
      </section>
    </template>
    <template #footer>
      <UiButton
        v-if="selectedTemplate && canManageTemplates"
        variant="secondary"
        :disabled="detailLoading"
        @click="beginEdit"
        >编辑模板</UiButton
      >
      <UiButton
        v-if="selectedTemplate && canManageTemplates"
        variant="secondary"
        :disabled="detailLoading"
        @click="askDeleteTemplate"
        >删除模板</UiButton
      >
      <UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="editorOpen"
    :title="editorMode === 'create' ? '新建项目计划模板' : '编辑项目计划模板'"
    description="模板和WBS条目会保存到当前租户。"
    size="xl"
    @close="cancelEditor"
  >
    <UiFormField
      label="模板名称"
      required
    >
      <UiInput
        v-model="templateForm.name"
        maxlength="128"
        :disabled="saving"
      />
    </UiFormField>
    <UiFormField
      label="模板说明"
      hint="最多1024个字符。"
    >
      <UiTextarea
        v-model="templateForm.description"
        maxlength="1024"
        :disabled="saving"
      />
    </UiFormField>
    <UiFormField label="模板状态">
      <UiSelect
        :model-value="templateForm.isEnabled ? 'enabled' : 'disabled'"
        :options="templateStatusOptions"
        label="模板状态"
        :disabled="saving"
        @update:model-value="handleTemplateStatus"
      />
    </UiFormField>
    <section>
      <h3>WBS结构</h3>
      <p>编码在模板内唯一；父级关系不能形成循环。</p>
      <p v-if="templateForm.nodes.length === 0">尚未添加WBS条目。</p>
      <article
        v-for="(node, index) in templateForm.nodes"
        :key="node.clientKey"
      >
        <h4>WBS条目 {{ index + 1 }}</h4>
        <UiFormField
          label="WBS编码"
          required
        >
          <UiInput
            v-model="node.wbsCode"
            maxlength="64"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="名称"
          required
        >
          <UiInput
            v-model="node.name"
            maxlength="128"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="类型"
          required
        >
          <UiSelect
            v-model="node.type"
            :options="nodeTypeOptions"
            label="WBS类型"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField label="父级">
          <UiSelect
            v-model="node.parentClientKey"
            :options="parentOptions(node)"
            label="WBS父级"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField
          label="排序"
          hint="使用大于或等于0的整数。"
        >
          <UiInput
            v-model="node.sortOrder"
            type="number"
            min="0"
            step="1"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField label="范围说明">
          <UiTextarea
            v-model="node.scopeDescription"
            maxlength="1024"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField label="交付说明">
          <UiTextarea
            v-model="node.deliveryDescription"
            maxlength="1024"
            :disabled="saving"
          />
        </UiFormField>
        <UiFormField label="估算说明">
          <UiTextarea
            v-model="node.estimateSummary"
            maxlength="512"
            :disabled="saving"
          />
        </UiFormField>
        <UiButton
          variant="secondary"
          :disabled="saving"
          @click="removeNode(node.clientKey)"
          >移除此条目</UiButton
        >
      </article>
      <UiButton
        variant="secondary"
        :disabled="saving"
        @click="addNode"
        >新增WBS条目</UiButton
      >
    </section>
    <p
      v-if="editorError"
      role="alert"
    >
      {{ editorError }}
    </p>
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="saving"
        @click="cancelEditor"
        >取消</UiButton
      >
      <UiButton
        :disabled="saving || !canManageTemplates"
        @click="saveTemplate"
      >
        {{ saving ? '保存并确认中…' : '保存模板' }}
      </UiButton>
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="deleteOpen"
    title="删除项目计划模板"
    description="此操作会删除模板及其WBS条目。"
  >
    <p>确认删除“{{ deleteTarget?.name }}”吗？</p>
    <p
      v-if="deleteError"
      role="alert"
    >
      {{ deleteError }}
    </p>
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="deleting"
        @click="deleteOpen = false"
        >取消</UiButton
      >
      <UiButton
        :disabled="deleting"
        @click="deleteTemplate"
        >{{ deleting ? '删除并确认中…' : '确认删除' }}</UiButton
      >
    </template>
  </UiDialog>

  <ConfigEditorDialog
    v-model:open="rateEditorOpen"
    :item="rateEditorItem"
    :tenant-scoped="tenantScoped"
    :can-manage="canManageRateConfig"
    :saving="rateSaving"
    :save-error="rateSaveError"
    @submit="saveRateConfig"
  />
</template>
