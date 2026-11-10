<script setup lang="ts">
import { ChevronRight, Moon } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <svg
      :class="$style.skyline"
      viewBox="0 0 1600 180"
      preserveAspectRatio="none"
      aria-hidden="true"
    >
      <circle
        cx="1330"
        cy="22"
        r="33"
      />
      <path
        d="M0 180V140h55v40h70v-62h95v62h40v-84h70v84h100v-118h120v118h50v-76h82v76h115v-96h92v96h40v-70h100v70h115v-82h105v82h68v-55h88v55h42v-72h82v72Z"
      />
      <g>
        <rect
          x="480"
          y="88"
          width="6"
          height="8"
        />
        <rect
          x="512"
          y="88"
          width="6"
          height="8"
        />
        <rect
          x="544"
          y="88"
          width="6"
          height="8"
        />
        <rect
          x="1180"
          y="126"
          width="6"
          height="8"
        />
        <rect
          x="1212"
          y="126"
          width="6"
          height="8"
        />
        <rect
          x="1244"
          y="126"
          width="6"
          height="8"
        />
      </g>
    </svg>
    <div :class="$style.content">
      <nav :class="$style.breadcrumb">
        <span>考勤假期</span
        ><ChevronRight
          :size="12"
          :stroke-width="1.8"
        /><strong>加班申请</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><Moon
            :size="21"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>加班管理</h1>
          <p>提交加班申请、查看加班记录、审批团队成员加班</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(135deg, #07163c 0%, #0b347d 58%, #1559b5 100%);
}
.skyline {
  @apply absolute inset-0 h-full w-full;
}
.skyline circle {
  fill: #f4f5f7;
  opacity: 0.52;
  filter: drop-shadow(0 0 18px rgb(255 255 255 / 0.5));
}
.skyline path {
  fill: #061333;
  opacity: 0.96;
}
.skyline rect {
  fill: #ffd166;
  opacity: 0.86;
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
  background: rgb(255 255 255 / 0.13);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-1 text-[12px];
  color: rgb(255 255 255 / 0.88);
}
</style>
