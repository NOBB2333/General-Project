<script setup lang="ts">
import { ChevronRight, Umbrella } from 'lucide-vue-next';
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
        cx="1320"
        cy="45"
        r="48"
      />
      <path d="M0 162 C130 150 220 172 350 158 S590 170 760 158 S1000 173 1180 158 S1450 170 1600 158" />
      <path d="M110 165 L145 42 L154 165 Z" />
      <path d="M147 54 C85 33 67 39 28 61 C80 59 103 70 143 80 Z" />
      <path d="M150 55 C198 18 250 22 283 35 C230 43 207 59 158 82 Z" />
      <path d="M150 56 C129 10 98 2 67 0 C103 27 119 47 147 84 Z" />
    </svg>
    <div :class="$style.content">
      <nav :class="$style.breadcrumb">
        <span>考勤假期</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <strong>请假管理</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><Umbrella
            :size="23"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>请假管理</h1>
          <p>管理您的假期余额、提交请假申请、审批团队成员请假</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(180deg, #56cce2 0%, #33a9cf 52%, #08729c 100%);
}
.scene {
  @apply pointer-events-none absolute inset-0 h-full w-full;
}
.scene circle {
  fill: #fff4a8;
  stroke: #ffe36b;
  stroke-width: 14;
}
.scene path:first-of-type {
  fill: none;
  stroke: rgb(255 255 255 / 0.48);
  stroke-width: 2;
}
.scene path:not(:first-of-type) {
  fill: #147a81;
}
.content {
  @apply relative px-6 py-5;
}
.breadcrumb {
  @apply mb-3 flex items-center gap-1 text-[12px] font-medium;
  color: rgb(255 255 255 / 0.78);
}
.breadcrumb strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex items-center gap-3;
}
.icon {
  @apply flex h-10 w-10 items-center justify-center rounded-lg border;
  border-color: rgb(255 255 255 / 0.25);
  background: rgb(255 255 255 / 0.16);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 0.88);
}
</style>
