<script setup lang="ts">
import { MessageSquareMore } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>CRM客户管理</span><span aria-hidden="true">›</span><strong>跟进记录</strong>
      </nav>
      <div :class="$style.titleRow">
        <span :class="$style.titleIcon"><MessageSquareMore :size="22" /></span>
        <div>
          <h1>跟进记录管理</h1>
          <p>记录和管理所有客户及商机的跟进活动</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.scene"
      aria-hidden="true"
    >
      <span :class="[$style.bar, $style.barOne]" />
      <span :class="[$style.bar, $style.barTwo]" />
      <span :class="[$style.bar, $style.barThree]" />
      <span :class="[$style.bar, $style.barFour]" />
      <span :class="[$style.bar, $style.barFive]" />
      <span :class="[$style.bar, $style.barSix]" />
      <span :class="[$style.bar, $style.barSeven]" />
      <span :class="[$style.bar, $style.barEight]" />
      <span :class="[$style.bar, $style.barNine]" />
      <span :class="[$style.bar, $style.barTen]" />
      <span :class="[$style.bar, $style.barEleven]" />
      <span :class="$style.chartLine" />
      <span :class="$style.target"><i /><i /><i /><b /></span>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(135deg, #00875a 0%, #0aa8c9 45%, #5243aa 100%);
}
.hero::after {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background: linear-gradient(90deg, rgb(0 135 90 / 55%) 0%, rgb(0 135 90 / 18%) 50%, transparent 100%);
}
.copy {
  @apply relative z-20;
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
  @apply flex h-[38px] w-[38px] shrink-0 items-center justify-center rounded border;
  border-color: rgb(255 255 255 / 30%);
  background: rgb(255 255 255 / 14%);
}
.titleRow h1 {
  @apply text-[22px] font-semibold;
}
.titleRow p {
  @apply mt-1 text-[13px];
  color: rgb(255 255 255 / 80%);
}
.scene {
  @apply pointer-events-none absolute inset-0 z-10 overflow-hidden;
}
.bar {
  @apply absolute bottom-0 w-10 rounded-t;
  background: linear-gradient(to bottom, #79e2f2, rgb(10 168 201 / 45%));
  opacity: 0.85;
}
.barOne {
  left: 13.75%;
  height: 15%;
}
.barTwo {
  left: 16.875%;
  height: 20%;
}
.barThree {
  left: 20%;
  height: 23.75%;
}
.barFour {
  left: 23.125%;
  height: 30%;
}
.barFive {
  left: 26.25%;
  height: 32.5%;
}
.barSix {
  left: 29.375%;
  height: 38.75%;
}
.barSeven {
  left: 32.5%;
  height: 41.25%;
}
.barEight {
  left: 35.625%;
  height: 47.5%;
}
.barNine {
  left: 38.75%;
  height: 52.5%;
}
.barTen {
  left: 41.875%;
  height: 55%;
}
.barEleven {
  left: 45%;
  height: 60%;
}
.chartLine {
  @apply absolute h-0.5 origin-left rotate-[-18deg];
  left: 16%;
  top: 67%;
  width: 31%;
  background: linear-gradient(to right, #79e2f2, #ffe066);
}
.target {
  @apply absolute right-[17%] top-1/2 h-[106px] w-[106px] -translate-y-1/2 rounded-full border-2;
  border-color: rgb(255 255 255 / 46%);
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
</style>
