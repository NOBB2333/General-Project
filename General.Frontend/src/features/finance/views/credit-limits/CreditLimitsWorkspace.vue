<script setup lang="ts">
import { Download, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type {
  CreditFreezeFilter,
  CreditLimitRow,
  CreditRiskFilter,
  CreditView,
  PendingCreditRequest,
} from '../../model/credit-limits.types';
import CreditLimitsTable from './CreditLimitsTable.vue';

defineProps<{
  view: CreditView;
  rows: readonly CreditLimitRow[];
  pendingRows: readonly PendingCreditRequest[];
  customerCount: number;
  pendingCount: number;
  riskCounts: Readonly<Record<CreditRiskFilter, number>>;
  freezeCounts: Readonly<Record<CreditFreezeFilter, number>>;
  riskFilter: CreditRiskFilter;
  freezeFilter: CreditFreezeFilter;
  query: string;
}>();

const emit = defineEmits<{
  'update:view': [value: CreditView];
  'update:riskFilter': [value: CreditRiskFilter];
  'update:freezeFilter': [value: CreditFreezeFilter];
  'update:query': [value: string];
  export: [];
  viewRow: [row: CreditLimitRow];
  viewRequest: [row: PendingCreditRequest];
}>();
</script>

<template>
  <section :class="$style.workspace">
    <header :class="$style.viewToolbar">
      <UiTabs
        :model-value="view"
        @update:model-value="emit('update:view', $event as CreditView)"
      >
        <UiTabsList
          label="授信工作台视图"
          :class="$style.viewTabs"
        >
          <UiTabsTrigger
            value="customers"
            :class="$style.viewTab"
            >授信客户 <span>({{ customerCount }})</span></UiTabsTrigger
          >
          <UiTabsTrigger
            value="pending"
            :class="$style.viewTab"
            >待审批授信 <span :class="$style.pendingCount">{{ pendingCount }}</span></UiTabsTrigger
          >
        </UiTabsList>
      </UiTabs>
    </header>

    <div :class="$style.filterToolbar">
      <div
        v-if="view === 'customers'"
        :class="$style.filterGroup"
        role="group"
        aria-label="风险状态"
      >
        <button
          v-for="filter in ['全部', '正常', '预警', '超限'] as const"
          :key="filter"
          type="button"
          :class="[$style.filterButton, riskFilter === filter && $style.filterActive]"
          :aria-pressed="riskFilter === filter"
          @click="emit('update:riskFilter', filter)"
        >
          {{ filter }}<span>({{ riskCounts[filter] }})</span>
        </button>
        <span :class="$style.filterDivider" />
        <button
          v-for="filter in ['全部', '正常', '已冻结'] as const"
          :key="`freeze-${filter}`"
          type="button"
          :class="[$style.freezeButton, freezeFilter === filter && $style.freezeActive]"
          :aria-pressed="freezeFilter === filter"
          @click="emit('update:freezeFilter', filter)"
        >
          {{ filter }}<span v-if="filter !== '全部'">({{ freezeCounts[filter] }})</span>
        </button>
      </div>
      <span
        v-else
        :class="$style.pendingHint"
        >待审批授信由负责人提交，财务审批后才会调整额度</span
      >
      <div :class="$style.actions">
        <UiButton
          variant="secondary"
          size="sm"
          @click="emit('export')"
        >
          <template #leading>
            <Download
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          导出
        </UiButton>
        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <UiInput
            :model-value="query"
            placeholder="搜索客户名称、编号、负责人..."
            aria-label="搜索授信"
            @update:model-value="emit('update:query', $event)"
          />
        </label>
      </div>
    </div>

    <CreditLimitsTable
      :view="view"
      :rows="rows"
      :pending-rows="pendingRows"
      @view-row="emit('viewRow', $event)"
      @view-request="emit('viewRequest', $event)"
    />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.viewToolbar {
  @apply flex min-h-[54px] items-center border-b px-4;
  border-color: var(--ui-border);
}
.viewTabs {
  @apply gap-1;
}
.viewTab {
  @apply min-h-[52px] rounded-none border-b-2 border-transparent px-4 text-[12px];
  color: var(--ui-text-subtle);
}
.viewTab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}
.viewTab span {
  @apply ml-1;
  color: var(--ui-text-subtle);
}
.pendingCount {
  @apply inline-flex h-[19px] min-w-[19px] items-center justify-center rounded-full px-1;
  color: white !important;
  background: var(--ui-warning);
}
.filterToolbar {
  @apply flex min-h-[54px] items-center gap-3 border-b px-4;
  border-color: var(--ui-border);
}
.filterGroup {
  @apply flex items-center gap-1;
}
.filterButton,
.freezeButton {
  @apply min-h-8 rounded px-3 text-[11px] font-semibold transition-colors;
  color: var(--ui-text-subtle);
}
.filterButton:hover,
.freezeButton:hover {
  background: var(--ui-surface-hover);
}
.filterButton span {
  @apply ml-1 font-normal;
}
.filterActive {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.freezeButton {
  @apply border;
  border-color: var(--ui-border);
}
.freezeActive {
  color: var(--app-primary);
  border-color: var(--app-primary);
  background: var(--app-primary-soft);
}
.filterDivider {
  @apply mx-1 h-5 w-px;
  background: var(--ui-border);
}
.pendingHint {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.actions {
  @apply ml-auto flex items-center gap-2;
}
.searchField {
  @apply flex h-8 w-[245px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: transparent;
  background: var(--ui-surface-hover);
}
.searchField:focus-within {
  border-color: var(--app-primary-light);
  background: var(--ui-surface);
}
.searchField input {
  @apply h-full border-0 bg-transparent p-0 shadow-none;
}
</style>
