<script setup lang="ts">
import { ChevronRight, Plane } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <svg
      :class="$style.scene"
      viewBox="0 0 1600 180"
      preserveAspectRatio="none"
      aria-hidden="true"
    >
      <circle
        cx="1260"
        cy="142"
        r="45"
      />
      <path d="M0 180 L370 58 L510 151 L650 84 L820 169 L975 99 L1120 170 L1325 80 L1600 170 Z" />
      <path
        d="M0 180 L120 147 L250 180 L390 140 L500 180 L640 152 L770 180 L920 158 L1070 180 L1210 148 L1370 180 L1510 150 L1600 180 Z"
      />
      <path d="M630 35 H1020" />
    </svg>
    <Plane
      :class="$style.flight"
      :size="34"
      :stroke-width="1.8"
      aria-hidden="true"
    />
    <div :class="$style.content">
      <nav :class="$style.breadcrumb">
        <span>考勤假期</span
        ><ChevronRight
          :size="12"
          :stroke-width="1.8"
        /><strong>出差申请</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><Plane
            :size="21"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>出差管理</h1>
          <p>查看出差记录、申请备用金、提交报销、审批团队成员出差</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(180deg, #ff9877 0%, #f5b46d 45%, #7896be 100%);
}
.scene {
  @apply absolute inset-0 h-full w-full;
}
.scene circle {
  fill: #ffe47a;
  opacity: 0.76;
}
.scene path:first-of-type {
  fill: #516896;
  opacity: 0.95;
}
.scene path:nth-of-type(2) {
  fill: #334a79;
  opacity: 0.9;
}
.scene path:last-of-type {
  fill: none;
  stroke: rgb(255 255 255 / 0.55);
  stroke-width: 2;
}
.flight {
  @apply absolute left-[46%] top-4 rotate-[-8deg];
  color: rgb(255 255 255 / 0.7);
}
.content {
  @apply relative px-6 py-5;
}
.breadcrumb {
  @apply mb-3 flex items-center gap-1 text-[12px] font-medium;
  color: rgb(255 255 255 / 0.8);
}
.breadcrumb strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 0.3);
  background: rgb(255 255 255 / 0.17);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 0.9);
}
</style>
