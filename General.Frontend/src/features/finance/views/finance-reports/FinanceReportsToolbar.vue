<script setup lang="ts">
import {
  CalendarDays,
  Clock3,
  Download,
  FileSpreadsheet,
  Landmark,
  TrendingDown,
  TrendingUp,
  Users,
} from 'lucide-vue-next';
import { UiButton, UiSelect, type UiSelectOption } from '@/components/ui';
import type { FinanceReportView } from '../../model/finance-reports.types';
defineProps<{ view: FinanceReportView; period: string }>();
const emit = defineEmits<{
  'update:view': [value: FinanceReportView];
  'update:period': [value: string];
  schedule: [];
  export: [format: 'PDF' | 'Excel'];
}>();
const views: readonly FinanceReportView[] = [
  '老板驾驶舱',
  '收入分析',
  '成本费用',
  '现金流',
  '项目盈利',
  '合同签约',
  '人力资源支出',
];
const periodOptions: readonly UiSelectOption[] = [
  { value: '本月', label: '本月' },
  { value: '本季', label: '本季' },
  { value: '本年', label: '本年' },
];
</script>

<template>
  <nav
    :class="$style.toolbar"
    aria-label="财务报告视图"
  >
    <div :class="$style.views">
      <button
        v-for="(item, index) in views"
        :key="item"
        type="button"
        :class="view === item && $style.active"
        @click="emit('update:view', item)"
      >
        <TrendingUp
          v-if="index < 2"
          :size="14"
          :stroke-width="1.8"
        /><TrendingDown
          v-else-if="index === 2"
          :size="14"
          :stroke-width="1.8"
        /><Landmark
          v-else-if="index < 6"
          :size="14"
          :stroke-width="1.8"
        /><Users
          v-else
          :size="14"
          :stroke-width="1.8"
        />{{ item }}
      </button>
    </div>
    <div :class="$style.actions">
      <UiSelect
        :model-value="period"
        :options="periodOptions"
        label="报告期间"
        @update:model-value="emit('update:period', $event)"
      />
      <UiButton
        size="sm"
        @click="emit('schedule')"
        ><template #leading
          ><Clock3
            :size="14"
            :stroke-width="1.8" /></template
        >定时发送</UiButton
      >
      <UiButton
        size="sm"
        @click="emit('export', 'PDF')"
        ><template #leading
          ><Download
            :size="14"
            :stroke-width="1.8" /></template
        >导出PDF</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('export', 'Excel')"
        ><template #leading
          ><FileSpreadsheet
            :size="14"
            :stroke-width="1.8" /></template
        >导出Excel</UiButton
      >
    </div>
  </nav>
</template>

<style module>
@reference "../../../../styles/index.css";
.toolbar {
  @apply mt-3 flex h-[52px] items-stretch justify-between border-b;
  border-color: var(--ui-border);
}
.views {
  @apply flex items-stretch;
}
.views button {
  @apply relative flex items-center gap-1.5 px-3 text-[11px] font-semibold transition-colors;
  color: var(--ui-text-subtle);
}
.views button:hover {
  color: var(--app-primary);
}
.views .active {
  color: var(--app-primary);
}
.views .active::after {
  content: '';
  @apply absolute inset-x-0 bottom-0 h-0.5;
  background: var(--app-primary);
}
.actions {
  @apply flex items-center gap-2;
}
.actions > :first-child {
  @apply w-[95px];
}
</style>
