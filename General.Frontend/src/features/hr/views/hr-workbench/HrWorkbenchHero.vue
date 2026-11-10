<script setup lang="ts">
import { LayoutGrid } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>人事管理 <span>›</span> 人事工作台</p>
      <div :class="$style.title">
        <span
          ><LayoutGrid
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>人力资源工作台</h1>
          <p>数据截止 2026-03-15</p>
        </div>
      </div>
    </div>
    <div :class="$style.visual">
      <div :class="$style.miniBars"><i /><i /><i /><i /></div>
      <div :class="$style.line"><i /><i /><i /><i /></div>
      <div :class="$style.donut" />
      <div :class="$style.people"><i /><i /><i /></div>
      <div :class="$style.list"><i /><i /><i /></div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[310px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #071b4d, #123c92 60%, #5b50bd);
}
.copy {
  @apply px-7 py-4;
  background: rgb(7 27 77 / 72%);
}
.copy > p {
  @apply m-0 text-[10px] font-semibold text-white/70;
}
.copy > p span {
  @apply px-1.5;
}
.title {
  @apply mt-3 flex items-center gap-3;
}
.title > span {
  @apply flex h-10 w-10 items-center justify-center rounded border border-white/25 bg-white/15;
}
.title h1 {
  @apply m-0 text-[22px] font-semibold;
}
.title p {
  @apply m-0 mt-1 text-[11px] text-white/75;
}
.visual {
  @apply relative grid grid-cols-5 items-center gap-8 px-8;
  background-image:
    linear-gradient(rgb(255 255 255 / 5%) 1px, transparent 1px),
    linear-gradient(90deg, rgb(255 255 255 / 5%) 1px, transparent 1px);
  background-size: 48px 48px;
}
.visual > div {
  @apply h-[72px] rounded-lg border border-white/10 bg-white/5;
}
.miniBars {
  @apply flex items-end justify-center gap-2 p-3;
}
.miniBars i {
  @apply w-4 bg-cyan-300/55;
}
.miniBars i:nth-child(1) {
  height: 40%;
}
.miniBars i:nth-child(2) {
  height: 65%;
}
.miniBars i:nth-child(3) {
  height: 48%;
}
.miniBars i:nth-child(4) {
  height: 82%;
}
.line {
  @apply relative;
}
.line::after {
  content: '';
  @apply absolute left-4 right-4 top-1/2 h-0.5 rotate-[-8deg] bg-cyan-200/65;
}
.donut {
  @apply rounded-full border-[9px] border-cyan-300/55;
}
.people {
  @apply flex flex-col justify-center gap-2 px-4;
}
.people i,
.list i {
  @apply block h-2 rounded bg-white/25;
}
.people i:nth-child(2) {
  @apply w-4/5;
}
.people i:nth-child(3) {
  @apply w-3/5;
}
.list {
  @apply flex flex-col justify-center gap-3 px-4;
}
.list i {
  @apply bg-white/30;
}
</style>
