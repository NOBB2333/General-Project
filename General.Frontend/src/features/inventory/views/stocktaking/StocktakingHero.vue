<script setup lang="ts">
import { Check, ChevronRight, ClipboardList } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.boxes"
      aria-hidden="true"
    >
      <i><b>×24</b></i
      ><i><b>×12</b></i>
    </div>
    <div
      :class="$style.checklist"
      aria-hidden="true"
    >
      <span
        v-for="index in 5"
        :key="index"
        ><i><Check :size="12" /></i><b
      /></span>
    </div>
    <div
      :class="$style.pencil"
      aria-hidden="true"
    />
    <div
      :class="$style.progressCard"
      aria-hidden="true"
    >
      <span>已盘点</span><strong>236</strong><small>/ 280 SKU</small>
    </div>
    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>库存管理</span
        ><ChevronRight
          :size="12"
          :stroke-width="1.8"
        /><strong>库存盘点</strong>
      </nav>
      <div :class="$style.heading">
        <span :class="$style.icon"
          ><ClipboardList
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>库存盘点</h1>
          <p>制定盘点计划、执行盘点与盘亏盘盈处理，共 11 条记录</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative h-[118px] overflow-hidden rounded-2xl;
  background: linear-gradient(102deg, #2b1769 0%, #3d278d 38%, #3459a8 70%, #12b9ce 100%);
}
.content {
  @apply relative px-6 py-5;
}
.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.72);
}
.breadcrumb strong,
.heading h1 {
  color: white;
}
.heading {
  @apply mt-4 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  color: white;
  background: rgba(255, 255, 255, 0.14);
  border-color: rgba(255, 255, 255, 0.3);
}
.heading h1 {
  @apply m-0 text-[21px] font-bold leading-8;
}
.heading p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.86);
}
.boxes {
  @apply absolute bottom-0 left-[7%] flex items-end gap-16;
}
.boxes i {
  @apply relative block h-[86px] w-[66px] border border-white/30;
  background: linear-gradient(90deg, #90604c, #ad7d62);
}
.boxes i:last-child {
  @apply h-[50px] w-[54px];
}
.boxes i::before,
.boxes i::after {
  @apply absolute bg-white/30 content-[''];
}
.boxes i::before {
  @apply left-1/2 top-0 h-full w-px;
}
.boxes i::after {
  @apply left-0 top-1/2 h-px w-full;
}
.boxes b {
  @apply absolute -right-9 -top-5 text-[10px] text-white/60;
}
.checklist {
  @apply absolute -top-4 left-[34%] h-[148px] w-[260px] rotate-[-1deg] border border-white/30 bg-[#E8E1DF]/90 p-7;
}
.checklist span {
  @apply mt-3 flex items-center gap-3;
}
.checklist i {
  @apply flex h-4 w-4 items-center justify-center rounded-sm border text-[#00875A];
  border-color: #00875a;
}
.checklist b {
  @apply h-1 flex-1 rounded bg-[#26385F]/40;
}
.pencil {
  @apply absolute bottom-2 left-[53%] h-2 w-24 bg-[#E2AF35];
  transform: rotate(1deg);
}
.pencil::before {
  @apply absolute -left-4 h-2 w-4 bg-[#C74436] content-[''];
}
.pencil::after {
  @apply absolute -right-4 border-y-4 border-l-[16px] border-y-transparent border-l-[#38465C] content-[''];
}
.progressCard {
  @apply absolute right-[13%] top-[17px] flex h-[84px] w-[152px] flex-col items-center justify-center rounded-xl text-white;
  background: rgba(255, 255, 255, 0.13);
}
.progressCard span {
  @apply text-[11px] font-semibold;
}
.progressCard strong {
  @apply text-[32px] leading-9;
}
.progressCard small {
  @apply text-[10px];
}
</style>
