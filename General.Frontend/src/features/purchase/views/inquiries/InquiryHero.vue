<script setup lang="ts">
import { BadgeDollarSign, ChevronRight } from 'lucide-vue-next';
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
        cy="200"
        r="190"
        :class="$style.halo"
      />
      <g transform="translate(1020,300)">
        <line
          x1="0"
          y1="0"
          x2="460"
          y2="0"
          :class="$style.axis"
        />
        <rect
          x="20"
          y="-150"
          width="46"
          height="150"
          rx="4"
          :class="$style.barOne"
        />
        <rect
          x="92"
          y="-130"
          width="46"
          height="130"
          rx="4"
          :class="$style.barTwo"
        />
        <rect
          x="164"
          y="-115"
          width="46"
          height="115"
          rx="4"
          :class="$style.barThree"
        />
        <rect
          x="236"
          y="-100"
          width="46"
          height="100"
          rx="4"
          :class="$style.barFour"
        />
        <rect
          x="308"
          y="-78"
          width="46"
          height="78"
          rx="4"
          :class="$style.bestBar"
        />
        <rect
          x="304"
          y="-82"
          width="54"
          height="86"
          rx="6"
          :class="$style.bestOutline"
        />
        <path
          d="M331,-100 L335,-92 L344,-91 L337,-85 L339,-77 L331,-81 L323,-77 L325,-85 L318,-91 L327,-92 Z"
          :class="$style.star"
        />
      </g>
      <g
        transform="translate(1500,160)"
        :class="$style.downArrow"
      >
        <path d="M0,0 L0,40" />
        <path d="M-10,28 L0,40 L10,28" />
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
        <strong>询价管理</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <BadgeDollarSign
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>询价管理</h1>
          <p>发起询价、收集报价、多供应商比价择优</p>
        </div>
      </div>
    </div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-2xl text-white;
  background: linear-gradient(135deg, #1b4f72 0%, #1b7193 55%, #0aa8c9 100%);
}
.scene,
.overlay {
  @apply pointer-events-none absolute inset-0 h-full w-full;
}
.overlay {
  background: linear-gradient(90deg, rgb(27 79 114 / 55%) 0%, rgb(27 79 114 / 18%) 50%, transparent 100%);
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
.halo {
  fill: #fff;
  opacity: 0.06;
}
.axis {
  stroke: #fff;
  stroke-width: 1.5;
  opacity: 0.4;
}
.barOne,
.barTwo,
.barThree,
.barFour,
.bestBar {
  fill: #fff;
}
.barOne {
  opacity: 0.45;
}
.barTwo {
  opacity: 0.55;
}
.barThree {
  opacity: 0.6;
}
.barFour {
  opacity: 0.7;
}
.bestBar {
  opacity: 0.95;
}
.bestOutline {
  fill: none;
  stroke: #ffe066;
  stroke-width: 2.5;
}
.star {
  fill: #ffe066;
}
.downArrow {
  fill: none;
  stroke: #ffe066;
  stroke-width: 3;
  stroke-linecap: round;
  stroke-linejoin: round;
}
</style>
