<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { Megaphone } from 'lucide-vue-next';
import { createPlatformUpdateLogApi, type PlatformUpdateLogApi } from '@/api/core/platform-update-log';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { toast, UiButton, UiDialog, UiFormField } from '@/components/ui';
import { sessionState } from '@/session/session-store';
import UpdateLogEditorDialog from './UpdateLogEditorDialog.vue';
import { parseUpdateLogTime, toUpdateLogRow, type UpdateLogRow } from './update-log.model';

const api = createPlatformUpdateLogApi(apiClient);
const pageSize = 20;
const items = ref<PlatformUpdateLogApi.Item[]>([]);
const currentPage = ref(1);
const totalCount = ref(0);
const searchQuery = ref('');
const loading = ref(false);
const loadError = ref('');
const actionError = ref('');
const saving = ref(false);
const deleting = ref(false);
const editorOpen = ref(false);
const editorError = ref('');
const editingItem = ref<null | PlatformUpdateLogApi.Item>(null);
const publishConfirmOpen = ref(false);
const publishError = ref('');
const pendingPublish = ref<null | PlatformUpdateLogApi.SaveInput>(null);
const deleteConfirmOpen = ref(false);
const deleteError = ref('');
const deleteTarget = ref<null | PlatformUpdateLogApi.Item>(null);
const detailOpen = ref(false);
const detailItem = ref<null | PlatformUpdateLogApi.Item>(null);
const canManage = computed(() =>
  Boolean(sessionState.user?.roles.some((role) => role.toLocaleLowerCase() === 'admin')),
);
const rows = computed(() => items.value.map(toUpdateLogRow));
const itemById = computed(() => new Map(items.value.map((item) => [item.id, item])));
const publishDescription = computed(() => {
  const input = pendingPublish.value;
  if (!input) return '创建更新公告后会立即通知全体用户。';
  return `发布“${input.title}”（${input.version}）后会立即向全体用户发送通知。通知不可撤回，请确认内容无误。`;
});
const pageModel = computed<ListPageModel<UpdateLogRow>>(() => ({
  actions: [
    { id: 'refresh', label: loading.value ? '读取中' : '刷新', kind: 'refresh' },
    ...(canManage.value
      ? [
          { id: 'create', label: '发布公告', kind: 'create' as const, tone: 'primary' as const },
          { id: 'edit', label: '编辑所选', kind: 'custom' as const, requiresSelection: true },
          {
            id: 'delete',
            label: '删除所选',
            kind: 'delete' as const,
            requiresSelection: true,
            tone: 'danger' as const,
          },
        ]
      : []),
  ],
  columns: [
    { key: 'version', title: '版本', width: 120 },
    { key: 'title', title: '更新标题', width: 260 },
    { key: 'publishedAt', title: '发布时间', width: 190 },
    { key: 'impactScope', title: '影响范围', width: 190 },
    { key: 'authorName', title: '发布人', width: 130 },
    { key: 'summary', title: '更新内容', width: 360 },
  ],
  demoPath: '/platform/update-logs',
  description: '查看平台版本公告。管理员可以发布、编辑和删除公告。',
  emptyHint: loading.value
    ? '正在读取更新公告…'
    : loadError.value
      ? '更新公告读取失败，请刷新重试。'
      : '当前没有更新公告。',
  filters: [],
  key: '平台中心|更新公告',
  label: '更新公告',
  listScopes: [{ id: 'all', label: '全部公告', count: totalCount.value }],
  listTabs: [{ id: 'all', label: '公告' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'platform-update-logs',
  pageSize,
  panels: [],
  path: '/platform/update-logs',
  primaryAction: canManage.value ? '发布公告' : '更新公告',
  replication: { status: 'implemented', referenceUrl: '/platform/update-logs' },
  routeName: 'platform-update-logs',
  rows: rows.value,
  searchPlaceholder: '搜索版本 / 标题 / 内容',
  stats: [
    { key: 'total', label: '公告总数', value: String(totalCount.value), tone: 'primary' },
    { key: 'page', label: '本页公告', value: String(items.value.length), tone: 'neutral' },
    { key: 'publish', label: '发布通知', value: '创建时通知全体用户', tone: 'warning' },
  ],
  surface: 'list',
  tabs: [],
  tools: [],
}));

const remotePagination = computed(() => ({
  currentPage: currentPage.value,
  pageSize,
  searchQuery: searchQuery.value,
  totalCount: totalCount.value,
}));

let activeRequest: AbortController | undefined;
let requestVersion = 0;
let searchTimer: number | undefined;

async function loadItems(page = currentPage.value): Promise<boolean> {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const version = ++requestVersion;
  currentPage.value = Math.max(1, page);
  loading.value = true;
  loadError.value = '';
  try {
    const result = await api.getPaged(
      {
        keyword: searchQuery.value.trim() || undefined,
        maxResultCount: pageSize,
        skipCount: (currentPage.value - 1) * pageSize,
      },
      controller.signal,
    );
    if (version !== requestVersion) return false;
    totalCount.value = result.totalCount;
    const lastPage = Math.max(1, Math.ceil(result.totalCount / pageSize));
    if (currentPage.value > lastPage) return loadItems(lastPage);
    items.value = result.items;
    return true;
  } catch (error) {
    if (version !== requestVersion || (error instanceof Error && error.name === 'AbortError')) return false;
    loadError.value = error instanceof Error ? error.message : '更新公告读取失败，请重试。';
    return false;
  } finally {
    if (version === requestVersion) loading.value = false;
  }
}

function selectedItem(selectedRows: ListPageRow[]): PlatformUpdateLogApi.Item | null {
  if (selectedRows.length !== 1) {
    toast.warning('每次请选择一条更新公告。');
    return null;
  }
  return itemById.value.get(selectedRows[0]?.id ?? '') ?? null;
}

function openCreate() {
  if (!canManage.value) return;
  editingItem.value = null;
  editorError.value = '';
  pendingPublish.value = null;
  editorOpen.value = true;
}

function openEdit(item: PlatformUpdateLogApi.Item) {
  if (!canManage.value) return;
  editingItem.value = item;
  editorError.value = '';
  editorOpen.value = true;
}

function openDelete(item: PlatformUpdateLogApi.Item) {
  if (!canManage.value) return;
  deleteTarget.value = item;
  deleteError.value = '';
  deleteConfirmOpen.value = true;
}

function handleAction(actionId: string, selectedRows: ListPageRow[]) {
  if (actionId === 'refresh') {
    void loadItems();
    return;
  }
  if (!canManage.value) return;
  if (actionId === 'create') {
    openCreate();
    return;
  }
  const item = selectedItem(selectedRows);
  if (!item) {
    if (selectedRows.length === 1) toast.error('所选公告已不在当前列表，请刷新。');
    return;
  }
  if (actionId === 'edit') openEdit(item);
  if (actionId === 'delete') openDelete(item);
}

async function saveItem(input: PlatformUpdateLogApi.SaveInput) {
  if (!canManage.value || saving.value) return;
  const item = editingItem.value;
  if (!item) {
    pendingPublish.value = input;
    publishError.value = '';
    publishConfirmOpen.value = true;
    return;
  }
  saving.value = true;
  editorError.value = '';
  try {
    const updated = await api.update(item.id, input);
    if (!updated) throw new Error('服务端未确认更新。');
    editorOpen.value = false;
    const refreshed = await loadItems();
    const readback = items.value.find((candidate) => candidate.id === item.id);
    if (!refreshed || !readback || !matchesSaveInput(readback, input)) {
      actionError.value = '更新已提交，但列表读回未确认预期内容；请刷新核对。';
      return;
    }
    actionError.value = '';
    toast.success('更新公告已保存并读回。');
  } catch (error) {
    editorError.value = error instanceof Error ? error.message : '更新公告保存失败，请重试。';
  } finally {
    saving.value = false;
  }
}

function matchesSaveInput(item: PlatformUpdateLogApi.Item, input: PlatformUpdateLogApi.SaveInput): boolean {
  return (
    item.version === input.version &&
    item.title === input.title &&
    item.summary === input.summary &&
    (item.impactScope ?? null) === input.impactScope &&
    parseUpdateLogTime(item.publishedAt).getTime() === new Date(input.publishedAt).getTime()
  );
}

async function publishItem() {
  const input = pendingPublish.value;
  if (!canManage.value || !input || saving.value) return;
  saving.value = true;
  publishError.value = '';
  try {
    const created = await api.create(input);
    if (!created) throw new Error('服务端未确认公告发布。');
    publishConfirmOpen.value = false;
    editorOpen.value = false;
    pendingPublish.value = null;
    let readBackConfirmed = false;
    try {
      const result = await api.getPaged({ keyword: input.title, maxResultCount: 100, skipCount: 0 });
      readBackConfirmed = result.items.some((item) => matchesSaveInput(item, input));
    } catch {
      readBackConfirmed = false;
    }
    const refreshed = await loadItems(1);
    actionError.value =
      readBackConfirmed && refreshed
        ? ''
        : '发布接口已确认并通知全体用户，但没有完成公告与列表读回核对；请刷新核对，避免重复发布。';
    if (readBackConfirmed && refreshed) toast.success('更新公告已发布、通知全体用户并从服务端读回。');
  } catch (error) {
    publishError.value = error instanceof Error ? error.message : '发布结果未确认。';
    actionError.value = '发布请求未获成功响应；请先刷新并核对公告是否已创建，再决定是否重新发布。';
    publishConfirmOpen.value = false;
    editorOpen.value = false;
    pendingPublish.value = null;
  } finally {
    saving.value = false;
  }
}

async function confirmDelete() {
  const target = deleteTarget.value;
  if (!canManage.value || !target || deleting.value) return;
  deleting.value = true;
  deleteError.value = '';
  try {
    const deleted = await api.delete(target.id);
    if (!deleted) throw new Error('服务端未确认删除。');
    deleteConfirmOpen.value = false;
    deleteTarget.value = null;
    const refreshed = await loadItems();
    if (!refreshed || items.value.some((item) => item.id === target.id)) {
      actionError.value = '删除已提交，但列表读回未确认；请刷新核对。';
      return;
    }
    actionError.value = '';
    toast.success('更新公告已删除并确认列表读回。');
  } catch (error) {
    deleteError.value = error instanceof Error ? error.message : '更新公告删除失败，请重试。';
  } finally {
    deleting.value = false;
  }
}

watch(
  searchQuery,
  () => {
    currentPage.value = 1;
    if (searchTimer !== undefined) window.clearTimeout(searchTimer);
    searchTimer = window.setTimeout(() => void loadItems(1), 300);
  },
  { flush: 'sync' },
);

onMounted(() => void loadItems(1));
onBeforeUnmount(() => {
  if (searchTimer !== undefined) window.clearTimeout(searchTimer);
  activeRequest?.abort();
});
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    :remote-pagination="remotePagination"
    @action="(action, selectedRows) => handleAction(action.id, selectedRows)"
    @page-change="(page) => void loadItems(page)"
    @search-change="
      (query) => {
        searchQuery = query;
      }
    "
    @row="
      (row) => {
        detailItem = itemById.get(row.id) ?? null;
        detailOpen = detailItem !== null;
      }
    "
  >
    <template #hero-icon><Megaphone :size="20" /></template>
    <template #notice>
      <p>创建公告会向全体用户发送即时通知；编辑不会再次通知。</p>
      <p v-if="!canManage">当前账号可查看公告，没有管理员发布权限。</p>
      <p
        v-if="actionError"
        role="alert"
      >
        {{ actionError }}
      </p>
      <p
        v-if="loadError"
        role="alert"
      >
        {{ loadError }}
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    :open="detailOpen"
    :title="detailItem?.title ?? '更新公告详情'"
    description="公告详情来自平台更新日志接口。"
    size="lg"
    @update:open="detailOpen = $event"
  >
    <template v-if="detailItem">
      <UiFormField label="版本">
        <span>{{ detailItem.version }}</span>
      </UiFormField>
      <UiFormField label="发布时间">
        <span>{{ toUpdateLogRow(detailItem).publishedAt }}</span>
      </UiFormField>
      <UiFormField label="影响范围">
        <span>{{ detailItem.impactScope || '—' }}</span>
      </UiFormField>
      <UiFormField label="发布人">
        <span>{{ detailItem.authorName || '—' }}</span>
      </UiFormField>
      <UiFormField label="更新内容">
        <p>{{ detailItem.summary }}</p>
      </UiFormField>
    </template>
    <template #footer>
      <UiButton
        v-if="canManage && detailItem"
        @click="openEdit(detailItem)"
        >编辑</UiButton
      >
      <UiButton @click="detailOpen = false">关闭</UiButton>
    </template>
  </UiDialog>

  <UpdateLogEditorDialog
    :open="editorOpen"
    :item="editingItem"
    :error="editorError"
    :saving="saving"
    @update:open="editorOpen = $event"
    @submit="saveItem"
  />

  <UiDialog
    :open="publishConfirmOpen"
    title="确认发布更新公告"
    :description="publishDescription"
    @update:open="publishConfirmOpen = $event"
  >
    <p
      v-if="publishError"
      role="alert"
    >
      {{ publishError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="publishConfirmOpen = false"
        >返回修改</UiButton
      >
      <UiButton
        variant="primary"
        :loading="saving"
        :disabled="saving"
        @click="publishItem"
        >确认发布并通知全体用户</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="deleteConfirmOpen"
    title="删除更新公告"
    :description="
      deleteTarget ? `将删除“${deleteTarget.title}”（${deleteTarget.version}）。删除后可从回收站恢复。` : ''
    "
    @update:open="deleteConfirmOpen = $event"
  >
    <p
      v-if="deleteError"
      role="alert"
    >
      {{ deleteError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="deleting"
        @click="deleteConfirmOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="danger"
        :loading="deleting"
        :disabled="deleting"
        @click="confirmDelete"
        >确认删除</UiButton
      >
    </template>
  </UiDialog>
</template>
