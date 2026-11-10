<script setup lang="ts">
import { BadgeCheck, CircleDollarSign, ReceiptText } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.heroCopy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 发票管理</p>
      <div :class="$style.titleRow">
        <span :class="$style.heroIcon">
          <ReceiptText
            :size="24"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>发票管理</h1>
          <p>销项 / 进项发票登记、税务校验、未开票预警</p>
        </div>
      </div>
    </div>

    <div
      :class="$style.invoiceArtwork"
      aria-hidden="true"
    >
      <span :class="$style.invoiceLine" />
      <span :class="$style.invoiceLineShort" />
      <span :class="$style.invoiceValue">¥ 3800.00</span>
      <span :class="$style.invoiceValueSecond">¥ 3000.00</span>
      <span :class="$style.invoiceValueThird">¥ 2200.00</span>
    </div>

    <div :class="$style.heroBadges">
      <span :class="$style.heroBadge">
        <BadgeCheck
          :size="22"
          :stroke-width="1.8"
        />
        <span><strong>已认证</strong><small>通过税务校验</small></span>
      </span>
      <span :class="$style.heroBadge">
        <CircleDollarSign
          :size="22"
          :stroke-width="1.8"
        />
        <span><strong>本月开票</strong><small>¥ 286,500.00</small></span>
      </span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(104deg, #d62906 0%, #ec3c1b 38%, #ff6b48 70%, #ffb749 100%);
}

.hero::after {
  @apply absolute inset-y-0 left-[46%] w-[13%];
  content: '';
  background: rgb(255 255 255 / 34%);
  transform: skewX(-5deg);
}

.heroCopy {
  @apply relative z-10;
}

.breadcrumb {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 78%);
}

.breadcrumb span {
  @apply px-1;
}

.titleRow {
  @apply mt-3 flex items-center gap-3;
}

.heroIcon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 13%);
}

.titleRow h1 {
  @apply m-0 text-[24px] font-semibold;
  letter-spacing: 0;
}

.titleRow p {
  @apply m-0 mt-1 text-[13px];
  color: rgb(255 255 255 / 85%);
}

.invoiceArtwork {
  @apply absolute inset-y-0 left-[31%] w-[32%] border-x;
  border-color: rgb(130 67 91 / 22%);
  background: rgb(255 246 248 / 78%);
  transform: skewX(-1deg);
}

.invoiceLine,
.invoiceLineShort {
  @apply absolute left-[19%] h-1 rounded-full;
  background: rgb(74 57 83 / 64%);
}

.invoiceLine {
  @apply top-8 w-[45%];
  box-shadow:
    0 13px 0 rgb(74 57 83 / 58%),
    0 26px 0 rgb(74 57 83 / 52%);
}

.invoiceLineShort {
  @apply top-[91px] w-[34%];
}

.invoiceValue,
.invoiceValueSecond,
.invoiceValueThird {
  @apply absolute right-[7%] text-[9px] font-semibold;
  color: #514567;
}

.invoiceValue {
  @apply top-[49px];
}

.invoiceValueSecond {
  @apply top-[69px];
}

.invoiceValueThird {
  @apply top-[89px];
}

.heroBadges {
  @apply absolute inset-y-0 right-[5%] z-10 flex w-[17%] flex-col justify-center gap-3;
}

.heroBadge {
  @apply flex min-h-[47px] items-center gap-3 rounded-lg px-3;
  background: rgb(255 255 255 / 16%);
}

.heroBadge:first-child > svg {
  color: #43c899;
}

.heroBadge:nth-child(2) > svg {
  color: #1452a5;
}

.heroBadge span {
  @apply flex min-w-0 flex-col;
}

.heroBadge strong {
  @apply text-[12px] font-semibold;
}

.heroBadge small {
  @apply mt-0.5 text-[9px];
  color: rgb(255 255 255 / 78%);
}
</style>
