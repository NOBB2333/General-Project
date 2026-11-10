<script setup lang="ts">
import { BarChart3, ChevronRight } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <div
      :class="$style.grid"
      aria-hidden="true"
    />
    <div
      :class="$style.trend"
      aria-hidden="true"
    />
    <div
      :class="$style.bars"
      aria-hidden="true"
    >
      <i /><i /><i /><i /><i />
    </div>
    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>考勤假期</span><ChevronRight :size="12" /><strong>考勤统计</strong>
      </nav>
      <div :class="$style.heading">
        <span :class="$style.icon"
          ><BarChart3
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>考勤统计分析</h1>
          <p>考勤数据可视化分析、请假出差趋势、部门维度图表</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative mb-4 h-[118px] overflow-hidden rounded-2xl;
  background: linear-gradient(105deg, #102b68 0%, #0f377e 35%, #2366ad 70%, #4d3cac 100%);
}
.grid {
  @apply absolute inset-0 opacity-30;
  background-image:
    linear-gradient(rgba(255, 255, 255, 0.12) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.12) 1px, transparent 1px);
  background-size: 68px 42px;
}
.trend {
  @apply absolute bottom-0 left-[30%] h-[102px] w-[70%] opacity-70;
  background: rgba(92, 167, 224, 0.34);
  clip-path: polygon(0 100%, 0 91%, 14% 76%, 27% 66%, 39% 61%, 53% 42%, 66% 31%, 78% 24%, 91% 12%, 100% 0, 100% 100%);
  filter: drop-shadow(0 -2px 0 #c5cf70);
}
.bars {
  @apply absolute bottom-0 right-12 flex h-14 items-end gap-20 opacity-45;
}
.bars i {
  @apply block w-8 rounded-t bg-[#54c7ee];
}
.bars i:nth-child(1) {
  height: 25px;
}
.bars i:nth-child(2) {
  height: 43px;
}
.bars i:nth-child(3) {
  height: 33px;
  background: #f2dd6a;
}
.bars i:nth-child(4) {
  height: 54px;
}
.bars i:nth-child(5) {
  height: 28px;
}
.content {
  @apply relative px-6 py-5;
}
.breadcrumb {
  @apply flex items-center gap-1 text-[11px] font-semibold;
  color: rgba(255, 255, 255, 0.76);
}
.breadcrumb strong {
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
  @apply m-0 text-[21px] font-bold leading-8 text-white;
}
.heading p {
  @apply m-0 text-[12px] leading-5;
  color: rgba(255, 255, 255, 0.88);
}
</style>
