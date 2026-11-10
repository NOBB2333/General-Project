<script setup lang="ts">
import type { OpportunityRow, OpportunityStage } from '@/features/crm/model/opportunities.types';

defineProps<{ rows: readonly OpportunityRow[]; stages: readonly OpportunityStage[] }>();
const currency = new Intl.NumberFormat('zh-CN');
</script>

<template>
  <div :class="$style.board">
    <section
      v-for="stage in stages"
      :key="stage"
      :class="$style.column"
    >
      <header>
        <strong>{{ stage }}</strong>
        <span>{{ rows.filter((row) => row.stage === stage).length }}</span>
      </header>
      <article
        v-for="row in rows.filter((item) => item.stage === stage)"
        :key="row.id"
      >
        <strong>{{ row.name }}</strong>
        <small>{{ row.customer }}</small>
        <footer>
          <b>¥{{ currency.format(row.amount) }}</b
          ><span>{{ row.probability }}%</span>
        </footer>
      </article>
      <p v-if="!rows.some((row) => row.stage === stage)">暂无商机</p>
    </section>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.board {
  @apply grid min-w-[1260px] grid-cols-7 gap-3 bg-white p-4;
}
.column {
  @apply min-h-[300px] rounded p-2;
  background: var(--ui-surface-hover);
}
.column > header {
  @apply mb-3 flex items-center justify-between px-1 text-[12px];
  color: var(--ui-text-muted);
}
.column > header span {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[10px];
  background: var(--ui-border-subtle);
}
.column article {
  @apply mb-2 rounded border bg-white p-3;
  border-color: var(--ui-border);
  box-shadow: 0 1px 2px rgb(9 30 66 / 8%);
}
.column article > strong {
  @apply block text-[12px] leading-5;
  color: var(--ui-text);
}
.column article small {
  @apply mt-1 block truncate text-[10px];
  color: var(--ui-text-subtle);
}
.column article footer {
  @apply mt-3 flex items-center justify-between text-[11px];
}
.column article footer b {
  color: var(--app-primary);
}
.column article footer span {
  color: var(--ui-text-subtle);
}
.column > p {
  @apply py-8 text-center text-[10px];
  color: #97a0af;
}
</style>
