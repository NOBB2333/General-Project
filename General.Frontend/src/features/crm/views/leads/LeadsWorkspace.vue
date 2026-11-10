<script setup lang="ts">
import { computed } from 'vue';
import { Download, Globe2, Plus, Search, UserRound } from 'lucide-vue-next';
import { UiButton, UiCheckbox, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { LeadRow, LeadScope, LeadSource, LeadStatus } from '@/features/crm/model/leads.types';
import LeadTableRow from './LeadTableRow.vue';

const props = defineProps<{
  scope: LeadScope;
  status: LeadStatus | 'all';
  source: LeadSource | 'all';
  query: string;
  rows: readonly LeadRow[];
  selectedIds: readonly string[];
  allVisibleSelected: boolean;
  scopeCounts: Record<LeadScope, number>;
  sources: readonly LeadSource[];
}>();
const emit = defineEmits<{
  'update:scope': [value: LeadScope];
  'update:status': [value: LeadStatus | 'all'];
  'update:source': [value: LeadSource | 'all'];
  'update:query': [value: string];
  select: [id: string, selected: boolean];
  'select-all': [selected: boolean];
  convert: [id: string];
  create: [];
  export: [];
}>();

const statusOptions: UiSelectOption[] = [
  { value: 'all', label: '全部状态' },
  ...['新线索', '已联系', '已确认', '已转化'].map((value) => ({ value, label: value })),
];
const sourceOptions = computed<UiSelectOption[]>(() => [
  { value: 'all', label: '全部来源' },
  ...props.sources.map((value) => ({ value, label: value })),
]);
const headerChecked = computed<boolean | 'indeterminate'>(() => {
  if (props.allVisibleSelected) return true;
  if (props.rows.some((row) => props.selectedIds.includes(row.id))) return 'indeterminate';
  return false;
});
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.scopes">
      <button
        type="button"
        :class="[$style.scope, scope === 'mine' && $style.scopeActive]"
        :aria-pressed="scope === 'mine'"
        @click="emit('update:scope', 'mine')"
      >
        <UserRound :size="14" />我的线索 <span>{{ scopeCounts.mine }}</span>
      </button>
      <button
        type="button"
        :class="[$style.scope, scope === 'public' && $style.scopeActive]"
        :aria-pressed="scope === 'public'"
        @click="emit('update:scope', 'public')"
      >
        <Globe2 :size="14" />线索公海 <span>{{ scopeCounts.public }}</span>
      </button>
    </div>
    <div :class="$style.toolbar">
      <label :class="$style.search">
        <Search :size="14" />
        <UiInput
          :model-value="query"
          :class="$style.searchInput"
          placeholder="搜索公司、联系人、线索号..."
          aria-label="搜索销售线索"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
      <UiSelect
        :model-value="status"
        :options="statusOptions"
        label="线索状态"
        size="sm"
        @update:model-value="emit('update:status', $event as LeadStatus | 'all')"
      />
      <UiSelect
        :model-value="source"
        :options="sourceOptions"
        label="线索来源"
        size="sm"
        @update:model-value="emit('update:source', $event as LeadSource | 'all')"
      />
      <UiButton
        :class="$style.exportButton"
        size="sm"
        @click="emit('export')"
      >
        <template #leading><Download :size="14" /></template>
        导出
      </UiButton>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
      >
        <template #leading><Plus :size="14" /></template>
        新建线索
      </UiButton>
    </div>
    <div :class="$style.bulk">
      <UiCheckbox
        :model-value="headerChecked"
        label="批量选择当前线索"
        @update:model-value="emit('select-all', $event === true)"
      />
      <span>{{ selectedIds.length ? `已选 ${selectedIds.length} 项` : '批量选择' }}</span>
    </div>
    <div :class="$style.scroll">
      <table :class="$style.table">
        <thead>
          <tr>
            <th aria-label="选择" />
            <th>线索号</th>
            <th>公司名称</th>
            <th>联系人</th>
            <th>状态</th>
            <th>来源</th>
            <th>评分</th>
            <th>预估金额</th>
            <th>地区/行业</th>
            <th>更新日期</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <LeadTableRow
            v-for="row in rows"
            :key="row.id"
            :row="row"
            :selected="selectedIds.includes(row.id)"
            @select="(id, selected) => emit('select', id, selected)"
            @convert="emit('convert', $event)"
          />
          <tr v-if="!rows.length">
            <td
              colspan="11"
              :class="$style.empty"
            >
              当前条件下没有线索
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <footer :class="$style.footer">共 {{ rows.length }} 条线索</footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workspace {
  @apply mt-4;
}
.scopes {
  @apply flex gap-2;
}
.scope {
  @apply flex h-9 items-center gap-1.5 rounded border px-4 text-[12px] font-medium transition-colors;
  color: var(--ui-text-subtle);
  background: white;
  border-color: var(--ui-border);
}
.scope:hover {
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}
.scope span {
  @apply rounded-full px-1.5 py-0.5 text-[10px];
  background: var(--ui-surface-hover);
}
.scopeActive,
.scopeActive:hover {
  color: var(--app-primary);
  background: var(--app-primary-soft);
  border-color: var(--app-primary);
}
.toolbar {
  @apply mt-3 flex items-center gap-3;
}
.search {
  @apply flex h-8 w-[380px] items-center gap-2 rounded px-3;
  color: #97a0af;
  background: var(--ui-surface-hover);
}
.searchInput {
  @apply h-auto border-0 bg-transparent p-0;
  box-shadow: none;
}
.searchInput:focus {
  box-shadow: none;
}
.exportButton {
  @apply ml-auto;
}
.bulk {
  @apply mt-3 flex h-8 items-center gap-2 text-[11px] font-medium;
  color: var(--ui-text-subtle);
}
.scroll {
  @apply overflow-x-auto rounded-t border bg-white;
  border-color: var(--ui-border);
}
.table {
  @apply w-full min-w-[1320px] border-collapse text-left text-[12px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-[42px] whitespace-nowrap px-3 font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
  border-bottom: 1px solid var(--ui-border);
}
.table th:first-child {
  width: 42px;
}
.table th:nth-child(2) {
  width: 120px;
}
.table th:nth-child(3) {
  width: 250px;
}
.table th:nth-child(4) {
  width: 170px;
}
.table th:nth-child(5) {
  width: 120px;
}
.table th:nth-child(6),
.table th:nth-child(7) {
  width: 100px;
}
.table th:nth-child(8) {
  width: 140px;
}
.table th:nth-child(9) {
  width: 180px;
}
.table th:nth-child(10) {
  width: 140px;
}
.table th:nth-child(11) {
  width: 90px;
}
.empty {
  @apply h-36 text-center;
  color: var(--ui-text-subtle);
}
.footer {
  @apply flex h-10 items-center rounded-b border border-t-0 bg-white px-4 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
</style>
