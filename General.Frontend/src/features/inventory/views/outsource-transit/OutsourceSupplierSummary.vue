<script setup lang="ts">
import { computed } from 'vue';
import type { OutsourceTransitRecord } from '../../model/outsource-transit.types';

const props = defineProps<{ rows: readonly OutsourceTransitRecord[] }>();
const money = new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'CNY', maximumFractionDigits: 0 });
const suppliers = computed(() =>
  [...new Set(props.rows.map((row) => row.supplier))].map((supplier) => {
    const records = props.rows.filter((row) => row.supplier === supplier);
    return {
      supplier,
      code: records[0]?.supplierCode ?? '-',
      skuCount: records.length,
      quantity: records.reduce((sum, row) => sum + row.balance, 0),
      amount: records.reduce((sum, row) => sum + row.amount, 0),
      maxAge: Math.max(...records.map((row) => row.ageDays)),
    };
  }),
);
</script>

<template>
  <div :class="$style.grid">
    <article
      v-for="supplier in suppliers"
      :key="supplier.supplier"
    >
      <header>
        <div>
          <strong>{{ supplier.supplier }}</strong
          ><span>{{ supplier.code }}</span>
        </div>
        <b>{{ supplier.maxAge }} 天</b>
      </header>
      <dl>
        <div>
          <dt>在途物料</dt>
          <dd>{{ supplier.skuCount }} SKU</dd>
        </div>
        <div>
          <dt>在外余量</dt>
          <dd>{{ supplier.quantity }}</dd>
        </div>
        <div>
          <dt>在外金额</dt>
          <dd>{{ money.format(supplier.amount) }}</dd>
        </div>
      </dl>
    </article>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-2 gap-3 rounded-b-lg border border-t-0 bg-white p-4;
  border-color: var(--ui-border);
}
.grid article {
  @apply rounded-lg border p-4;
  border-color: var(--ui-border);
}
.grid header {
  @apply flex items-start justify-between;
}
.grid header strong,
.grid header span {
  @apply block;
}
.grid header span {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.grid header b {
  @apply rounded px-2 py-1 text-[10px];
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.grid dl {
  @apply mt-4 grid grid-cols-3 gap-3;
}
.grid dl div {
  @apply border-l pl-3;
  border-color: var(--ui-border-subtle);
}
.grid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.grid dd {
  @apply mt-1 text-[14px] font-semibold;
}
</style>
