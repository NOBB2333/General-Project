<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { createPlatformCacheApi, type PlatformCacheApi } from '@/api/core/platform-cache';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { toast, UiButton, UiDialog } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import { hasConfirmedCacheRefresh, toCacheRow, type CacheRow } from './cache.model';

const api = createPlatformCacheApi(apiClient);
const areas = ref<PlatformCacheApi.Area[]>([]);
const loading = ref(false);
const loadError = ref('');
const refreshError = ref('');
const selectedArea = ref<PlatformCacheApi.Area | null>(null);
const refreshOpen = ref(false);
const refreshing = ref(false);
const canManage = computed(() => canAccess('Platform.Cache.Manage'));
const rows = computed(() => areas.value.map(toCacheRow));
let activeRequest: AbortController | undefined;
let refreshRequest: AbortController | undefined;
let requestVersion = 0;

const pageModel = computed<ListPageModel<CacheRow>>(() => ({
  actions: [
    { id: 'reload', label: loading.value ? '读取中' : '重新读取', kind: 'refresh' },
    ...(canManage.value
      ? [
          {
            id: 'refresh-selected',
            label: refreshing.value ? '刷新中' : '刷新所选缓存域',
            requiresSelection: true,
            kind: 'custom' as const,
            tone: 'primary' as const,
          },
        ]
      : []),
  ],
  columns: [
    { key: 'area', title: '缓存域', width: 180 },
    { key: 'description', title: '作用范围', width: 420 },
    { key: 'version', title: '当前版本', width: 220 },
  ],
  demoPath: '/platform/cache',
  description: '查看平台登记的缓存域，并在确认后刷新单个缓存域。',
  emptyHint: loading.value ? '正在读取缓存域…' : loadError.value ? '缓存域读取失败，请重新读取。' : '暂无缓存域。',
  filters: [],
  key: '平台中心|缓存管理',
  label: '缓存管理',
  listScopes: [{ id: 'all', label: '已登记缓存域', count: areas.value.length }],
  listTabs: [{ id: 'all', label: '全部缓存域' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'platform-cache',
  pageSize: 20,
  panels: [],
  path: '/platform/cache',
  primaryAction: '刷新所选缓存域',
  replication: { status: 'implemented', referenceUrl: '/platform/cache' },
  routeName: 'platform-cache',
  rows: rows.value,
  searchPlaceholder: '搜索缓存域 / 作用范围',
  stats: [
    { key: 'total', label: '已登记缓存域', value: String(areas.value.length), tone: 'primary' },
    { key: 'scope', label: '操作范围', value: '单个缓存域', tone: 'neutral' },
    { key: 'mode', label: '刷新方式', value: '确认后执行', tone: 'success' },
  ],
  surface: 'list',
  tabs: [],
  tools: [],
}));

async function loadAreas() {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const currentVersion = ++requestVersion;
  loading.value = true;
  loadError.value = '';
  try {
    const result = await api.getAreas(controller.signal);
    if (currentVersion === requestVersion) areas.value = result;
  } catch (error) {
    if (currentVersion === requestVersion && !(error instanceof Error && error.name === 'AbortError')) {
      loadError.value = error instanceof Error ? error.message : '缓存域读取失败，请重试。';
    }
  } finally {
    if (currentVersion === requestVersion) loading.value = false;
  }
}

function openRefresh(area: PlatformCacheApi.Area | null) {
  if (!area || !canManage.value || refreshing.value) return;
  selectedArea.value = area;
  refreshError.value = '';
  refreshOpen.value = true;
}

function handleAction(actionId: string, selectedRows: ListPageRow[]) {
  if (actionId === 'reload') {
    void loadAreas();
    return;
  }
  if (actionId !== 'refresh-selected' || !canManage.value) return;
  if (selectedRows.length !== 1) {
    toast.error('请只选择一个缓存域后再刷新。');
    return;
  }
  const area = areas.value.find((item) => item.area === selectedRows[0].id) ?? null;
  openRefresh(area);
}

async function confirmRefresh() {
  const target = selectedArea.value;
  if (!target || !canManage.value || refreshing.value) return;

  const controller = new AbortController();
  refreshRequest = controller;
  refreshing.value = true;
  refreshError.value = '';
  let writeConfirmed = false;
  try {
    if (!(await api.refresh(target.area, controller.signal))) {
      throw new Error('服务未确认本次刷新。');
    }
    writeConfirmed = true;
    const refreshedAreas = await api.getAreas(controller.signal);
    if (!hasConfirmedCacheRefresh(target, refreshedAreas)) {
      throw new Error('刷新后的缓存域版本未变化或未返回，无法确认读回结果。');
    }
    areas.value = refreshedAreas;
    loadError.value = '';
    refreshOpen.value = false;
    selectedArea.value = null;
    toast.success(`缓存域“${target.area}”已刷新并确认版本变化。`);
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') return;
    const message = error instanceof Error ? error.message : '缓存域刷新失败，请重试。';
    refreshError.value = writeConfirmed ? `刷新请求已完成，但未确认读回结果。请重新读取核对。${message}` : message;
  } finally {
    refreshing.value = false;
  }
}

function updateDialogOpen(open: boolean) {
  if (refreshing.value) return;
  refreshOpen.value = open;
  if (!open) {
    selectedArea.value = null;
    refreshError.value = '';
  }
}

onMounted(() => void loadAreas());
onBeforeUnmount(() => {
  activeRequest?.abort();
  refreshRequest?.abort();
});
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="(action, selectedRows) => handleAction(action.id, selectedRows)"
    @row="(row) => openRefresh(areas.find((area) => area.area === row.id) ?? null)"
  >
    <template #notice>
      <p>刷新只作用于一个已登记缓存域；确认后通过新版本使该域旧缓存失效。</p>
      <p v-if="!canManage">当前账号没有缓存管理权限，只能查看状态。</p>
      <p
        v-if="loadError"
        role="alert"
      >
        {{ loadError }}
      </p>
      <p
        v-if="refreshError"
        role="alert"
      >
        {{ refreshError }}
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    :open="refreshOpen"
    title="确认刷新缓存域"
    description="刷新会使所选缓存域的旧版本失效，并影响依赖该缓存的后续读取。"
    @update:open="updateDialogOpen"
  >
    <template v-if="selectedArea">
      <dl>
        <dt>缓存域</dt>
        <dd>{{ selectedArea.area }}</dd>
        <dt>作用范围</dt>
        <dd>{{ selectedArea.description }}</dd>
        <dt>当前版本</dt>
        <dd>{{ selectedArea.version }}</dd>
      </dl>
      <p>本次只刷新上述缓存域。确认后会重新读取版本，以核实操作结果。</p>
      <p
        v-if="refreshError"
        role="alert"
      >
        {{ refreshError }}
      </p>
    </template>
    <template #footer>
      <UiButton
        :disabled="refreshing"
        @click="updateDialogOpen(false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="refreshing"
        :disabled="refreshing || !canManage"
        @click="confirmRefresh"
      >
        确认刷新
      </UiButton>
    </template>
  </UiDialog>
</template>
