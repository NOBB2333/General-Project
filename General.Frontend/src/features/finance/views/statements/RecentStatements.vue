<script setup lang="ts">
import { CircleCheck, Clock3, TriangleAlert } from 'lucide-vue-next';
import type { StatementRow } from '../../model/statements.types';
defineProps<{ rows: readonly StatementRow[]; activeParty: '全部' | '客户' | '供应商' }>();
const emit = defineEmits<{ open: [row: StatementRow]; clearFilter: [] }>();
function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.panel">
    <header>
      <h2>{{ activeParty === '全部' ? '最近对账单' : `${activeParty}对账单` }}</h2>
      <button
        v-if="activeParty !== '全部'"
        type="button"
        @click="emit('clearFilter')"
      >
        查看全部
      </button>
    </header>
    <button
      v-for="row in rows"
      :key="row.id"
      type="button"
      :class="$style.row"
      @click="emit('open', row)"
    >
      <span :class="$style.identity"
        ><strong>{{ row.code }}</strong
        ><b>{{ row.company }}</b
        ><small :data-party="row.party">{{ row.party }}</small></span
      >
      <span :class="$style.period">{{ row.period }}</span
      ><strong :class="$style.amount">{{ money(row.amount) }}</strong>
      <span
        :class="$style.status"
        :data-status="row.status"
        ><CircleCheck
          v-if="row.status === '已确认'"
          :size="11"
          :stroke-width="1.8"
        /><TriangleAlert
          v-else-if="row.status === '有差异'"
          :size="11"
          :stroke-width="1.8"
        /><Clock3
          v-else
          :size="11"
          :stroke-width="1.8"
        />{{ row.status }}</span
      >
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.panel {
  @apply mt-5 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.panel > header {
  @apply flex h-[54px] items-center justify-between border-b px-5;
  border-color: var(--ui-border);
}
.panel h2 {
  @apply m-0 text-[14px] font-semibold;
}
.panel header button {
  @apply text-[11px] font-semibold;
  color: var(--app-primary);
}
.row {
  @apply grid h-[51px] w-full grid-cols-[minmax(340px,1fr)_240px_110px_120px] items-center gap-4 border-b px-5 text-left transition-colors last:border-b-0;
  border-color: var(--ui-border-subtle);
}
.row:hover {
  background: var(--ui-surface-hover);
}
.identity {
  @apply flex min-w-0 items-center gap-3 text-[11px];
}
.identity strong {
  color: #0c66e4;
}
.identity b {
  @apply truncate font-semibold;
}
.identity small {
  @apply rounded-full px-2 py-0.5 text-[9px];
  color: #0c66e4;
  background: #deebff;
}
.identity small[data-party='供应商'] {
  color: #6554c0;
  background: #eae6ff;
}
.period {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.amount {
  @apply text-right text-[11px];
}
.status {
  @apply ml-auto flex items-center gap-1 rounded-full px-2 py-1 text-[9px] font-semibold;
  color: #ff8b00;
  background: #fff0b3;
}
.status[data-status='已确认'] {
  color: #00875a;
  background: #e3fcef;
}
.status[data-status='有差异'] {
  color: #de350b;
  background: #ffebe6;
}
.status[data-status='草稿'] {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
</style>
