<script setup lang="ts">
import { ChevronLeft, ChevronRight } from 'lucide-vue-next';
import { UiButton, UiSelect, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { DispatchCalendarMode } from '../../model/dispatch-calendar.types';

defineProps<{
  mode: DispatchCalendarMode;
  periodLabel: string;
  region: string;
  engineer: string;
  regionOptions: readonly UiSelectOption[];
  engineerOptions: readonly UiSelectOption[];
  scheduleCount: number;
  engineerCount: number;
}>();

const emit = defineEmits<{
  'update:mode': [value: DispatchCalendarMode];
  'update:region': [value: string];
  'update:engineer': [value: string];
  previous: [];
  next: [];
  current: [];
}>();

function updateMode(value: string) {
  if (value === 'week' || value === 'biweek' || value === 'month') emit('update:mode', value);
}
</script>

<template>
  <section :class="$style.toolbar">
    <div :class="$style.controls">
      <UiTabs
        :model-value="mode"
        @update:model-value="updateMode"
      >
        <UiTabsList
          :class="$style.modeList"
          label="日历周期"
        >
          <UiTabsTrigger
            :class="$style.modeTrigger"
            value="week"
          >
            周
          </UiTabsTrigger>
          <UiTabsTrigger
            :class="$style.modeTrigger"
            value="biweek"
          >
            双周
          </UiTabsTrigger>
          <UiTabsTrigger
            :class="$style.modeTrigger"
            value="month"
          >
            月
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>

      <div
        :class="$style.periodNavigation"
        aria-label="日期范围"
      >
        <UiButton
          variant="ghost"
          size="icon-sm"
          aria-label="上一周期"
          title="上一周期"
          @click="emit('previous')"
        >
          <ChevronLeft
            :size="15"
            :stroke-width="1.8"
          />
        </UiButton>
        <strong
          :class="$style.periodLabel"
          aria-live="polite"
        >
          {{ periodLabel }}
        </strong>
        <UiButton
          variant="ghost"
          size="icon-sm"
          aria-label="下一周期"
          title="下一周期"
          @click="emit('next')"
        >
          <ChevronRight
            :size="15"
            :stroke-width="1.8"
          />
        </UiButton>
      </div>

      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('current')"
      >
        回到本周
      </UiButton>

      <UiSelect
        :class="$style.regionSelect"
        :model-value="region"
        :options="regionOptions"
        label="筛选地区"
        size="sm"
        @update:model-value="emit('update:region', $event)"
      />
      <UiSelect
        :class="$style.engineerSelect"
        :model-value="engineer"
        :options="engineerOptions"
        label="筛选工程师"
        size="sm"
        @update:model-value="emit('update:engineer', $event)"
      />
    </div>

    <p :class="$style.resultSummary">共 {{ scheduleCount }} 项排程 · {{ engineerCount }} 工程师</p>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.toolbar {
  @apply mb-3 flex min-h-[60px] items-center justify-between gap-4 rounded border bg-white px-3 py-2;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.controls {
  @apply flex min-w-0 items-center gap-2;
}

.modeList {
  @apply gap-0 rounded bg-[#f4f5f7] p-0.5;
}

.modeTrigger {
  @apply min-h-7 px-3 py-1 text-[12px];
}

.modeTrigger[data-state='active'] {
  color: var(--app-primary);
  background: white;
  box-shadow: 0 1px 3px rgba(9, 30, 66, 0.16);
}

.periodNavigation {
  @apply flex h-8 items-center rounded border bg-white;
  border-color: var(--ui-border);
}

.periodLabel {
  @apply min-w-[104px] px-1 text-center text-[13px];
  color: var(--ui-text);
}

.regionSelect {
  @apply min-w-[118px];
}

.engineerSelect {
  @apply min-w-[146px];
}

.resultSummary {
  @apply shrink-0 text-[11px];
  color: var(--ui-text-subtle);
}

@media (max-width: 1120px) {
  .toolbar {
    @apply items-start;
  }

  .controls {
    @apply flex-wrap;
  }
}
</style>
