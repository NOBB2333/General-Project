<script setup lang="ts">
import { ChevronRight, Warehouse } from 'lucide-vue-next';

const dots = Array.from({ length: 18 }, (_, index) => ({
  x: 60 + ((index * 47) % 760),
  y: 60 + Math.floor((index * 47) / 760) * 80 + (index % 3) * 40,
  radius: 1.5 + (index % 3) * 0.5,
}));

const shelves = [
  { x: 40, y: 70, width: 60, phase: 1 },
  { x: 110, y: 70, width: 60, phase: 2 },
  { x: 180, y: 70, width: 60, phase: 3 },
  { x: 250, y: 70, width: 60, phase: 4 },
  { x: 320, y: 70, width: 40, phase: 1 },
  { x: 40, y: 120, width: 60, phase: 2 },
  { x: 110, y: 120, width: 60, phase: 3 },
  { x: 180, y: 120, width: 60, phase: 4 },
  { x: 250, y: 120, width: 60, phase: 1 },
  { x: 320, y: 120, width: 40, phase: 2 },
] as const;
</script>

<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav
        :class="$style.breadcrumb"
        aria-label="面包屑"
      >
        <span>基础资料</span>
        <ChevronRight
          :size="12"
          :stroke-width="1.8"
        />
        <strong>仓库管理</strong>
      </nav>
      <div :class="$style.identity">
        <span :class="$style.icon">
          <Warehouse
            :size="22"
            :stroke-width="1.8"
          />
        </span>
        <div>
          <h1>仓库管理</h1>
          <p>管理主仓 / 项目仓 / 备品仓，掌握库存、采购、销售全景</p>
        </div>
      </div>
    </div>

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
        r="200"
        :class="$style.halo"
      />
      <g transform="translate(1080,130)">
        <path
          d="M0,40 L200,0 L400,40 L400,52 L0,52 Z"
          :class="$style.roof"
        />
        <rect
          x="20"
          y="52"
          width="360"
          height="160"
          rx="2"
          :class="$style.building"
        />
        <rect
          x="20"
          y="52"
          width="360"
          height="160"
          rx="2"
          :class="$style.buildingOutline"
        />
        <rect
          v-for="(shelf, index) in shelves"
          :key="index"
          :x="shelf.x"
          :y="shelf.y"
          :width="shelf.width"
          height="40"
          rx="3"
          :class="[$style.shelf, $style[`shelf${shelf.phase}`]]"
        />
        <rect
          x="20"
          y="170"
          width="360"
          height="6"
          :class="$style.floor"
        />
        <rect
          x="180"
          y="180"
          width="40"
          height="32"
          :class="$style.door"
        />
      </g>
      <g
        :class="$style.forklift"
        transform="translate(1480,300)"
      >
        <rect
          x="-14"
          y="-14"
          width="22"
          height="14"
          rx="2"
        />
        <rect
          x="6"
          y="-22"
          width="6"
          height="22"
        />
        <rect
          x="12"
          y="-22"
          width="14"
          height="3"
        />
        <circle
          cx="-8"
          cy="2"
          r="5"
          :class="$style.wheel"
        />
        <circle
          cx="6"
          cy="2"
          r="5"
          :class="$style.wheel"
        />
      </g>
      <g
        :class="$style.badge"
        transform="translate(1500,100)"
      >
        <rect
          x="-50"
          y="-14"
          width="100"
          height="28"
          rx="14"
        />
        <text
          x="0"
          y="5"
          text-anchor="middle"
        >
          仓库 12
        </text>
      </g>
    </svg>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden rounded-2xl px-6 py-5 text-white;
  background: linear-gradient(135deg, #003884 0%, #1b4f72 55%, #2563eb 100%);
}
.copy {
  @apply relative z-[2];
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
  border-color: rgb(255 255 255 / 22%);
  background: rgb(255 255 255 / 14%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
  letter-spacing: 0;
}
.identity p {
  @apply mt-1 text-[12.5px];
  color: rgb(255 255 255 / 85%);
}
.scene {
  @apply pointer-events-none absolute inset-0 h-full w-full;
}
.dot {
  fill: #fff;
  opacity: 0.16;
}
.halo {
  fill: #fff;
  opacity: 0.06;
}
.roof {
  fill: #fff;
  opacity: 0.92;
}
.building {
  fill: #fff;
  opacity: 0.18;
}
.buildingOutline {
  fill: none;
  stroke: #fff;
  stroke-width: 1.5;
  opacity: 0.45;
}
.shelf {
  fill: #fff;
}
.shelf1 {
  fill-opacity: 0.7;
}
.shelf2 {
  fill-opacity: 0.82;
}
.shelf3 {
  fill-opacity: 0.62;
}
.shelf4 {
  fill-opacity: 0.9;
}
.floor {
  fill: #fff;
  opacity: 0.4;
}
.door {
  fill: #fff;
  opacity: 0.45;
}
.forklift rect,
.badge rect {
  fill: #fff;
  opacity: 0.95;
}
.wheel {
  fill: #172b4d;
}
.badge text {
  @apply text-[12px] font-bold;
  fill: #003884;
}
@media (max-width: 900px) {
  .scene {
    @apply opacity-45;
  }
}
</style>
