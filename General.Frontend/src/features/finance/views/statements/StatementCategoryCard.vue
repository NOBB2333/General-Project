<script setup lang="ts">
import { ArrowRight, CircleCheck, Send, ShoppingCart, TriangleAlert, Users } from 'lucide-vue-next';
import type { StatementCategory } from '../../model/statements.types';
defineProps<{ category: StatementCategory }>();
const emit = defineEmits<{ select: [] }>();
function money(value: number) {
  return `¥${(value / 10000).toFixed(1)}万`;
}
</script>

<template>
  <button
    type="button"
    :class="$style.card"
    @click="emit('select')"
  >
    <header :class="$style.header">
      <span :class="[$style.categoryIcon, category.party === '供应商' && $style.supplierIcon]">
        <Users
          v-if="category.party === '客户'"
          :size="22"
          :stroke-width="1.8"
        /><ShoppingCart
          v-else
          :size="22"
          :stroke-width="1.8"
        />
      </span>
      <span :class="$style.heading"
        ><strong>{{ category.party }}对账单</strong><small>{{ category.subtitle }}</small></span
      >
      <ArrowRight
        :size="18"
        :stroke-width="1.8"
      />
    </header>
    <div :class="$style.metrics">
      <span
        ><small>对账单数量</small><strong>{{ category.count }}</strong></span
      >
      <span :class="$style.balance"
        ><small>⌁ {{ category.party === '客户' ? '应收' : '应付' }}余额合计</small
        ><strong>{{ money(category.balance) }}</strong></span
      >
    </div>
    <footer :class="$style.statuses">
      <span
        v-for="status in category.statuses"
        :key="status.label"
        :data-tone="status.label"
      >
        <CircleCheck
          v-if="status.label === '已确认'"
          :size="11"
          :stroke-width="1.8"
        /><Send
          v-else-if="status.label === '已发送待确认'"
          :size="11"
          :stroke-width="1.8"
        /><TriangleAlert
          v-else-if="status.label === '有差异'"
          :size="11"
          :stroke-width="1.8"
        />{{ status.label }} {{ status.value }}
      </span>
    </footer>
  </button>
</template>

<style module>
@reference "../../../../styles/index.css";
.card {
  @apply min-w-0 rounded-lg border bg-white p-5 text-left transition-colors;
  border-color: var(--ui-border);
}
.card:hover {
  border-color: var(--app-primary);
}
.header {
  @apply flex items-center gap-3;
}
.categoryIcon {
  @apply flex h-11 w-11 items-center justify-center rounded;
  color: #0c66e4;
  background: #deebff;
}
.supplierIcon {
  color: #6554c0;
  background: #eae6ff;
}
.heading {
  @apply flex min-w-0 flex-1 flex-col;
}
.heading strong {
  @apply text-[17px];
  color: var(--ui-text);
}
.heading small {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.header > svg {
  color: #b3bac5;
}
.metrics {
  @apply mt-5 grid grid-cols-2 gap-3;
}
.metrics > span {
  @apply flex h-[76px] flex-col justify-center rounded px-3;
  background: var(--ui-surface-subtle);
}
.metrics small {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.metrics strong {
  @apply mt-2 text-[20px];
  color: var(--ui-text);
}
.metrics .balance {
  border-left: 3px solid var(--app-primary);
}
.metrics .balance strong {
  color: var(--app-primary);
}
.statuses {
  @apply mt-4 flex flex-wrap gap-2 border-t pt-3;
  border-color: var(--ui-border-subtle);
}
.statuses span {
  @apply flex items-center gap-1 rounded-full px-2 py-1 text-[9px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.statuses span[data-tone='已确认'] {
  color: #00875a;
  background: #e3fcef;
}
.statuses span[data-tone='已发送待确认'] {
  color: #0c66e4;
  background: #deebff;
}
.statuses span[data-tone='有差异'] {
  color: #de350b;
  background: #ffebe6;
}
</style>
