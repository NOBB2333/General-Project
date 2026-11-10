<script setup lang="ts">
import { CalendarDays, Download, Plus, Search, X } from 'lucide-vue-next';
import { ref } from 'vue';
import { UiButton, UiInput, UiPopover, UiSelect, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { purchaseReturnTabs, purchaseReturnTimeOptions } from '../../model/purchase-returns.data';
import type {
  PurchaseReturnRecord,
  PurchaseReturnTab,
  PurchaseReturnTimeFilter,
} from '../../model/purchase-returns.types';
import PurchaseReturnsTable from './PurchaseReturnsTable.vue';

defineProps<{
  rows: readonly PurchaseReturnRecord[];
  activeStatus: PurchaseReturnTab;
  supplier: string;
  query: string;
  supplierOptions: readonly UiSelectOption[];
  timeFilter: PurchaseReturnTimeFilter;
  timeLabel: string;
  startDate: string;
  endDate: string;
  pendingCount: number;
}>();

const emit = defineEmits<{
  'update:activeStatus': [value: PurchaseReturnTab];
  'update:supplier': [value: string];
  'update:query': [value: string];
  'update:timeFilter': [value: PurchaseReturnTimeFilter];
  'update:startDate': [value: string];
  'update:endDate': [value: string];
  open: [row: PurchaseReturnRecord];
  export: [];
  create: [];
}>();

const timeOpen = ref(false);

function selectTime(value: PurchaseReturnTimeFilter) {
  emit('update:timeFilter', value);
  if (value !== 'custom') timeOpen.value = false;
}

function clearTime() {
  emit('update:timeFilter', 'all');
  emit('update:startDate', '');
  emit('update:endDate', '');
  timeOpen.value = false;
}
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.toolbar">
      <UiTabs
        :model-value="activeStatus"
        @update:model-value="emit('update:activeStatus', $event as PurchaseReturnTab)"
      >
        <UiTabsList
          label="退货状态"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            v-for="tab in purchaseReturnTabs"
            :key="tab"
            :value="tab"
          >
            {{ tab }}
            <span
              v-if="tab === '待审核' && pendingCount"
              :class="$style.countBadge"
              >{{ pendingCount }}</span
            >
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>

      <div :class="$style.filters">
        <UiSelect
          :model-value="supplier"
          :options="supplierOptions"
          label="供应商筛选"
          size="sm"
          @update:model-value="emit('update:supplier', $event)"
        />
        <UiPopover
          v-model:open="timeOpen"
          size="md"
          align="end"
        >
          <template #trigger>
            <UiButton
              size="sm"
              :class="timeFilter !== 'all' && $style.activeTime"
            >
              <template #leading
                ><CalendarDays
                  :size="13"
                  :stroke-width="1.8"
              /></template>
              {{ timeFilter === 'all' ? '时间筛选' : timeLabel }}
              <template
                v-if="timeFilter !== 'all'"
                #trailing
              >
                <X
                  :size="11"
                  :stroke-width="1.8"
                  @click.stop="clearTime"
                />
              </template>
            </UiButton>
          </template>
          <div :class="$style.timePopover">
            <strong>按时间筛选</strong>
            <div :class="$style.quickTimes">
              <button
                v-for="option in purchaseReturnTimeOptions.filter((item) => item.value !== 'custom')"
                :key="option.value"
                type="button"
                :class="timeFilter === option.value && $style.quickTimeActive"
                @click="selectTime(option.value)"
              >
                {{ option.label }}
              </button>
            </div>
            <p>自定义范围</p>
            <div :class="$style.dateRange">
              <UiInput
                :model-value="startDate"
                type="date"
                @update:model-value="
                  emit('update:startDate', $event);
                  emit('update:timeFilter', 'custom');
                "
              />
              <span>至</span>
              <UiInput
                :model-value="endDate"
                type="date"
                @update:model-value="
                  emit('update:endDate', $event);
                  emit('update:timeFilter', 'custom');
                "
              />
            </div>
            <footer>
              <button
                type="button"
                @click="clearTime"
              >
                清除
              </button>
              <UiButton
                variant="primary"
                size="xs"
                @click="timeOpen = false"
                >确定</UiButton
              >
            </footer>
          </div>
        </UiPopover>
        <label :class="$style.search">
          <Search
            :size="14"
            :stroke-width="1.8"
          />
          <UiInput
            :model-value="query"
            placeholder="搜索退货单号、供应商..."
            @update:model-value="emit('update:query', $event)"
          />
        </label>
        <UiButton
          size="sm"
          @click="emit('export')"
        >
          <template #leading
            ><Download
              :size="13"
              :stroke-width="1.8" /></template
          >导出
        </UiButton>
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('create')"
        >
          <template #leading
            ><Plus
              :size="13"
              :stroke-width="1.8" /></template
          >新建退货
        </UiButton>
      </div>
    </div>

    <PurchaseReturnsTable
      :rows="rows"
      @open="emit('open', $event)"
    />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workspace {
  @apply overflow-hidden rounded-b-lg border border-t-0 bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-[60px] items-end border-b;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.tabs {
  @apply self-stretch gap-0;
}
.tabs button[data-state] {
  @apply min-h-[54px] rounded-none px-4 text-[13px] font-normal;
  background: transparent;
}
.tabs button[data-state='active'] {
  @apply font-semibold;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}
.countBadge {
  @apply ml-0.5 rounded-full px-1.5 py-0.5 text-[9px] font-bold;
  color: #ff5630;
  background: #ffebe6;
}
.filters {
  @apply ml-auto flex items-center gap-2 pb-2 pr-0;
}
.activeTime {
  color: var(--app-primary);
  border-color: #4c9aff;
  background: var(--app-primary-soft);
}
.search {
  @apply relative block w-[220px];
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.search input {
  @apply h-8 border-0 pl-8 text-[13px];
  background: var(--ui-surface-hover);
  box-shadow: none;
}
.timePopover {
  @apply p-3;
}
.timePopover > strong {
  @apply text-[13px] font-semibold;
}
.quickTimes {
  @apply my-3 grid grid-cols-3 gap-1.5;
}
.quickTimes button {
  @apply rounded px-2.5 py-2 text-[12px] transition-colors;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.quickTimes button:hover {
  background: var(--ui-border-subtle);
}
.quickTimes .quickTimeActive {
  @apply font-semibold text-white;
  background: var(--app-primary);
}
.timePopover > p {
  @apply mb-2 text-[12px] font-semibold;
  color: #5e6c84;
}
.dateRange {
  @apply flex items-center gap-2;
}
.dateRange input {
  @apply h-8 min-w-0 px-2 text-[11px];
}
.dateRange span {
  @apply shrink-0 text-[11px];
  color: var(--ui-text-subtle);
}
.timePopover footer {
  @apply mt-3 flex items-center justify-between;
}
.timePopover footer > button:first-child {
  @apply text-[12px] hover:underline;
  color: var(--ui-text-subtle);
}
@media (max-width: 1280px) {
  .toolbar {
    @apply flex-wrap items-center pt-2;
  }
  .filters {
    @apply ml-0 w-full pb-2;
  }
}
</style>
