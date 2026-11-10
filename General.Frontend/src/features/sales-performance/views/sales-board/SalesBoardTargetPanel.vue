<script setup lang="ts">
import { Target } from 'lucide-vue-next';

const emit = defineEmits<{ open: [] }>();
</script>

<template>
  <article :class="$style.panel">
    <header :class="$style.header">
      <span :class="$style.icon">
        <Target
          :size="17"
          :stroke-width="1.8"
        />
      </span>
      <div>
        <h2>年度目标完成率</h2>
        <p>2026 年度</p>
      </div>
    </header>
    <button
      type="button"
      :class="$style.chart"
      aria-label="年度目标完成率 7.3%，已完成 1,742,161.60 元"
      @click="emit('open')"
    >
      <span :class="$style.ring">
        <span :class="$style.value">7.3%</span>
        <span>已完成</span>
        <strong>¥1,742,161.60</strong>
      </span>
    </button>
    <dl :class="$style.summary">
      <div>
        <dt>年度目标</dt>
        <dd>¥24,000,000.00</dd>
      </div>
      <div>
        <dt>目标缺口</dt>
        <dd>¥22,257,838.40</dd>
      </div>
    </dl>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply min-w-0 rounded border pb-4;
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.header {
  @apply flex min-h-[58px] items-center gap-3 px-5 py-3;
}

.icon {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.header h2 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.header p {
  @apply mt-0.5 text-[11px];
  color: var(--ui-text-subtle);
}

.chart {
  @apply mx-auto flex h-[190px] w-[190px] items-center justify-center rounded-full outline-none;
  background: conic-gradient(var(--ui-warning) 0deg 26.28deg, var(--ui-warning-soft) 26.28deg 360deg);
}

.chart:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.ring {
  @apply flex h-[140px] w-[140px] flex-col items-center justify-center rounded-full text-[11px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface);
}

.value {
  @apply text-[28px] font-bold leading-9;
  color: var(--ui-warning);
}

.ring strong {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}

.summary {
  @apply mt-3 grid grid-cols-2 gap-2 px-4;
}

.summary div {
  @apply rounded px-3 py-2 text-center;
  background: var(--ui-surface-subtle);
}

.summary dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.summary dd {
  @apply mt-1 text-[11px] font-semibold;
  color: var(--ui-text);
}
</style>
