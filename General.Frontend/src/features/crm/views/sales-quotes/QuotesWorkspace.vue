<script setup lang="ts">
import { Download, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput } from '@/components/ui';
import type { QuoteStatus, SalesQuoteRow } from '@/features/crm/model/sales-quotes.types';
import QuotesTable from './QuotesTable.vue';

defineProps<{
  query: string;
  status: QuoteStatus | 'all';
  rows: readonly SalesQuoteRow[];
  counts: Record<'all' | QuoteStatus, number>;
}>();
const emit = defineEmits<{
  'update:query': [value: string];
  'update:status': [value: QuoteStatus | 'all'];
  create: [];
  export: [];
}>();
const tabs: { value: QuoteStatus | 'all'; label: string }[] = [
  { value: 'all', label: '全部' },
  { value: '草稿', label: '草稿' },
  { value: '待审核', label: '待审核' },
  { value: '已发送', label: '已发送' },
  { value: '已接受', label: '已接受' },
  { value: '已拒绝', label: '已拒绝' },
  { value: '已过期', label: '已过期' },
];
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.toolbar">
      <div
        :class="$style.tabs"
        role="tablist"
        aria-label="报价状态"
      >
        <button
          v-for="tab in tabs"
          :key="tab.value"
          type="button"
          role="tab"
          :aria-selected="status === tab.value"
          :class="[$style.tab, status === tab.value && $style.tabActive]"
          @click="emit('update:status', tab.value)"
        >
          {{ tab.label }}<span v-if="tab.value !== 'all'">({{ counts[tab.value] }})</span>
        </button>
      </div>
      <label :class="$style.search">
        <Search :size="14" />
        <UiInput
          :model-value="query"
          :class="$style.searchInput"
          placeholder="搜索报价单号、客户、项目..."
          aria-label="搜索销售报价"
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
        新建报价
      </UiButton>
    </div>
    <QuotesTable :rows="rows" />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workspace {
  @apply mt-4 overflow-hidden rounded-t-lg border bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex h-[40px] min-h-[40px] items-center gap-2 border-b;
  border-color: var(--ui-border);
}
.tabs {
  @apply flex h-[40px] items-end gap-0 self-stretch;
}
.tab {
  @apply flex h-[40px] items-center gap-0.5 whitespace-nowrap border-b-2 border-transparent px-4 text-[12px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.tab:hover {
  color: var(--ui-text);
}
.tabActive {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.search {
  @apply ml-auto flex h-8 min-w-[250px] items-center gap-2 rounded px-3;
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
@media (max-width: 1100px) {
  .toolbar {
    @apply flex-wrap py-2;
  }
  .tabs {
    @apply h-10 w-full overflow-x-auto;
  }
  .search {
    @apply ml-0 flex-1;
  }
}
</style>
