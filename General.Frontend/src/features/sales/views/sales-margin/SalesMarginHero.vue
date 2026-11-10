<script setup lang="ts">
import { ChartNoAxesCombined, TrendingUp } from 'lucide-vue-next';
</script>
<template>
  <header :class="$style.hero">
    <div :class="$style.copy">
      <nav aria-label="面包屑"><span>销售业务</span><b aria-hidden="true">›</b><strong>销售毛利</strong></nav>
      <div :class="$style.identity">
        <span :class="$style.icon"
          ><ChartNoAxesCombined
            :size="22"
            :stroke-width="1.8"
        /></span>
        <div>
          <h1>销售毛利分析</h1>
          <p>收入/成本/毛利率多维度对比，洞察盈利能力与趋势</p>
        </div>
      </div>
    </div>
    <svg
      :class="$style.chart"
      viewBox="0 0 520 120"
      aria-hidden="true"
    >
      <g :class="$style.bars">
        <rect
          x="80"
          y="80"
          width="26"
          height="32"
        />
        <rect
          x="130"
          y="67"
          width="26"
          height="45"
        />
        <rect
          x="180"
          y="74"
          width="26"
          height="38"
        />
        <rect
          x="230"
          y="43"
          width="26"
          height="69"
        />
        <rect
          x="280"
          y="54"
          width="26"
          height="58"
        />
        <rect
          x="330"
          y="20"
          width="26"
          height="92"
        />
        <rect
          x="380"
          y="10"
          width="26"
          height="102"
        />
      </g>
      <polyline points="67,101 117,78 167,64 217,47 267,54 317,23 367,34 418,5" />
      <g :class="$style.points">
        <circle
          cx="67"
          cy="101"
          r="4"
        />
        <circle
          cx="117"
          cy="78"
          r="4"
        />
        <circle
          cx="167"
          cy="64"
          r="4"
        />
        <circle
          cx="217"
          cy="47"
          r="4"
        />
        <circle
          cx="267"
          cy="54"
          r="4"
        />
        <circle
          cx="317"
          cy="23"
          r="4"
        />
        <circle
          cx="367"
          cy="34"
          r="4"
        />
        <circle
          cx="418"
          cy="5"
          r="4"
        />
      </g>
    </svg>
    <TrendingUp
      :class="$style.arrow"
      :size="82"
      :stroke-width="1.8"
      aria-hidden="true"
    />
  </header>
</template>
<style module>
@reference "../../../../styles/index.css";
.hero {
  @apply relative mb-4 min-h-[120px] overflow-hidden px-6 py-5 text-white;
  border-radius: 14px;
  background: linear-gradient(108deg, #0747a6 0%, #0052cc 62%, #00b8d9 100%);
}
.hero::before {
  @apply pointer-events-none absolute inset-0;
  content: '';
  background: radial-gradient(circle at 18% 8%, rgb(255 255 255 / 14%) 0 2px, transparent 3px);
  background-size: 120px 120px;
}
.copy {
  @apply relative z-[2];
}
.copy nav {
  @apply mb-3 flex items-center gap-1.5 text-[12px];
  color: rgb(255 255 255 / 78%);
}
.copy nav strong {
  @apply font-semibold text-white;
}
.identity {
  @apply flex items-center gap-3;
}
.icon {
  @apply flex h-11 w-11 items-center justify-center rounded-xl border;
  border-color: rgb(255 255 255 / 22%);
  background: rgb(255 255 255 / 15%);
}
.identity h1 {
  @apply text-[22px] font-bold leading-7;
}
.identity p {
  @apply mt-1 text-[12.5px];
  color: rgb(255 255 255 / 85%);
}
.chart {
  @apply absolute bottom-0 right-20 h-[120px] w-[520px] opacity-75;
}
.chart polyline {
  fill: none;
  stroke: white;
  stroke-width: 3;
  stroke-dasharray: 7 5;
}
.bars rect {
  fill: rgb(255 255 255 / 35%);
}
.points circle {
  fill: #deebff;
}
.arrow {
  @apply absolute right-6 top-5 opacity-80;
}
</style>
