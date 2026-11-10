<script setup lang="ts">
import { computed } from 'vue';
import { TrendingDown, TrendingUp } from 'lucide-vue-next';

const props = defineProps<{
  title: string;
  subtitle: string;
  values: readonly number[];
  labels: readonly string[];
  current: string;
  delta: string;
  positive: boolean;
  tone: 'blue' | 'purple';
}>();

const points = computed(() => {
  const highest = Math.max(...props.values);
  const lowest = Math.min(...props.values);
  const range = Math.max(1, highest - lowest);
  return props.values.map((value, index) => ({
    x: (index / Math.max(1, props.values.length - 1)) * 96 + 2,
    y: 88 - ((value - lowest) / range) * 76,
    value,
    label: props.labels[index],
  }));
});
const linePoints = computed(() => points.value.map((point) => `${point.x},${point.y}`).join(' '));
const areaPoints = computed(() => `2,100 ${linePoints.value} 98,100`);
const gradientId = computed(() => `warehouse-trend-${props.tone}`);
const axisLabels = computed(() => props.labels.filter((_, index) => index % 2 === 0));
</script>

<template>
  <article :class="$style.card">
    <header :class="$style.header">
      <div>
        <h2>{{ title }}</h2>
        <p>{{ subtitle }}</p>
      </div>
      <div :class="$style.current">
        <strong :class="$style[tone]">{{ current }}</strong>
        <span :class="positive ? $style.positive : $style.negative">
          <TrendingUp
            v-if="positive"
            :size="11"
            :stroke-width="1.8"
          />
          <TrendingDown
            v-else
            :size="11"
            :stroke-width="1.8"
          />
          {{ delta }}
        </span>
      </div>
    </header>
    <div :class="$style.plot">
      <svg
        viewBox="0 0 100 100"
        preserveAspectRatio="none"
        role="img"
        :aria-label="`${title}，当前 ${current}`"
      >
        <defs>
          <linearGradient
            :id="gradientId"
            x1="0"
            x2="0"
            y1="0"
            y2="1"
          >
            <stop
              offset="0%"
              :class="$style[`${tone}Stop`]"
              stop-opacity="0.22"
            />
            <stop
              offset="100%"
              :class="$style[`${tone}Stop`]"
              stop-opacity="0"
            />
          </linearGradient>
        </defs>
        <polygon
          :points="areaPoints"
          :fill="`url(#${gradientId})`"
        />
        <polyline
          :points="linePoints"
          :class="$style[`${tone}Line`]"
          fill="none"
          stroke-width="1.6"
          vector-effect="non-scaling-stroke"
          stroke-linejoin="round"
          stroke-linecap="round"
        />
        <circle
          v-for="point in points"
          :key="point.label"
          :cx="point.x"
          :cy="point.y"
          r="1.6"
          :class="$style[`${tone}Dot`]"
        >
          <title>{{ point.label }} · {{ point.value }}</title>
        </circle>
      </svg>
    </div>
    <div :class="$style.axis">
      <span
        v-for="label in axisLabels"
        :key="label"
        >{{ label }}</span
      >
    </div>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.card {
  @apply rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.header {
  @apply mb-3 flex items-start justify-between;
}
.header h2 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}
.header p {
  @apply mt-0.5 text-[11px];
  color: #97a0af;
}
.current {
  @apply text-right;
}
.current strong {
  @apply block text-[18px] font-bold;
}
.current span {
  @apply mt-0.5 flex items-center justify-end gap-1 text-[11px];
}
.blue {
  color: #0052cc;
}
.purple {
  color: #6554c0;
}
.positive {
  color: #00875a;
}
.negative {
  color: #de350b;
}
.plot {
  @apply h-[100px] w-full;
}
.plot svg {
  @apply h-full w-full overflow-visible;
}
.blueLine {
  stroke: #0052cc;
}
.purpleLine {
  stroke: #6554c0;
}
.blueStop {
  stop-color: #0052cc;
}
.purpleStop {
  stop-color: #6554c0;
}
.blueDot {
  fill: #0052cc;
}
.purpleDot {
  fill: #6554c0;
}
.plot circle {
  @apply opacity-0 transition-opacity;
}
.plot:hover circle {
  @apply opacity-100;
}
.axis {
  @apply mt-2 flex justify-between text-[10px];
  color: #97a0af;
}
@media (prefers-reduced-motion: reduce) {
  .plot circle {
    transition: none;
  }
}
</style>
