<script setup lang="ts">
import { Archive, ArrowDownLeft, ArrowUpRight, WalletCards } from 'lucide-vue-next';
import { UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { FinanceAccountSummary, FinanceAccountSurface } from '../../model/accounts.types';

defineProps<{
  modelValue: FinanceAccountSurface;
  summary: FinanceAccountSummary;
  isOpening: boolean;
}>();

const emit = defineEmits<{
  'update:modelValue': [value: FinanceAccountSurface];
}>();

const surfaceTabs: readonly {
  value: FinanceAccountSurface;
  label: string;
  tone: 'default' | 'warning';
  direction: 'in' | 'out';
}[] = [
  { value: 'receivable', label: '应收账款', tone: 'default', direction: 'in' },
  { value: 'payable', label: '应付账款', tone: 'default', direction: 'out' },
  { value: 'opening-receivable', label: '期初应收建账', tone: 'warning', direction: 'in' },
  { value: 'opening-payable', label: '期初应付建账', tone: 'warning', direction: 'out' },
];
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.heroCopy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 应收应付</p>
      <div :class="$style.heroTitleRow">
        <span :class="$style.heroIcon">
          <WalletCards
            :size="23"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>应收应付管理</h1>
          <p>统一查看应收应付账款、管理期初建账、跟踪资金双向流动</p>
        </div>
      </div>
    </div>

    <div
      :class="$style.heroFlow"
      aria-hidden="true"
    >
      <span :class="[$style.flowLine, $style.flowLineTop]" />
      <span :class="[$style.flowLine, $style.flowLineBottom]" />
      <span :class="$style.flowSpine" />
      <span :class="[$style.flowNode, $style.flowNodeIn]">¥</span>
      <span :class="[$style.flowNode, $style.flowNodeOut]">¥</span>
      <span :class="$style.flowInLabel">应收 IN</span>
      <span :class="$style.flowOutLabel">应付 OUT</span>
    </div>
  </header>

  <UiTabs
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event as FinanceAccountSurface)"
  >
    <UiTabsList
      label="应收应付业务视图"
      :class="$style.surfaceTabs"
    >
      <UiTabsTrigger
        v-for="tab in surfaceTabs"
        :key="tab.value"
        :value="tab.value"
        :variant="tab.tone"
        :class="$style.surfaceTab"
      >
        <Archive
          v-if="tab.value.startsWith('opening-')"
          :size="15"
          :stroke-width="1.8"
        />
        <ArrowDownLeft
          v-else-if="tab.direction === 'in'"
          :size="15"
          :stroke-width="1.8"
        />
        <ArrowUpRight
          v-else
          :size="15"
          :stroke-width="1.8"
        />
        {{ tab.label }}
      </UiTabsTrigger>
    </UiTabsList>
  </UiTabs>

  <section
    v-if="!isOpening"
    :class="$style.metrics"
  >
    <article :class="$style.metricCard">
      <span>{{ summary.totalLabel }}</span>
      <strong>{{ summary.totalValue }}</strong>
      <small>{{ summary.totalHint }}</small>
    </article>
    <article :class="$style.metricCard">
      <span>{{ summary.overdueLabel }}</span>
      <strong :class="$style.dangerValue">{{ summary.overdueValue }}</strong>
      <small>{{ summary.overdueHint }}</small>
    </article>
    <article :class="$style.metricCard">
      <span>{{ summary.countLabel }}</span>
      <strong :class="$style.warningValue">{{ summary.countValue }}</strong>
      <small>{{ summary.countHint }}</small>
    </article>
    <article :class="$style.metricCard">
      <span>{{ summary.cycleLabel }}</span>
      <strong>{{ summary.cycleValue }}</strong>
      <small>{{ summary.cycleHint }}</small>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-6 pb-3 pt-5 text-white;
  background: linear-gradient(104deg, #00875a 0%, #006644 34%, #344563 68%, #ff8b00 100%);
}

.heroCopy {
  @apply relative z-10;
}

.breadcrumb {
  @apply m-0 text-[12px] font-medium;
  color: color-mix(in srgb, white 74%, transparent);
}

.breadcrumb span {
  @apply px-1;
}

.heroTitleRow {
  @apply mt-5 flex items-center gap-3;
}

.heroIcon {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-lg border;
  color: white;
  background: color-mix(in srgb, white 15%, transparent);
  border-color: color-mix(in srgb, white 24%, transparent);
}

.heroTitleRow h1 {
  @apply m-0 text-[22px] font-semibold;
  letter-spacing: 0;
}

.heroTitleRow p {
  @apply m-0 mt-1 text-[13px];
  color: color-mix(in srgb, white 82%, transparent);
}

.heroFlow {
  @apply absolute inset-y-0 right-0 w-[65%];
  opacity: 0.74;
}

.flowLine,
.flowSpine {
  @apply absolute block;
  background: color-mix(in srgb, white 62%, transparent);
}

.flowLine {
  @apply h-0.5 w-[68%];
  right: 5%;
  transform-origin: right;
}

.flowLineTop {
  top: 38%;
  transform: rotate(-3deg);
}

.flowLineBottom {
  top: 66%;
  transform: rotate(3deg);
}

.flowSpine {
  @apply h-[112%] w-0.5;
  left: 49%;
  top: -6%;
  transform: rotate(-1deg);
}

.flowNode {
  @apply absolute flex h-7 w-7 items-center justify-center rounded-full text-[13px] font-semibold;
  top: 45%;
  color: #ffeb3b;
  background: color-mix(in srgb, #ff8b00 45%, #344563);
  border: 2px solid color-mix(in srgb, white 35%, transparent);
}

.flowNodeIn {
  left: 31%;
}

.flowNodeOut {
  left: 70%;
}

.flowInLabel,
.flowOutLabel {
  @apply absolute rounded px-2 py-1 text-[10px] font-semibold;
  top: 12%;
  background: color-mix(in srgb, #091e42 26%, transparent);
}

.flowInLabel {
  left: 12%;
}

.flowOutLabel {
  right: 10%;
}

.surfaceTabs {
  @apply mt-3 flex-wrap gap-1.5;
}

.surfaceTab {
  @apply min-h-10 border bg-white px-5 py-2.5 text-[13px] font-semibold;
  border-color: var(--ui-border);
}

.surfaceTab[data-state='active'] {
  color: white;
  background: var(--app-primary);
  border-color: var(--app-primary);
  box-shadow: 0 1px 3px color-mix(in srgb, var(--app-primary-dark) 25%, transparent);
}

.surfaceTab:nth-child(n + 3) {
  color: #974f0c;
  border-color: #ffe380;
}

.surfaceTab:nth-child(n + 3)[data-state='active'] {
  color: #974f0c;
  background: #fffae6;
  border-color: #ffab00;
}

.metrics {
  @apply grid grid-cols-4 gap-4;
}

.metricCard {
  @apply min-h-[102px] rounded border bg-white px-4 py-4;
  border-color: var(--ui-border);
}

.metricCard span {
  @apply block text-[12px] font-medium;
  color: var(--ui-text-subtle);
}

.metricCard strong {
  @apply mt-2 block text-[23px] font-semibold;
  color: var(--ui-text);
  letter-spacing: 0;
}

.metricCard small {
  @apply mt-1 block text-[11px];
  color: #97a0af;
}

.dangerValue {
  color: #ff5630 !important;
}

.warningValue {
  color: var(--ui-warning) !important;
}

@media (max-width: 900px) {
  .heroFlow {
    @apply hidden;
  }

  .metrics {
    @apply grid-cols-2;
  }
}

@media (max-width: 600px) {
  .hero {
    @apply min-h-0 px-4 py-4;
  }

  .heroTitleRow {
    @apply mt-3 items-start;
  }

  .heroTitleRow h1 {
    @apply text-[18px];
  }

  .surfaceTabs {
    @apply grid grid-cols-2;
  }

  .surfaceTab {
    @apply min-w-0 px-2 text-[11px];
  }

  .metrics {
    @apply grid-cols-1;
  }
}

@media (prefers-reduced-motion: reduce) {
  .surfaceTab {
    transition: none;
  }
}
</style>
