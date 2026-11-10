<script setup lang="ts">
import { ArrowUpRight, ChartNoAxesCombined } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import { salesBoardTrend } from '../../model/sales-board.data';

const emit = defineEmits<{ open: [] }>();
const maxValue = 80;

function pointX(index: number) {
  return 74 + index * 146;
}

function pointY(value: number) {
  return 226 - (value / maxValue) * 180;
}

function linePath(key: 'performance' | 'contracts') {
  return salesBoardTrend
    .map((point, index) => `${index === 0 ? 'M' : 'L'} ${pointX(index)} ${pointY(point[key])}`)
    .join(' ');
}
</script>

<template>
  <article :class="$style.panel">
    <header :class="$style.header">
      <span :class="$style.icon">
        <ChartNoAxesCombined
          :size="17"
          :stroke-width="1.8"
        />
      </span>
      <div>
        <h2>业绩 / 合同 / 回款 趋势</h2>
        <p>过去 6 个月</p>
      </div>
      <UiButton
        :class="$style.detailButton"
        variant="link"
        size="xs"
        @click="emit('open')"
      >
        详情
        <template #trailing>
          <ArrowUpRight
            :size="13"
            :stroke-width="1.8"
          />
        </template>
      </UiButton>
    </header>

    <div :class="$style.chartWrap">
      <svg
        viewBox="0 0 880 270"
        role="img"
        aria-label="过去六个月业绩、合同和回款趋势"
        :class="$style.chart"
      >
        <g :class="$style.gridLines">
          <line
            v-for="value in [0, 20, 40, 60, 80]"
            :key="value"
            x1="48"
            x2="854"
            :y1="pointY(value)"
            :y2="pointY(value)"
          />
        </g>
        <g :class="$style.axisLabels">
          <text
            v-for="value in [0, 20, 40, 60, 80]"
            :key="value"
            x="39"
            :y="pointY(value) + 4"
            text-anchor="end"
          >
            {{ value }}万
          </text>
        </g>
        <g :class="$style.bars">
          <rect
            v-for="(point, index) in salesBoardTrend"
            :key="point.month"
            :x="pointX(index) - 12"
            :y="pointY(point.collections)"
            width="24"
            :height="226 - pointY(point.collections)"
            rx="5"
          />
        </g>
        <path
          :d="linePath('performance')"
          :class="$style.performanceLine"
        />
        <path
          :d="linePath('contracts')"
          :class="$style.contractLine"
        />
        <g :class="$style.monthLabels">
          <text
            v-for="(point, index) in salesBoardTrend"
            :key="point.month"
            :x="pointX(index)"
            y="252"
            text-anchor="middle"
          >
            {{ point.month }}
          </text>
        </g>
      </svg>
      <div
        :class="$style.legend"
        aria-label="趋势图图例"
      >
        <span data-series="performance"><i />确认业绩</span>
        <span data-series="contracts"><i />合同总额</span>
        <span data-series="collections"><i />实际回款</span>
      </div>
    </div>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply min-w-0 rounded border;
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.header {
  @apply flex min-h-[58px] items-center gap-3 px-5 py-3;
}

.icon {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.header h2 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.header p {
  @apply mt-0.5 text-[11px];
  color: var(--ui-text-subtle);
}

.detailButton {
  @apply ml-auto;
}

.chartWrap {
  @apply px-4 pb-3;
}

.chart {
  @apply block w-full;
  height: 270px;
}

.gridLines line {
  stroke: var(--ui-border-subtle);
  stroke-dasharray: 3 4;
}

.axisLabels,
.monthLabels {
  fill: var(--ui-text-subtle);
  font-size: 10px;
}

.bars rect {
  fill: #36b37e;
}

.performanceLine,
.contractLine {
  fill: none;
  stroke-width: 2.5;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.performanceLine {
  stroke: var(--app-primary);
}

.contractLine {
  stroke: var(--ui-warning);
  stroke-dasharray: 7 5;
}

.legend {
  @apply flex items-center justify-center gap-5 text-[10px];
  color: var(--ui-text-subtle);
}

.legend span {
  @apply inline-flex items-center gap-1.5;
}

.legend i {
  @apply block h-0.5 w-4;
  background: var(--app-primary);
}

.legend span[data-series='contracts'] i {
  background: var(--ui-warning);
}

.legend span[data-series='collections'] i {
  @apply h-2 w-2 rounded-sm;
  background: #36b37e;
}
</style>
