<script setup lang="ts">
import { CalendarDays, Filter, LayoutGrid } from 'lucide-vue-next';
import { UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { salesBoardPeriods, salesBoardScopes, salesBoardTabs } from '../../model/sales-board.data';
import type { SalesBoardPeriod, SalesBoardScope, SalesBoardSection } from '../../model/sales-board.types';

defineProps<{
  section: SalesBoardSection;
  scope: SalesBoardScope;
  period: SalesBoardPeriod;
  updatedAt: string;
}>();

const emit = defineEmits<{
  'update:section': [value: SalesBoardSection];
  'update:scope': [value: SalesBoardScope];
  'update:period': [value: SalesBoardPeriod];
}>();

defineSlots<{
  default: () => unknown;
}>();

function updateSection(value: string) {
  emit('update:section', value as SalesBoardSection);
}

function updateScope(value: string) {
  emit('update:scope', value as SalesBoardScope);
}

function updatePeriod(value: string) {
  emit('update:period', value as SalesBoardPeriod);
}
</script>

<template>
  <UiTabs
    :model-value="section"
    data-board-section-tabs
    @update:model-value="updateSection"
  >
    <header :class="$style.header">
      <div :class="$style.titleRow">
        <span :class="$style.titleIcon">
          <LayoutGrid
            :size="23"
            :stroke-width="1.8"
          />
        </span>
        <div :class="$style.heading">
          <div :class="$style.headingLine">
            <h1>销售看板</h1>
            <span :class="$style.viewBadge">全角色视角</span>
          </div>
          <p>多角色 · 多维度 · 一站式销售数据洞察</p>
        </div>
      </div>

      <UiTabsList
        label="销售看板分区"
        :class="$style.sectionTabs"
      >
        <UiTabsTrigger
          v-for="tab in salesBoardTabs"
          :key="tab.id"
          :value="tab.id"
          :class="$style.sectionTab"
        >
          <component
            :is="tab.icon"
            :size="16"
            :stroke-width="1.8"
          />
          <strong>{{ tab.label }}</strong>
          <span>{{ tab.badge }}</span>
        </UiTabsTrigger>
      </UiTabsList>

      <div :class="$style.controls">
        <div :class="$style.controlGroup">
          <Filter
            :size="16"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <span :class="$style.controlLabel">视角</span>
          <UiTabs
            :model-value="scope"
            data-board-scope-tabs
            @update:model-value="updateScope"
          >
            <UiTabsList
              label="数据视角"
              :class="$style.segmentList"
            >
              <UiTabsTrigger
                v-for="item in salesBoardScopes"
                :key="item.id"
                :value="item.id"
                :class="$style.segmentTrigger"
              >
                {{ item.label }}
              </UiTabsTrigger>
            </UiTabsList>
          </UiTabs>
        </div>

        <span :class="$style.divider" />

        <div :class="$style.controlGroup">
          <CalendarDays
            :size="16"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <span :class="$style.controlLabel">周期</span>
          <UiTabs
            :model-value="period"
            data-board-period-tabs
            @update:model-value="updatePeriod"
          >
            <UiTabsList
              label="统计周期"
              :class="$style.segmentList"
            >
              <UiTabsTrigger
                v-for="item in salesBoardPeriods"
                :key="item.id"
                :value="item.id"
                :class="$style.segmentTrigger"
              >
                {{ item.label }}
              </UiTabsTrigger>
            </UiTabsList>
          </UiTabs>
        </div>

        <p :class="$style.updatedAt">数据更新于 {{ updatedAt }}</p>
      </div>
    </header>

    <slot />
  </UiTabs>
</template>

<style module>
@reference "../../../../styles/index.css";

.header {
  @apply shrink-0;
}

.titleRow {
  @apply flex items-center gap-3 px-1 pt-1;
}

.titleIcon {
  @apply flex h-8 w-8 shrink-0 items-center justify-center;
  color: var(--app-primary);
}

.heading {
  @apply min-w-0;
}

.headingLine {
  @apply flex items-center gap-2;
}

.headingLine h1 {
  @apply text-[20px] font-bold leading-7;
  color: var(--ui-text);
}

.viewBadge {
  @apply rounded px-2 py-0.5 text-[11px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.heading p {
  @apply mt-0.5 text-[12px];
  color: var(--ui-text-subtle);
}

.sectionTabs {
  @apply mt-5 flex w-full items-stretch gap-0 border-b;
  border-color: var(--ui-border);
}

.sectionTab {
  @apply min-h-[42px] flex-1 whitespace-nowrap rounded-none px-2 py-2 text-[12px];
  color: var(--ui-text-subtle);
}

.sectionTab strong {
  @apply font-semibold;
}

.sectionTab span {
  @apply rounded px-1.5 py-0.5 text-[10px] font-medium;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.sectionTab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}

.sectionTab[data-state='active'] span {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.controls {
  @apply mt-4 flex min-h-[52px] items-center gap-3 rounded border px-4;
  color: var(--ui-text-subtle);
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.controlGroup {
  @apply flex items-center gap-2;
}

.controlLabel {
  @apply text-[12px] font-semibold;
  color: var(--ui-text-muted);
}

.divider {
  @apply h-6 w-px;
  background: var(--ui-border);
}

.segmentList {
  @apply gap-0 rounded p-0.5;
  background: var(--ui-surface-hover);
}

.segmentTrigger {
  @apply min-h-7 rounded px-3 py-1 text-[12px];
}

.segmentTrigger[data-state='active'] {
  color: var(--app-primary);
  background: var(--ui-surface);
  box-shadow: 0 1px 3px color-mix(in srgb, var(--app-primary-dark) 16%, transparent);
}

.updatedAt {
  @apply ml-auto whitespace-nowrap text-[11px];
  color: var(--ui-text-subtle);
}

@media (max-width: 1180px) {
  .sectionTabs {
    @apply overflow-x-auto;
  }

  .sectionTab {
    @apply flex-none;
  }
}

@media (prefers-reduced-motion: reduce) {
  .sectionTab,
  .segmentTrigger {
    transition: none;
  }
}
</style>
