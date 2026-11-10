<script setup lang="ts">
import { FileChartColumnIncreasing } from 'lucide-vue-next';
import type { SalesTargetTeamRow, SalesTargetUnit } from '../../model/sales-targets.types';

const props = defineProps<{
  teams: readonly SalesTargetTeamRow[];
  unit: SalesTargetUnit;
  formatAmount: (valueWan: number, unit: SalesTargetUnit, precision?: number) => string;
}>();

const emit = defineEmits<{ select: [row: SalesTargetTeamRow] }>();
const scaleValues = [0, 250, 500, 750, 1000] as const;
const chartBottom = 228;
const chartHeight = 223;
const maxTarget = 1000;

function groupX(index: number) {
  return 139 + index * 185;
}

function barHeight(value: number) {
  return (value / maxTarget) * chartHeight;
}

function barY(value: number) {
  return chartBottom - barHeight(value);
}

function scaleY(value: number) {
  return chartBottom - (value / maxTarget) * chartHeight;
}
</script>

<template>
  <article
    :class="$style.panel"
    data-target-team-comparison
  >
    <header :class="$style.header">
      <FileChartColumnIncreasing
        :size="16"
        :stroke-width="1.8"
      />
      <h2>各团队业绩目标对比</h2>
    </header>

    <svg
      :class="$style.chart"
      viewBox="0 0 820 280"
      role="img"
      aria-label="各团队目标金额与实际完成金额柱状图"
    >
      <g :class="$style.gridLines">
        <line
          v-for="value in scaleValues"
          :key="value"
          x1="66"
          x2="794"
          :y1="scaleY(value)"
          :y2="scaleY(value)"
        />
      </g>
      <g :class="$style.scaleLabels">
        <text
          v-for="value in scaleValues"
          :key="value"
          x="58"
          :y="scaleY(value) + 4"
          text-anchor="end"
        >
          {{ value }}万
        </text>
      </g>
      <g
        v-for="(team, index) in teams"
        :key="team.id"
      >
        <rect
          :class="$style.targetBar"
          :x="groupX(index) - 48"
          :y="barY(team.targetWan)"
          width="72"
          :height="barHeight(team.targetWan)"
          rx="3"
        />
        <rect
          :class="$style.actualBar"
          :x="groupX(index) + 28"
          :y="barY(team.actualWan)"
          width="72"
          :height="barHeight(team.actualWan)"
          rx="3"
        />
        <text
          :class="$style.teamLabel"
          :x="groupX(index) + 26"
          y="254"
          text-anchor="middle"
        >
          {{ team.name }}
        </text>
      </g>
    </svg>

    <div
      :class="$style.legend"
      aria-label="图表图例"
    >
      <span data-series="target"><i />目标</span>
      <span data-series="actual"><i />实际</span>
    </div>

    <div :class="$style.teamGrid">
      <button
        v-for="team in teams"
        :key="team.id"
        type="button"
        :class="$style.teamCard"
        @click="emit('select', team)"
      >
        <span :class="$style.teamHeading">
          <strong>{{ team.name }}</strong>
          <em>{{ team.rate.toFixed(1) }}%</em>
        </span>
        <progress
          :value="team.rate"
          max="100"
          :aria-label="`${team.name}完成率 ${team.rate.toFixed(1)}%`"
        />
        <small>
          {{ props.formatAmount(team.actualWan, unit, team.actualPrecision) }} /
          {{ props.formatAmount(team.targetWan, unit, 1) }} · {{ team.memberCount }}人
        </small>
      </button>
    </div>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply min-w-0 overflow-hidden rounded border;
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.header {
  @apply mx-5 mb-4 mt-5 flex h-[21px] items-center gap-2;
  color: var(--app-primary);
}

.header h2 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.chart {
  @apply mx-5 block;
  width: calc(100% - 35px);
  height: 280px;
}

.gridLines line {
  stroke: var(--ui-border-subtle);
  stroke-dasharray: 3 4;
}

.scaleLabels,
.teamLabel {
  fill: var(--ui-text-subtle);
  font-size: 10px;
}

.targetBar {
  fill: #b3d4ff;
}

.actualBar {
  fill: var(--app-primary);
}

.legend {
  @apply flex items-center justify-center gap-5 pb-3 text-[10px];
  color: var(--ui-text-subtle);
}

.legend span {
  @apply inline-flex items-center gap-1.5;
}

.legend i {
  @apply h-3 w-3 rounded-sm;
  background: #b3d4ff;
}

.legend span[data-series='actual'] i {
  background: var(--app-primary);
}

.teamGrid {
  @apply grid grid-cols-2 gap-2 border-t p-5;
  border-color: var(--ui-border-subtle);
}

.teamCard {
  @apply rounded border p-3 text-left outline-none transition-colors;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface);
}

.teamCard:hover {
  background: var(--ui-surface-hover);
}

.teamCard:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.teamHeading {
  @apply flex items-center justify-between gap-3;
}

.teamHeading strong {
  @apply truncate text-[11px] font-semibold;
}

.teamHeading em {
  @apply shrink-0 text-[11px] font-semibold not-italic;
  color: var(--ui-danger);
}

.teamCard progress {
  @apply mt-2 block h-1 w-full overflow-hidden rounded-full border-0;
  color: var(--app-primary);
  background: var(--ui-border-subtle);
}

.teamCard progress::-webkit-progress-bar {
  background: var(--ui-border-subtle);
}

.teamCard progress::-webkit-progress-value {
  background: currentColor;
}

.teamCard progress::-moz-progress-bar {
  background: currentColor;
}

.teamCard small {
  @apply mt-1.5 block text-[9px];
  color: var(--ui-text-subtle);
}

@media (prefers-reduced-motion: reduce) {
  .teamCard {
    transition: none;
  }
}
</style>
