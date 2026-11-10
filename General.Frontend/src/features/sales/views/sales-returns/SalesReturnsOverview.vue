<script setup lang="ts">
import { Star, UserRound, UsersRound } from 'lucide-vue-next';
import type { SalesReturnScope } from '../../model/sales-returns.types';

defineProps<{
  activeScope: SalesReturnScope;
  scopeCounts: Record<SalesReturnScope, number>;
  total: number;
  pending: number;
  amount: number;
}>();

const emit = defineEmits<{
  'update:activeScope': [scope: SalesReturnScope];
}>();

const scopes = [
  { id: 'all', label: '所有', icon: UsersRound },
  { id: 'mine', label: '我负责的', icon: UserRound },
  { id: 'team', label: '下属负责的', icon: UsersRound },
  { id: 'followed', label: '我关注的', icon: Star },
] as const;
</script>

<template>
  <nav
    :class="$style.scopes"
    aria-label="退货负责人范围"
  >
    <button
      v-for="scope in scopes"
      :key="scope.id"
      type="button"
      :class="[$style.scopeButton, activeScope === scope.id && $style.scopeActive]"
      :aria-pressed="activeScope === scope.id"
      @click="emit('update:activeScope', scope.id)"
    >
      <component
        :is="scope.icon"
        :size="14"
        :stroke-width="1.8"
      />
      {{ scope.label }}
      <span>{{ scopeCounts[scope.id] }}</span>
    </button>
  </nav>

  <section
    :class="$style.metrics"
    aria-label="销售退货概览"
  >
    <article :class="$style.metricCard">
      <span>退货总数</span>
      <strong>{{ total }}</strong>
    </article>
    <article :class="[$style.metricCard, $style.pendingCard]">
      <span>待处理</span>
      <strong>{{ pending }}</strong>
    </article>
    <article :class="[$style.metricCard, $style.amountCard]">
      <span>退货金额</span>
      <strong>¥{{ (amount / 10_000).toFixed(1) }}万</strong>
    </article>
    <article :class="$style.metricCard">
      <span>退货率</span>
      <strong>1.8%</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.scopes {
  @apply mt-4 flex min-h-10 items-center gap-1 rounded border bg-white p-1;
  border-color: var(--ui-border);
}

.scopeButton {
  @apply inline-flex min-h-8 items-center gap-1.5 rounded px-3 text-[11px] font-semibold transition-colors;
  color: var(--ui-text-subtle);
}

.scopeButton:hover {
  background: var(--ui-surface-hover);
}

.scopeButton span {
  @apply inline-flex h-[18px] min-w-[18px] items-center justify-center rounded-full px-1 text-[9px];
  color: var(--ui-text-subtle);
  background: var(--ui-border-subtle);
}

.scopeActive {
  @apply text-white;
  background: var(--app-primary);
}

.scopeActive:hover {
  background: var(--app-primary);
}

.scopeActive span {
  color: white;
  background: rgb(255 255 255 / 20%);
}

.metrics {
  @apply mt-4 grid grid-cols-4 gap-4;
}

.metricCard {
  @apply flex min-h-[84px] flex-col justify-center rounded border bg-white px-[18px] py-3;
  border-color: var(--ui-border);
}

.metricCard span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.metricCard strong {
  @apply mt-2 text-[24px] font-semibold leading-none;
  color: var(--ui-text);
}

.pendingCard strong {
  color: var(--ui-warning);
}

.amountCard strong {
  color: #ff5630;
}
</style>
