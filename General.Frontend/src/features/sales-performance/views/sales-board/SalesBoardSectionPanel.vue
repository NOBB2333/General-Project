<script setup lang="ts">
import { ArrowUpRight } from 'lucide-vue-next';
import { computed } from 'vue';
import { salesBoardTabs } from '../../model/sales-board.data';
import type { SalesBoardPeriod, SalesBoardScope, SalesBoardSection } from '../../model/sales-board.types';

const props = defineProps<{
  section: SalesBoardSection;
  scope: SalesBoardScope;
  period: SalesBoardPeriod;
}>();

const emit = defineEmits<{
  inspect: [label: string];
}>();

const tab = computed(() => salesBoardTabs.find((item) => item.id === props.section)!);

const cards = computed(() => [
  { id: `${props.section}-primary`, label: `${tab.value.label}核心数据`, value: tab.value.badge },
  { id: `${props.section}-scope`, label: '当前视角', value: scopeLabel(props.scope) },
  { id: `${props.section}-period`, label: '统计周期', value: periodLabel(props.period) },
]);

function scopeLabel(scope: SalesBoardScope) {
  return { mine: '我的', team: '我的团队', company: '全公司' }[scope];
}

function periodLabel(period: SalesBoardPeriod) {
  return { month: '本月', quarter: '本季度', year: '本年度' }[period];
}
</script>

<template>
  <section
    :class="$style.panel"
    :data-board-section="section"
  >
    <header :class="$style.header">
      <span :class="$style.icon">
        <component
          :is="tab.icon"
          :size="21"
          :stroke-width="1.8"
        />
      </span>
      <div>
        <h2>{{ tab.label }}</h2>
        <p>{{ tab.badge }} · {{ scopeLabel(scope) }} · {{ periodLabel(period) }}</p>
      </div>
    </header>
    <div :class="$style.cards">
      <button
        v-for="card in cards"
        :key="card.id"
        type="button"
        @click="emit('inspect', card.label)"
      >
        <span>{{ card.label }}</span>
        <strong>{{ card.value }}</strong>
        <ArrowUpRight
          :size="15"
          :stroke-width="1.8"
        />
      </button>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply mt-4 min-h-[390px] rounded border p-6;
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.header {
  @apply flex items-center gap-3;
}

.icon {
  @apply flex h-10 w-10 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.header h2 {
  @apply text-[17px] font-semibold;
  color: var(--ui-text);
}

.header p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}

.cards {
  @apply mt-6 grid grid-cols-3 gap-3;
}

.cards button {
  @apply flex min-h-[110px] flex-col items-start rounded border p-4 text-left outline-none transition-colors;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.cards button:hover {
  border-color: #b3d4ff;
  background: var(--app-primary-soft);
}

.cards button:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.cards span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.cards strong {
  @apply mt-3 text-[18px] font-semibold;
  color: var(--ui-text);
}

.cards svg {
  @apply ml-auto mt-auto;
  color: var(--app-primary);
}

@media (prefers-reduced-motion: reduce) {
  .cards button {
    transition: none;
  }
}
</style>
