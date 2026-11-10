<script setup lang="ts">
import { CircleDollarSign, MoveDownRight, MoveUpRight, ReceiptText, TriangleAlert } from 'lucide-vue-next';
import type { InvoiceMetric } from '../../model/invoices.types';

defineProps<{
  metrics: readonly InvoiceMetric[];
  pendingCount: number;
  pendingAmount: number;
}>();

const emit = defineEmits<{
  handleTasks: [];
}>();

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section
    :class="$style.metrics"
    aria-label="发票概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.metricCard, $style[`metric_${metric.tone}`]]"
    >
      <div :class="$style.metricLabel">
        <MoveUpRight
          v-if="metric.id === 'outbound'"
          :size="17"
          :stroke-width="1.8"
        />
        <MoveDownRight
          v-else-if="metric.id === 'inbound'"
          :size="17"
          :stroke-width="1.8"
        />
        <CircleDollarSign
          v-else-if="metric.id === 'tax'"
          :size="17"
          :stroke-width="1.8"
        />
        <TriangleAlert
          v-else
          :size="17"
          :stroke-width="1.8"
        />
        <span>{{ metric.label }}</span>
      </div>
      <strong :class="$style.metricValue">{{ metric.value }}</strong>
      <small>{{ metric.hint }}</small>
    </article>
  </section>

  <aside :class="$style.pendingBanner">
    <span :class="$style.pendingIcon">
      <ReceiptText
        :size="17"
        :stroke-width="1.8"
      />
    </span>
    <div>
      <p>
        您有 <strong>{{ pendingCount }}</strong> 项已审批的开票任务待处理
        <span>待办</span>
      </p>
      <small
        >待开票金额 <strong>{{ currency(pendingAmount) }}</strong> · 最早申请 2026-03-10 09:00（IA-2026-002）</small
      >
    </div>
    <button
      type="button"
      :class="$style.handleButton"
      @click="emit('handleTasks')"
    >
      立即处理 <span aria-hidden="true">→</span>
    </button>
  </aside>
</template>

<style module>
@reference "../../../../styles/index.css";

.metrics {
  @apply mt-4 grid grid-cols-4 gap-4;
}

.metricCard {
  @apply flex min-h-[100px] flex-col justify-center rounded border bg-white px-[18px] py-3;
  border-color: var(--ui-border);
}

.metricLabel {
  @apply flex items-center gap-2 text-[12px] font-medium;
  color: var(--ui-text-subtle);
}

.metricValue {
  @apply mt-2 text-[24px] font-semibold leading-none;
}

.metricCard small {
  @apply mt-2 text-[11px];
  color: var(--ui-text-subtle);
}

.metric_primary .metricLabel svg,
.metric_primary .metricValue {
  color: var(--app-primary);
}

.metric_danger .metricLabel svg,
.metric_danger .metricValue {
  color: #ff5630;
}

.metric_warning .metricLabel svg,
.metric_warning .metricValue {
  color: var(--ui-warning);
}

.pendingBanner {
  @apply mt-4 flex min-h-[60px] items-center gap-3 rounded border px-4;
  color: var(--ui-text);
  border-color: #ffab91;
  background: #fff2ee;
}

.pendingIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-white;
  background: #ff5630;
}

.pendingBanner p {
  @apply m-0 text-[13px] font-semibold;
}

.pendingBanner p > strong {
  @apply px-1 text-[17px];
  color: #ff5630;
}

.pendingBanner p > span {
  @apply ml-2 rounded px-1.5 py-0.5 text-[9px] text-white;
  background: #ff5630;
}

.pendingBanner small {
  @apply mt-1 block text-[11px];
  color: var(--ui-text-subtle);
}

.pendingBanner small strong {
  color: var(--ui-text);
}

.handleButton {
  @apply ml-auto inline-flex min-h-8 items-center gap-2 rounded px-3 text-[12px] font-semibold text-white transition-colors active:scale-[.97];
  background: #ff5630;
}

.handleButton:hover {
  background: #de350b;
}
</style>
