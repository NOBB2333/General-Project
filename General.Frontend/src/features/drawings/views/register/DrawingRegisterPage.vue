<script setup lang="ts">
import { Download, Eye, Filter, MoreHorizontal, Plus, RefreshCw, Search } from 'lucide-vue-next';
import { computed, reactive, ref, type Component } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import {
  UiButton,
  UiDialog,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
  type UiSelectOption,
} from '@/components/ui';
import type {
  DrawingRegisterColumn,
  DrawingRegisterFixture,
  DrawingRegisterRow,
} from '../../model/drawing-register.types';
import DrawingHero from '../components/DrawingHero.vue';

const props = defineProps<{
  fixture: DrawingRegisterFixture;
  icon: Component;
  implementation: string;
  route: string;
}>();

const rows = ref<DrawingRegisterRow[]>(cloneReferenceRows(props.fixture.rows));
const activeTab = ref(props.fixture.tabs[0]?.id ?? 'all');
const draftFilters = reactive<Record<string, string>>(initialFilters());
const appliedFilters = ref<Record<string, string>>(initialFilters());
const detailOpen = ref(false);
const editorOpen = ref(false);
const activeRow = ref<DrawingRegisterRow | null>(null);
const newCode = ref('');
const newName = ref('');

const activeTabConfig = computed(() => props.fixture.tabs.find((tab) => tab.id === activeTab.value));
const filteredRows = computed(() =>
  rows.value.filter((row) => {
    const tabValue = activeTabConfig.value?.value;
    if (tabValue && row.values[props.fixture.statusKey] !== tabValue) return false;
    return Object.entries(appliedFilters.value).every(([key, raw]) => {
      const query = raw.trim();
      if (!query || query === '全部') return true;
      if (key === 'keyword') return Object.values(row.values).some((value) => includes(value, query));
      if (key === 'from') return comparableDate(row) >= query;
      if (key === 'to') return comparableDate(row) <= query;
      return includes(row.values[key] ?? '', query);
    });
  }),
);
const firstColumns = computed(() => props.fixture.columns.filter((column) => column.key !== 'action').slice(0, 2));

function initialFilters() {
  return Object.fromEntries(
    props.fixture.filters.map((filter) => [filter.key, filter.type === 'select' ? (filter.options?.[0] ?? '') : '']),
  );
}
function includes(value: string, query: string) {
  return value.toLocaleLowerCase('zh-CN').includes(query.toLocaleLowerCase('zh-CN'));
}
function comparableDate(row: DrawingRegisterRow) {
  return row.values.appliedAt ?? row.values.issuedAt ?? row.values.date ?? '';
}
function optionsFor(values?: readonly string[]): UiSelectOption[] {
  return (values ?? []).map((value) => ({ value, label: value }));
}
function valueOf(row: DrawingRegisterRow, column: DrawingRegisterColumn) {
  return row.values[column.key] || '—';
}
function toneFor(value: string) {
  if (
    value.includes('退回') ||
    value.includes('取消') ||
    value === '有' ||
    value.includes('逾期') ||
    value.includes('绝密')
  )
    return 'danger';
  if (value.includes('待') || value.includes('重要') || value.includes('机密') || value.includes('部分'))
    return 'warning';
  if (
    value.includes('完成') ||
    value.includes('通过') ||
    value.includes('发布') ||
    value.includes('确认') ||
    value.includes('采纳') ||
    value.includes('回执')
  )
    return 'success';
  if (value.includes('执行') || value.includes('会审') || value.includes('评审')) return 'purple';
  if (value.includes('接收') || value.includes('部门') || value.includes('系统')) return 'blue';
  return 'neutral';
}
function applyFilters() {
  appliedFilters.value = { ...draftFilters };
  toast.success(`查询完成，共 ${filteredRows.value.length} 条记录`);
}
function resetFilters() {
  Object.assign(draftFilters, initialFilters());
  appliedFilters.value = initialFilters();
}
function openDetails(row: DrawingRegisterRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function refreshRows() {
  rows.value = cloneReferenceRows(props.fixture.rows);
  activeTab.value = props.fixture.tabs[0]?.id ?? 'all';
  resetFilters();
  toast.success(`${props.fixture.title}已刷新`);
}
function createRecord() {
  if (!newCode.value.trim() || !newName.value.trim()) return;
  const values: Record<string, string> = {};
  for (const column of props.fixture.columns) values[column.key] = '—';
  const [codeColumn, nameColumn] = firstColumns.value;
  if (codeColumn) values[codeColumn.key] = newCode.value.trim();
  if (nameColumn) values[nameColumn.key] = newName.value.trim();
  values[props.fixture.statusKey] = props.fixture.tabs.find((tab) => tab.value)?.value ?? '草稿';
  rows.value = [{ id: `local-${Date.now()}`, values }, ...rows.value];
  editorOpen.value = false;
  toast.success(`${newCode.value} 已保存到当前本地会话`);
  newCode.value = '';
  newName.value = '';
}
function exportRows() {
  const columns = props.fixture.columns.filter((column) => column.key !== 'action');
  const csv = [
    columns.map((column) => column.label),
    ...filteredRows.value.map((row) => columns.map((column) => row.values[column.key] ?? '')),
  ]
    .map((line) => line.map((value) => `"${value.replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = `${props.implementation}.csv`;
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${filteredRows.value.length} 条记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    :data-page-implementation="implementation"
    :data-page-route="route"
  >
    <DrawingHero
      :title="fixture.title"
      :description="fixture.description"
      :icon="icon"
      :variant="fixture.variant"
    />

    <section
      v-if="fixture.metrics?.length"
      :class="$style.metrics"
      aria-label="图纸统计"
    >
      <article
        v-for="metric in fixture.metrics"
        :key="metric.id"
        :data-tone="metric.tone"
      >
        <small>{{ metric.label }}</small
        ><strong>{{ metric.value }}</strong>
      </article>
    </section>

    <section
      v-if="fixture.layout === 'filter-first'"
      :class="$style.filterCard"
    >
      <header>
        <h2>
          <Filter
            :size="15"
            :stroke-width="1.8"
          />筛选条件
        </h2>
        <div :class="$style.headerActions">
          <UiButton
            size="sm"
            @click="exportRows"
            ><Download
              :size="14"
              :stroke-width="1.8"
            />导出</UiButton
          ><UiButton
            size="sm"
            @click="refreshRows"
            ><RefreshCw
              :size="14"
              :stroke-width="1.8"
            />刷新</UiButton
          ><UiButton
            variant="primary"
            size="sm"
            @click="editorOpen = true"
            ><Plus
              :size="14"
              :stroke-width="1.8"
            />{{ fixture.primaryAction }}</UiButton
          >
        </div>
      </header>
      <div :class="$style.filters">
        <label
          v-for="filter in fixture.filters"
          :key="filter.key"
          ><span>{{ filter.label }}</span>
          <UiSelect
            v-if="filter.type === 'select'"
            v-model="draftFilters[filter.key]"
            :options="optionsFor(filter.options)"
            :label="filter.label"
          />
          <UiInput
            v-else
            v-model="draftFilters[filter.key]"
            :type="filter.type"
            :placeholder="filter.placeholder ?? ''"
          />
        </label>
      </div>
      <footer>
        <UiButton
          variant="link"
          size="sm"
          >展开更多筛选</UiButton
        ><UiButton
          variant="primary"
          size="sm"
          @click="applyFilters"
          ><Search
            :size="14"
            :stroke-width="1.8"
          />查询</UiButton
        ><UiButton
          size="sm"
          @click="resetFilters"
          >重置</UiButton
        >
      </footer>
    </section>

    <div :class="$style.tabToolbar">
      <UiTabs v-model="activeTab"
        ><UiTabsList
          :class="$style.tabs"
          :label="`${fixture.title}状态`"
          ><UiTabsTrigger
            v-for="tab in fixture.tabs"
            :key="tab.id"
            :class="$style.tab"
            :value="tab.id"
            >{{ tab.label }}</UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      >
      <div
        v-if="fixture.layout !== 'filter-first'"
        :class="$style.headerActions"
      >
        <UiButton
          size="sm"
          @click="exportRows"
          ><Download
            :size="14"
            :stroke-width="1.8"
          />导出</UiButton
        ><UiButton
          size="sm"
          @click="refreshRows"
          ><RefreshCw
            :size="14"
            :stroke-width="1.8"
          />刷新</UiButton
        ><UiButton
          variant="primary"
          size="sm"
          @click="editorOpen = true"
          ><Plus
            :size="14"
            :stroke-width="1.8"
          />{{ fixture.primaryAction }}</UiButton
        >
      </div>
    </div>

    <section
      v-if="fixture.layout !== 'filter-first'"
      :class="$style.filterCard"
    >
      <header v-if="fixture.layout === 'tabs-first'">
        <h2>
          <Filter
            :size="15"
            :stroke-width="1.8"
          />筛选条件
        </h2>
      </header>
      <div :class="$style.filters">
        <label
          v-for="filter in fixture.filters"
          :key="filter.key"
          ><span>{{ filter.label }}</span>
          <UiSelect
            v-if="filter.type === 'select'"
            v-model="draftFilters[filter.key]"
            :options="optionsFor(filter.options)"
            :label="filter.label"
          />
          <UiInput
            v-else
            v-model="draftFilters[filter.key]"
            :type="filter.type"
            :placeholder="filter.placeholder ?? ''"
          />
        </label>
      </div>
      <footer>
        <UiButton
          variant="link"
          size="sm"
          >展开更多筛选</UiButton
        ><UiButton
          variant="primary"
          size="sm"
          @click="applyFilters"
          ><Search
            :size="14"
            :stroke-width="1.8"
          />查询</UiButton
        ><UiButton
          size="sm"
          @click="resetFilters"
          >重置</UiButton
        >
      </footer>
    </section>

    <section :class="$style.tableCard">
      <div :class="$style.tableScroller">
        <table>
          <thead>
            <tr>
              <th
                v-for="column in fixture.columns"
                :key="column.key"
              >
                {{ column.label }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in filteredRows"
              :key="row.id"
            >
              <td
                v-for="column in fixture.columns"
                :key="column.key"
              >
                <template v-if="column.key === 'action'"
                  ><button
                    :class="$style.viewButton"
                    type="button"
                    @click="openDetails(row)"
                  >
                    <Eye
                      :size="13"
                      :stroke-width="1.8"
                    />查看</button
                  ><button
                    :class="$style.moreButton"
                    type="button"
                    aria-label="更多操作"
                    @click="openDetails(row)"
                  >
                    <MoreHorizontal
                      :size="15"
                      :stroke-width="1.8"
                    /></button
                ></template>
                <button
                  v-else-if="column.presentation === 'primary'"
                  :class="$style.primaryCell"
                  type="button"
                  @click="openDetails(row)"
                >
                  {{ valueOf(row, column) }}
                </button>
                <span
                  v-else-if="column.presentation === 'status'"
                  :class="$style.status"
                  :data-tone="toneFor(valueOf(row, column))"
                  >{{ valueOf(row, column) }}</span
                >
                <span
                  v-else-if="column.presentation === 'danger'"
                  :class="$style.dangerCell"
                  :data-active="toneFor(valueOf(row, column)) === 'danger' || undefined"
                  >{{ valueOf(row, column) }}</span
                >
                <span v-else>{{ valueOf(row, column) }}</span>
              </td>
            </tr>
          </tbody>
        </table>
        <p
          v-if="!filteredRows.length"
          :class="$style.empty"
        >
          当前筛选下没有记录
        </p>
      </div>
      <footer :class="$style.pagination">
        <span>共 {{ filteredRows.length }} 条记录，第 1 / 1 页</span><strong>10 条/页</strong>
      </footer>
    </section>

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.values[fixture.columns[0]?.key ?? ''] ?? fixture.title"
      description="当前记录的完整字段"
      size="lg"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div
          v-for="column in fixture.columns.filter((item) => item.key !== 'action')"
          :key="column.key"
        >
          <dt>{{ column.label }}</dt>
          <dd>{{ activeRow.values[column.key] || '—' }}</dd>
        </div>
      </dl>
      <template #footer
        ><UiButton
          variant="primary"
          size="sm"
          @click="detailOpen = false"
          >关闭</UiButton
        ></template
      >
    </UiDialog>

    <UiDialog
      v-model:open="editorOpen"
      :title="fixture.primaryAction"
      description="新增记录只保存在当前本地会话"
      size="sm"
    >
      <form
        :class="$style.editor"
        @submit.prevent="createRecord"
      >
        <label
          ><span>{{ firstColumns[0]?.label ?? '编号' }}</span
          ><UiInput
            v-model="newCode"
            placeholder="请输入编号"
            autofocus /></label
        ><label
          ><span>{{ firstColumns[1]?.label ?? '名称' }}</span
          ><UiInput
            v-model="newName"
            placeholder="请输入名称"
        /></label>
      </form>
      <template #footer
        ><UiButton
          size="sm"
          @click="editorOpen = false"
          >取消</UiButton
        ><UiButton
          variant="primary"
          size="sm"
          :disabled="!newCode.trim() || !newName.trim()"
          @click="createRecord"
          >保存</UiButton
        ></template
      >
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../../styles/index.css";
.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.metrics {
  @apply mt-4 grid grid-cols-7 gap-3;
}
.metrics article {
  @apply min-h-[74px] rounded border bg-white px-4 py-3;
  border-color: var(--ui-border);
}
.metrics small {
  @apply block text-[11px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-1 block text-[20px];
}
.metrics article[data-tone='blue'] strong {
  color: #0052cc;
}
.metrics article[data-tone='green'] strong {
  color: #00875a;
}
.metrics article[data-tone='orange'] strong {
  color: #ff8b00;
}
.metrics article[data-tone='purple'] strong {
  color: #6554c0;
}
.metrics article[data-tone='red'] strong {
  color: #de350b;
}
.filterCard {
  @apply mt-4 rounded border bg-white;
  border-color: var(--ui-border);
}
.filterCard > header {
  @apply flex h-[46px] items-center justify-between border-b px-4;
  border-color: var(--ui-border);
}
.filterCard h2 {
  @apply flex items-center gap-2 text-[13px] font-semibold;
}
.filters {
  @apply grid grid-cols-5 gap-x-4 gap-y-3 px-4 pb-2 pt-4;
}
.filters label {
  @apply min-w-0;
}
.filters label > span {
  @apply mb-1.5 block text-[10px] font-semibold;
  color: var(--ui-text-subtle);
}
.filters label :global(button) {
  @apply w-full;
}
.filterCard > footer {
  @apply flex items-center gap-2 px-4 pb-4;
}
.filterCard > footer > :first-child {
  @apply mr-auto;
}
.headerActions {
  @apply flex items-center gap-2;
}
.tabToolbar {
  @apply mt-4 flex min-h-[48px] items-center justify-between border-b;
  border-color: var(--ui-border);
}
.tabs {
  @apply gap-0 bg-transparent p-0;
}
.tab {
  @apply h-[48px] rounded-none px-4;
  background: transparent;
}
.tab[data-state='active'] {
  color: #0052cc;
  background: transparent;
  box-shadow: inset 0 -2px 0 #0052cc;
}
.tableCard {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.tableScroller {
  @apply overflow-x-auto;
}
.tableScroller table {
  @apply w-full min-w-[1280px] border-collapse text-left text-[11px];
}
.tableScroller th {
  @apply whitespace-nowrap px-4 py-3 font-semibold;
  color: var(--ui-text-subtle);
  background: #f4f5f7;
}
.tableScroller td {
  @apply whitespace-nowrap border-t px-4 py-3;
  border-color: var(--ui-border-subtle);
}
.tableScroller tbody tr:hover {
  background: #f7faff;
}
.primaryCell,
.viewButton {
  @apply inline-flex items-center gap-1 font-semibold;
  color: #0052cc;
}
.moreButton {
  @apply ml-2 inline-flex p-1;
  color: var(--ui-text-subtle);
}
.status {
  @apply inline-flex rounded px-2 py-1 text-[10px] font-semibold;
  color: #6b778c;
  background: #f4f5f7;
}
.status[data-tone='success'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-tone='warning'] {
  color: #ff8b00;
  background: #fffae6;
}
.status[data-tone='danger'] {
  color: #de350b;
  background: #ffebe6;
}
.status[data-tone='purple'] {
  color: #6554c0;
  background: #eae6ff;
}
.status[data-tone='blue'] {
  color: #0052cc;
  background: #deebff;
}
.dangerCell[data-active] {
  @apply font-semibold;
  color: #de350b;
}
.empty {
  @apply p-10 text-center text-[12px];
  color: var(--ui-text-subtle);
}
.pagination {
  @apply flex h-[48px] items-center justify-between border-t px-4 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.pagination strong {
  @apply rounded border px-3 py-2 font-normal;
  border-color: var(--ui-border);
}
.detailGrid {
  @apply grid grid-cols-3 gap-3 p-5;
}
.detailGrid div {
  @apply rounded border bg-[#FAFBFC] p-3;
  border-color: var(--ui-border-subtle);
}
.detailGrid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.detailGrid dd {
  @apply mt-1 text-[12px] font-semibold;
}
.editor {
  @apply grid gap-4 p-5;
}
.editor label span {
  @apply mb-1.5 block text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
@media (max-width: 1100px) {
  .metrics {
    @apply grid-cols-4;
  }
  .filters {
    @apply grid-cols-3;
  }
}
</style>
