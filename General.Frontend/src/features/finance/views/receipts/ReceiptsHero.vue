<script setup lang="ts">
import { Banknote, CircleCheck, Clock3, ReceiptText, WalletCards } from 'lucide-vue-next';
import type { ReceiptOverviewMetric } from '../../model/receipts.types';

defineProps<{
  metrics: readonly ReceiptOverviewMetric[];
}>();
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.heroCopy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 收款管理</p>
      <div :class="$style.titleRow">
        <span :class="$style.heroIcon">
          <ReceiptText
            :size="25"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>收款管理</h1>
          <p>登记收款单据、银行流水匹配、客户预收款核销</p>
        </div>
      </div>
    </div>

    <div
      :class="$style.heroArtwork"
      aria-hidden="true"
    >
      <span :class="$style.artworkCard">
        <Banknote
          :size="68"
          :stroke-width="1.25"
        />
      </span>
      <span :class="$style.artworkCoin">¥</span>
      <span :class="$style.artworkLine" />
    </div>
  </header>

  <section
    :class="$style.overviewGrid"
    aria-label="收款概览"
  >
    <article
      v-for="metric in metrics"
      :key="metric.id"
      :class="[$style.overviewCard, $style[`metric_${metric.tone}`]]"
    >
      <div :class="$style.metricLabel">
        <Banknote
          v-if="metric.id === 'total'"
          :size="17"
          :stroke-width="1.8"
        />
        <Clock3
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
      <strong
        >{{ metric.value }} <small v-if="metric.suffix">{{ metric.suffix }}</small></strong
      >
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[152px] overflow-hidden rounded-lg px-7 pb-5 pt-5 text-white;
  background: linear-gradient(103deg, #007a4d 0%, #00875a 49%, #52b788 76%, #f7c64a 120%);
}

.hero::before {
  @apply absolute block h-[220px] w-[620px] rounded-[50%] border-2 border-dashed;
  content: '';
  right: 16%;
  top: 20px;
  border-color: rgb(255 255 255 / 34%);
  transform: rotate(7deg);
}

.heroCopy {
  @apply relative z-10;
}

.breadcrumb {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 74%);
}

.breadcrumb span {
  @apply px-1;
}

.titleRow {
  @apply mt-6 flex items-center gap-4;
}

.heroIcon {
  @apply flex h-12 w-12 shrink-0 items-center justify-center rounded-lg border;
  color: white;
  border-color: rgb(255 255 255 / 26%);
  background: rgb(255 255 255 / 15%);
}

.titleRow h1 {
  @apply m-0 text-[24px] font-semibold;
  letter-spacing: 0;
}

.titleRow p {
  @apply m-0 mt-1 text-[13px];
  color: rgb(255 255 255 / 84%);
}

.heroArtwork {
  @apply absolute inset-y-0 right-10 w-[360px];
}

.artworkCard {
  @apply absolute right-[74px] top-8 flex h-[126px] w-[154px] items-center justify-center rounded-t-[70px];
  color: rgb(0 102 68 / 58%);
  background: rgb(255 255 255 / 22%);
}

.artworkCoin {
  @apply absolute right-[50px] top-[22px] flex h-12 w-12 items-center justify-center rounded-full text-[22px] font-semibold;
  color: rgb(0 102 68 / 72%);
  background: rgb(255 255 255 / 28%);
}

.artworkLine {
  @apply absolute right-0 top-[92px] h-px w-[300px];
  background: rgb(255 255 255 / 28%);
  transform: rotate(-9deg);
}

.overviewGrid {
  @apply mt-4 grid grid-cols-4 gap-4;
}

.overviewCard {
  @apply flex min-h-[106px] flex-col justify-center rounded border bg-white px-[18px] py-4;
  border-color: var(--ui-border);
}

.metricLabel {
  @apply flex items-center gap-2 text-[12px] font-medium;
  color: var(--ui-text-subtle);
}

.overviewCard strong {
  @apply mt-2 text-[25px] font-semibold leading-none;
  letter-spacing: 0;
}

.overviewCard small {
  @apply text-[14px] font-semibold;
}

.metric_success strong,
.metric_success .metricLabel svg {
  color: var(--ui-success);
}

.metric_warning strong,
.metric_warning .metricLabel svg {
  color: var(--ui-warning);
}

.metric_primary strong,
.metric_primary .metricLabel svg {
  color: var(--app-primary);
}

.metric_purple strong,
.metric_purple .metricLabel svg {
  color: #6554c0;
}

@media (prefers-reduced-motion: reduce) {
  .hero::before,
  .artworkLine {
    transform: none;
  }
}
</style>
