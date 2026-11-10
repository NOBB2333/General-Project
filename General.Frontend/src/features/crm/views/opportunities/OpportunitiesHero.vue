<script setup lang="ts">
import { Target, TrendingUp } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>CRM客户管理</span><span aria-hidden="true">›</span><strong>商机管理</strong>
      </nav>
      <div :class="$style.titleRow">
        <span :class="$style.titleIcon"
          ><Target
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>商机管理</h1>
          <p>跟踪和管理所有销售商机，推进成交转化</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="$style.chartLine" />
      <span :class="[$style.bar, $style.barOne]" />
      <span :class="[$style.bar, $style.barTwo]" />
      <span :class="[$style.bar, $style.barThree]" />
      <span :class="[$style.bar, $style.barFour]" />
      <span :class="$style.trend"><TrendingUp :size="34" /></span>
      <span :class="$style.target"><i /><i /><i /><b /></span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[119px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(102deg, #408c69 0%, #40907e 20%, #579caa 50%, #4785ab 68%, #436cab 85%, #4952a8 100%);
}
.hero::after {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background: linear-gradient(90deg, rgb(9 70 70 / 4%) 0%, transparent 62%);
}
.copy {
  @apply relative z-10;
}
.breadcrumb {
  @apply mb-3 flex items-center gap-2 text-[12px];
  color: rgb(255 255 255 / 74%);
}
.breadcrumb strong {
  color: white;
}
.titleRow {
  @apply flex items-center gap-3;
}
.titleIcon {
  @apply flex h-[38.5px] w-[38.5px] shrink-0 items-center justify-center rounded border;
  border-color: rgb(255 255 255 / 30%);
  background: rgb(255 255 255 / 12%);
}
.titleRow h1 {
  @apply text-[22px] font-semibold;
}
.titleRow p {
  @apply mt-1 text-[13px];
  color: rgb(255 255 255 / 80%);
}
.scene {
  @apply pointer-events-none absolute inset-y-0 right-0 w-[86%] overflow-hidden;
}
.bar {
  @apply absolute bottom-0 w-10;
  background: rgb(255 255 255 / 17%);
}
.barOne {
  left: 24%;
  height: 52%;
}
.barTwo {
  left: 30%;
  height: 66%;
}
.barThree {
  left: 36%;
  height: 82%;
}
.barFour {
  left: 42%;
  height: 110%;
}
.chartLine {
  @apply absolute h-0.5 origin-left rotate-[-18deg];
  left: 20%;
  top: 68%;
  width: 54%;
  background: #ffe380;
}
.trend {
  @apply absolute text-white opacity-60;
  left: 52%;
  top: 18px;
}
.target {
  @apply absolute right-[16%] top-1/2 h-[106px] w-[106px] -translate-y-1/2 rounded-full border-2;
  border-color: rgb(255 255 255 / 44%);
}
.target i,
.target b {
  @apply absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 rounded-full border-2;
  border-color: rgb(255 255 255 / 54%);
}
.target i:first-child {
  @apply h-[78px] w-[78px];
}
.target i:nth-child(2) {
  @apply h-[52px] w-[52px];
  border-color: #ffe380;
}
.target i:nth-child(3) {
  @apply h-5 w-5 border-0;
  background: #ffe380;
}
.target b {
  @apply h-3 w-3 border-0;
  background: #de350b;
}
@media (max-width: 900px) {
  .scene {
    @apply opacity-35;
  }
}
</style>
