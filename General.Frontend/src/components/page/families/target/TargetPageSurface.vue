<script setup lang="ts">
import { CalendarDays, Plus, UsersRound } from 'lucide-vue-next';
import { UiButton, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { TargetPageSurfaceModel } from './target-page.types';

defineProps<{
  model: TargetPageSurfaceModel;
  unit: string;
  activeTab: string;
}>();

const emit = defineEmits<{
  'update:unit': [value: string];
  'update:activeTab': [value: string];
  primaryAction: [];
}>();
</script>

<template>
  <main
    :class="$style.page"
    data-domain-page
    data-module-target-board
    data-page-family="target"
    :data-page-route="model.path"
    :data-page-label="model.label"
    :data-page-module="model.module"
    :data-target-unit="unit"
    :data-target-tab="activeTab"
  >
    <header :class="$style.header">
      <div :class="$style.heading">
        <h1>{{ model.title }}</h1>
        <p>{{ model.description }}</p>
      </div>
      <div :class="$style.headerActions">
        <div
          :class="$style.unitControl"
          role="group"
          aria-label="显示单位"
        >
          <span>显示单位</span>
          <button
            v-for="item in model.units"
            :key="item.value"
            type="button"
            :class="[$style.unitButton, unit === item.value && $style.unitActive]"
            :aria-pressed="unit === item.value"
            @click="emit('update:unit', item.value)"
          >
            {{ item.label }}
          </button>
        </div>
        <UiButton
          variant="primary"
          size="md"
          @click="emit('primaryAction')"
        >
          <template #leading>
            <Plus
              :size="15"
              :stroke-width="1.8"
            />
          </template>
          {{ model.primaryActionLabel }}
        </UiButton>
      </div>
    </header>

    <section
      :class="$style.summaryGrid"
      aria-label="目标摘要"
    >
      <article
        v-for="summary in model.summaries"
        :key="summary.id"
        :class="$style.summaryCard"
        :data-tone="summary.tone"
      >
        <header :class="$style.summaryHeader">
          <span :class="$style.summaryIcon">
            <component
              :is="summary.icon"
              :size="18"
              :stroke-width="1.8"
            />
          </span>
          <div>
            <h2>{{ summary.label }}</h2>
            <p>{{ summary.description }}</p>
          </div>
          <strong :class="$style.rate">{{ summary.rate.toFixed(1) }}%</strong>
        </header>
        <strong :class="$style.targetValue">{{ summary.target }}</strong>
        <p :class="$style.actualValue">
          {{ summary.actualLabel }} <strong>{{ summary.actual }}</strong>
        </p>
        <progress
          :class="$style.progress"
          :value="summary.rate"
          max="100"
          :aria-label="`${summary.label}完成率 ${summary.rate.toFixed(1)}%`"
        />
      </article>
    </section>

    <UiTabs
      :model-value="activeTab"
      @update:model-value="emit('update:activeTab', $event)"
    >
      <div :class="$style.tabBar">
        <UiTabsList label="销售目标视图">
          <UiTabsTrigger
            v-for="tab in model.tabs"
            :key="tab.value"
            :value="tab.value"
            :class="$style.tabTrigger"
          >
            <component
              :is="tab.icon"
              :size="14"
              :stroke-width="1.8"
            />
            {{ tab.label }}
          </UiTabsTrigger>
        </UiTabsList>
        <span :class="$style.period">
          <CalendarDays
            :size="14"
            :stroke-width="1.8"
          />
          {{ model.periodLabel }}
        </span>
        <span :class="$style.participants">
          <UsersRound
            :size="14"
            :stroke-width="1.8"
          />
          {{ model.participantLabel }}
        </span>
      </div>
      <slot />
    </UiTabs>
  </main>
</template>

<style module>
@reference "../../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1050px] px-6 pb-8 pt-6;
  color: var(--ui-text);
}

.header {
  @apply mb-6 flex min-h-[56px] items-start justify-between gap-6;
}

.heading h1 {
  @apply text-[24px] font-semibold leading-[33px];
}

.heading p {
  @apply mt-1 text-[12px];
  color: var(--ui-text-subtle);
}

.headerActions {
  @apply mt-2 flex shrink-0 items-center gap-3;
}

.unitControl {
  @apply inline-flex h-9 items-center overflow-hidden rounded border;
  color: var(--ui-text-subtle);
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.unitControl > span {
  @apply px-3 text-[11px];
}

.unitButton {
  @apply h-full min-w-8 border-l px-2 text-[11px] font-medium outline-none transition-colors;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}

.unitButton:hover {
  background: var(--ui-surface-hover);
}

.unitButton:focus-visible {
  box-shadow: inset 0 0 0 2px var(--ui-focus);
}

.unitActive {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.summaryGrid {
  @apply grid grid-cols-3 gap-4;
}

.summaryCard {
  @apply min-w-0 rounded border p-5;
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.summaryHeader {
  @apply flex items-start gap-3;
}

.summaryIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.summaryCard[data-tone='purple'] .summaryIcon {
  color: #6554c0;
  background: #eae6ff;
}

.summaryCard[data-tone='success'] .summaryIcon {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.summaryHeader h2 {
  @apply text-[13px] font-semibold;
}

.summaryHeader p {
  @apply mt-0.5 text-[10px];
  color: var(--ui-text-subtle);
}

.rate {
  @apply ml-auto rounded px-2 py-1 text-[11px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.targetValue {
  @apply mt-4 block text-[22px] font-semibold leading-8;
}

.actualValue {
  @apply mt-0.5 text-[11px];
  color: var(--ui-text-subtle);
}

.actualValue strong {
  color: var(--app-primary);
}

.summaryCard[data-tone='purple'] .actualValue strong {
  color: #6554c0;
}

.summaryCard[data-tone='success'] .actualValue strong {
  color: var(--ui-success);
}

.progress {
  @apply mt-3 block h-1.5 w-full overflow-hidden rounded-full border-0;
  color: var(--app-primary);
  background: var(--ui-border-subtle);
}

.progress::-webkit-progress-bar {
  background: var(--ui-border-subtle);
}

.progress::-webkit-progress-value {
  background: currentColor;
}

.progress::-moz-progress-bar {
  background: currentColor;
}

.summaryCard[data-tone='purple'] .progress {
  color: #6554c0;
}

.summaryCard[data-tone='success'] .progress {
  color: var(--ui-success);
}

.tabBar {
  @apply mt-5 flex min-h-10 items-center border-b;
  border-color: var(--ui-border);
}

.tabTrigger {
  @apply rounded-b-none border-b-2 border-transparent;
}

.tabTrigger[data-state='active'] {
  border-color: var(--app-primary);
  background: var(--ui-surface);
}

.period,
.participants {
  @apply inline-flex items-center gap-1.5 text-[11px];
  color: var(--ui-text-subtle);
}

.period {
  @apply ml-3;
}

.participants {
  @apply ml-auto;
}

@media (max-width: 1180px) {
  .summaryGrid {
    @apply grid-cols-2;
  }

  .summaryCard:last-child {
    @apply col-span-2;
  }
}

@media (prefers-reduced-motion: reduce) {
  .unitButton {
    transition: none;
  }
}
</style>
