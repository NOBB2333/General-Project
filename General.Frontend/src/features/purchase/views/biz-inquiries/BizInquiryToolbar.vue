<script setup lang="ts">
import { Download, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect, UiTabs, UiTabsList, UiTabsTrigger, type UiSelectOption } from '@/components/ui';
import type { BizInquiryRow, BizInquiryStatus } from '../../model/biz-inquiries.types';

const props = defineProps<{
  rows: readonly BizInquiryRow[];
  status: '全部' | BizInquiryStatus;
  query: string;
  salesPerson: string;
  assignedBuyer: string;
  salesOptions: readonly UiSelectOption[];
  buyerOptions: readonly UiSelectOption[];
}>();
const emit = defineEmits<{
  'update:status': [value: '全部' | BizInquiryStatus];
  'update:query': [value: string];
  'update:salesPerson': [value: string];
  'update:assignedBuyer': [value: string];
  export: [];
  create: [];
}>();
const statuses: readonly ('全部' | BizInquiryStatus)[] = [
  '全部',
  '待分配',
  '已分配',
  '报价中',
  '已报价',
  '已完成',
  '已取消',
];
function count(item: '全部' | BizInquiryStatus) {
  return item === '全部' ? props.rows.length : props.rows.filter((row) => row.status === item).length;
}
</script>

<template>
  <section :class="$style.workspace">
    <UiTabs
      :model-value="status"
      @update:model-value="emit('update:status', $event as '全部' | BizInquiryStatus)"
      ><UiTabsList
        :class="$style.tabs"
        label="处理状态"
        ><UiTabsTrigger
          v-for="item in statuses"
          :key="item"
          :class="$style.tab"
          :value="item"
          >{{ item }}<span v-if="count(item)">{{ count(item) }}</span></UiTabsTrigger
        ></UiTabsList
      ></UiTabs
    >
    <div :class="$style.controls">
      <label :class="$style.search"
        ><Search
          :size="14"
          :stroke-width="1.8" /><UiInput
          :model-value="query"
          aria-label="搜索业务询价"
          placeholder="搜索询价单号、标题、客户、销售..."
          @update:model-value="emit('update:query', String($event))"
      /></label>
      <UiSelect
        :model-value="salesPerson"
        :options="salesOptions"
        label="负责销售"
        size="sm"
        @update:model-value="emit('update:salesPerson', $event)"
      />
      <UiSelect
        :model-value="assignedBuyer"
        :options="buyerOptions"
        label="分配采购"
        size="sm"
        @update:model-value="emit('update:assignedBuyer', $event)"
      />
      <span :class="$style.spacer" />
      <UiButton
        size="sm"
        @click="emit('export')"
        ><template #leading
          ><Download
            :size="13"
            :stroke-width="1.8" /></template
        >导出</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
        ><template #leading
          ><Plus
            :size="13"
            :stroke-width="1.8" /></template
        >新建业务询价</UiButton
      >
      <span :class="$style.total">共 {{ rows.length }} 条</span>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply overflow-hidden rounded-t-lg border bg-white;
  border-color: var(--ui-border);
}
.tabs {
  @apply min-h-[46px] gap-0 border-b p-1;
  border-color: var(--ui-border);
}
.tab {
  @apply min-h-9 gap-1 rounded px-3.5 text-[12px] font-normal;
}
.tab span {
  @apply rounded-full px-1.5 py-0.5 text-[10px];
  color: #6b778c;
  background: #ebecf0;
}
.tab[data-state='active'] {
  @apply font-semibold;
  color: #0052cc;
  background: #deebff;
}
.tab[data-state='active'] span {
  @apply text-white;
  background: #0052cc;
}
.controls {
  @apply flex min-h-[74px] items-center gap-2 px-5;
}
.search {
  @apply flex h-9 w-[425px] items-center gap-2 rounded border px-3;
  color: #97a0af;
  border-color: var(--ui-border);
}
.search :global(input) {
  @apply h-auto border-0 bg-transparent p-0 shadow-none;
}
.spacer {
  @apply flex-1;
}
.total {
  @apply ml-1 text-[12px];
  color: #6b778c;
}
</style>
