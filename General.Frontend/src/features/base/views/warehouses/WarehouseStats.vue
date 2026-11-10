<script setup lang="ts">
import {
  Boxes,
  CircleDollarSign,
  PackageOpen,
  ShoppingCart,
  TrendingUp,
  TriangleAlert,
  Warehouse,
} from 'lucide-vue-next';
import { computed } from 'vue';
import type { WarehouseRow } from '../../model/warehouses.types';

const props = defineProps<{ rows: readonly WarehouseRow[] }>();

const summary = computed(() => {
  const counts = { 主仓: 0, 项目仓: 0, 备品仓: 0, 委外仓: 0, 退货仓: 0, 中转仓: 0 };
  let totalSKU = 0;
  let totalQty = 0;
  let totalPurchase = 0;
  let estimatedSales = 0;
  let noPriceSKU = 0;

  for (const row of props.rows) {
    counts[row.type] += 1;
    totalSKU += row.totalSKU;
    totalQty += row.totalQty;
    totalPurchase += row.totalPurchase;
    estimatedSales += row.estimatedSales;
    noPriceSKU += row.noPriceSKU;
  }

  return { counts, totalSKU, totalQty, totalPurchase, estimatedSales, noPriceSKU };
});

function formatWan(value: number) {
  return `¥${(value / 10_000).toFixed(1)}万`;
}
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="仓库统计"
  >
    <article :class="$style.card">
      <p :class="$style.label">
        <Warehouse
          :class="$style.blueIcon"
          :size="14"
          :stroke-width="1.8"
        />
        总仓库数
      </p>
      <strong>{{ rows.length }}</strong>
      <span
        >主仓 {{ summary.counts.主仓 }} · 项目仓 {{ summary.counts.项目仓 }} · 备品仓 {{ summary.counts.备品仓 }} ·
        委外仓 {{ summary.counts.委外仓 }}</span
      >
    </article>
    <article :class="$style.card">
      <p :class="$style.label">
        <PackageOpen
          :class="$style.purpleIcon"
          :size="14"
          :stroke-width="1.8"
        />
        总库存 SKU
      </p>
      <strong>{{ summary.totalSKU }}</strong>
      <span :class="$style.warning">
        <TriangleAlert
          :size="10"
          :stroke-width="1.8"
        />
        {{ summary.noPriceSKU }} 个无销售价
      </span>
    </article>
    <article :class="$style.card">
      <p :class="$style.label">
        <Boxes
          :class="$style.greenIcon"
          :size="14"
          :stroke-width="1.8"
        />
        总库存数量
      </p>
      <strong>{{ summary.totalQty.toLocaleString('en-US') }}</strong>
      <span :class="$style.success">
        <TrendingUp
          :size="10"
          :stroke-width="1.8"
        />
        +3.2% 较上月
      </span>
    </article>
    <article :class="$style.card">
      <p :class="$style.label">
        <ShoppingCart
          :class="$style.blueIcon"
          :size="14"
          :stroke-width="1.8"
        />
        总采购金额
      </p>
      <strong>{{ formatWan(summary.totalPurchase) }}</strong>
      <span>累计采购入库</span>
    </article>
    <article :class="$style.card">
      <p :class="$style.label">
        <CircleDollarSign
          :class="$style.greenIcon"
          :size="14"
          :stroke-width="1.8"
        />
        预估销售总额
      </p>
      <strong>{{ formatWan(summary.estimatedSales) }}</strong>
      <span :class="$style.success">毛利约 {{ formatWan(summary.estimatedSales - summary.totalPurchase) }}</span>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mb-5 grid grid-cols-5 gap-4;
}
.card {
  @apply min-w-0 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.label {
  @apply mb-1.5 flex items-center gap-2 whitespace-nowrap text-[11px];
  color: var(--ui-text-subtle);
}
.card strong {
  @apply block text-[22px] font-bold leading-7;
  color: var(--ui-text);
}
.card > span {
  @apply mt-0.5 flex items-center gap-1 truncate text-[11px];
  color: #97a0af;
}
.blueIcon {
  color: #0052cc;
}
.purpleIcon {
  color: #6554c0;
}
.greenIcon,
.success {
  color: #00875a !important;
}
.warning {
  color: #ff8b00 !important;
}
@media (max-width: 1080px) {
  .grid {
    @apply grid-cols-2;
  }
}
</style>
