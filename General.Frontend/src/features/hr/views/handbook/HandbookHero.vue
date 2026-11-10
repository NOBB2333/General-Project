<script setup lang="ts">
import { BookOpen } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> 员工手册</p>
      <div :class="$style.title">
        <span
          ><BookOpen
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>员工手册</h1>
          <p>7 章 · 18 条 · 当前版本 V3.2 · 全员已读率 74%</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.visual"
      aria-hidden="true"
    >
      <i /><i /><b /><span /><span />
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[500px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(100deg, #573115, #8a5625 48%, #f2c57b);
}
.copy {
  @apply relative z-10 px-7 py-4;
  background: linear-gradient(90deg, rgb(72 35 13 / 82%), transparent);
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
  background: linear-gradient(90deg, #9e632c, #e2a554 62%, #ffd693);
}
.visual::before,
.visual::after {
  content: '';
  @apply absolute bottom-0 h-[124px] border border-[#805026] bg-[#eee0c8];
  width: 250px;
}
.visual::before {
  @apply left-[5%] rotate-[-1deg];
}
.visual::after {
  @apply left-[36%] rotate-[1deg];
}
.visual i {
  @apply absolute z-10 h-px w-[200px];
  left: 10%;
  background: #47526a;
  box-shadow:
    0 17px #47526a,
    0 34px #47526a,
    0 51px #47526a,
    0 68px #47526a,
    0 85px #47526a;
}
.visual i:first-child {
  @apply top-4;
}
.visual i:nth-child(2) {
  @apply left-[41%] top-4;
}
.visual b {
  @apply absolute left-[60%] top-0 z-20 h-8 w-9;
  background: #d33268;
  clip-path: polygon(0 0, 100% 0, 100% 100%, 50% 72%, 0 100%);
}
.visual span {
  @apply absolute right-[8%] top-2 h-[74px] w-[64px] rotate-[14deg] rounded border bg-[#fff4df];
  border-color: #936638;
}
.visual span:last-child {
  @apply right-[1%] top-[58px] rotate-[-8deg];
}
</style>
