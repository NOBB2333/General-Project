<script setup lang="ts">
import { ChevronDown, Download, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiDropdownMenu, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { InquiryStatus } from '../../model/inquiries.types';

defineProps<{ status: '全部' | InquiryStatus; query: string }>();
const emit = defineEmits<{
  'update:status': [value: '全部' | InquiryStatus];
  'update:query': [value: string];
  export: [];
  create: [];
}>();
const statuses: readonly ('全部' | InquiryStatus)[] = [
  '全部',
  '待发布',
  '报价收集中',
  '已完成比价',
  '已转采购单',
  '失效关闭',
];
const exportItems = [
  { id: 'csv', label: '导出 CSV' },
  { id: 'excel', label: '导出 Excel（CSV）' },
  { id: 'pdf', label: '打印为 PDF' },
];
</script>

<template>
  <div :class="$style.toolbar">
    <UiTabs
      :model-value="status"
      @update:model-value="emit('update:status', $event as '全部' | InquiryStatus)"
    >
      <UiTabsList
        :class="$style.tabs"
        label="询价状态"
      >
        <UiTabsTrigger
          v-for="item in statuses"
          :key="item"
          :class="$style.tab"
          :value="item"
          >{{ item }}</UiTabsTrigger
        >
      </UiTabsList>
    </UiTabs>
    <div :class="$style.controls">
      <label :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        />
        <UiInput
          :model-value="query"
          aria-label="搜索询价单"
          placeholder="搜索询价单号/标题/项目..."
          @update:model-value="emit('update:query', String($event))"
        />
      </label>
      <UiDropdownMenu
        :items="exportItems"
        label="导出询价单"
        @select="emit('export')"
      >
        <template #trigger>
          <UiButton size="sm">
            <template #leading
              ><Download
                :size="13"
                :stroke-width="1.8"
            /></template>
            导出
            <template #trailing
              ><ChevronDown
                :size="11"
                :stroke-width="1.8"
            /></template>
          </UiButton>
        </template>
      </UiDropdownMenu>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
      >
        <template #leading
          ><Plus
            :size="13"
            :stroke-width="1.8"
        /></template>
        新建询价单
      </UiButton>
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply flex min-h-[52px] items-end border-b;
  border-color: var(--ui-border);
}
.tabs {
  @apply gap-0;
}
.tab {
  @apply min-h-[43px] rounded-none px-4 py-2.5 text-[13px] font-normal;
  background: transparent !important;
}
.tab[data-state='active'] {
  @apply font-semibold;
  color: var(--app-primary);
}
.tab[data-state='active']::after {
  @apply absolute bottom-0 left-0 right-0 h-0.5;
  content: '';
  background: var(--app-primary);
}
.controls {
  @apply ml-auto flex items-center gap-2 pb-2;
}
.search {
  @apply flex h-8 w-[240px] items-center gap-1.5 rounded px-3;
  color: #6b778c;
  background: #f4f5f7;
}
.search :global(input) {
  @apply h-auto border-0 bg-transparent p-0 shadow-none;
}
@media (max-width: 1180px) {
  .toolbar {
    @apply flex-col items-start overflow-x-auto;
  }
  .controls {
    @apply ml-0 min-w-max;
  }
}
</style>
