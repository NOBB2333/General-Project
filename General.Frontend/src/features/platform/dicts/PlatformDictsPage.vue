<script setup lang="ts">
import { BookOpen, ChevronRight, Database, ListTree, PackageCheck, Plus, RefreshCw, Tags } from 'lucide-vue-next';
import { computed, onMounted } from 'vue';
import { toast, UiButton, UiDialog, UiInput } from '@/components/ui';
import DictDataTable from './components/DictDataTable.vue';
import DictTypeTable from './components/DictTypeTable.vue';
import PlatformDictDataDialog from './PlatformDictDataDialog.vue';
import PlatformDictTypeDialog from './PlatformDictTypeDialog.vue';
import { displayDictText, getDictTagTone } from './dict.model';
import { usePlatformDictsPage } from './usePlatformDictsPage';

const page = usePlatformDictsPage();
const {
  confirmDeleteData,
  confirmDeleteType,
  dataDraft,
  dataFormError,
  dataFormMode,
  dataFormOpen,
  dataLoadError,
  dataSaving,
  dataSearch,
  deleteDataTarget,
  deleteTypeTarget,
  deletingData,
  deletingType,
  filteredData,
  filteredTypes,
  isDataLoading,
  isLoading,
  loadWorkspace,
  metrics,
  openCreateData,
  openCreateType,
  openEditData,
  openEditType,
  optionLoadError,
  publicItems,
  requestDeleteData,
  requestDeleteType,
  saveData,
  saveType,
  savingEnabledId,
  selectType,
  selectedType,
  selectedTypeId,
  setDataEnabled,
  typeDraft,
  typeFormError,
  typeFormMode,
  typeFormOpen,
  typeLoadError,
  typeSaving,
  typeSearch,
} = page;

const metricIcons = { types: ListTree, items: Database, options: PackageCheck, disabled: Tags } as const;
const displayMetrics = computed(() => metrics.value.map((metric) => ({ ...metric, icon: metricIcons[metric.id] })));
const deleteTypeDialogOpen = computed({
  get: () => deleteTypeTarget.value !== null,
  set: (open: boolean) => {
    if (!open && !deletingType.value) deleteTypeTarget.value = null;
  },
});
const deleteDataDialogOpen = computed({
  get: () => deleteDataTarget.value !== null,
  set: (open: boolean) => {
    if (!open && !deletingData.value) deleteDataTarget.value = null;
  },
});
const deleteTypeDescription = computed(() => {
  const target = deleteTypeTarget.value;
  if (!target) return '';
  return `将删除“${displayDictText(target.name)}”（${target.code}）及其 ${target.dataCount} 条字典项。此操作不可撤销。`;
});
const deleteDataDescription = computed(() => {
  const target = deleteDataTarget.value;
  if (!target) return '';
  return `将从“${displayDictText(selectedType.value?.name)}”中删除“${displayDictText(target.label)}”（${displayDictText(target.value)}）。此操作不可撤销。`;
});

function showHelp() {
  toast.info('维护列表包含启用和停用的字典项；业务选项通过批量接口读取，停用项不会返回。');
}

onMounted(() => {
  void loadWorkspace();
});
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="board"
    data-page-route="/platform/dicts"
    data-platform-dicts-page
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
          <strong>字典管理</strong>
        </nav>
        <h1>字典管理</h1>
      </div>
      <UiButton
        variant="primary"
        size="md"
        @click="openCreateType"
      >
        <template #leading>
          <Plus
            :size="16"
            :stroke-width="1.8"
          />
        </template>
        新建字典类型
      </UiButton>
    </header>

    <section
      :class="$style.metricGrid"
      aria-label="字典概览"
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

    <p
      v-if="typeLoadError"
      :class="$style.pageError"
      role="alert"
      aria-live="polite"
    >
      {{ typeLoadError }}
    </p>

    <section :class="$style.contentGrid">
      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <div>
            <div :class="$style.panelTitle">
              <ListTree
                :size="16"
                :stroke-width="1.8"
              />
              <h2>字典类型</h2>
            </div>
            <p>编码不可变更；删除类型会同时删除其字典项。</p>
          </div>
          <UiButton
            size="sm"
            :disabled="isLoading"
            @click="openCreateType"
            >新建</UiButton
          >
        </header>
        <label :class="$style.searchField">
          <BookOpen
            :size="14"
            :stroke-width="1.8"
          />
          <UiInput
            v-model="typeSearch"
            aria-label="搜索字典类型"
            placeholder="搜索名称、编码或备注"
          />
        </label>
        <p
          v-if="isLoading && filteredTypes.length === 0"
          :class="$style.statusText"
          role="status"
        >
          正在读取字典类型…
        </p>
        <DictTypeTable
          :rows="filteredTypes"
          :selected-id="selectedTypeId"
          @select="selectType"
          @edit="openEditType"
          @delete="requestDeleteType"
        />
      </article>

      <article :class="$style.panel">
        <header :class="$style.panelHeader">
          <div>
            <div :class="$style.panelTitle">
              <Database
                :size="16"
                :stroke-width="1.8"
              />
              <h2>{{ selectedType ? displayDictText(selectedType.name) : '字典项' }}</h2>
            </div>
            <p>
              <code>{{ selectedType?.code ?? '请选择字典类型' }}</code>
              <span v-if="selectedType">
                · {{ selectedType.enabledDataCount }} 项启用 / {{ selectedType.dataCount }} 项总计</span
              >
            </p>
          </div>
          <div :class="$style.panelActions">
            <UiButton
              size="sm"
              variant="secondary"
              :loading="isLoading"
              aria-label="刷新字典"
              @click="loadWorkspace(selectedType?.code)"
            >
              <template #leading>
                <RefreshCw
                  :size="14"
                  :stroke-width="1.8"
                />
              </template>
              刷新
            </UiButton>
            <UiButton
              size="sm"
              variant="primary"
              :disabled="!selectedType || isLoading"
              @click="openCreateData"
              >新增字典项</UiButton
            >
          </div>
        </header>
        <label :class="$style.searchField">
          <BookOpen
            :size="14"
            :stroke-width="1.8"
          />
          <UiInput
            v-model="dataSearch"
            aria-label="搜索字典项"
            placeholder="搜索名称、值、标签或备注"
            :disabled="!selectedType"
          />
        </label>
        <div
          :class="$style.optionPreview"
          data-dict-options-preview
        >
          <div>
            <strong>公共选项</strong>
            <span v-if="!optionLoadError">批量接口返回 {{ publicItems.length }} 项启用值</span>
          </div>
          <span
            v-if="optionLoadError"
            :class="$style.optionError"
            role="status"
            >{{ optionLoadError }}</span
          >
          <span
            v-else-if="publicItems.length === 0"
            :class="$style.statusText"
            >无启用选项</span
          >
          <div
            v-else
            :class="$style.optionList"
          >
            <span
              v-for="item in publicItems.slice(0, 5)"
              :key="item.value"
              :class="$style.optionChip"
              :data-tone="getDictTagTone(item.tagColor)"
              :title="item.value"
            >
              {{ displayDictText(item.label) }}
            </span>
            <span
              v-if="publicItems.length > 5"
              :class="$style.moreOptions"
              >+{{ publicItems.length - 5 }}</span
            >
          </div>
        </div>
        <p
          v-if="dataLoadError"
          :class="$style.pageError"
          role="alert"
          aria-live="polite"
        >
          {{ dataLoadError }}
        </p>
        <p
          v-else-if="isDataLoading"
          :class="$style.statusText"
          role="status"
        >
          正在读取字典项…
        </p>
        <DictDataTable
          :rows="filteredData"
          :saving-id="savingEnabledId"
          @edit="openEditData"
          @delete="requestDeleteData"
          @toggle-enabled="setDataEnabled"
        />
      </article>
    </section>

    <PlatformDictTypeDialog
      v-model:open="typeFormOpen"
      v-model:draft="typeDraft"
      :error="typeFormError"
      :mode="typeFormMode"
      :saving="typeSaving"
      @submit="saveType"
    />
    <PlatformDictDataDialog
      v-model:open="dataFormOpen"
      v-model:draft="dataDraft"
      :error="dataFormError"
      :mode="dataFormMode"
      :saving="dataSaving"
      @submit="saveData"
    />

    <UiDialog
      v-model:open="deleteTypeDialogOpen"
      title="确认删除字典类型"
      :description="deleteTypeDescription"
      size="sm"
    >
      <p role="alert">删除会同时清理关联的所有字典项，并影响引用这些字典值的下拉选项。</p>
      <template #footer>
        <UiButton
          :disabled="deletingType"
          @click="deleteTypeDialogOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="danger"
          :loading="deletingType"
          @click="confirmDeleteType"
          >{{ deletingType ? '正在删除…' : '确认删除' }}</UiButton
        >
      </template>
    </UiDialog>
    <UiDialog
      v-model:open="deleteDataDialogOpen"
      title="确认删除字典项"
      :description="deleteDataDescription"
      size="sm"
    >
      <template #footer>
        <UiButton
          :disabled="deletingData"
          @click="deleteDataDialogOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="danger"
          :loading="deletingData"
          @click="confirmDeleteData"
          >{{ deletingData ? '正在删除…' : '确认删除' }}</UiButton
        >
      </template>
    </UiDialog>
    <footer :class="$style.pageFooter">
      <UiButton
        size="sm"
        variant="ghost"
        @click="showHelp"
        >字典使用说明</UiButton
      >
    </footer>
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
  @apply flex min-h-[94px] flex-col border bg-white px-4 py-4;
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
.contentGrid {
  @apply grid items-start gap-3;
  grid-template-columns: minmax(340px, 0.9fr) minmax(540px, 1.4fr);
}
.panel {
  @apply min-w-0 border bg-white p-4;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.panelHeader {
  @apply mb-3 flex items-start justify-between gap-3;
}
.panelTitle {
  @apply flex items-center gap-2;
  color: var(--app-primary);
}
.panelTitle h2 {
  @apply m-0 text-[14px] font-semibold;
  color: var(--ui-text);
}
.panelHeader p {
  @apply mb-0 mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.panelHeader code {
  @apply text-[10px];
  color: var(--ui-text-muted);
}
.panelActions {
  @apply flex shrink-0 items-center gap-2;
}
.searchField {
  @apply mb-3 flex h-9 items-center gap-2 border bg-white px-3;
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
.optionPreview {
  @apply mb-3 flex min-h-10 items-center justify-between gap-3 border px-3 py-2;
  border-color: var(--ui-border-subtle);
  border-radius: var(--app-control-radius);
  background: #f4f5f7;
}
.optionPreview > div:first-child {
  @apply flex shrink-0 items-baseline gap-2;
}
.optionPreview strong {
  @apply text-[11px] font-semibold;
  color: var(--ui-text-muted);
}
.optionPreview span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.optionList {
  @apply flex min-w-0 flex-wrap items-center justify-end gap-1;
}
.optionChip {
  @apply rounded-full border bg-white px-2 py-0.5 text-[10px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.optionChip[data-tone='primary'] {
  color: var(--app-primary);
  border-color: #b3d4ff;
}
.optionChip[data-tone='success'] {
  color: #006644;
  border-color: #abf5d1;
}
.optionChip[data-tone='warning'] {
  color: #974f0c;
  border-color: #ffe380;
}
.optionChip[data-tone='danger'] {
  color: #bf2600;
  border-color: #ffbdad;
}
.moreOptions {
  color: var(--ui-text-subtle);
}
.optionError,
.pageError {
  color: var(--ui-danger);
}
.pageError {
  @apply my-2 text-[11px];
}
.statusText {
  @apply my-2 text-[11px];
  color: var(--ui-text-subtle);
}
.pageFooter {
  @apply mt-2 flex justify-end;
}

@media (max-width: 1200px) {
  .metricGrid {
    @apply grid-cols-2;
  }
  .contentGrid {
    @apply grid-cols-1;
  }
}
</style>
