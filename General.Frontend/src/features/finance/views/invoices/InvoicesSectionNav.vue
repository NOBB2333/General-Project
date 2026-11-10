<script setup lang="ts">
import { ChartNoAxesCombined, Download, FileClock, FileDown, FileUp, Files, ReceiptText } from 'lucide-vue-next';
import { UiButton, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { InvoiceDirection, InvoiceRange } from '../../model/invoices.types';

defineProps<{
  activeRange: InvoiceRange;
  pendingTaskCount: number;
}>();

const emit = defineEmits<{
  'update:activeRange': [value: InvoiceRange];
  recognize: [direction: InvoiceDirection];
  export: [];
}>();

const tabs = [
  { value: 'overview', label: '总览', icon: ChartNoAxesCombined },
  { value: 'outbound', label: '销项发票', icon: FileUp },
  { value: 'inbound', label: '进项发票', icon: FileDown },
  { value: 'uninvoiced', label: '未开票汇总', count: 9, icon: Files },
  { value: 'tasks', label: '开票任务', icon: FileClock },
] as const;

const ranges: readonly { value: InvoiceRange; label: string }[] = [
  { value: 'month', label: '本月' },
  { value: 'quarter', label: '本季' },
  { value: 'half-year', label: '半年' },
  { value: 'year', label: '全年' },
];
</script>

<template>
  <header :class="$style.toolbar">
    <UiTabsList
      label="发票业务视图"
      :class="$style.tabs"
    >
      <UiTabsTrigger
        v-for="tab in tabs"
        :key="tab.value"
        :value="tab.value"
        :class="$style.tab"
      >
        <component
          :is="tab.icon"
          :size="14"
          :stroke-width="1.8"
        />
        {{ tab.label }}
        <span
          v-if="tab.value === 'tasks' && pendingTaskCount"
          :class="$style.alertCount"
          >{{ pendingTaskCount }}</span
        >
        <span
          v-else-if="'count' in tab"
          :class="$style.softCount"
          >{{ tab.count }}</span
        >
      </UiTabsTrigger>
    </UiTabsList>

    <div :class="$style.actions">
      <div
        :class="$style.rangeControl"
        role="group"
        aria-label="统计周期"
      >
        <button
          v-for="range in ranges"
          :key="range.value"
          type="button"
          :class="[$style.rangeButton, activeRange === range.value && $style.rangeButtonActive]"
          :aria-pressed="activeRange === range.value"
          @click="emit('update:activeRange', range.value)"
        >
          {{ range.label }}
        </button>
      </div>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('recognize', '销项')"
      >
        <template #leading>
          <ReceiptText
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        销项批量识别
      </UiButton>
      <UiButton
        variant="danger"
        size="sm"
        @click="emit('recognize', '进项')"
      >
        <template #leading>
          <ReceiptText
            :size="14"
            :stroke-width="1.8"
          />
        </template>
        进项批量识别
      </UiButton>
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
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply mt-5 flex min-h-[47px] items-center justify-between gap-4 border-b;
  border-color: var(--ui-border);
}

.tabs {
  @apply min-w-0 gap-1 overflow-x-auto;
  scrollbar-width: thin;
}

.tab {
  @apply min-h-[43px] shrink-0 rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-subtle);
}

.tab:hover,
.tab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
}

.tab[data-state='active'] {
  border-bottom-color: var(--app-primary);
}

.softCount,
.alertCount {
  @apply ml-0.5 inline-flex h-[18px] min-w-[18px] items-center justify-center rounded-full px-1 text-[9px] font-semibold;
}

.softCount {
  color: #ff5630;
  background: #fff0eb;
}

.alertCount {
  @apply text-white;
  background: #ff5630;
}

.actions {
  @apply flex shrink-0 items-center gap-2;
}

.rangeControl {
  @apply mr-1 inline-flex overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.rangeButton {
  @apply min-h-8 border-r px-3 text-[11px] font-semibold transition-colors last:border-r-0;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}

.rangeButton:hover {
  background: var(--ui-surface-hover);
}

.rangeButtonActive {
  @apply text-white;
  background: var(--app-primary);
}

.rangeButtonActive:hover {
  background: var(--app-primary);
}

@media (max-width: 1360px) {
  .toolbar {
    @apply items-end overflow-x-auto;
  }

  .actions {
    @apply pb-2;
  }
}
</style>
