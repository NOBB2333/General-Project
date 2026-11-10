<script setup lang="ts">
import { BarChart3 } from 'lucide-vue-next';
</script>

<template>
  <section :class="$style.hero">
    <div :class="$style.copy">
      <p>财务管理 <span>›</span> 财务管理报告</p>
      <div :class="$style.titleRow">
        <span :class="$style.icon"
          ><BarChart3
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>财务管理报告</h1>
          <p>全面了解公司经营状况 · 数据截至 2026年2月</p>
        </div>
      </div>
    </div>
    <div :class="$style.visual">
      <div :class="$style.ring"><small>利润率</small><strong>28.5%</strong><span>↑ 4.2pt YoY</span></div>
      <div :class="$style.bars">
        <i
          v-for="height in [18, 35, 26, 48, 39, 61, 52, 78]"
          :key="height"
        />
      </div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply grid h-[120px] grid-cols-[400px_1fr] overflow-hidden rounded-lg text-white;
  background: linear-gradient(105deg, #091e58, #18549b 55%, #00afc8);
}
.copy {
  @apply px-7 py-4;
  background: rgb(9 30 88 / 72%);
}
.copy > p {
  @apply m-0 text-[10px] font-semibold text-white/75;
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
  @apply m-0 text-[23px] font-semibold;
}
.titleRow p {
  @apply m-0 mt-1 text-[11px] text-white/75;
}
.visual {
  @apply relative flex items-center justify-center overflow-hidden;
}
.ring {
  @apply flex h-[144px] w-[144px] flex-col items-center justify-center rounded-full border-[12px] border-white/15;
  box-shadow: -8px 9px 0 -2px #e4c94f;
}
.ring small {
  @apply text-[10px];
}
.ring strong {
  @apply text-[27px];
  color: #f4d64e;
}
.ring span {
  @apply text-[8px] font-semibold text-white/65;
}
.bars {
  @apply absolute bottom-0 right-20 flex h-[96px] w-[250px] items-end gap-9;
}
.bars i {
  @apply w-6 rounded-t bg-cyan-200/55;
}
.bars i:nth-child(1) {
  height: 18%;
}
.bars i:nth-child(2) {
  height: 35%;
}
.bars i:nth-child(3) {
  height: 26%;
}
.bars i:nth-child(4) {
  height: 48%;
}
.bars i:nth-child(5) {
  height: 39%;
}
.bars i:nth-child(6) {
  height: 61%;
}
.bars i:nth-child(7) {
  height: 52%;
}
.bars i:nth-child(8) {
  height: 78%;
}
</style>
