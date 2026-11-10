<script setup lang="ts">
import { ChevronRight, FileText } from 'lucide-vue-next';

const dots = Array.from({ length: 18 }, (_, index) => ({
  x: 60 + ((index * 47) % 760),
  y: 60 + Math.floor((index * 47) / 760) * 80 + (index % 3) * 40,
  radius: 1.5 + (index % 3) * 0.5,
}));
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
        v-for="(dot, index) in dots"
        :key="index"
        :cx="dot.x"
        :cy="dot.y"
        :r="dot.radius"
        :class="$style.dot"
      />
      <circle
        cx="1300"
        cy="200"
        r="190"
        :class="$style.halo"
      />
      <g transform="translate(1080,210)">
        <rect
          x="0"
          y="-18"
          width="120"
          height="60"
          rx="6"
          :class="$style.sheetBack"
        />
        <rect
          x="14"
          y="-28"
          width="120"
          height="60"
          rx="6"
          :class="$style.sheetMiddle"
        />
        <g transform="translate(28,-22)">
          <rect
            x="0"
            y="-18"
            width="120"
            height="60"
            rx="6"
            :class="$style.sheetFront"
          />
          <rect
            x="10"
            y="-8"
            width="60"
            height="4"
            rx="2"
            :class="$style.sheetTitle"
          />
          <rect
            x="10"
            y="2"
            width="90"
            height="3"
            rx="1.5"
            :class="$style.sheetLine"
          />
          <rect
            x="10"
            y="11"
            width="76"
            height="3"
            rx="1.5"
            :class="$style.sheetLine"
          />
          <rect
            x="10"
            y="22"
            width="38"
            height="10"
            rx="3"
            :class="$style.sheetStatus"
          />
        </g>
      </g>
      <g transform="translate(1280,260)">
        <rect
          x="0"
          y="-30"
          width="20"
          height="30"
          rx="2"
          :class="$style.barOne"
        />
        <rect
          x="28"
          y="-50"
          width="20"
          height="50"
          rx="2"
          :class="$style.barTwo"
        />
        <rect
          x="56"
          y="-72"
          width="20"
          height="72"
          rx="2"
          :class="$style.barThree"
        />
        <rect
          x="84"
          y="-100"
          width="20"
          height="100"
          rx="2"
          :class="$style.barFour"
        />
      </g>
      <g
        transform="translate(1420,140)"
        :class="$style.arrow"
      >
        <path d="M0,40 L0,0" />
        <path d="M-12,12 L0,0 L12,12" />
      </g>
      <g transform="translate(1480,90)">
        <circle
          cx="0"
          cy="0"
          r="26"
          :class="$style.checkCircle"
        />
        <path
          d="M-10,2 L-3,9 L11,-7"
          :class="$style.check"
        />
      </g>
    </svg>
    <div :class="$style.overlay" />
    <div :class="$style.content">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>采购管理</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <strong>采购申请</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <FileText
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>采购申请</h1>
          <p>发起采购申请、审批流转、转询价/订单全流程跟踪</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(135deg, #0747a6 0%, #1b5bc5 60%, #2563eb 100%);
}
.scene,
.overlay {
  @apply pointer-events-none absolute inset-0 h-full w-full;
}
.overlay {
  background: linear-gradient(90deg, rgb(7 71 166 / 55%) 0%, rgb(7 71 166 / 18%) 50%, transparent 100%);
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
  border-color: rgb(255 255 255 / 20%);
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
.dot,
.sheetFront,
.barOne,
.barTwo,
.barThree,
.barFour,
.checkCircle {
  fill: #fff;
}
.dot {
  opacity: 0.18;
}
.halo {
  fill: #fff;
  opacity: 0.06;
}
.sheetBack {
  fill: #fff;
  opacity: 0.3;
}
.sheetMiddle {
  fill: #fff;
  opacity: 0.55;
}
.sheetFront,
.checkCircle {
  opacity: 0.95;
}
.sheetTitle {
  fill: #0747a6;
  opacity: 0.7;
}
.sheetLine {
  fill: #42526e;
  opacity: 0.45;
}
.sheetStatus {
  fill: #36b37e;
  opacity: 0.95;
}
.barOne {
  opacity: 0.35;
}
.barTwo {
  opacity: 0.45;
}
.barThree {
  opacity: 0.55;
}
.barFour {
  opacity: 0.7;
}
.arrow,
.check {
  fill: none;
  stroke: #fff;
  stroke-linecap: round;
  stroke-linejoin: round;
}
.arrow {
  stroke-width: 3;
}
.check {
  stroke: #0747a6;
  stroke-width: 4;
}
</style>
