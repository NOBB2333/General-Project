<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { createPlatformRecycleBinApi, type PlatformRecycleBinApi } from '@/api/core/platform-recycle-bin';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { toast, UiButton, UiDialog, UiFormField } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import {
  canPermanentlyDeleteRecycleBinEntity,
  canRestoreRecycleBinEntity,
  recycleBinEntityTypes,
  toRecycleBinRow,
  type RecycleBinRow,
} from './recycle-bin.model';

const api = createPlatformRecycleBinApi(apiClient);
const pageSize = 20;
const currentPage = ref(1);
const items = ref<PlatformRecycleBinApi.Item[]>([]);
const totalCount = ref(0);
const selectedEntityType = ref('');
const loading = ref(false);
const loadError = ref('');
const actionError = ref('');
const mutating = ref(false);
const detailOpen = ref(false);
const detailItem = ref<PlatformRecycleBinApi.Item | null>(null);
const confirmationOpen = ref(false);
const confirmationAction = ref<null | 'delete' | 'restore'>(null);
const confirmationItems = ref<PlatformRecycleBinApi.Item[]>([]);
const confirmationError = ref('');
const canManage = computed(() => canAccess('Platform.RecycleBin.Manage'));
const rows = computed(() => items.value.map(toRecycleBinRow));
const rowById = computed(() => new Map(rows.value.map((row, index) => [row.id, items.value[index]])));
const selectedTypeLabel = computed(
  () => recycleBinEntityTypes.find((option) => option.value === selectedEntityType.value)?.label ?? '全部类型',
);
const remoteFilterValues = computed(() => ({ entityType: selectedTypeLabel.value }));
const confirmationTitle = computed(() => (confirmationAction.value === 'delete' ? '永久删除文件' : '恢复回收站对象'));
const confirmationDescription = computed(() => {
  const targets = confirmationItems.value;
  if (confirmationAction.value === 'delete') {
    const target = targets[0];
    return target ? `将永久删除“${target.displayName}”及其存储对象。此操作不可撤销，文件无法恢复。` : '';
  }
  if (targets.length === 1) {
    const target = targets[0];
    return target ? `将恢复“${target.displayName}”到原业务位置。` : '';
  }
  return `将恢复所选的 ${targets.length} 个对象到原业务位置。`;
});

const pageModel = computed<ListPageModel<RecycleBinRow>>(() => ({
  actions: [
    { id: 'reload', label: loading.value ? '读取中' : '刷新', kind: 'refresh' },
    ...(canManage.value
      ? [{ id: 'restore-selected', label: '恢复所选', kind: 'custom' as const, requiresSelection: true }]
      : []),
  ],
  columns: [
    { key: 'displayName', title: '对象名称', width: 220 },
    { key: 'entityTypeLabel', title: '对象类型', width: 130 },
    { key: 'originalLocation', title: '原位置 / 标识', width: 260 },
    { key: 'removedBy', title: '删除人', width: 130 },
    { key: 'deletionTime', title: '删除时间', width: 180 },
  ],
  demoPath: '/platform/recycle-bin',
  description: '按对象类型查看软删除记录；恢复沿用后端支持范围，永久删除只适用于文件。',
  emptyHint: loading.value
    ? '正在读取回收站…'
    : loadError.value
      ? '回收站读取失败，请刷新重试。'
      : '当前筛选下没有回收站记录。',
  filters: [{ key: 'entityType', label: '对象类型', options: recycleBinEntityTypes.map((option) => option.label) }],
  key: '平台中心|回收中心',
  label: '回收中心',
  listScopes: [{ id: 'all', label: '回收站对象', count: totalCount.value }],
  listTabs: [{ id: 'all', label: '全部对象' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'platform-recycle-bin',
  pageSize,
  panels: [],
  path: '/platform/recycle-bin',
  primaryAction: '恢复所选',
  replication: { status: 'implemented', referenceUrl: '/platform/files' },
  routeName: 'platform-recycle-bin',
  rows: rows.value,
  searchPlaceholder: '按对象类型筛选',
  stats: [
    { key: 'total', label: '筛选结果', value: String(totalCount.value), tone: 'primary' },
    { key: 'page', label: '当前页', value: String(items.value.length), tone: 'neutral' },
    { key: 'operation', label: '永久删除范围', value: '仅文件', tone: 'warning' },
  ],
  surface: 'list',
  tabs: [],
  tools: [],
}));

let activeRequest: AbortController | undefined;
let requestVersion = 0;

async function loadItems(): Promise<boolean> {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const version = ++requestVersion;
  loading.value = true;
  loadError.value = '';
  try {
    const result = await api.getItems(
      {
        entityType: selectedEntityType.value || undefined,
        maxResultCount: pageSize,
        skipCount: (currentPage.value - 1) * pageSize,
      },
      controller.signal,
    );
    if (version !== requestVersion) return false;
    totalCount.value = result.totalCount;
    const lastPage = Math.max(1, Math.ceil(result.totalCount / pageSize));
    if (currentPage.value > lastPage) {
      currentPage.value = lastPage;
      await loadItems();
      return true;
    }
    items.value = result.items;
    return true;
  } catch (error) {
    if (version !== requestVersion || (error instanceof Error && error.name === 'AbortError')) return false;
    loadError.value = error instanceof Error ? error.message : '回收站读取失败，请重试。';
    return false;
  } finally {
    if (version === requestVersion) loading.value = false;
  }
}

function updateFilter(key: string, value: string) {
  if (key !== 'entityType') return;
  selectedEntityType.value = recycleBinEntityTypes.find((option) => option.label === value)?.value ?? '';
  currentPage.value = 1;
  void loadItems();
}

function handleAction(actionId: string, selectedRows: ListPageRow[]) {
  if (actionId === 'reload') {
    void loadItems();
    return;
  }
  if (actionId !== 'restore-selected' || !canManage.value) return;
  const targets = selectedRows.flatMap((row) => {
    const item = rowById.value.get(row.id);
    return item && canRestoreRecycleBinEntity(item.entityType) ? [item] : [];
  });
  if (targets.length !== selectedRows.length || targets.length === 0) {
    toast.error('所选对象中包含当前版本不支持恢复的类型。');
    return;
  }
  openConfirmation('restore', targets);
}

function openDetail(row: RecycleBinRow) {
  detailItem.value = rowById.value.get(row.id) ?? null;
  detailOpen.value = detailItem.value !== null;
}

function openConfirmation(action: 'delete' | 'restore', targets: PlatformRecycleBinApi.Item[]) {
  if (!canManage.value || mutating.value || targets.length === 0) return;
  if (
    action === 'delete' &&
    (targets.length !== 1 || !canPermanentlyDeleteRecycleBinEntity(targets[0]?.entityType ?? ''))
  ) {
    toast.error('后端只支持对单个文件执行永久删除。');
    return;
  }
  confirmationAction.value = action;
  confirmationItems.value = targets;
  confirmationError.value = '';
  confirmationOpen.value = true;
}

async function confirmAction() {
  const action = confirmationAction.value;
  const targets = confirmationItems.value;
  if (!action || targets.length === 0 || !canManage.value || mutating.value) return;
  mutating.value = true;
  confirmationError.value = '';
  let completedCount = 0;
  let actionFailure = '';
  try {
    for (const target of targets) {
      try {
        const confirmed =
          action === 'restore'
            ? await api.restore(target.entityType, target.id)
            : canPermanentlyDeleteRecycleBinEntity(target.entityType)
              ? await api.deletePermanently(target.entityType, target.id)
              : false;
        if (!confirmed) throw new Error('服务端未确认本次操作。');
        completedCount += 1;
      } catch (error) {
        actionFailure = error instanceof Error ? error.message : '回收站操作失败。';
        break;
      }
    }

    if (completedCount === 0) {
      confirmationError.value = actionFailure || '没有对象完成操作。';
      return;
    }
    confirmationOpen.value = false;
    detailOpen.value = false;
    const refreshed = await loadItems();
    if (!refreshed) {
      actionError.value = `${completedCount}项操作已提交，但列表刷新失败；请重新读取核对。`;
    } else {
      const stillVisible = targets.some((target) =>
        items.value.some((item) => item.id === target.id && item.entityType === target.entityType),
      );
      if (stillVisible) {
        actionError.value = `${completedCount}项操作已提交，但列表仍显示部分目标；请刷新核对。`;
      } else {
        actionError.value = '';
        toast.success(
          action === 'restore' ? `已恢复${completedCount}项并确认列表读回。` : '文件已永久删除并确认列表读回。',
        );
      }
    }
    if (actionFailure) actionError.value = `已完成${completedCount}项，其余操作失败：${actionFailure}`;
    confirmationItems.value = [];
    confirmationAction.value = null;
  } finally {
    mutating.value = false;
  }
}

function updateConfirmationOpen(open: boolean) {
  if (mutating.value) return;
  confirmationOpen.value = open;
  if (!open) {
    confirmationItems.value = [];
    confirmationAction.value = null;
    confirmationError.value = '';
  }
}

onMounted(() => void loadItems());
onBeforeUnmount(() => activeRequest?.abort());
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    :hide-search="true"
    :remote-filter-values="remoteFilterValues"
    :remote-pagination="{ currentPage, pageSize, searchQuery: '', totalCount }"
    @action="(action, selectedRows) => handleAction(action.id, selectedRows)"
    @filter-change="updateFilter"
    @page-change="
      (page) => {
        currentPage = page;
        void loadItems();
      }
    "
    @row="openDetail"
  >
    <template #notice>
      <p>当前后端支持恢复菜单、字典、文件、调度任务/触发器、更新日志和开放接口应用。</p>
      <p>永久删除只支持单个文件，并会一并删除存储对象；没有批量清空接口。</p>
      <p v-if="!canManage">当前账号没有回收中心管理权限。</p>
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
    :title="detailItem?.displayName ?? '回收站对象详情'"
    description="对象恢复和永久删除范围由后端契约与当前权限控制。"
    @update:open="detailOpen = $event"
  >
    <template v-if="detailItem">
      <UiFormField label="对象类型">
        <span>{{
          rows.find((row) => row.entityId === detailItem?.id && row.entityType === detailItem?.entityType)
            ?.entityTypeLabel ?? detailItem.entityType
        }}</span>
      </UiFormField>
      <UiFormField label="原位置 / 标识">
        <span>{{ detailItem.originalLocation || '—' }}</span>
      </UiFormField>
      <UiFormField label="删除人">
        <span>{{ detailItem.deleterName ?? '—' }}</span>
      </UiFormField>
      <UiFormField label="删除时间">
        <span>{{ detailItem.deletionTime ?? '—' }}</span>
      </UiFormField>
    </template>
    <template #footer>
      <UiButton
        v-if="canManage && detailItem && canRestoreRecycleBinEntity(detailItem.entityType)"
        :disabled="mutating"
        @click="detailItem && openConfirmation('restore', [detailItem])"
        >恢复</UiButton
      >
      <UiButton
        v-if="canManage && detailItem && canPermanentlyDeleteRecycleBinEntity(detailItem.entityType)"
        variant="danger"
        :disabled="mutating"
        @click="detailItem && openConfirmation('delete', [detailItem])"
        >永久删除</UiButton
      >
      <UiButton
        :disabled="mutating"
        @click="detailOpen = false"
        >关闭</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="confirmationOpen"
    :title="confirmationTitle"
    :description="confirmationDescription"
    @update:open="updateConfirmationOpen"
  >
    <p
      v-if="confirmationError"
      role="alert"
    >
      {{ confirmationError }}
    </p>
    <template #footer>
      <UiButton
        :disabled="mutating"
        @click="updateConfirmationOpen(false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="mutating"
        :disabled="mutating"
        @click="confirmAction"
        >{{ confirmationAction === 'delete' ? '确认永久删除' : '确认恢复' }}</UiButton
      >
    </template>
  </UiDialog>
</template>
