<script setup lang="ts">
import { BarChart3, ChevronLeft, ChevronRight, LayoutDashboard, Plus, Search } from 'lucide-vue-next';
import { computed, ref, watch } from 'vue';
import { UiButton, UiDialog, UiInput, toast } from '@/components/ui';
import { cloneReferenceRows } from '../../reference';
import type { PageModel, PageRow, StatTone } from '../../page.types';

interface RemotePagination {
  currentPage: number;
  pageSize: number;
  totalCount: number;
}

const props = withDefaults(
  defineProps<{
    loading?: boolean;
    model: PageModel;
    remoteMode?: boolean;
    remotePagination?: RemotePagination;
    remoteRows?: PageRow[];
    remoteSearch?: string;
  }>(),
  { loading: false, remoteMode: false },
);
const emit = defineEmits<{
  'page-change': [page: number];
  row: [row: PageRow];
  'search-change': [query: string];
  tool: [tool: string];
}>();
defineSlots<{
  filters?: () => unknown;
  notice?: () => unknown;
}>();

const rows = ref<PageRow[]>(
  props.remoteMode ? [...(props.remoteRows ?? props.model.rows)] : cloneReferenceRows(props.model.rows),
);
const query = ref(props.remoteSearch ?? '');
const activeRow = ref<PageRow | null>(null);
const detailOpen = ref(false);

watch(
  () => props.remoteRows,
  (value) => {
    if (props.remoteMode) rows.value = [...(value ?? props.model.rows)];
  },
);

watch(
  () => props.remoteSearch,
  (value) => {
    if (props.remoteMode) query.value = value ?? '';
  },
);

const visibleRows = computed(() => {
  if (props.remoteMode) return rows.value;
  const keyword = query.value.trim().toLowerCase();
  if (!keyword) return rows.value;
  return rows.value.filter((row) =>
    Object.values(row).some((value) =>
      String(value ?? '')
        .toLowerCase()
        .includes(keyword),
    ),
  );
});

const lanes = computed(() => {
  const statuses = [...new Set(visibleRows.value.map((row) => String(row.status ?? '待处理')))];
  const visibleStatuses = props.remoteMode ? statuses : statuses.slice(0, 5);
  return visibleStatuses.map((status) => ({
    id: status,
    label: status,
    rows: visibleRows.value.filter((row) => String(row.status ?? '待处理') === status),
  }));
});

function toneClass(tone: StatTone) {
  return {
    primary: 'statPrimary',
    success: 'statSuccess',
    warning: 'statWarning',
    danger: 'statDanger',
    neutral: 'statNeutral',
  }[tone];
}

function cellValue(row: PageRow, key: string) {
  const value = row[key];
  return value === undefined || value === null || value === '' ? '—' : String(value);
}

function rowTitle(row: PageRow) {
  return cellValue(row, 'name') !== '—'
    ? cellValue(row, 'name')
    : cellValue(row, 'title') !== '—'
      ? cellValue(row, 'title')
      : cellValue(row, 'code');
}

function openRow(row: PageRow) {
  if (props.remoteMode) {
    emit('row', row);
    return;
  }
  activeRow.value = row;
  detailOpen.value = true;
}

function updateSearch(value: string | number) {
  query.value = String(value);
  if (props.remoteMode) emit('search-change', query.value);
}

function runTool(tool: string) {
  if (props.remoteMode) {
    emit('tool', tool);
    return;
  }
  if (/新增|新建|发起/.test(tool)) {
    rows.value = [
      {
        id: `${props.model.pageSlug}-local-${Date.now()}`,
        code: `LOCAL-${rows.value.length + 1}`,
        name: `本地${props.model.label}`,
        status: '草稿',
        updatedAt: '2026-08-16 10:00',
      },
      ...rows.value,
    ];
    toast.success(`${props.model.label}本地草稿已创建`);
    return;
  }
  toast.info(`${tool}仅在本地复刻页执行`);
}

const recordCount = computed(() => props.remotePagination?.totalCount ?? visibleRows.value.length);
const pageCount = computed(() =>
  props.remotePagination
    ? Math.max(1, Math.ceil(props.remotePagination.totalCount / props.remotePagination.pageSize))
    : 1,
);

function changePage(delta: number) {
  if (!props.remotePagination) return;
  const nextPage = Math.max(1, Math.min(pageCount.value, props.remotePagination.currentPage + delta));
  if (nextPage !== props.remotePagination.currentPage) emit('page-change', nextPage);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="board"
    :data-page-route="props.model.path"
  >
    <header :class="$style.hero">
      <div :class="$style.heroCopy">
        <span :class="$style.heroIcon"
          ><LayoutDashboard
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <p :class="$style.breadcrumb">{{ props.model.module }} / {{ props.model.label }}</p>
          <h1 :class="$style.title">{{ props.model.label }}</h1>
          <p :class="$style.description">{{ props.model.description }}</p>
        </div>
      </div>
      <div :class="$style.actions">
        <UiButton
          v-for="tool in props.model.tools.slice(0, 4)"
          :key="tool"
          :variant="/新增|新建|发起/.test(tool) ? 'primary' : 'secondary'"
          size="sm"
          @click="runTool(tool)"
          ><Plus
            v-if="/新增|新建|发起/.test(tool)"
            :size="14"
            :stroke-width="1.8"
          />{{ tool }}</UiButton
        >
      </div>
    </header>

    <slot name="notice" />

    <section
      v-if="props.model.stats.length"
      :class="$style.stats"
    >
      <article
        v-for="stat in props.model.stats"
        :key="stat.key"
        :class="[$style.stat, $style[toneClass(stat.tone)]]"
      >
        <span>{{ stat.label }}</span
        ><strong>{{ stat.value }}</strong
        ><small v-if="stat.hint">{{ stat.hint }}</small>
      </article>
    </section>

    <section :class="$style.toolbar">
      <label :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        />
        <UiInput
          :model-value="query"
          :placeholder="props.model.searchPlaceholder"
          @update:model-value="updateSearch"
        />
      </label>
      <span>{{ recordCount }} 条记录</span>
      <slot name="filters" />
      <div
        v-if="props.remotePagination"
        :class="$style.actions"
      >
        <UiButton
          variant="ghost"
          size="sm"
          :disabled="props.loading || props.remotePagination.currentPage <= 1"
          aria-label="上一页"
          @click="changePage(-1)"
        >
          <ChevronLeft :size="14" />
        </UiButton>
        <span>第 {{ props.remotePagination.currentPage }} / {{ pageCount }} 页</span>
        <UiButton
          variant="ghost"
          size="sm"
          :disabled="props.loading || props.remotePagination.currentPage >= pageCount"
          aria-label="下一页"
          @click="changePage(1)"
        >
          <ChevronRight :size="14" />
        </UiButton>
      </div>
    </section>

    <section
      :class="$style.board"
      aria-label="业务看板"
    >
      <article
        v-for="lane in lanes"
        :key="lane.id"
        :class="$style.lane"
      >
        <header :class="$style.laneHeader">
          <strong>{{ lane.label }}</strong
          ><span>{{ lane.rows.length }}</span>
        </header>
        <div :class="$style.laneBody">
          <button
            v-for="row in lane.rows"
            :key="row.id"
            type="button"
            :class="$style.card"
            :disabled="props.loading"
            @click="openRow(row)"
          >
            <span :class="$style.cardCode">{{ cellValue(row, 'code') }}</span
            ><strong>{{ rowTitle(row) }}</strong>
            <p>{{ cellValue(row, 'customer') !== '—' ? cellValue(row, 'customer') : cellValue(row, 'owner') }}</p>
            <small>{{ cellValue(row, 'updatedAt') }}</small>
          </button>
          <div
            v-if="!lane.rows.length"
            :class="$style.empty"
          >
            暂无记录
          </div>
        </div>
      </article>
    </section>

    <section
      v-if="!lanes.length"
      :class="$style.emptyBoard"
    >
      <BarChart3
        :size="28"
        :stroke-width="1.6"
      /><strong>暂无{{ props.model.label }}数据</strong>
    </section>
  </main>

  <UiDialog
    v-if="!props.remoteMode"
    v-model:open="detailOpen"
    :title="activeRow ? rowTitle(activeRow) : `${props.model.label}详情`"
    description="参考记录只读，本地操作只影响当前会话"
  >
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in props.model.columns"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ cellValue(activeRow, column.key) }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.page {
  @apply min-w-0 space-y-4 px-5 py-4 text-[#172B4D];
}
.hero {
  @apply flex min-h-[92px] items-start justify-between gap-6 rounded-lg border border-[#DFE1E6] bg-white px-5 py-4;
}
.heroCopy {
  @apply flex min-w-0 items-start gap-3;
}
.heroIcon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-[#DEEBFF] text-[#0052CC];
}
.breadcrumb {
  @apply text-[11px] text-[#6B778C];
}
.title {
  @apply mt-1 text-[20px] font-semibold;
}
.description {
  @apply mt-1 max-w-[760px] text-[12px] leading-5 text-[#42526E];
}
.actions {
  @apply flex shrink-0 flex-wrap justify-end gap-2;
}
.stats {
  @apply grid grid-cols-4 gap-3;
}
.stat {
  @apply flex min-h-[86px] flex-col rounded border border-[#DFE1E6] bg-white p-3;
}
.stat span {
  @apply text-[11px] text-[#6B778C];
}
.stat strong {
  @apply mt-1 text-[22px] font-semibold;
}
.stat small {
  @apply mt-auto text-[10px] text-[#97A0AF];
}
.statPrimary strong {
  @apply text-[#0052CC];
}
.statSuccess strong {
  @apply text-[#00875A];
}
.statWarning strong {
  @apply text-[#FF8B00];
}
.statDanger strong {
  @apply text-[#DE350B];
}
.statNeutral strong {
  @apply text-[#42526E];
}
.toolbar {
  @apply flex items-center justify-between gap-4 rounded border border-[#DFE1E6] bg-white px-3 py-2 text-[11px] text-[#6B778C];
}
.search {
  @apply flex min-w-[280px] items-center gap-2 text-[#6B778C];
}
.board {
  @apply grid min-h-[420px] grid-cols-4 gap-3 overflow-x-auto pb-2;
}
.lane {
  @apply min-w-[250px] rounded border border-[#DFE1E6] bg-[#F4F5F7];
}
.laneHeader {
  @apply flex h-10 items-center justify-between border-b border-[#DFE1E6] px-3 text-[12px];
}
.laneHeader span {
  @apply rounded-full bg-white px-2 py-0.5 text-[10px] text-[#6B778C];
}
.laneBody {
  @apply space-y-2 p-2;
}
.card {
  @apply flex w-full flex-col items-start rounded border border-[#DFE1E6] bg-white p-3 text-left transition-colors hover:border-[#4C9AFF] hover:bg-[#FAFBFC];
}
.cardCode {
  @apply text-[10px] text-[#6B778C];
}
.card strong {
  @apply mt-1 line-clamp-2 text-[12px];
}
.card p {
  @apply mt-2 text-[11px] text-[#42526E];
}
.card small {
  @apply mt-2 text-[10px] text-[#97A0AF];
}
.empty {
  @apply flex h-20 items-center justify-center text-[11px] text-[#97A0AF];
}
.emptyBoard {
  @apply flex min-h-[320px] flex-col items-center justify-center gap-2 rounded border border-dashed border-[#DFE1E6] bg-white text-[#6B778C];
}
.detailGrid {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 text-[12px];
}
.detailGrid dt {
  @apply text-[#6B778C];
}
.detailGrid dd {
  @apply mt-1 font-medium text-[#172B4D];
}
@media (max-width: 1100px) {
  .stats {
    @apply grid-cols-2;
  }
  .board {
    @apply grid-cols-2;
  }
}
</style>
