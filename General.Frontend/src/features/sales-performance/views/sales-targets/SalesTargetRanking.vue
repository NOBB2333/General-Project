<script setup lang="ts">
import { Trophy } from 'lucide-vue-next';
import type { SalesTargetPersonRow, SalesTargetUnit } from '../../model/sales-targets.types';

const props = defineProps<{
  people: readonly SalesTargetPersonRow[];
  unit: SalesTargetUnit;
  formatAmount: (valueWan: number, unit: SalesTargetUnit, precision?: number) => string;
}>();

const emit = defineEmits<{ select: [row: SalesTargetPersonRow] }>();
</script>

<template>
  <article
    :class="$style.panel"
    data-target-person-ranking
  >
    <header :class="$style.header">
      <Trophy
        :size="16"
        :stroke-width="1.8"
      />
      <h2>个人业绩贡献榜</h2>
    </header>
    <ol :class="$style.list">
      <li
        v-for="person in people"
        :key="person.id"
      >
        <button
          type="button"
          @click="emit('select', person)"
        >
          <span :class="$style.rank">{{ person.rank }}</span>
          <span :class="$style.personBody">
            <span :class="$style.personHeading">
              <strong>{{ person.name }}</strong>
              <em>{{ person.rate.toFixed(1) }}%</em>
            </span>
            <progress
              :value="person.rate"
              max="100"
              :aria-label="`${person.name}完成率 ${person.rate.toFixed(1)}%`"
            />
            <small>
              {{ props.formatAmount(person.actualWan, unit, person.actualPrecision) }} /
              {{ props.formatAmount(person.targetWan, unit, 1) }}
            </small>
          </span>
        </button>
      </li>
    </ol>
    <p :class="$style.hint">点击可查看该员工历年业绩</p>
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
  color: var(--ui-warning);
}

.header h2 {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}

.list {
  @apply px-5;
}

.list button {
  @apply flex w-full items-center gap-3 border-b py-3 text-left outline-none transition-colors;
  border-color: var(--ui-border-subtle);
}

.list button:hover {
  background: var(--ui-surface-hover);
}

.list button:focus-visible {
  box-shadow: inset 0 0 0 2px var(--ui-focus);
}

.rank {
  @apply flex h-7 w-7 shrink-0 items-center justify-center rounded text-[11px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.list li:first-child .rank,
.list li:nth-child(3) .rank {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.personBody {
  @apply min-w-0 flex-1;
}

.personHeading {
  @apply flex items-center justify-between gap-3;
}

.personHeading strong {
  @apply text-[11px] font-semibold;
}

.personHeading em {
  @apply text-[10px] font-semibold not-italic;
  color: var(--ui-danger);
}

.personBody progress {
  @apply mt-1.5 block h-1 w-full overflow-hidden rounded-full border-0;
  color: var(--app-primary);
  background: var(--ui-border-subtle);
}

.personBody progress::-webkit-progress-bar {
  background: var(--ui-border-subtle);
}

.personBody progress::-webkit-progress-value {
  background: currentColor;
}

.personBody progress::-moz-progress-bar {
  background: currentColor;
}

.personBody small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}

.hint {
  @apply px-5 py-3 text-center text-[9px];
  color: var(--ui-text-subtle);
}

@media (prefers-reduced-motion: reduce) {
  .list button {
    transition: none;
  }
}
</style>
