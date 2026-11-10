<script setup lang="ts">
import { computed } from 'vue';
import { Download, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PublicSeaRow, PublicSeaSource, PublicSeaTab } from '@/features/crm/model/public-sea.types';
import PublicSeaTableRow from './PublicSeaTableRow.vue';

const props = defineProps<{
  tab: PublicSeaTab;
  industry: string;
  source: PublicSeaSource | 'all';
  query: string;
  rows: readonly PublicSeaRow[];
  counts: Record<PublicSeaTab, number>;
  industries: readonly string[];
  sources: readonly PublicSeaSource[];
}>();
const emit = defineEmits<{
  'update:tab': [value: PublicSeaTab];
  'update:industry': [value: string];
  'update:source': [value: PublicSeaSource | 'all'];
  'update:query': [value: string];
  create: [];
  export: [];
  follow: [id: string];
  claim: [id: string];
}>();

const tabs: { value: PublicSeaTab; label: string }[] = [
  { value: 'all', label: '全部公海' },
  { value: 'recent', label: '最近释放' },
  { value: 'dormant', label: '长期未跟进' },
  { value: 'valuable', label: '高价值' },
];
const industryOptions = computed<UiSelectOption[]>(() => [
  { value: 'all', label: '全部' },
  ...props.industries.map((value) => ({ value, label: value })),
]);
const sourceOptions = computed<UiSelectOption[]>(() => [
  { value: 'all', label: '全部' },
  ...props.sources.map((value) => ({ value, label: value })),
]);
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.toolbar">
      <div
        :class="$style.tabs"
        role="tablist"
        aria-label="公海客户分类"
      >
        <button
          v-for="item in tabs"
          :key="item.value"
          type="button"
          role="tab"
          :aria-selected="tab === item.value"
          :aria-label="`${item.label} ${counts[item.value]}条`"
          :class="[$style.tab, tab === item.value && $style.tabActive]"
          @click="emit('update:tab', item.value)"
        >
          {{ item.label }}
        </button>
      </div>
      <UiSelect
        :model-value="industry"
        :options="industryOptions"
        label="行业筛选"
        size="sm"
        :class="$style.filterSelect"
        @update:model-value="emit('update:industry', $event)"
      />
      <UiSelect
        :model-value="source"
        :options="sourceOptions"
        label="来源筛选"
        size="sm"
        :class="$style.filterSelect"
        @update:model-value="emit('update:source', $event as PublicSeaSource | 'all')"
      />
      <label :class="$style.search">
        <Search :size="14" />
        <UiInput
          :model-value="query"
          :class="$style.searchInput"
          placeholder="搜索客户名称..."
          aria-label="搜索公海客户"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
      <UiButton
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
        新建公海客户
      </UiButton>
    </div>
    <div :class="$style.scroll">
      <table :class="$style.table">
        <thead>
          <tr>
            <th aria-label="关注" />
            <th>客户名称</th>
            <th>行业</th>
            <th>级别</th>
            <th>联系人</th>
            <th>所在城市</th>
            <th>来源</th>
            <th>预估价值</th>
            <th>释放天数</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <PublicSeaTableRow
            v-for="row in rows"
            :key="row.id"
            :row="row"
            @follow="emit('follow', $event)"
            @claim="emit('claim', $event)"
          />
          <tr v-if="!rows.length">
            <td
              colspan="10"
              :class="$style.empty"
            >
              当前筛选下没有公海客户
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <footer :class="$style.footer">共 {{ rows.length }} 条记录</footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workspace {
  @apply mt-4;
}
.toolbar {
  @apply flex min-h-[48px] items-center gap-2;
}
.tabs {
  @apply flex h-10 items-end gap-0;
}
.tab {
  @apply h-10 whitespace-nowrap border-b-2 border-transparent px-4 text-[12px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.tab:hover {
  color: var(--ui-text);
}
.tabActive {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.filterSelect {
  @apply ml-auto w-[125px];
}
.filterSelect + .filterSelect {
  @apply ml-0;
}
.search {
  @apply flex h-8 w-[230px] items-center gap-2 rounded px-3;
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
.scroll {
  @apply overflow-x-auto rounded-t border bg-white;
  border-color: var(--ui-border);
}
.table {
  @apply w-full min-w-[1260px] border-collapse text-left text-[12px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-[49px] whitespace-nowrap px-4 font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
  border-bottom: 1px solid var(--ui-border);
}
.table th:first-child {
  width: 40px;
}
.table th:nth-child(2) {
  width: 250px;
}
.table th:nth-child(3) {
  width: 180px;
}
.table th:nth-child(4) {
  width: 90px;
}
.table th:nth-child(5),
.table th:nth-child(6) {
  width: 140px;
}
.table th:nth-child(7) {
  width: 150px;
}
.table th:nth-child(8) {
  width: 160px;
}
.table th:nth-child(9) {
  width: 130px;
}
.table th:nth-child(10) {
  width: 90px;
}
.empty {
  @apply h-36 text-center;
  color: var(--ui-text-subtle);
}
.footer {
  @apply flex h-10 items-center rounded-b border border-t-0 bg-white px-5 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
@media (max-width: 1180px) {
  .toolbar {
    @apply overflow-x-auto;
  }
  .tabs,
  .filterSelect,
  .search {
    @apply shrink-0;
  }
}
</style>
