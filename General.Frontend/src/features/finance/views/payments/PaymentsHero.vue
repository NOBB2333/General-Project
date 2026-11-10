<script setup lang="ts">
import { Banknote, CircleCheck, Clock3, Info, Landmark, WalletCards } from 'lucide-vue-next';
import type { PaymentMetric } from '../../model/payments.types';

defineProps<{
  metrics: readonly PaymentMetric[];
}>();

const emit = defineEmits<{
  openExpenseCenter: [];
}>();
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.heroCopy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 付款管理</p>
      <div :class="$style.titleRow">
        <span :class="$style.heroIcon">
          <Landmark
            :size="25"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>付款管理</h1>
          <p>付款任务统筹、付款单核销、银行流水匹配、供应商预付款</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.heroArtwork"
      aria-hidden="true"
    >
      <span :class="$style.coinStack">¥<i>¥</i><b>¥</b></span>
      <span :class="$style.flowLine" />
      <span :class="$style.paidStamp"><strong>PAID</strong><small>已付款 · 已核销</small></span>
    </div>
  </header>

  <div :class="$style.notice">
    <Info
      :size="15"
      :stroke-width="1.8"
    />
    <p>
      <strong>付款任务</strong>
      汇总所有待处理应付。采购/外协/运输等需业务发起付款申请并审批；报销/工资/税费等上游审批即可直接登记付款。 也可前往
      <button
        type="button"
        @click="emit('openExpenseCenter')"
      >
        费用中心 · 付款申请
      </button>
      新建独立付款申请。
    </p>
  </div>

  <section
    :class="$style.metrics"
    aria-label="付款概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.metricCard, $style[`metric_${metric.tone}`]]"
    >
      <div>
        <Clock3
          v-if="metric.id === 'approval'"
          :size="17"
          :stroke-width="1.8"
        />
        <Banknote
          v-else-if="metric.id === 'pending'"
          :size="17"
          :stroke-width="1.8"
        />
        <CircleCheck
          v-else-if="metric.id === 'writeoff'"
          :size="17"
          :stroke-width="1.8"
        />
        <WalletCards
          v-else
          :size="17"
          :stroke-width="1.8"
        />
        <span>{{ metric.label }}</span>
      </div>
      <p>
        <strong>{{ metric.value }}</strong> <small v-if="metric.hint">{{ metric.hint }}</small>
      </p>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(108deg, #0c55be 0%, #0052cc 38%, #6554c0 67%, #de350b 112%);
}

.heroCopy {
  @apply relative z-10;
}

.breadcrumb {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 72%);
}

.breadcrumb span {
  @apply px-1;
}

.titleRow {
  @apply mt-3 flex items-center gap-3;
}

.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  color: white;
  border-color: rgb(255 255 255 / 26%);
  background: rgb(255 255 255 / 14%);
}

.titleRow h1 {
  @apply m-0 text-[24px] font-semibold;
  letter-spacing: 0;
}

.titleRow p {
  @apply m-0 mt-1 text-[13px];
  color: rgb(255 255 255 / 82%);
}

.heroArtwork {
  @apply absolute inset-y-0 right-0 w-[62%];
}

.flowLine {
  @apply absolute left-8 right-20 top-[66px] border-t-2 border-dashed;
  border-color: rgb(255 255 255 / 48%);
  transform: rotate(-1deg);
}

.coinStack {
  @apply absolute left-8 top-[46px] flex h-7 w-7 items-center justify-center rounded-full text-[11px] font-semibold;
  color: #5e4a1c;
  background: #f6c453;
}

.coinStack i,
.coinStack b {
  @apply absolute flex h-7 w-7 items-center justify-center rounded-full not-italic;
  color: #5e4a1c;
  background: #e0a93e;
}

.coinStack i {
  top: 10px;
}

.coinStack b {
  top: 20px;
}

.paidStamp {
  @apply absolute right-[15%] top-5 flex h-[85px] w-[170px] flex-col items-center justify-center rounded border-[5px];
  color: #de350b;
  border-color: #de350b;
  background: rgb(9 30 66 / 16%);
}

.paidStamp strong {
  @apply text-[28px] font-bold;
  letter-spacing: 4px;
}

.paidStamp small {
  @apply mt-1 text-[10px] font-semibold;
}

.notice {
  @apply mt-4 flex min-h-10 items-center gap-2 rounded border px-4 text-[11px];
  color: var(--ui-text-muted);
  border-color: var(--app-primary-light);
  background: var(--app-primary-soft);
}

.notice > svg {
  @apply shrink-0;
  color: var(--app-primary);
}

.notice p {
  @apply m-0 leading-5;
}

.notice strong,
.notice button {
  @apply font-semibold;
  color: var(--app-primary);
}

.notice button {
  @apply underline decoration-transparent transition-colors;
}

.notice button:hover {
  text-decoration-color: currentColor;
}

.metrics {
  @apply mt-4 grid grid-cols-4 gap-4;
}

.metricCard {
  @apply flex min-h-[84px] flex-col justify-center rounded border bg-white px-[18px] py-3;
  border-color: var(--ui-border);
}

.metricCard > div {
  @apply flex items-center gap-2 text-[12px] font-medium;
  color: var(--ui-text-subtle);
}

.metricCard p {
  @apply m-0 mt-2;
}

.metricCard strong {
  @apply text-[24px] font-semibold leading-none;
}

.metricCard small {
  @apply ml-1 text-[11px];
}

.metric_purple strong,
.metric_purple svg {
  color: #6554c0;
}

.metric_primary strong,
.metric_primary svg {
  color: var(--app-primary);
}

.metric_success strong,
.metric_success svg {
  color: var(--ui-success);
}

.metric_danger strong,
.metric_danger small,
.metric_danger svg {
  color: var(--ui-danger);
}

@media (prefers-reduced-motion: reduce) {
  .flowLine {
    transform: none;
  }

  .notice button {
    transition: none;
  }
}
</style>
