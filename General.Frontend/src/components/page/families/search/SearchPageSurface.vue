<script setup lang="ts">
import { Box, ChevronRight, Clock3, ScanSearch, Search } from 'lucide-vue-next';
import { computed, toRef } from 'vue';
import { UiButton, UiDialog, UiInput } from '@/components/ui';
import type { PageRow } from '../../page.types';
import type { SearchPageModel } from './search-page.types';
import { useSearchPageState } from './useSearchPageState';

const props = defineProps<{
  model: SearchPageModel;
  rows?: readonly PageRow[];
}>();
const page = toRef(props, 'model');
const referenceRows = computed(() => (props.rows?.length ? props.rows : page.value.rows));
const searchAction = computed(() => page.value.actions.find((action) => action.id === 'search'));
const clearHistoryAction = computed(() => page.value.actions.find((action) => action.id === 'clear-history'));

const {
  query,
  activeRow,
  detailOpen,
  hasSubmitted,
  visibleRows,
  resultTitle,
  submitSearch,
  submitSuggestion,
  resetSearch,
  clearHistory,
  openDetail,
} = useSearchPageState(page, referenceRows);

function formatValue(value: unknown) {
  return value == null || value === '' ? '—' : String(value);
}

function resultValue(row: PageRow, key: string) {
  return formatValue(row[key]);
}

function optionalResultValue(row: PageRow, key: string | undefined, fallback: string) {
  if (!key) return fallback;
  const value = row[key];
  return value == null || value === '' ? fallback : String(value);
}
</script>

<template>
  <main
    :class="$style.page"
    data-domain-page
    data-module-search
    data-page-family="search"
    :data-page-label="page.label"
    :data-page-module="page.module"
    :data-page-surface="page.surface"
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
          <span :class="$style.titleIcon">
            <ScanSearch
              :size="22"
              :stroke-width="1.8"
            />
          </span>
          <div>
            <h1 :class="$style.title">{{ page.label }}</h1>
            <p :class="$style.description">{{ page.description }}</p>
          </div>
        </div>
      </div>
      <div
        :class="$style.heroPattern"
        aria-hidden="true"
      >
        <span
          v-for="index in 12"
          :key="index"
        />
      </div>
    </header>

    <section :class="$style.searchSurface">
      <form
        :class="$style.searchForm"
        role="search"
        @submit.prevent="submitSearch"
      >
        <Search
          :size="20"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          v-model="query"
          :class="$style.searchInput"
          :placeholder="page.searchPlaceholder"
          :aria-label="page.copy.inputLabel"
        />
        <button
          type="submit"
          :class="$style.visuallyHidden"
        >
          {{ searchAction?.label ?? '搜索' }}
        </button>
      </form>
    </section>

    <section
      v-if="!hasSubmitted"
      :class="$style.searchGuide"
    >
      <Search
        :size="42"
        :stroke-width="1.8"
        aria-hidden="true"
      />
      <h2>{{ page.copy.heading }}</h2>
      <p>{{ page.copy.description }}</p>
      <div :class="$style.suggestions">
        <button
          v-for="suggestion in page.suggestions"
          :key="suggestion"
          type="button"
          :class="$style.suggestionButton"
          @click="submitSuggestion(suggestion)"
        >
          <Search
            :size="12"
            :stroke-width="1.8"
          />
          {{ suggestion }}
        </button>
      </div>
    </section>

    <section
      :class="$style.results"
      aria-live="polite"
    >
      <header :class="$style.resultsHeader">
        <div :class="$style.resultsTitle">
          <Clock3
            :size="15"
            :stroke-width="1.8"
          />
          <h2>{{ resultTitle }}</h2>
        </div>
        <UiButton
          v-if="hasSubmitted"
          variant="link"
          size="xs"
          @click="resetSearch"
          >返回最近查看</UiButton
        >
        <UiButton
          v-else-if="visibleRows.length && clearHistoryAction"
          variant="link"
          size="xs"
          @click="clearHistory"
          >{{ clearHistoryAction.label }}</UiButton
        >
      </header>

      <div
        v-if="visibleRows.length"
        :class="$style.resultList"
      >
        <button
          v-for="row in visibleRows"
          :key="String(row.id)"
          type="button"
          :class="$style.resultItem"
          @click="openDetail(row)"
        >
          <span :class="$style.resultIcon">
            <Box
              :size="16"
              :stroke-width="1.8"
            />
          </span>
          <span :class="$style.resultCopy">
            <span :class="$style.resultPrimary">
              <strong>{{ resultValue(row, page.resultFields.code) }}</strong>
              <span>{{ resultValue(row, page.resultFields.title) }}</span>
              <small>{{
                optionalResultValue(row, page.resultFields.category, page.resultFields.categoryFallback)
              }}</small>
            </span>
          </span>
          <time>{{ optionalResultValue(row, page.resultFields.timestamp, '—') }}</time>
          <ChevronRight
            :size="15"
            :stroke-width="1.8"
            aria-hidden="true"
          />
        </button>
      </div>

      <div
        v-else
        :class="$style.emptyState"
      >
        <Search
          :size="28"
          :stroke-width="1.8"
        />
        <strong>{{ hasSubmitted ? page.copy.notFoundTitle : page.copy.recentEmptyTitle }}</strong>
        <p>{{ hasSubmitted ? page.copy.notFoundDescription : page.copy.recentEmptyDescription }}</p>
        <UiButton
          v-if="hasSubmitted"
          size="sm"
          variant="secondary"
          @click="resetSearch"
          >返回最近查看</UiButton
        >
      </div>
    </section>

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow ? resultValue(activeRow, page.resultFields.title) : page.label"
      :description="
        activeRow ? `${page.copy.detailCodeLabel}：${resultValue(activeRow, page.resultFields.code)}` : page.description
      "
      size="md"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailList"
      >
        <div
          v-for="column in page.columns.filter((item) => item.key !== 'action')"
          :key="column.key"
          :class="$style.detailItem"
        >
          <dt>{{ column.title }}</dt>
          <dd>{{ formatValue(activeRow[column.key]) }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../../styles/index.css";

.page {
  @apply mx-auto flex min-h-full w-full max-w-[1400px] flex-col gap-4;
}

.hero {
  @apply relative flex min-h-[120px] overflow-hidden rounded-lg px-6 py-5 text-white;
  background: linear-gradient(112deg, #1f5f84 0%, #257b98 58%, #18b5cc 100%);
}

.heroCopy {
  @apply relative z-10 min-w-0;
}

.breadcrumb {
  @apply mb-4 flex items-center gap-2 text-[12px];
  color: color-mix(in srgb, white 76%, transparent);
}

.breadcrumb strong {
  color: white;
}

.titleRow {
  @apply flex items-center gap-3;
}

.titleIcon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-lg border;
  border-color: color-mix(in srgb, white 34%, transparent);
  background: color-mix(in srgb, white 14%, transparent);
}

.title {
  @apply text-[22px] font-semibold;
}

.description {
  @apply mt-1 text-[13px];
  color: color-mix(in srgb, white 82%, transparent);
}

.heroPattern {
  @apply absolute -top-7 right-64 grid grid-cols-4 gap-3 opacity-45;
  transform: rotate(0deg);
}

.heroPattern span {
  @apply h-12 w-14 rounded;
  background: color-mix(in srgb, white 72%, transparent);
}

.searchSurface,
.searchGuide,
.results {
  @apply rounded border;
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.searchSurface {
  @apply flex min-h-[106px] items-center justify-center px-6;
}

.searchForm {
  @apply flex h-[62px] w-full max-w-[760px] items-center gap-3 rounded border px-4;
  color: var(--ui-text-subtle);
  background: var(--ui-surface);
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 2px color-mix(in srgb, var(--ui-focus) 24%, transparent);
}

.searchInput {
  @apply h-full min-w-0 flex-1 border-0 px-0 text-[15px] shadow-none;
}

.searchInput:focus {
  border-color: transparent;
  box-shadow: none;
}

.visuallyHidden {
  @apply sr-only;
}

.searchGuide {
  @apply flex min-h-[250px] flex-col items-center justify-center px-4 py-8 text-center;
  color: var(--ui-text-subtle);
}

.searchGuide h2 {
  @apply mt-5 text-[16px] font-semibold;
  color: var(--ui-text);
}

.searchGuide p {
  @apply mt-2 text-[13px];
  color: var(--ui-text-muted);
}

.suggestions {
  @apply mt-5 flex flex-wrap items-center justify-center gap-3;
}

.suggestionButton {
  @apply flex min-h-9 items-center gap-2 rounded px-3 text-[12px] font-semibold transition-colors active:scale-[.97];
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.suggestionButton:hover,
.suggestionButton:focus-visible {
  color: var(--app-primary);
  outline: none;
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.results {
  @apply overflow-hidden;
}

.resultsHeader {
  @apply flex min-h-[54px] items-center justify-between border-b px-5;
  border-color: var(--ui-border);
}

.resultsTitle {
  @apply flex items-center gap-2;
  color: var(--ui-text-muted);
}

.resultsTitle h2 {
  @apply text-[14px] font-semibold;
  color: var(--ui-text);
}

.resultList {
  @apply flex flex-col;
}

.resultItem {
  @apply flex min-h-[62px] w-full items-center gap-3 border-b px-5 text-left transition-colors;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}

.resultItem:last-child {
  border-bottom: 0;
}

.resultItem:hover,
.resultItem:focus-visible {
  background: var(--ui-surface-hover);
  outline: none;
}

.resultIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.resultCopy {
  @apply min-w-0 flex-1;
}

.resultPrimary {
  @apply flex min-w-0 items-center gap-2;
}

.resultPrimary strong {
  @apply shrink-0 text-[13px] font-semibold;
  color: var(--ui-text);
}

.resultPrimary > span {
  @apply truncate text-[13px];
  color: var(--ui-text-muted);
}

.resultPrimary small {
  @apply shrink-0 rounded px-2 py-1 text-[10px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.resultItem time {
  @apply ml-auto shrink-0 text-[11px];
  color: var(--ui-text-subtle);
}

.emptyState {
  @apply flex min-h-48 flex-col items-center justify-center gap-2 px-4 py-10 text-center;
  color: var(--ui-text-muted);
}

.emptyState strong {
  @apply text-[14px] font-medium;
  color: var(--ui-text);
}

.emptyState p {
  @apply m-0 max-w-md text-[12px] leading-5;
}

.detailList {
  @apply m-0 grid gap-2 p-5;
}

.detailItem {
  @apply grid grid-cols-[140px_1fr] gap-3 border-b py-2 text-[13px];
  border-color: var(--ui-border-subtle);
}

.detailItem dt {
  color: var(--ui-text-subtle);
}

.detailItem dd {
  @apply m-0;
  color: var(--ui-text);
}

@media (max-width: 760px) {
  .page {
    @apply gap-3;
  }

  .heroPattern {
    @apply right-0 opacity-20;
  }

  .resultPrimary {
    @apply flex-wrap;
  }

  .resultItem time {
    @apply hidden;
  }
}

@media (prefers-reduced-motion: reduce) {
  .resultItem,
  .suggestionButton {
    transition: none;
  }
}
</style>
