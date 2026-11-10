<script setup lang="ts">
import { GitCompareArrows } from 'lucide-vue-next';
const amounts = ['¥120,000', '¥155,000', '¥190,000', '¥225,000'];
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>财务管理 <span>›</span> 对账单</p>
      <div :class="$style.titleRow">
        <span :class="$style.icon"
          ><GitCompareArrows
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>对账中心</h1>
          <p>管理客户和供应商对账单，支持多维度对账与差异处理</p>
        </div>
      </div>
    </div>
    <div :class="$style.ledger">
      <div :class="$style.book">
        <span
          v-for="amount in amounts"
          :key="`left-${amount}`"
          >{{ amount }}</span
        >
      </div>
      <div :class="$style.equal">=</div>
      <div :class="[$style.book, $style.rightBook]">
        <span
          v-for="amount in amounts"
          :key="`right-${amount}`"
          >{{ amount }}</span
        >
      </div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[380px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #075985 0 35%, #16b6d4 62%, #5542b8 100%);
}
.copy {
  @apply relative z-10 px-7 py-4;
  background: rgb(3 105 161 / 72%);
}
.copy > p {
  @apply m-0 text-[11px] font-semibold text-white/75;
}
.copy > p span {
  @apply px-1.5;
}
.titleRow {
  @apply mt-3 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded border border-white/25 bg-white/15;
}
.titleRow h1 {
  @apply m-0 text-[24px] font-semibold;
}
.titleRow p {
  @apply m-0 mt-1 text-[12px] text-white/80;
}
.ledger {
  @apply grid grid-cols-[1fr_112px_1fr] items-center gap-4 px-4;
}
.book {
  @apply grid gap-2 rounded bg-white/20 p-3;
}
.book span {
  @apply h-5 rounded px-3 text-right text-[9px] leading-5 text-[#083466];
  background: #0472ba;
}
.book span:nth-child(even) {
  background: #26a67a;
}
.rightBook span {
  background: #5239a7;
}
.rightBook span:nth-child(even) {
  background: #20a779;
}
.equal {
  @apply flex h-16 items-center justify-center rounded-full bg-white/15 text-[38px] font-light;
}
</style>
