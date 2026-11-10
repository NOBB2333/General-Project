<script setup lang="ts">
import type { StockLedgerRecord } from '../../model/stock-ledger.types';

const props = defineProps<{ rows: readonly StockLedgerRecord[] }>();
const categories = ['采购入库', '组装入库', '销售出库', '委外发料'] as const;
</script>

<template>
  <section :class="$style.report">
    <header>
      <h2>库存流水报表</h2>
      <p>按业务类型汇总当前筛选范围内的出入库记录</p>
    </header>
    <div :class="$style.reportGrid">
      <article
        v-for="category in categories"
        :key="category"
      >
        <span>{{ category }}</span
        ><strong>{{ props.rows.filter((row) => row.businessType === category).length }}</strong
        ><i :data-count="Math.min(3, props.rows.filter((row) => row.businessType === category).length)" />
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.report {
  @apply p-5;
}
.report header {
  @apply border-b pb-4;
  border-color: var(--ui-border-subtle);
}
.report h2 {
  @apply m-0 text-[15px] font-semibold;
}
.report p {
  @apply m-0 mt-1 text-[11px];
  color: var(--ui-text-muted);
}
.reportGrid {
  @apply mt-5 grid grid-cols-2 gap-4;
}
.reportGrid article {
  @apply relative overflow-hidden rounded-lg border p-4;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.reportGrid span {
  @apply text-[11px];
  color: var(--ui-text-muted);
}
.reportGrid strong {
  @apply mt-2 block text-[22px];
}
.reportGrid i {
  @apply mt-4 block h-2 max-w-full rounded bg-[#4C9AFF];
}
.reportGrid i[data-count='0'] {
  width: 12%;
}
.reportGrid i[data-count='1'] {
  width: 32%;
}
.reportGrid i[data-count='2'] {
  width: 58%;
}
.reportGrid i[data-count='3'] {
  width: 84%;
}
</style>
