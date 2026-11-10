<script setup lang="ts">
import { Undo2 } from 'lucide-vue-next';
</script>

<template>
  <header :class="$style.hero">
    <svg
      :class="$style.chart"
      viewBox="0 0 1600 400"
      preserveAspectRatio="xMidYMid slice"
      aria-hidden="true"
    >
      <defs>
        <linearGradient
          id="purchaseReturnArea"
          x1="0"
          y1="0"
          x2="0"
          y2="1"
        >
          <stop
            offset="0%"
            stop-color="#ffffff"
            stop-opacity=".45"
          />
          <stop
            offset="100%"
            stop-color="#ffffff"
            stop-opacity="0"
          />
        </linearGradient>
      </defs>
      <g :class="$style.gridLines">
        <line
          x1="980"
          y1="140"
          x2="1540"
          y2="140"
        />
        <line
          x1="980"
          y1="200"
          x2="1540"
          y2="200"
        />
        <line
          x1="980"
          y1="260"
          x2="1540"
          y2="260"
        />
      </g>
      <circle
        cx="1300"
        cy="200"
        r="190"
        fill="#ffffff"
        opacity=".06"
      />
      <path
        :class="$style.area"
        d="M980,140 L1060,160 L1140,180 L1220,200 L1300,220 L1380,250 L1460,275 L1540,295 L1540,320 L980,320 Z"
        fill="url(#purchaseReturnArea)"
      />
      <path
        :class="$style.line"
        d="M980,140 L1060,160 L1140,180 L1220,200 L1300,220 L1380,250 L1460,275 L1540,295"
      />
      <g :class="$style.points">
        <circle
          cx="980"
          cy="140"
          r="3.5"
        />
        <circle
          cx="1140"
          cy="180"
          r="3"
        />
        <circle
          cx="1300"
          cy="220"
          r="3"
        />
        <circle
          cx="1460"
          cy="275"
          r="3"
        />
      </g>
      <g
        :class="$style.arrow"
        transform="translate(1500,150)"
      >
        <path d="M0,0 L0,40" />
        <path d="M-10,28 L0,40 L10,28" />
      </g>
    </svg>
    <div :class="$style.overlay" />
    <div :class="$style.copy">
      <nav aria-label="面包屑"><span>采购管理</span><b aria-hidden="true">›</b><strong>采购退货</strong></nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><Undo2
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>采购退货</h1>
          <p>处理质量不符/规格异常的退货流程，跟进供应商退款</p>
        </div>
      </div>
    </div>
    <div :class="$style.rate">退货率 ↓ 26%</div>
  </header>
</template>

<style module>
@reference "../../../../styles/index.css";

.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(135deg, #bf2600 0%, #de350b 55%, #ff7452 100%);
}
.chart {
  @apply pointer-events-none absolute inset-0 h-full w-full;
}
.gridLines {
  stroke: #fff;
  stroke-width: 1;
  opacity: 0.08;
}
.area {
  animation: areaPulse 4s ease-in-out infinite;
}
.line {
  fill: none;
  stroke: #fff;
  stroke-width: 3;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-dasharray: 600;
  animation: lineFlow 6s ease-in-out infinite;
}
.points {
  fill: #fff;
  opacity: 0.9;
}
.arrow {
  fill: none;
  stroke: #fff;
  stroke-width: 3;
  stroke-linecap: round;
  stroke-linejoin: round;
  animation: arrowDown 2.5s ease-in-out infinite;
}
.overlay {
  @apply pointer-events-none absolute inset-0;
  background: linear-gradient(90deg, rgb(191 38 0 / 55%) 0%, rgb(191 38 0 / 18%) 50%, transparent 100%);
}
.copy {
  @apply relative z-[1];
}
.copy nav {
  @apply mb-3 flex items-center gap-1.5 text-[12px];
  color: rgb(255 255 255 / 75%);
}
.copy nav strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex items-center gap-3;
}
.icon {
  @apply flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 20%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-1 text-[12.5px];
  color: rgb(255 255 255 / 85%);
}
.rate {
  @apply absolute right-[12%] top-8 rounded-full px-4 py-1.5 text-[11px] font-bold;
  color: #36b37e;
  background: rgb(255 255 255 / 95%);
  animation: ratePulse 2.5s ease-in-out infinite;
}
@keyframes lineFlow {
  from {
    stroke-dashoffset: 600;
  }
  to {
    stroke-dashoffset: 0;
  }
}
@keyframes areaPulse {
  0%,
  100% {
    opacity: 0.18;
  }
  50% {
    opacity: 0.32;
  }
}
@keyframes arrowDown {
  0%,
  100% {
    opacity: 0.7;
    transform: translateY(0);
  }
  50% {
    opacity: 1;
    transform: translateY(4px);
  }
}
@keyframes ratePulse {
  0%,
  100% {
    opacity: 0.7;
  }
  50% {
    opacity: 1;
  }
}
@media (prefers-reduced-motion: reduce) {
  .area,
  .line,
  .arrow,
  .rate {
    animation: none;
  }
}
</style>
