<script setup lang="ts">
import { ChevronRight, FolderTree, KeyRound, Network, PanelTop, Plus, Search } from 'lucide-vue-next';
import { computed, onMounted } from 'vue';
import { toast, UiButton, UiDialog, UiSelect } from '@/components/ui';
import { appCodeOptions, getMenuCompatibility } from './menu.model';
import PlatformMenuFormDialog from './PlatformMenuFormDialog.vue';
import PlatformMenuTreeTable from './components/PlatformMenuTreeTable.vue';
import { usePlatformMenusPage } from './usePlatformMenusPage';

const page = usePlatformMenusPage();
const {
  appCodeFilter,
  changeType,
  collapseAll,
  confirmDelete,
  deleteSaving,
  deleteTarget,
  descendantCount,
  draft,
  expandAll,
  expandedIds,
  filterOptions,
  formError,
  formMode,
  formOpen,
  formSaving,
  isLoading,
  loadError,
  loadMenus,
  menuTypeOptions,
  metrics,
  openCreate,
  openEdit,
  parentIds,
  parentOptions,
  requestDelete,
  saveMenu,
  savingEnabledId,
  search,
  searchable,
  setEnabled,
  toggleExpanded,
  visibleRows,
} = page;

const metricIcons = { catalog: FolderTree, total: Network, pages: PanelTop, actions: KeyRound } as const;
const displayMetrics = computed(() => metrics.value.map((metric) => ({ ...metric, icon: metricIcons[metric.icon] })));
const componentCompatibility = computed(() => getMenuCompatibility(draft.value));
const deleteDialogOpen = computed({
  get: () => deleteTarget.value !== null,
  set: (isOpen: boolean) => {
    if (!isOpen && !deleteSaving.value) deleteTarget.value = null;
  },
});
const deleteDescription = computed(() => {
  if (!deleteTarget.value) return '';
  const childText = descendantCount.value ? `同时删除 ${descendantCount.value} 个子节点` : '该节点没有子节点';
  return `将删除“${deleteTarget.value.title}”（${deleteTarget.value.name}），${childText}，并清除对应菜单授权关系。此操作不可撤销。`;
});

function showPageHelp() {
  toast.info('菜单修改会清理后端菜单缓存；浏览器刷新后，当前导航按已授权的本地页面白名单重新生成。');
}

onMounted(() => {
  void loadMenus();
});
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="board"
    data-page-route="/platform/menus"
    data-platform-menu-page
  >
    <header :class="$style.pageHeader">
      <div>
        <nav
          :class="$style.breadcrumb"
          aria-label="面包屑"
        >
          <span>平台中心</span>
          <ChevronRight
            :size="12"
            :stroke-width="1.8"
          />
          <strong>菜单管理</strong>
        </nav>
        <h1>菜单管理</h1>
      </div>
      <UiButton
        variant="primary"
        size="md"
        @click="openCreate()"
      >
        <template #leading>
          <Plus
            :size="16"
            :stroke-width="1.8"
          />
        </template>
        新建菜单
      </UiButton>
    </header>

    <section
      :class="$style.metricGrid"
      aria-label="菜单概览"
    >
      <article
        v-for="metric in displayMetrics"
        :key="metric.id"
        :class="$style.metricCard"
        :data-tone="metric.tone"
      >
        <div :class="$style.metricLabel">
          <component
            :is="metric.icon"
            :size="16"
            :stroke-width="1.8"
          />
          <span>{{ metric.label }}</span>
        </div>
        <strong>{{ metric.value }}</strong>
      </article>
    </section>

    <div :class="$style.toolbar">
      <label :class="$style.searchField">
        <Search
          :size="15"
          :stroke-width="1.8"
        />
        <input
          v-model="search"
          type="search"
          aria-label="搜索菜单"
          placeholder="搜索名称、路径、组件、权限码..."
        />
      </label>
      <UiSelect
        v-model="appCodeFilter"
        label="应用筛选"
        :options="filterOptions"
        size="sm"
      />
      <UiButton
        size="sm"
        :disabled="isLoading || parentIds.size === 0"
        @click="expandAll"
        >全部展开</UiButton
      >
      <UiButton
        size="sm"
        :disabled="isLoading || parentIds.size === 0"
        @click="collapseAll"
        >全部折叠</UiButton
      >
      <UiButton
        size="sm"
        :loading="isLoading"
        @click="loadMenus"
        >刷新</UiButton
      >
      <UiButton
        size="sm"
        variant="ghost"
        aria-label="菜单兼容说明"
        @click="showPageHelp"
        >兼容说明</UiButton
      >
    </div>

    <p
      v-if="loadError"
      role="alert"
      aria-live="polite"
    >
      {{ loadError }}
    </p>
    <p
      v-else-if="isLoading && visibleRows.length === 0"
      role="status"
      aria-live="polite"
    >
      正在读取菜单…
    </p>

    <PlatformMenuTreeTable
      :rows="visibleRows"
      :expanded-ids="expandedIds"
      :child-parent-ids="parentIds"
      :searchable="searchable"
      :saving-enabled-id="savingEnabledId"
      @toggle="toggleExpanded"
      @edit="openEdit"
      @create-child="openCreate"
      @toggle-enabled="setEnabled"
      @delete="requestDelete"
    />

    <PlatformMenuFormDialog
      v-model:open="formOpen"
      v-model:draft="draft"
      :app-code-options="appCodeOptions"
      :component-compatibility="componentCompatibility"
      :error="formError"
      :mode="formMode"
      :parent-options="parentOptions"
      :saving="formSaving"
      :type-options="menuTypeOptions"
      @submit="saveMenu"
      @type-change="changeType"
    />

    <UiDialog
      v-model:open="deleteDialogOpen"
      title="确认删除菜单节点"
      :description="deleteDescription"
      size="sm"
    >
      <p role="alert">删除会递归清理所有子节点及角色菜单关联。</p>
      <template #footer>
        <UiButton
          :disabled="deleteSaving"
          @click="deleteDialogOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="danger"
          :loading="deleteSaving"
          @click="confirmDelete"
          >{{ deleteSaving ? '正在删除…' : '确认删除' }}</UiButton
        >
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-w-[960px] pb-6;
  color: var(--ui-text);
}
.pageHeader {
  @apply mb-5 flex items-end justify-between;
}
.breadcrumb {
  @apply mb-2 flex items-center gap-1 text-[11px];
  color: var(--ui-text-subtle);
}
.breadcrumb strong {
  @apply font-semibold;
  color: var(--ui-text-muted);
}
.pageHeader h1 {
  @apply m-0 text-[22px] font-bold leading-8;
}
.metricGrid {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metricCard {
  @apply flex min-h-[100px] flex-col border bg-white px-4 py-4;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.metricLabel {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-text-subtle);
}
.metricCard strong {
  @apply mt-3 text-[23px] font-bold leading-7;
  color: var(--app-primary);
}
.metricCard[data-tone='primary'] .metricLabel {
  color: var(--app-primary);
}
.metricCard[data-tone='purple'] .metricLabel,
.metricCard[data-tone='purple'] strong {
  color: #6554c0;
}
.metricCard[data-tone='success'] .metricLabel,
.metricCard[data-tone='success'] strong {
  color: var(--ui-success);
}
.metricCard[data-tone='warning'] .metricLabel,
.metricCard[data-tone='warning'] strong {
  color: var(--ui-warning);
}
.toolbar {
  @apply mb-3 flex items-center gap-2;
}
.searchField {
  @apply flex h-9 w-[355px] items-center gap-2 border bg-white px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.searchField:focus-within {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 1px var(--ui-focus);
}
.searchField input {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] outline-none;
  color: var(--ui-text);
}
.searchField input::placeholder {
  color: #97a0af;
}

@media (max-width: 1100px) {
  .metricGrid {
    @apply grid-cols-2;
  }
}
</style>
