<script setup lang="ts">
import { BadgeCheck, TrendingUp } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <p :class="$style.breadcrumb">财务管理 <span>›</span> 销售收入确认</p>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><TrendingUp
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>销售收入确认</h1>
          <p>按发货、里程碑、验收、周期确认销售收入，跟踪开票与回款</p>
        </div>
      </div>
    </div>
    <div
      :class="$style.chart"
      aria-hidden="true"
    >
      <i /><i /><i /><i /><i /><i /><i />
      <span />
    </div>
    <div
      :class="$style.confirmed"
      aria-hidden="true"
    >
      <BadgeCheck
        :size="60"
        :stroke-width="1.4"
      />
      <small>已确认</small>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative min-h-[120px] overflow-hidden rounded-lg px-7 py-4 text-white;
  background: linear-gradient(106deg, #006644 0%, #1fa86d 54%, #65d7dd 110%);
}
.copy {
  @apply relative z-10;
}
.breadcrumb {
  @apply m-0 text-[12px] font-semibold;
  color: rgb(255 255 255 / 75%);
}
.breadcrumb span {
  @apply px-1;
}
.identity {
  @apply mt-3 flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 24%);
  background: rgb(255 255 255 / 14%);
}
.identity h1,
.identity p {
  @apply m-0;
}
.identity h1 {
  @apply text-[22px] font-semibold;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 84%);
}
.chart {
  @apply absolute bottom-0 left-[35%] flex h-[92px] w-[56%] items-end gap-8;
}
.chart i {
  @apply block w-[38px];
  background: rgb(255 255 255 / 13%);
}
.chart i:nth-child(1) {
  height: 20px;
}
.chart i:nth-child(2) {
  height: 12px;
}
.chart i:nth-child(3) {
  height: 45px;
}
.chart i:nth-child(4) {
  height: 36px;
}
.chart i:nth-child(5) {
  height: 68px;
}
.chart i:nth-child(6) {
  height: 58px;
}
.chart i:nth-child(7) {
  height: 92px;
}
.chart span {
  @apply absolute bottom-[18px] left-0 right-0 h-0.5;
  background: #d9d483;
  transform: rotate(-12deg);
  transform-origin: left;
}
.confirmed {
  @apply absolute right-[11%] top-0 flex flex-col items-center;
  color: rgb(255 255 255 / 88%);
}
.confirmed small {
  @apply mt-1 text-[10px] font-semibold;
}
</style>
