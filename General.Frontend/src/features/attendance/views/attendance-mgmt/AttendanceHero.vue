<script setup lang="ts">
import { ChevronRight, Clock3 } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <svg
      :class="$style.scene"
      viewBox="0 0 1600 180"
      preserveAspectRatio="none"
      aria-hidden="true"
    >
      <g>
        <line
          x1="1050"
          y1="35"
          x2="1050"
          y2="155"
        />
        <line
          x1="990"
          y1="95"
          x2="1110"
          y2="95"
        />
        <circle
          cx="1050"
          cy="95"
          r="100"
        />
        <line
          x1="1050"
          y1="95"
          x2="1050"
          y2="32"
        />
        <line
          x1="1050"
          y1="95"
          x2="1000"
          y2="95"
        />
      </g>
      <path d="M0 170 L180 20 M85 180 L330 5 M180 180 L500 45" />
    </svg>
    <div :class="$style.content">
      <nav :class="$style.breadcrumb">
        <span>考勤假期</span
        ><ChevronRight
          :size="12"
          :stroke-width="1.8"
        /><strong>考勤管理</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><Clock3
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>考勤管理</h1>
          <p>查看每日打卡记录、筛选异常考勤、提交补签纠正申请</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(115deg, #2d75ce 0%, #46a1d4 52%, #ffc04f 100%);
}
.scene {
  @apply absolute inset-0 h-full w-full;
}
.scene g {
  fill: none;
  stroke: rgb(255 255 255 / 0.5);
  stroke-width: 3;
}
.scene circle {
  stroke-width: 2;
}
.scene path {
  fill: none;
  stroke: rgb(255 255 255 / 0.2);
  stroke-width: 2;
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
  border-color: rgb(255 255 255 / 0.3);
  background: rgb(255 255 255 / 0.16);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 0.9);
}
</style>
