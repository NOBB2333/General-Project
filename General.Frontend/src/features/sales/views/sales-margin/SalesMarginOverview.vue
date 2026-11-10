<script setup lang="ts">
import {
  Banknote,
  ChartColumnIncreasing,
  Star,
  TrendingDown,
  TrendingUp,
  UserRound,
  UsersRound,
} from 'lucide-vue-next';
import type { MarginScope, MarginTotals } from '../../model/sales-margin.types';
defineProps<{ scope: MarginScope; totals: MarginTotals }>();
const emit = defineEmits<{ 'update:scope': [value: MarginScope] }>();
const scopes: readonly { value: MarginScope; label: string; icon: typeof UserRound }[] = [
  { value: 'all', label: '所有', icon: UsersRound },
  { value: 'mine', label: '我负责的', icon: UserRound },
  { value: 'team', label: '下属负责的', icon: UsersRound },
  { value: 'followed', label: '我关注的', icon: Star },
];
const money = (value: number) => `¥${Math.round(value).toLocaleString('zh-CN')}`;
</script>
<template>
  <section aria-label="毛利概览">
    <div :class="$style.scopeBar">
      <button
        v-for="option in scopes"
        :key="option.value"
        type="button"
        :class="[$style.scopeButton, scope === option.value && $style.scopeActive]"
        :aria-pressed="scope === option.value"
        @click="emit('update:scope', option.value)"
      >
        <component
          :is="option.icon"
          :size="15"
          :stroke-width="1.8"
        />{{ option.label }}
      </button>
    </div>
    <div :class="$style.metrics">
      <article :class="[$style.metric, $style.revenue]">
        <Banknote
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>销售收入</span><strong>{{ money(totals.revenue) }}</strong
          ><small>本月累计</small>
        </div>
      </article>
      <article :class="[$style.metric, $style.cost]">
        <TrendingDown
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>销售成本</span><strong>{{ money(totals.cost) }}</strong
          ><small>占比 {{ (100 - totals.margin).toFixed(1) }}%</small>
        </div>
      </article>
      <article :class="[$style.metric, $style.profit]">
        <TrendingUp
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>销售毛利</span><strong>{{ money(totals.grossProfit) }}</strong
          ><small>↑ 环比{{ totals.change }}%</small>
        </div>
      </article>
      <article :class="[$style.metric, $style.margin]">
        <ChartColumnIncreasing
          :size="18"
          :stroke-width="1.8"
        />
        <div>
          <span>综合毛利率</span><strong>{{ totals.margin.toFixed(1) }}%</strong><small>目标 ≥30%</small>
        </div>
      </article>
    </div>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.scopeBar {
  @apply mb-4 flex min-h-10 items-center gap-1 rounded border bg-white p-1;
  border-color: var(--ui-border);
}
.scopeButton {
  @apply inline-flex min-h-8 items-center gap-1.5 rounded px-3 text-[12px] font-medium transition-colors active:scale-[.97];
  color: var(--ui-text-subtle);
}
.scopeButton:hover {
  background: var(--ui-surface-hover);
}
.scopeActive {
  @apply text-white;
  background: var(--app-primary);
  box-shadow: 0 1px 3px rgb(0 82 204 / 25%);
}
.scopeActive:hover {
  background: var(--app-primary);
}
.metrics {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.metric {
  @apply flex min-h-[102px] items-start gap-3 rounded border bg-white px-4 py-4;
  border-color: var(--ui-border);
}
.metric > svg {
  @apply mt-0.5 shrink-0;
}
.metric span {
  @apply block text-[12px];
  color: var(--ui-text-subtle);
}
.metric strong {
  @apply mt-1.5 block text-[23px] font-bold leading-7;
}
.metric small {
  @apply mt-1.5 block text-[11px];
  color: var(--ui-text-subtle);
}
.revenue {
  color: #0052cc;
}
.cost {
  color: #ff5630;
}
.profit,
.margin {
  color: #00875a;
}
.metric div {
  color: inherit;
}
</style>
