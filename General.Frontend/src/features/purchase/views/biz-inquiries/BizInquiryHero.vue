<script setup lang="ts">
import { BriefcaseBusiness, ChevronRight } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <svg
      :class="$style.scene"
      viewBox="0 0 1600 400"
      preserveAspectRatio="xMidYMid slice"
      aria-hidden="true"
    >
      <circle
        cx="1300"
        cy="205"
        r="190"
        :class="$style.halo"
      />
      <path
        d="M980 330 L1100 300 L1210 250 L1320 210 L1440 140 L1580 78"
        :class="$style.area"
      />
      <path
        d="M980 330 L1100 300 L1210 250 L1320 210 L1440 140 L1580 78"
        :class="$style.line"
      />
      <circle
        v-for="point in [
          [1100, 300],
          [1210, 250],
          [1320, 210],
          [1440, 140],
        ]"
        :key="point[0]"
        :cx="point[0]"
        :cy="point[1]"
        r="4"
        :class="$style.dot"
      />
    </svg>
    <div :class="$style.overlay" />
    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>采购管理</span
        ><ChevronRight
          :size="12"
          :stroke-width="1.8"
        /><strong>业务询价</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><BriefcaseBusiness
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>业务询价</h1>
          <p>销售团队提交询价需求，采购人员协作报价处理与反馈</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(135deg, #c96600 0%, #e87900 58%, #ff971d 100%);
}
.scene,
.overlay {
  @apply pointer-events-none absolute inset-0 h-full w-full;
}
.overlay {
  background: linear-gradient(90deg, rgb(173 77 0 / 45%) 0%, rgb(173 77 0 / 12%) 55%, transparent 100%);
}
.content {
  @apply relative px-6 py-5;
}
.breadcrumb {
  @apply mb-3 flex items-center gap-1 text-[12px] font-medium;
  color: rgb(255 255 255 / 75%);
}
.breadcrumb svg {
  color: rgb(255 255 255 / 50%);
}
.breadcrumb strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex min-w-0 items-center gap-3;
}
.icon {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 24%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
  letter-spacing: 0;
}
.identity p {
  @apply mt-1 text-[12.5px];
  color: rgb(255 255 255 / 85%);
}
.halo {
  fill: #fff;
  opacity: 0.06;
}
.area {
  fill: none;
  stroke: rgb(255 255 255 / 12%);
  stroke-width: 75;
}
.line {
  fill: none;
  stroke: white;
  stroke-width: 4;
  stroke-linecap: round;
  stroke-linejoin: round;
}
.dot {
  fill: white;
}
</style>
