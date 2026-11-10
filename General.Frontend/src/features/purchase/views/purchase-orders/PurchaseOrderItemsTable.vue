<script setup lang="ts">
import { computed } from 'vue';
import type { PurchaseOrderRow } from '../../model/purchase-orders.types';

const props = defineProps<{ rows: readonly PurchaseOrderRow[] }>();

const productNames = ['工业触摸屏', 'PLC控制器', '变频器', '开关电源', '通讯模块', '伺服驱动器'];
const itemRows = computed(() =>
  props.rows
    .flatMap((row) =>
      Array.from({ length: row.itemCount }, (_, index) => ({
        id: `${row.id}-item-${index}`,
        orderCode: row.code,
        supplier: row.supplier,
        product: productNames[index % productNames.length],
        quantity: index + 1,
        unitPrice: Math.round(row.amount / row.itemCount / (index + 1)),
        stockProgress: row.stockProgress,
      })),
    )
    .slice(0, 18),
);
</script>

<template>
  <div :class="$style.scroller">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>采购订单</th>
          <th>供应商</th>
          <th>商品名称</th>
          <th>采购数量</th>
          <th>采购单价</th>
          <th>入库进度</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="item in itemRows"
          :key="item.id"
        >
          <td>
            <strong>{{ item.orderCode }}</strong>
          </td>
          <td>{{ item.supplier }}</td>
          <td>{{ item.product }}</td>
          <td>{{ item.quantity }}</td>
          <td>¥{{ item.unitPrice.toLocaleString('en-US') }}</td>
          <td>{{ item.stockProgress }}%</td>
        </tr>
      </tbody>
    </table>
  </div>
  <footer :class="$style.footer">共 {{ itemRows.length }} 条订单明细</footer>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply overflow-x-auto;
  scrollbar-width: thin;
}

.table {
  @apply w-full min-w-[1100px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-[40px] border-b px-5 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[54px] border-b px-5;
  border-color: var(--ui-border-subtle);
}

.table strong {
  color: var(--app-primary);
}

.footer {
  @apply flex h-12 items-center px-5 text-[11px];
  color: var(--ui-text-subtle);
}
</style>
