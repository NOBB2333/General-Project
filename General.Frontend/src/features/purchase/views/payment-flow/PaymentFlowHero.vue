<script setup lang="ts">
import { Banknote, BarChart3, Check, WalletCards } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>采购管理</span><b aria-hidden="true">›</b><strong>付款流水</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <WalletCards
            :size="23"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>采购付款流水</h1>
          <p>登记并展示所有付款流水 · 指定订单直接确认 · 未完成分配仅手动分配到订单</p>
        </div>
      </div>
    </div>

    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="$style.wallet">
        <Banknote
          :size="52"
          :stroke-width="1.35"
        />
      </span>
      <span :class="$style.coin">¥</span>
      <span :class="$style.checkCard"><Check :size="24" /></span>
      <span :class="$style.lineCard"><i /><i /></span>
      <span :class="$style.chart"><BarChart3 :size="56" /></span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(105deg, #08784f 0%, #00875a 47%, #19b985 100%);
}

.copy {
  @apply relative z-[2];
}

.breadcrumb {
  @apply flex items-center gap-1.5 text-[11px] font-semibold;
  color: rgb(255 255 255 / 72%);
}

.breadcrumb strong {
  color: white;
}

.identity {
  @apply mt-3 flex items-center gap-3;
}

.icon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 28%);
  background: rgb(255 255 255 / 13%);
}

.identity h1 {
  @apply m-0 text-[22px] font-semibold leading-7;
  letter-spacing: 0;
}

.identity p {
  @apply m-0 mt-1 text-[12px];
  color: rgb(255 255 255 / 84%);
}

.scene {
  @apply absolute inset-y-0 right-0 w-[520px];
  background: rgb(255 255 255 / 5%);
  clip-path: ellipse(62% 130% at 72% 58%);
}

.wallet {
  @apply absolute left-[88px] top-[42px] flex h-[54px] w-[96px] items-center justify-center rounded-lg;
  color: #079368;
  background: rgb(255 255 255 / 78%);
}

.coin {
  @apply absolute left-[171px] top-[53px] flex h-7 w-7 items-center justify-center rounded-full text-[13px] font-semibold;
  color: #a46400;
  background: #ffd666;
}

.checkCard,
.lineCard {
  @apply absolute right-[106px] flex h-[38px] w-[118px] items-center rounded-lg;
  background: rgb(255 255 255 / 70%);
}

.checkCard {
  @apply top-[-2px] justify-end pr-3;
  color: #00a878;
}

.lineCard {
  @apply top-[46px] flex-col items-start justify-center gap-2 px-3;
}

.lineCard i {
  @apply block h-1 rounded-full;
  width: 64px;
  background: rgb(8 120 79 / 42%);
}

.lineCard i:last-child {
  width: 92px;
  opacity: 0.5;
}

.chart {
  @apply absolute bottom-[-8px] right-3;
  color: rgb(255 255 255 / 62%);
}
</style>
