<script setup lang="ts" generic="TRow extends ListPageRow">
import { ChevronLeft, ChevronRight, Search } from 'lucide-vue-next';
import { computed, toRef } from 'vue';
import { UiButton, UiCheckbox, UiInput, UiSelect, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PageAction } from '../../page.contracts';
import type { ListPageModel, ListPageRow } from './list-page.types';
import { useListPageState } from './useListPageState';

export interface RemoteListPagePagination {
  currentPage: number;
  pageSize: number;
  searchQuery: string;
  totalCount: number;
}

const props = defineProps<{
  hideSearch?: boolean;
  model: ListPageModel<TRow>;
  remoteFilterValues?: Readonly<Record<string, string>>;
  remotePagination?: RemoteListPagePagination;
}>();
const emit = defineEmits<{
  action: [action: PageAction, rows: TRow[]];
  'filter-change': [key: string, value: string];
  'page-change': [page: number];
  row: [row: TRow];
  'search-change': [query: string];
}>();

const page = toRef(props, 'model');
const {
  query,
  activeScopeId,
  activeViewId,
  activeTabId,
  filterValues,
  selected,
  filteredRows,
  visibleRows,
  selectedCount,
  allVisibleSelected,
  pageNumber,
  pageCount,
  toggleRow,
  toggleAllVisible,
  setActiveScope,
  setActiveView,
  setActiveTab,
  setFilter,
} = useListPageState(page);

function selectOptions(filter: ListPageModel<TRow>['filters'][number]): UiSelectOption[] {
  return filter.options.map((option) => ({ value: option, label: option }));
}

const displayPageNumber = computed(() => props.remotePagination?.currentPage ?? pageNumber.value);
const displayPageCount = computed(() =>
  props.remotePagination
    ? Math.max(1, Math.ceil(props.remotePagination.totalCount / props.remotePagination.pageSize))
    : pageCount.value,
);
const displayTotalCount = computed(() => props.remotePagination?.totalCount ?? filteredRows.value.length);

function updateSearch(value: string | number) {
  const nextQuery = String(value);
  if (props.remotePagination) {
    emit('search-change', nextQuery);
    return;
  }
  query.value = nextQuery;
}

function updateFilter(key: string, value: string) {
  if (props.remoteFilterValues) {
    emit('filter-change', key, value);
    return;
  }
  setFilter(key, value);
}

function changePage(delta: number) {
  if (props.remotePagination) {
    const nextPage = Math.max(1, Math.min(displayPageCount.value, displayPageNumber.value + delta));
    if (nextPage !== displayPageNumber.value) emit('page-change', nextPage);
    return;
  }
  pageNumber.value += delta;
}

function runAction(action: PageAction) {
  if (action.requiresSelection && !selectedCount.value) return;
  emit(
    'action',
    action,
    page.value.rows.filter((row) => selected.value.has(row.id)),
  );
}

function cellValue(row: ListPageRow, key: string) {
  const value = row[key];
  return value === undefined || value === null || value === '' ? '—' : String(value);
}

function progressLevel(value: string) {
  const percentage = Number.parseFloat(value);
  if (!Number.isFinite(percentage) || percentage <= 0) return 'progress0';
  if (percentage <= 25) return 'progress25';
  if (percentage <= 50) return 'progress50';
  if (percentage <= 75) return 'progress75';
  return 'progress100';
}

function statusTone(value: string) {
  if (value.includes('取消') || value.includes('退回')) return 'statusDanger';
  if (value.includes('待')) return 'statusWarning';
  if (value.includes('完成') || value.includes('交付')) return 'statusSuccess';
  if (value.includes('草稿')) return 'statusNeutral';
  return 'statusPrimary';
}

function openRow(row: ListPageRow) {
  emit('row', row as TRow);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    :data-page-route="page.path"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <nav
          :class="$style.breadcrumb"
          aria-label="面包屑"
        >
          <span>{{ page.module }}</span>
          <span aria-hidden="true">›</span>
          <strong>{{ page.label }}</strong>
        </nav>
        <div :class="$style.titleRow">
          <slot name="hero-icon" />
          <div>
            <h1 :class="$style.title">{{ page.label }}</h1>
            <p :class="$style.description">{{ page.description }}</p>
          </div>
        </div>
      </div>
      <slot name="hero-art" />
    </header>

    <section :class="$style.notice">
      <slot name="notice" />
    </section>

    <nav
      :class="$style.scopes"
      aria-label="数据范围"
    >
      <button
        v-for="scope in page.listScopes"
        :key="scope.id"
        type="button"
        :class="[$style.scopeButton, activeScopeId === scope.id && $style.scopeActive]"
        :aria-pressed="activeScopeId === scope.id"
        @click="setActiveScope(scope.id)"
      >
        {{ scope.label }}
        <span v-if="scope.count !== undefined">{{ scope.count }}</span>
      </button>
    </nav>

    <section :class="$style.stats">
      <article
        v-for="stat in page.stats"
        :key="stat.key"
        :class="[$style.statCard, $style[`stat_${stat.tone}`]]"
      >
        <span>{{ stat.label }}</span>
        <strong>{{ stat.value }}</strong>
        <small v-if="stat.hint">{{ stat.hint }}</small>
      </article>
    </section>

    <section :class="$style.listCard">
      <header :class="$style.listToolbar">
        <UiTabs
          :model-value="activeViewId"
          @update:model-value="setActiveView"
        >
          <UiTabsList
            :class="$style.viewTabs"
            label="列表视图"
          >
            <UiTabsTrigger
              v-for="view in page.listViews"
              :key="view.id"
              :class="$style.viewButton"
              :value="view.id"
            >
              {{ view.label }}
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>

        <UiTabs
          :model-value="activeTabId"
          @update:model-value="setActiveTab"
        >
          <UiTabsList
            :class="$style.statusTabs"
            label="状态筛选"
          >
            <UiTabsTrigger
              v-for="tab in page.listTabs"
              :key="tab.id"
              :class="$style.statusButton"
              :value="tab.id"
            >
              {{ tab.label }}
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>

        <label
          v-if="!hideSearch"
          :class="$style.searchField"
        >
          <Search
            :size="14"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <UiInput
            :model-value="props.remotePagination?.searchQuery ?? query"
            :class="$style.searchInput"
            :placeholder="page.searchPlaceholder"
            :aria-label="page.searchPlaceholder"
            @update:model-value="updateSearch"
          />
        </label>

        <div :class="$style.actions">
          <UiButton
            v-for="action in page.actions"
            :key="action.id"
            :variant="action.tone === 'danger' ? 'danger' : action.tone === 'primary' ? 'primary' : 'secondary'"
            size="sm"
            :disabled="action.requiresSelection && !selectedCount"
            @click="runAction(action)"
          >
            {{ action.label }}
          </UiButton>
        </div>
      </header>

      <div
        v-if="page.filters.length"
        :class="$style.filters"
      >
        <UiSelect
          v-for="filter in page.filters"
          :key="filter.key"
          :model-value="remoteFilterValues?.[filter.key] ?? filterValues[filter.key] ?? filter.options[0] ?? ''"
          :options="selectOptions(filter)"
          :label="filter.label"
          size="sm"
          @update:model-value="updateFilter(filter.key, $event)"
        />
      </div>

      <div :class="$style.tableScroller">
        <table :class="$style.table">
          <colgroup>
            <col :class="$style.selectionColumn" />
            <col
              v-for="column in page.columns.filter((item) => item.key !== 'action')"
              :key="column.key"
            />
          </colgroup>
          <thead>
            <tr>
              <th :class="$style.selectionCell">
                <UiCheckbox
                  :model-value="allVisibleSelected"
                  label="选择当前页全部记录"
                  @update:model-value="toggleAllVisible"
                />
              </th>
              <th
                v-for="column in page.columns.filter((item) => item.key !== 'action')"
                :key="column.key"
              >
                {{ column.title }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in visibleRows"
              :key="row.id"
              :class="selected.has(row.id) && $style.rowSelected"
            >
              <td :class="$style.selectionCell">
                <UiCheckbox
                  :model-value="selected.has(row.id)"
                  :label="`选择 ${cellValue(row, page.columns[0]?.key ?? 'id')}`"
                  @update:model-value="toggleRow(row.id)"
                />
              </td>
              <td
                v-for="column in page.columns.filter((item) => item.key !== 'action')"
                :key="column.key"
              >
                <button
                  v-if="column.key === page.columns[0]?.key"
                  type="button"
                  :class="$style.rowLink"
                  @click="openRow(row)"
                >
                  {{ cellValue(row, column.key) }}
                </button>
                <span
                  v-else-if="column.presentation === 'progress'"
                  :class="$style.progressCell"
                >
                  <span :class="$style.progressTrack">
                    <i
                      :class="[
                        $style.progressFill,
                        $style[progressLevel(cellValue(row, column.key))],
                        $style[`progress_${column.progressTone ?? 'primary'}`],
                      ]"
                    />
                  </span>
                  <strong>{{ cellValue(row, column.key) }}</strong>
                </span>
                <span
                  v-else-if="column.presentation === 'status' || column.key === 'status'"
                  :class="[$style.statusTag, $style[statusTone(cellValue(row, column.key))]]"
                  >{{ cellValue(row, column.key) }}</span
                >
                <span v-else>{{ cellValue(row, column.key) }}</span>
              </td>
            </tr>
          </tbody>
        </table>
        <div
          v-if="!visibleRows.length"
          :class="$style.emptyState"
        >
          {{ page.emptyHint }}
        </div>
      </div>

      <footer :class="$style.footer">
        <span>共 {{ displayTotalCount }} 条</span>
        <strong v-if="selectedCount">已选择 {{ selectedCount }} 条</strong>
        <div :class="$style.pagination">
          <UiButton
            variant="ghost"
            size="icon-sm"
            aria-label="上一页"
            :disabled="displayPageNumber <= 1"
            @click="changePage(-1)"
          >
            <ChevronLeft
              :size="14"
              :stroke-width="1.8"
            />
          </UiButton>
          <span>{{ displayPageNumber }} / {{ displayPageCount }}</span>
          <UiButton
            variant="ghost"
            size="icon-sm"
            aria-label="下一页"
            :disabled="displayPageNumber >= displayPageCount"
            @click="changePage(1)"
          >
            <ChevronRight
              :size="14"
              :stroke-width="1.8"
            />
          </UiButton>
        </div>
      </footer>
    </section>
  </main>
</template>

<style module>
@reference "../../../../styles/index.css";

.page {
  @apply mx-auto flex min-h-full w-full max-w-[1400px] flex-col gap-3;
}
.hero {
  @apply relative flex overflow-hidden rounded-lg px-6 py-5 text-white;
  min-height: 120px;
  background: linear-gradient(115deg, #006644 0%, #00875a 62%, #36b37e 100%);
}
.heroCopy {
  @apply relative z-10;
}
.breadcrumb {
  @apply mb-4 flex items-center gap-2 text-[12px];
  color: color-mix(in srgb, white 76%, transparent);
}
.breadcrumb strong {
  color: white;
}
.title {
  @apply text-[22px] font-semibold;
}
.titleRow {
  @apply flex items-center gap-3;
}
.description {
  @apply mt-1 text-[13px];
  color: color-mix(in srgb, white 82%, transparent);
}
.notice {
  @apply min-h-0;
}
.notice:empty {
  display: none;
}
.scopes {
  @apply flex items-center gap-1 rounded border p-1;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}
.scopeButton {
  @apply flex min-h-8 items-center gap-2 rounded px-3 text-[12px] transition-colors;
  color: var(--ui-text-muted);
}
.scopeButton span {
  @apply rounded-full px-1.5 py-0.5 text-[10px];
  background: var(--ui-surface-hover);
}
.scopeActive {
  color: white;
  background: var(--app-primary);
}
.stats {
  @apply grid grid-cols-4 gap-3 xl:grid-cols-8;
}
.statCard {
  @apply flex min-h-[72px] flex-col justify-center rounded border px-3 py-2;
}
.statCard span {
  @apply text-[11px];
  color: var(--ui-text-muted);
}
.statCard strong {
  @apply mt-1 text-[20px] font-semibold;
}
.statCard small {
  @apply mt-0.5 text-[10px] font-normal;
  color: var(--ui-text-subtle);
}
.stat_primary {
  color: var(--app-primary);
  border-color: var(--app-primary-border);
  background: var(--app-primary-soft);
}
.stat_success {
  color: var(--ui-success);
  border-color: color-mix(in srgb, var(--ui-success) 30%, transparent);
  background: var(--ui-success-soft);
}
.stat_warning {
  color: var(--ui-warning);
  border-color: color-mix(in srgb, var(--ui-warning) 35%, transparent);
  background: var(--ui-warning-soft);
}
.stat_danger {
  color: var(--ui-danger);
  border-color: color-mix(in srgb, var(--ui-danger) 30%, transparent);
  background: var(--ui-danger-soft);
}
.stat_neutral {
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.listCard {
  @apply min-h-0 overflow-hidden rounded border;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}
.listToolbar {
  @apply flex min-h-[62px] flex-wrap items-center gap-3 border-b px-4 py-2;
  border-color: var(--ui-border);
}
.viewTabs,
.statusTabs {
  @apply flex items-center;
}
.viewButton,
.statusButton {
  @apply min-h-8 border-b-2 border-transparent px-3 text-[12px] transition-colors;
  color: var(--ui-text-muted);
  background: transparent;
  border-radius: 0;
}
.viewButton[data-state='active'],
.statusButton[data-state='active'] {
  color: var(--app-primary);
  border-color: var(--app-primary);
  font-weight: 600;
  background: transparent;
}
.searchField {
  @apply ml-auto flex min-w-[240px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.searchInput {
  @apply border-0 px-0 shadow-none;
}
.searchInput:focus {
  border-color: transparent;
  box-shadow: none;
}
.actions,
.filters {
  @apply flex items-center gap-2;
}
.filters {
  @apply border-b px-4 py-2;
  border-color: var(--ui-border-subtle);
}
.tableScroller {
  @apply min-h-0 overflow-auto;
}
.table {
  @apply w-full min-w-[1600px] table-fixed border-collapse text-left text-[12px];
}
.selectionColumn {
  width: 48px;
}
.table th {
  @apply sticky top-0 z-10 border-b px-3 py-3 text-[11px] font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.table td {
  @apply border-b px-3 py-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.table tbody tr:hover,
.rowSelected {
  background: var(--ui-primary-soft);
}
.selectionCell {
  @apply text-center;
}
.rowLink {
  @apply font-semibold;
  color: var(--app-primary);
}
.statusTag {
  @apply inline-flex rounded px-2 py-1 font-semibold;
}
.statusPrimary {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.statusWarning {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.statusSuccess {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.statusDanger {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.statusNeutral {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.progressCell {
  @apply flex min-w-[88px] items-center gap-2;
}
.progressCell strong {
  @apply text-[11px] font-semibold;
  color: var(--ui-text-muted);
}
.progressTrack {
  @apply block h-1.5 w-14 overflow-hidden rounded-full;
  background: var(--ui-border-subtle);
}
.progressFill {
  @apply block h-full rounded-full;
}
.progress_primary {
  background: var(--app-primary);
}
.progress_warning {
  background: var(--ui-warning);
}
.progress_success {
  background: var(--ui-success);
}
.progress0 {
  width: 0;
}
.progress25 {
  width: 25%;
}
.progress50 {
  width: 50%;
}
.progress75 {
  width: 75%;
}
.progress100 {
  width: 100%;
}
.emptyState {
  @apply flex min-h-40 items-center justify-center px-4 text-[13px];
  color: var(--ui-text-subtle);
}
.footer {
  @apply flex min-h-11 items-center gap-4 border-t px-4 text-[12px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.footer strong {
  color: var(--app-primary);
}
.pagination {
  @apply ml-auto flex items-center gap-2;
}
</style>
