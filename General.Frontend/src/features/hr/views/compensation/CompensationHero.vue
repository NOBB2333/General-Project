<script setup lang="ts">
import { WalletCards } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> 薪酬福利</p>
      <div :class="$style.title">
        <span
          ><WalletCards
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>薪酬福利</h1>
          <p>工资条、薪酬结构、社保公积金、年终奖与福利计划</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.visual"
      aria-hidden="true"
    >
      <i data-bar="1" /><i data-bar="2" /><i data-bar="3" /><i data-bar="4" /><i data-bar="5" /><span
        :class="$style.coins"
        ><b>¥</b></span
      ><span :class="$style.bag">¥</span>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[520px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #155b35, #06836c 52%, #08a9c8);
}
.copy {
  @apply relative z-10 px-7 py-4;
  background: linear-gradient(90deg, rgb(20 83 48 / 76%), transparent);
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
.visual {
  @apply relative overflow-hidden;
}
.visual i {
  @apply absolute bottom-0 w-12 rounded-t bg-white/15;
}
.visual i[data-bar='1'] {
  @apply left-[5%] h-3;
}
.visual i[data-bar='2'] {
  @apply left-[13%] h-9;
}
.visual i[data-bar='3'] {
  @apply left-[21%] h-5;
}
.visual i[data-bar='4'] {
  @apply left-[29%] h-14;
}
.visual i[data-bar='5'] {
  @apply left-[37%] h-7;
}
.visual::before {
  content: '';
  @apply absolute bottom-3 left-[4%] h-px w-[36%] rotate-[-12deg] bg-white/30;
}
.coins {
  @apply absolute bottom-0 left-[58%] h-[76px] w-[90px];
  background: repeating-linear-gradient(0deg, #d9a313 0 9px, #f0bd2b 10px 15px);
  border-radius: 50% 50% 12% 12%;
  box-shadow: inset 0 0 0 2px rgb(128 83 0 / 25%);
}
.coins b {
  @apply absolute left-1/2 top-1 -translate-x-1/2 text-[14px];
  color: #8a5c0a;
}
.bag {
  @apply absolute bottom-0 right-[13%] flex h-[82px] w-[64px] items-center justify-center rounded-[42%_42%_20%_20%] text-[30px] font-bold;
  color: #9b680e;
  background: #e5ce68;
  box-shadow: inset 0 0 0 2px rgb(132 89 8 / 55%);
}
</style>
