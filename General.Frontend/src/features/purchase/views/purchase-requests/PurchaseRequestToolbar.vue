<script setup lang="ts">
import { CalendarDays, Download, Plus, Search } from 'lucide-vue-next';
import {
  UiButton,
  UiInput,
  UiPopover,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  type UiSelectOption,
} from '@/components/ui';
import type { PurchaseRequestStatus } from '../../model/purchase-requests.types';

defineProps<{
  status: '全部' | PurchaseRequestStatus;
  supplier: string;
  requestor: string;
  timeRange: string;
  query: string;
  supplierOptions: readonly UiSelectOption[];
  requestorOptions: readonly UiSelectOption[];
}>();
const emit = defineEmits<{
  'update:status': [value: '全部' | PurchaseRequestStatus];
  'update:supplier': [value: string];
  'update:requestor': [value: string];
  'update:timeRange': [value: string];
  'update:query': [value: string];
  export: [];
  create: [];
}>();

const statuses: readonly ('全部' | PurchaseRequestStatus)[] = [
  '全部',
  '草稿',
  '审批中',
  '询价中',
  '比价中',
  '采购中',
  '已完成',
  '已驳回',
];
const timeOptions = [
  { value: 'all', label: '全部时间' },
  { value: 'today', label: '今天' },
  { value: '7d', label: '近7天' },
  { value: 'month', label: '本月' },
  { value: '3m', label: '近三个月' },
] as const;

function timeLabel(value: string) {
  return timeOptions.find((option) => option.value === value)?.label ?? '时间筛选';
}
</script>

<template>
  <div :class="$style.toolbar">
    <UiTabs
      :model-value="status"
      @update:model-value="emit('update:status', $event as '全部' | PurchaseRequestStatus)"
    >
      <UiTabsList
        :class="$style.tabs"
        label="申请状态"
      >
        <UiTabsTrigger
          v-for="item in statuses"
          :key="item"
          :class="$style.tab"
          :value="item"
        >
          {{ item }}
        </UiTabsTrigger>
      </UiTabsList>
    </UiTabs>

    <div :class="$style.controls">
      <UiSelect
        :model-value="supplier"
        :options="supplierOptions"
        label="供应商"
        size="sm"
        @update:model-value="emit('update:supplier', $event)"
      />
      <UiSelect
        :model-value="requestor"
        :options="requestorOptions"
        label="申请人"
        size="sm"
        @update:model-value="emit('update:requestor', $event)"
      />
      <UiPopover
        size="sm"
        align="end"
      >
        <template #trigger>
          <button
            type="button"
            :class="[$style.timeButton, timeRange !== 'all' && $style.timeButtonActive]"
          >
            <CalendarDays
              :size="13"
              :stroke-width="1.8"
            />
            {{ timeRange === 'all' ? '时间筛选' : timeLabel(timeRange) }}
          </button>
        </template>
        <div :class="$style.timeMenu">
          <strong>按时间筛选</strong>
          <button
            v-for="option in timeOptions"
            :key="option.value"
            type="button"
            :class="[$style.timeOption, timeRange === option.value && $style.timeOptionActive]"
            @click="emit('update:timeRange', option.value)"
          >
            {{ option.label }}
          </button>
        </div>
      </UiPopover>
      <label :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        />
        <UiInput
          :model-value="query"
          aria-label="搜索采购申请"
          placeholder="搜索申请编号、标题..."
          @update:model-value="emit('update:query', String($event))"
        />
      </label>
      <UiButton
        size="sm"
        @click="emit('export')"
      >
        <template #leading>
          <Download
            :size="13"
            :stroke-width="1.8"
          />
        </template>
        导出
      </UiButton>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
      >
        <template #leading>
          <Plus
            :size="13"
            :stroke-width="1.8"
          />
        </template>
        新建申请
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
.timeButton {
  @apply flex h-8 items-center gap-1.5 rounded border px-3 text-[12px] transition-colors;
  color: var(--ui-text-muted);
  background: var(--ui-surface);
  border-color: var(--ui-border);
}
.timeButton:hover {
  background: var(--ui-surface-hover);
}
.timeButtonActive {
  @apply font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
  border-color: var(--ui-focus);
}
.timeMenu {
  @apply grid grid-cols-2 gap-1.5 p-3;
}
.timeMenu strong {
  @apply col-span-2 mb-1 text-[13px] font-semibold;
  color: var(--ui-text);
}
.timeOption {
  @apply rounded px-2.5 py-2 text-[12px] transition-colors;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.timeOptionActive {
  @apply font-semibold text-white;
  background: var(--app-primary);
}
.search {
  @apply flex h-8 w-[220px] items-center gap-1.5 rounded px-3;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.search :global(input) {
  @apply h-auto border-0 bg-transparent p-0 shadow-none;
}
@media (max-width: 1240px) {
  .toolbar {
    @apply flex-col items-start gap-2 overflow-x-auto;
  }
  .controls {
    @apply ml-0 min-w-max;
  }
}
</style>
