<script setup lang="ts">
import { Gift } from 'lucide-vue-next';
</script>
<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> 福利发放</p>
      <div :class="$style.title">
        <span
          ><Gift
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>福利发放管理</h1>
          <p>节日福利、生日礼物、员工关怀礼品发放与领取记录</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.gifts"
      aria-hidden="true"
    >
      <i data-gift="1" /><i data-gift="2" /><i data-gift="3" /><b>✦</b>
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[520px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #823268, #b42f79 55%, #fc7b93);
}
.copy {
  @apply relative z-10 px-7 py-4;
  background: linear-gradient(90deg, rgb(105 38 86 / 75%), transparent);
}
.copy > p {
  @apply m-0 text-[10px] font-semibold text-white/75;
}
.copy > p span {
  @apply px-1.5;
}
.title {
  @apply mt-3 flex items-center gap-3;
}
.title > span {
  @apply flex h-10 w-10 items-center justify-center rounded border border-white/30 bg-white/15;
}
.title h1 {
  @apply m-0 text-[22px] font-semibold;
}
.title p {
  @apply m-0 mt-1 text-[11px] text-white/75;
}
.gifts {
  @apply relative;
}
.gifts i {
  @apply absolute bottom-0 block rounded-t border-2 border-pink-300;
  background: #65c4d7;
}
.gifts i::before,
.gifts i::after {
  content: '';
  @apply absolute bg-pink-500;
}
.gifts i::before {
  @apply left-1/2 top-0 h-full w-3 -translate-x-1/2;
}
.gifts i::after {
  @apply left-0 top-4 h-3 w-full;
}
.gifts i[data-gift='1'] {
  @apply left-[8%] h-[53px] w-[112px];
  background: #c99e61;
}
.gifts i[data-gift='2'] {
  @apply left-[45%] h-[38px] w-[96px];
  background: #e5ad4f;
}
.gifts i[data-gift='3'] {
  @apply right-[18%] h-[48px] w-[108px];
}
.gifts b {
  @apply absolute right-[6%] top-[42%] text-amber-100;
}
</style>
