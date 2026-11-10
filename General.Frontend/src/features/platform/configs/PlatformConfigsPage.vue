<script setup lang="ts">
import { Settings2 } from 'lucide-vue-next';
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { createConfigApi, type ConfigApi } from '@/api/core/config';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel } from '@/components/page/families/list/list-page.types';
import { toast } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import { hasCurrentTenantScope } from '@/session/token-storage';
import { sessionState } from '@/session/session-store';
import ConfigEditorDialog from './ConfigEditorDialog.vue';
import { buildConfigSaveInput, canEditConfig, expectedConfigValue, toConfigRow, type ConfigRow } from './config.model';

const api = createConfigApi(apiClient);
const configs = ref<ConfigApi.Item[]>([]);
const loading = ref(false);
const errorMessage = ref('');
const selected = ref<ConfigApi.Item | null>(null);
const editorOpen = ref(false);
const saving = ref(false);
const saveError = ref('');
const tenantScoped = computed(() => Boolean(sessionState.user && hasCurrentTenantScope()));
const canManage = computed(() => canAccess('Platform.Config.Manage'));
let activeRequest: AbortController | undefined;
let saveRequest: AbortController | undefined;
let version = 0;
const rows = computed(() => configs.value.map((item) => toConfigRow(item, tenantScoped.value)));
const pageModel = computed<ListPageModel<ConfigRow>>(() => ({
  actions: [{ id: 'refresh', label: loading.value ? '刷新中' : '刷新配置', kind: 'refresh' }],
  columns: [
    { key: 'name', title: '参数名称', width: 200 },
    { key: 'group', title: '分组', width: 120 },
    { key: 'value', title: '当前值', width: 200 },
    { key: 'defaultValue', title: '默认值', width: 150 },
    { key: 'scope', title: '当前值来源', width: 170 },
    { key: 'type', title: '类型', width: 80 },
    { key: 'editing', title: '编辑状态', width: 130 },
  ],
  demoPath: '/platform/configs',
  description: '维护平台允许在线修改的运行参数。点击参数查看完整值、编码和说明，保存后重新读取确认。',
  emptyHint: loading.value ? '正在加载配置…' : errorMessage.value ? '配置加载失败，请刷新重试。' : '暂无匹配的配置项。',
  filters: [
    { key: 'group', label: '配置分组', options: ['全部分组', ...new Set(rows.value.map((row) => row.group))] },
    { key: 'scope', label: '值来源', options: ['全部来源', ...new Set(rows.value.map((row) => row.scope))] },
  ],
  key: '平台中心|配置参数',
  label: '配置参数',
  listScopes: [{ id: 'all', label: tenantScoped.value ? '当前租户配置' : '全局配置' }],
  listTabs: [{ id: 'all', label: '全部参数' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'platform-configs',
  pageSize: 20,
  panels: [],
  path: '/platform/configs',
  primaryAction: '刷新配置',
  replication: { status: 'implemented', referenceUrl: '/platform/configs' },
  routeName: 'platform-configs',
  rows: rows.value,
  searchPlaceholder: '搜索参数名称 / 编码 / 说明',
  stats: [
    { key: 'total', label: '配置总数', value: String(configs.value.length), tone: 'primary' },
    { key: 'editable', label: '可编辑', value: String(configs.value.filter(canEditConfig).length), tone: 'neutral' },
    {
      key: 'overrides',
      label: '当前租户覆盖',
      value: String(configs.value.filter((item) => item.hasTenantValue).length),
      tone: 'neutral',
    },
  ],
  surface: 'list',
  tabs: [],
  tools: [],
}));

async function loadConfigs() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const requestVersion = ++version;
  loading.value = true;
  configs.value = [];
  errorMessage.value = '';
  try {
    const result = await api.getList(controller.signal);
    if (requestVersion === version) configs.value = result;
  } catch (error) {
    if (requestVersion === version && !(error instanceof Error && error.name === 'AbortError')) {
      errorMessage.value = error instanceof Error ? error.message : '配置加载失败，请刷新重试。';
    }
  } finally {
    if (requestVersion === version) loading.value = false;
  }
}

function openConfig(row: ConfigRow) {
  selected.value = configs.value.find((item) => item.code === row.code) ?? null;
  saveError.value = '';
  editorOpen.value = Boolean(selected.value);
}

async function saveConfig(value: string) {
  const target = selected.value;
  if (!target || !canManage.value || saving.value) return;
  const controller = new AbortController();
  saveRequest = controller;
  saving.value = true;
  saveError.value = '';
  let writeConfirmed = false;
  try {
    const input = buildConfigSaveInput(target, value, tenantScoped.value);
    if (!(await api.update(target.code, input, controller.signal)))
      throw new Error('服务未确认本次保存，请刷新后核对。');
    writeConfirmed = true;
    const refreshed = await api.getList(controller.signal);
    const readback = refreshed.find((item) => item.code === target.code);
    if (!readback || readback.value !== expectedConfigValue(target, input))
      throw new Error('配置读回值与保存值不一致，请刷新核对。');
    configs.value = refreshed;
    editorOpen.value = false;
    toast.success('配置已保存并读回确认。');
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') return;
    const message = error instanceof Error ? error.message : '保存失败，请重试。';
    saveError.value = writeConfirmed ? `保存请求已完成，但未确认读回结果。请刷新核对。${message}` : message;
  } finally {
    saving.value = false;
  }
}

onMounted(() => void loadConfigs());
onBeforeUnmount(() => {
  activeRequest?.abort();
  saveRequest?.abort();
});
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="(action) => action.id === 'refresh' && loadConfigs()"
    @row="openConfig"
  >
    <template #hero-icon><Settings2 :size="20" /></template>
    <template #notice>
      <p>本次保存范围：{{ tenantScoped ? '当前租户' : '全局' }}。填入默认值仅修改编辑草稿，点击保存后才生效。</p>
      <p
        v-if="errorMessage"
        role="alert"
      >
        {{ errorMessage }}
      </p>
    </template>
  </ListPageSurface>
  <ConfigEditorDialog
    v-model:open="editorOpen"
    :item="selected"
    :tenant-scoped="tenantScoped"
    :can-manage="canManage"
    :saving="saving"
    :save-error="saveError"
    @submit="saveConfig"
  />
</template>
