<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ArrowLeft, BrainCircuit } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import { toast, UiAlertDialog, UiButton, UiDialog } from '@/components/ui';
import AiModelFormDialog from './AiModelFormDialog.vue';
import AiProviderFormDialog from './AiProviderFormDialog.vue';
import { toModelRow, toProviderRow, type ModelRow, type ProviderRow } from './providers.model';

type PageMode = 'models' | 'providers';
type DeleteTarget = { kind: 'model'; value: AiApi.Model } | { kind: 'provider'; value: AiApi.Provider };

const api = createAiApi(apiClient);
const providers = ref<AiApi.Provider[]>([]);
const models = ref<AiApi.Model[]>([]);
const mode = ref<PageMode>('providers');
const selectedProvider = ref<AiApi.Provider | null>(null);
const loading = ref(false);
const savingProvider = ref(false);
const savingModel = ref(false);
const deleting = ref(false);
const actionError = ref('');
const loadError = ref('');
const providerError = ref('');
const modelError = ref('');
const providerDialogOpen = ref(false);
const modelDialogOpen = ref(false);
const editingProvider = ref<AiApi.Provider | null>(null);
const editingModel = ref<AiApi.Model | null>(null);
const deleteTarget = ref<DeleteTarget | null>(null);
const deleteDialogOpen = ref(false);
const discoveryDialogOpen = ref(false);
const discoveryProvider = ref('');
const discoveryResult = ref<AiApi.ProviderModelDiscoveryResult | null>(null);
const connectionResult = ref('');
const activeRequest = ref<AbortController | null>(null);
const canManage = computed(() => canAccess('Ai.Provider.Manage'));
const providerRows = computed<ProviderRow[]>(() => providers.value.map(toProviderRow));
const modelRows = computed<ModelRow[]>(() => models.value.map(toModelRow));

const pageModel = computed<ListPageModel<ListPageRow>>(() => {
  const actions: PageAction[] = [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }];
  if (mode.value === 'providers') {
    if (canManage.value) {
      actions.push(
        { id: 'create-provider', kind: 'create', label: '新增供应商' },
        { id: 'edit-provider', kind: 'custom', label: '编辑供应商', requiresSelection: true },
        { id: 'models', kind: 'custom', label: '管理模型', requiresSelection: true },
        { id: 'test', kind: 'custom', label: '测试连接', requiresSelection: true },
        { id: 'discover', kind: 'custom', label: '发现模型', requiresSelection: true },
        { id: 'sync', kind: 'custom', label: '同步模型', requiresSelection: true },
        { id: 'delete-provider', kind: 'delete', label: '删除供应商', requiresSelection: true },
      );
    }
  } else {
    actions.push({ id: 'back', kind: 'custom', label: '返回供应商' });
    if (canManage.value) {
      actions.push(
        { id: 'create-model', kind: 'create', label: '新增模型' },
        { id: 'edit-model', kind: 'custom', label: '编辑模型', requiresSelection: true },
        { id: 'delete-model', kind: 'delete', label: '删除模型', requiresSelection: true },
      );
    }
  }

  const currentRows = mode.value === 'providers' ? providerRows.value : modelRows.value;
  const enabledCount = currentRows.filter((row) => row.status === '启用').length;
  const defaultCount = currentRows.filter((row) => row.defaultStatus !== '—').length;
  const itemLabel = mode.value === 'providers' ? '供应商' : '模型';
  const title = mode.value === 'providers' ? '模型供应商' : `${selectedProvider.value?.name ?? '供应商'} · 模型`;

  return {
    actions,
    columns:
      mode.value === 'providers'
        ? [
            { key: 'name', title: '供应商名称', width: 220, sticky: true },
            { key: 'providerType', title: '协议', width: 180 },
            { key: 'endpoint', title: 'Endpoint', width: 300 },
            { key: 'keyStatus', title: 'API 密钥', width: 120, presentation: 'status' },
            { key: 'modelCount', title: '模型数', width: 100 },
            { key: 'status', title: '状态', width: 100, presentation: 'status' },
            { key: 'defaultStatus', title: '默认项', width: 130, presentation: 'status' },
          ]
        : [
            { key: 'displayName', title: '模型名称', width: 220, sticky: true },
            { key: 'modelCode', title: '模型编码', width: 220 },
            { key: 'modelType', title: '类型', width: 140 },
            { key: 'capabilities', title: '能力', width: 260 },
            { key: 'status', title: '状态', width: 100, presentation: 'status' },
            { key: 'defaultStatus', title: '默认项', width: 130, presentation: 'status' },
          ],
    demoPath: '/ai/providers',
    description:
      mode.value === 'providers'
        ? '维护模型供应商连接、密钥状态和默认供应商。'
        : '维护当前供应商的模型、服务端启用状态和默认模型。',
    emptyHint: loading.value
      ? `正在读取${itemLabel}…`
      : loadError.value
        ? `${itemLabel}读取失败，请刷新重试。`
        : `暂无${itemLabel}。`,
    filters: [],
    key: 'AI管理|模型供应商',
    label: title,
    listScopes: [{ id: 'all', label: `全部${itemLabel}`, count: currentRows.length }],
    listTabs: [{ id: 'all', label: `全部${itemLabel}` }],
    listViews: [{ id: 'list', label: '列表' }],
    module: 'AI 管理',
    moduleSlug: 'ai',
    pageSlug: 'ai-providers',
    pageSize: 20,
    panels: [],
    path: '/ai/providers',
    primaryAction: mode.value === 'providers' ? '新增供应商' : '新增模型',
    replication: { status: 'implemented', referenceUrl: '/integration/connectors' },
    routeName: 'ai-providers',
    rows: currentRows,
    searchPlaceholder: mode.value === 'providers' ? '搜索供应商名称、协议或地址' : '搜索模型名称、编码或能力',
    stats: [
      { key: 'total', label: `全部${itemLabel}`, value: String(currentRows.length), tone: 'primary' },
      { key: 'enabled', label: '启用', value: String(enabledCount), tone: 'success' },
      { key: 'default', label: '默认项', value: String(defaultCount), tone: 'warning' },
      {
        key: 'disabled',
        label: '停用',
        value: String(currentRows.length - enabledCount),
        tone: 'neutral',
      },
    ],
    surface: 'list',
    tabs: [],
    tools: [],
  };
});

const deleteDescription = computed(() => {
  if (!deleteTarget.value) return '';
  if (deleteTarget.value.kind === 'provider') {
    const provider = deleteTarget.value.value;
    return `删除“${provider.name}”及其 ${provider.modelCount} 个模型配置？此操作无法撤销。`;
  }
  return `删除模型“${deleteTarget.value.value.displayName}”？此操作无法撤销。`;
});

function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

async function loadProviders(): Promise<void> {
  activeRequest.value?.abort();
  const controller = new AbortController();
  activeRequest.value = controller;
  loading.value = true;
  loadError.value = '';
  try {
    providers.value = await api.getProviders(controller.signal);
  } catch (error) {
    if (!controller.signal.aborted) loadError.value = errorMessage(error);
  } finally {
    if (activeRequest.value === controller) {
      activeRequest.value = null;
      loading.value = false;
    }
  }
}

async function loadModels(providerId: string): Promise<void> {
  activeRequest.value?.abort();
  const controller = new AbortController();
  activeRequest.value = controller;
  loading.value = true;
  loadError.value = '';
  try {
    models.value = await api.getManageModels(providerId, controller.signal);
  } catch (error) {
    if (!controller.signal.aborted) loadError.value = errorMessage(error);
  } finally {
    if (activeRequest.value === controller) {
      activeRequest.value = null;
      loading.value = false;
    }
  }
}

function openProviderForm(provider: AiApi.Provider | null) {
  editingProvider.value = provider;
  providerError.value = '';
  providerDialogOpen.value = true;
}

function openModelForm(model: AiApi.Model | null) {
  if (!selectedProvider.value) return;
  editingModel.value = model;
  modelError.value = '';
  modelDialogOpen.value = true;
}

function oneSelected(rows: ListPageRow[]): ListPageRow | null {
  if (rows.length !== 1) {
    actionError.value = '请选择一条记录后再操作。';
    return null;
  }
  actionError.value = '';
  return rows[0] ?? null;
}

async function openModels(provider: AiApi.Provider): Promise<void> {
  selectedProvider.value = provider;
  mode.value = 'models';
  await loadModels(provider.id);
}

async function runProviderAction(id: string, kind: 'discover' | 'sync' | 'test'): Promise<void> {
  const provider = providers.value.find((item) => item.id === id);
  if (!provider) return;
  actionError.value = '';
  try {
    if (kind === 'test') {
      const result = await api.testProvider(provider.id);
      connectionResult.value = `${provider.name}：${result.message}`;
      if (result.success) toast.success(result.message || '连接测试通过。');
      else toast.error(result.message || '连接测试失败。');
      return;
    }
    const result =
      kind === 'discover' ? await api.discoverProviderModels(provider.id) : await api.syncProviderModels(provider.id);
    if (!result.success) {
      actionError.value = result.message || '模型请求失败。';
      toast.error(actionError.value);
      if (kind === 'discover') {
        discoveryProvider.value = provider.name;
        discoveryResult.value = result;
        discoveryDialogOpen.value = true;
      }
      return;
    }
    if (kind === 'discover') {
      discoveryProvider.value = provider.name;
      discoveryResult.value = result;
      discoveryDialogOpen.value = true;
      return;
    }
    await loadProviders();
    toast.success(result.message || '模型同步完成。');
  } catch (error) {
    actionError.value = errorMessage(error);
    toast.error(actionError.value);
  }
}

function handleAction(action: PageAction, rows: ListPageRow[]) {
  if (action.id === 'reload') {
    if (mode.value === 'providers') void loadProviders();
    else if (selectedProvider.value) void loadModels(selectedProvider.value.id);
    return;
  }
  if (mode.value === 'providers') {
    if (action.id === 'create-provider') {
      openProviderForm(null);
      return;
    }
    const selected = oneSelected(rows);
    if (!selected) return;
    const provider = providers.value.find((item) => item.id === selected.id);
    if (!provider) return;
    if (action.id === 'edit-provider') openProviderForm(provider);
    if (action.id === 'models') void openModels(provider);
    if (action.id === 'test' || action.id === 'discover' || action.id === 'sync') {
      void runProviderAction(provider.id, action.id);
    }
    if (action.id === 'delete-provider') {
      deleteTarget.value = { kind: 'provider', value: provider };
      deleteDialogOpen.value = true;
    }
    return;
  }
  if (action.id === 'back') {
    activeRequest.value?.abort();
    selectedProvider.value = null;
    models.value = [];
    mode.value = 'providers';
    void loadProviders();
    return;
  }
  if (action.id === 'create-model') {
    openModelForm(null);
    return;
  }
  const selected = oneSelected(rows);
  if (!selected) return;
  const model = models.value.find((item) => item.id === selected.id);
  if (!model) return;
  if (action.id === 'edit-model') openModelForm(model);
  if (action.id === 'delete-model') {
    deleteTarget.value = { kind: 'model', value: model };
    deleteDialogOpen.value = true;
  }
}

function openRow(row: ListPageRow) {
  if (mode.value === 'providers') {
    const provider = providers.value.find((item) => item.id === row.id);
    if (provider) openProviderForm(provider);
    return;
  }
  const model = models.value.find((item) => item.id === row.id);
  if (model) openModelForm(model);
}

async function saveProvider(input: AiApi.ProviderSaveInput) {
  if (savingProvider.value) return;
  if (!input.name) {
    providerError.value = '请输入供应商名称。';
    return;
  }
  savingProvider.value = true;
  providerError.value = '';
  try {
    if (editingProvider.value) await api.updateProvider(editingProvider.value.id, input);
    else await api.createProvider(input);
    providerDialogOpen.value = false;
    await loadProviders();
    toast.success('供应商配置已保存。');
  } catch (error) {
    providerError.value = errorMessage(error);
  } finally {
    savingProvider.value = false;
  }
}

async function saveModel(input: AiApi.ModelSaveInput) {
  if (savingModel.value) return;
  if (!selectedProvider.value) return;
  savingModel.value = true;
  modelError.value = '';
  try {
    if (editingModel.value) await api.updateModel(editingModel.value.id, input);
    else await api.createModel(input);
    modelDialogOpen.value = false;
    await loadModels(selectedProvider.value.id);
    await loadProviders();
    toast.success('模型配置已保存。');
  } catch (error) {
    modelError.value = errorMessage(error);
  } finally {
    savingModel.value = false;
  }
}

function closeDeleteDialog(open: boolean) {
  deleteDialogOpen.value = open;
}

async function confirmDelete() {
  if (deleting.value || !deleteTarget.value) return;
  deleting.value = true;
  const target = deleteTarget.value;
  try {
    if (target.kind === 'provider') {
      await api.deleteProvider(target.value.id);
      mode.value = 'providers';
      selectedProvider.value = null;
      models.value = [];
      await loadProviders();
    } else {
      await api.deleteModel(target.value.id);
      if (selectedProvider.value) {
        await loadModels(selectedProvider.value.id);
        await loadProviders();
      }
    }
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    toast.success(target.kind === 'provider' ? '供应商及其模型已删除。' : '模型已删除。');
  } catch (error) {
    actionError.value = errorMessage(error);
    toast.error(actionError.value);
  } finally {
    deleting.value = false;
  }
}

function closeDiscovery(open: boolean) {
  discoveryDialogOpen.value = open;
  if (!open) discoveryResult.value = null;
}

onMounted(() => void loadProviders());
onBeforeUnmount(() => activeRequest.value?.abort());
</script>

<template>
  <ListPageSurface
    :key="mode"
    :model="pageModel"
    @action="handleAction"
    @row="openRow"
  >
    <template #hero-icon>
      <ArrowLeft
        v-if="mode === 'models'"
        :size="22"
        aria-hidden="true"
      />
      <BrainCircuit
        v-else
        :size="22"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <p>密钥只在提交时发送给后端；后端返回配置状态，不返回 API 密钥明文。</p>
      <p>模型启用状态、默认项和模型列表均来自服务端；连接测试结果由服务端实时返回。</p>
      <p
        v-if="connectionResult"
        role="status"
      >
        {{ connectionResult }}
      </p>
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
        当前账号没有模型供应商管理权限。
      </p>
    </template>
  </ListPageSurface>

  <AiProviderFormDialog
    :open="providerDialogOpen"
    :provider="editingProvider"
    :saving="savingProvider"
    :error="providerError"
    @update:open="providerDialogOpen = $event"
    @save="saveProvider"
  />

  <AiModelFormDialog
    :open="modelDialogOpen"
    :model="editingModel"
    :models="models"
    :provider-id="selectedProvider?.id ?? ''"
    :saving="savingModel"
    :error="modelError"
    @update:open="modelDialogOpen = $event"
    @save="saveModel"
  />

  <UiAlertDialog
    :open="deleteDialogOpen"
    title="确认删除配置"
    :description="deleteDescription"
    confirm-label="删除"
    @update:open="closeDeleteDialog"
    @confirm="confirmDelete"
    @cancel="deleteTarget = null"
  />

  <UiDialog
    :open="discoveryDialogOpen"
    :title="`${discoveryProvider} · 发现模型`"
    description="以下清单来自服务端发现结果；只有执行同步后才会保存到模型配置。"
    size="md"
    @update:open="closeDiscovery"
  >
    <p
      v-if="discoveryResult?.message"
      role="status"
    >
      {{ discoveryResult.message }}
    </p>
    <ul v-if="discoveryResult?.models.length">
      <li
        v-for="model in discoveryResult.models"
        :key="model.modelCode"
      >
        {{ model.displayName }}（{{ model.modelCode }}，{{ model.modelType }}）
      </li>
    </ul>
    <p v-else>服务端没有返回可发现模型。</p>
    <template #footer>
      <UiButton @click="closeDiscovery(false)">关闭</UiButton>
    </template>
  </UiDialog>
</template>
