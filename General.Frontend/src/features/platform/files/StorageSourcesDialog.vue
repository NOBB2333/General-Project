<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import {
  createPlatformFileStorageSourcesApi,
  type PlatformFileStorageSourceApi,
} from '@/api/core/platform-file-storage-sources';
import { apiClient } from '@/api/request';
import { toast, UiButton, UiDialog, UiFormField, UiSelect } from '@/components/ui';
import { toStorageSourceSaveInput, validateStorageSourceDraft, type StorageSourceDraft } from './storage-sources.model';
import StorageSourceEditorDialog from './StorageSourceEditorDialog.vue';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [open: boolean] }>();
const api = createPlatformFileStorageSourcesApi(apiClient);
const sources = ref<PlatformFileStorageSourceApi.Item[]>([]);
const selectedId = ref('');
const loading = ref(false);
const saving = ref(false);
const loadError = ref('');
const operationError = ref('');
const editorOpen = ref(false);
const editorError = ref('');
const editorSource = ref<null | PlatformFileStorageSourceApi.Item>(null);
const confirmOpen = ref(false);
const confirmBusy = ref(false);
const confirmError = ref('');
const pendingOperation = ref<null | {
  kind: 'default' | 'delete' | 'toggle';
  source: PlatformFileStorageSourceApi.Item;
  enabled?: boolean;
}>(null);
let activeRequest: AbortController | undefined;
let requestVersion = 0;

const selectedSource = computed(() => sources.value.find((source) => source.id === selectedId.value) ?? null);
const sourceOptions = computed(() =>
  sources.value.map((source) => ({
    label: `${source.name} · ${source.providerName}${source.isDefault ? ' · 默认' : ''}${source.isEnabled ? '' : ' · 停用'}`,
    value: source.id,
  })),
);
const confirmationTitle = computed(() => {
  const operation = pendingOperation.value;
  if (operation?.kind === 'default') return '设为默认存储源';
  if (operation?.kind === 'toggle') return operation.enabled ? '启用存储源' : '停用存储源';
  return '删除存储源配置';
});
const confirmationDescription = computed(() => {
  const operation = pendingOperation.value;
  if (!operation) return '';
  if (operation.kind === 'default') return `“${operation.source.name}”将用于未指定存储源的新文件。`;
  if (operation.kind === 'toggle') {
    return operation.enabled
      ? `将重新启用“${operation.source.name}”。`
      : `停用“${operation.source.name}”后，新文件不能再写入该源；已有文件仍保留原存储位置。`;
  }
  return `将删除“${operation.source.name}”的配置。默认存储源或已被文件引用的存储源不能删除。`;
});

async function loadSources(): Promise<boolean> {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const version = ++requestVersion;
  loading.value = true;
  loadError.value = '';
  try {
    const result = await api.getList(false, controller.signal);
    if (version !== requestVersion) return false;
    sources.value = result;
    if (!result.some((source) => source.id === selectedId.value)) {
      selectedId.value = result[0]?.id ?? '';
    }
    return true;
  } catch (error) {
    if (version === requestVersion && !(error instanceof Error && error.name === 'AbortError')) {
      loadError.value = error instanceof Error ? error.message : '存储源读取失败，请重试。';
    }
    return false;
  } finally {
    if (version === requestVersion) loading.value = false;
  }
}

function openCreate() {
  editorSource.value = null;
  editorError.value = '';
  editorOpen.value = true;
}

function openEdit() {
  if (!selectedSource.value) return;
  editorSource.value = selectedSource.value;
  editorError.value = '';
  editorOpen.value = true;
}

function askOperation(
  kind: 'default' | 'delete' | 'toggle',
  source: PlatformFileStorageSourceApi.Item,
  enabled?: boolean,
) {
  if (confirmBusy.value || saving.value) return;
  if (kind === 'default' && !source.isEnabled) {
    toast.error('停用的存储源不能设为默认。');
    return;
  }
  if (kind === 'toggle' && source.isDefault && enabled === false) {
    toast.error('默认存储源不能停用。请先将其他启用的存储源设为默认。');
    return;
  }
  pendingOperation.value = { enabled, kind, source };
  confirmError.value = '';
  confirmOpen.value = true;
}

async function confirmOperation() {
  const operation = pendingOperation.value;
  if (!operation || confirmBusy.value) return;
  confirmBusy.value = true;
  confirmError.value = '';
  try {
    if (operation.kind === 'default') await api.setDefault(operation.source.id);
    else if (operation.kind === 'toggle') await api.toggle(operation.source.id, Boolean(operation.enabled));
    else await api.delete(operation.source.id);

    confirmOpen.value = false;
    pendingOperation.value = null;
    const refreshed = await loadSources();
    if (!refreshed) {
      operationError.value = '存储源操作已提交，但列表刷新失败；请重新读取确认。';
      return;
    }
    const readback = sources.value.find((source) => source.id === operation.source.id);
    const confirmed =
      operation.kind === 'default'
        ? readback?.isDefault === true
        : operation.kind === 'toggle'
          ? readback?.isEnabled === operation.enabled
          : readback === undefined;
    if (!confirmed) {
      operationError.value = '存储源操作已提交，但未从服务端读回预期结果；请重新读取核对。';
      return;
    }
    operationError.value = '';
    toast.success(
      operation.kind === 'default'
        ? '默认存储源已更新。'
        : operation.kind === 'toggle'
          ? '存储源状态已更新。'
          : '存储源配置已删除。',
    );
  } catch (error) {
    confirmError.value = error instanceof Error ? error.message : '存储源操作失败，请重试。';
  } finally {
    confirmBusy.value = false;
  }
}

async function saveSource(input: PlatformFileStorageSourceApi.SaveInput) {
  if (saving.value) return;
  saving.value = true;
  editorError.value = '';
  try {
    const source = editorSource.value;
    const saved = source ? await api.update(source.id, input) : await api.create(input);
    const defaultMatches =
      saved.isDefault === input.isDefault ||
      (!source && sources.value.length === 0 && saved.isDefault) ||
      Boolean(source?.isDefault && saved.isDefault);
    const responseMatches =
      saved.name === input.name &&
      saved.providerName === input.providerName &&
      saved.accessKeyId === input.accessKeyId &&
      (saved.bucketName ?? null) === input.bucketName &&
      (saved.customDomain ?? null) === input.customDomain &&
      (saved.endpoint ?? null) === input.endpoint &&
      (saved.pathTemplate ?? null) === input.pathTemplate &&
      (saved.region ?? null) === input.region &&
      (saved.remark ?? null) === input.remark &&
      (saved.rootPath ?? null) === input.rootPath &&
      saved.isEnabled === input.isEnabled &&
      saved.isPublic === input.isPublic &&
      saved.useSsl === input.useSsl &&
      defaultMatches;
    if (!responseMatches) throw new Error('服务端返回的存储源配置与提交不一致，未确认保存。');

    editorOpen.value = false;
    editorSource.value = null;
    selectedId.value = saved.id;
    const refreshed = await loadSources();
    operationError.value = refreshed ? '' : '存储源配置已由服务端确认，但列表刷新失败；请重新读取核对。';
    if (refreshed && !sources.value.some((item) => item.id === saved.id)) {
      operationError.value = '存储源配置已提交，但列表读回未找到该项；请重新读取核对。';
    } else if (refreshed) {
      toast.success(source ? '存储源配置已保存并读回。' : '存储源已创建并读回。');
    }
  } catch (error) {
    editorError.value = error instanceof Error ? error.message : '存储源保存失败，请重试。';
  } finally {
    saving.value = false;
  }
}

function closeEditor(open: boolean) {
  if (saving.value) return;
  editorOpen.value = open;
  if (!open) {
    editorSource.value = null;
    editorError.value = '';
  }
}

function updateManagerOpen(open: boolean) {
  if (saving.value || confirmBusy.value) return;
  emit('update:open', open);
  if (!open) {
    editorOpen.value = false;
    editorSource.value = null;
    editorError.value = '';
    confirmOpen.value = false;
    pendingOperation.value = null;
  }
}

watch(
  () => props.open,
  (open) => {
    if (open) {
      operationError.value = '';
      void loadSources();
    }
  },
);

onBeforeUnmount(() => activeRequest?.abort());
</script>

<template>
  <UiDialog
    :open="open"
    title="文件存储源"
    description="管理General.Admin文件服务可使用的存储位置。连接能力通过真实文件读写验证。"
    size="lg"
    @update:open="updateManagerOpen"
  >
    <UiFormField label="已登记存储源">
      <UiSelect
        v-model="selectedId"
        label="已登记存储源"
        :options="sourceOptions"
        :disabled="loading || sources.length === 0"
        placeholder="请选择存储源"
      />
    </UiFormField>
    <dl v-if="selectedSource">
      <dt>名称</dt>
      <dd>{{ selectedSource.name }}</dd>
      <dt>类型</dt>
      <dd>{{ selectedSource.providerName }}</dd>
      <dt>状态</dt>
      <dd>{{ selectedSource.isEnabled ? '启用' : '停用' }}</dd>
      <dt>默认源</dt>
      <dd>{{ selectedSource.isDefault ? '是' : '否' }}</dd>
      <dt>Endpoint / 根目录</dt>
      <dd>{{ selectedSource.endpoint || selectedSource.rootPath || '服务端默认目录' }}</dd>
      <dt>Bucket</dt>
      <dd>{{ selectedSource.bucketName || '—' }}</dd>
      <dt>AccessKeyId</dt>
      <dd>{{ selectedSource.accessKeyId || '—' }}</dd>
      <dt>SecretKey</dt>
      <dd>已保存，不向前端回传</dd>
    </dl>
    <p v-else-if="!loading && !loadError">暂无存储源。</p>
    <p
      v-if="loading"
      role="status"
    >
      正在读取存储源…
    </p>
    <p
      v-if="loadError"
      role="alert"
    >
      {{ loadError }}
    </p>
    <p
      v-if="operationError"
      role="alert"
    >
      {{ operationError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="loading || saving || confirmBusy"
        @click="void loadSources()"
        >重新读取</UiButton
      >
      <UiButton
        :disabled="loading || saving || confirmBusy"
        @click="openCreate"
        >新增</UiButton
      >
      <UiButton
        :disabled="!selectedSource || loading || saving || confirmBusy"
        @click="openEdit"
        >编辑</UiButton
      >
      <UiButton
        v-if="selectedSource && !selectedSource.isDefault"
        :disabled="!selectedSource.isEnabled || loading || saving || confirmBusy"
        @click="askOperation('default', selectedSource)"
        >设为默认</UiButton
      >
      <UiButton
        v-if="selectedSource"
        :disabled="loading || saving || confirmBusy || (selectedSource.isDefault && selectedSource.isEnabled)"
        @click="askOperation('toggle', selectedSource, !selectedSource.isEnabled)"
        >{{ selectedSource.isEnabled ? '停用' : '启用' }}</UiButton
      >
      <UiButton
        v-if="selectedSource"
        variant="danger"
        :disabled="selectedSource.isDefault || loading || saving || confirmBusy"
        @click="askOperation('delete', selectedSource)"
        >删除配置</UiButton
      >
      <UiButton
        :disabled="saving || confirmBusy"
        @click="updateManagerOpen(false)"
        >关闭</UiButton
      >
    </template>
  </UiDialog>

  <StorageSourceEditorDialog
    :open="editorOpen"
    :source="editorSource"
    :saving="saving"
    :error="editorError"
    @update:open="closeEditor"
    @submit="saveSource"
  />

  <UiDialog
    :open="confirmOpen"
    :title="confirmationTitle"
    :description="confirmationDescription"
    @update:open="confirmOpen = $event"
  >
    <p
      v-if="confirmError"
      role="alert"
    >
      {{ confirmError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="confirmBusy"
        @click="confirmOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="confirmBusy"
        :disabled="confirmBusy"
        @click="confirmOperation"
        >确认</UiButton
      >
    </template>
  </UiDialog>
</template>
