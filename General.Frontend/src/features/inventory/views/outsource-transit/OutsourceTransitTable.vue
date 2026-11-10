<script setup lang="ts">
import type { OutsourceTransitRecord } from '../../model/outsource-transit.types';

defineProps<{ rows: readonly OutsourceTransitRecord[] }>();
const emit = defineEmits<{ view: [row: OutsourceTransitRecord] }>();
const money = new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'CNY', maximumFractionDigits: 0 });
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>工单号</th>
          <th>采购订单</th>
          <th>供应商</th>
          <th>产品编号</th>
          <th>品名</th>
          <th>规格</th>
          <th>已发 / 已耗 / 已退</th>
          <th>在外余量</th>
          <th>在外金额</th>
          <th>单位</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <strong>{{ row.workOrder }}</strong
            ><small>{{ row.workOrderName }}</small>
          </td>
          <td>
            <button
              :class="$style.link"
              @click="emit('view', row)"
            >
              {{ row.purchaseOrder }}
            </button>
          </td>
          <td>
            <strong>{{ row.supplier }}</strong
            ><small>{{ row.supplierCode }}</small>
          </td>
          <td>{{ row.productCode }}</td>
          <td>
            <strong>{{ row.productName }}</strong>
          </td>
          <td>{{ row.specification }}</td>
          <td>
            <b :class="$style.sent">{{ row.sent }}</b> / <b>{{ row.consumed }}</b> / <span>{{ row.returned }}</span>
          </td>
          <td>
            <strong :class="$style.balance">{{ row.balance }}</strong
            ><small :class="$style.age">账龄 {{ row.ageDays }} 天</small>
          </td>
          <td>
            <strong>{{ money.format(row.amount) }}</strong>
          </td>
          <td>{{ row.unit }}</td>
          <td>
            <button
              :class="$style.detailButton"
              @click="emit('view', row)"
            >
              详情
            </button>
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的委外库存
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.scroller table {
  @apply w-full min-w-[1510px] table-fixed border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.scroller th:nth-child(1) {
  width: 180px;
}
.scroller th:nth-child(2) {
  width: 180px;
}
.scroller th:nth-child(3) {
  width: 185px;
}
.scroller th:nth-child(4) {
  width: 155px;
}
.scroller th:nth-child(5) {
  width: 155px;
}
.scroller th:nth-child(6) {
  width: 145px;
}
.scroller th:nth-child(7) {
  width: 160px;
}
.scroller th:nth-child(8) {
  width: 120px;
}
.scroller th:nth-child(9) {
  width: 105px;
}
.scroller th:nth-child(10) {
  width: 65px;
}
.scroller th:nth-child(11) {
  width: 80px;
}
.scroller td {
  @apply h-[58px] overflow-hidden whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.scroller td strong,
.scroller td small {
  @apply block truncate;
}
.scroller td small {
  @apply mt-1 text-[9px];
  color: var(--ui-text-subtle);
}
.link,
.detailButton {
  @apply border-0 bg-transparent p-0 font-semibold;
  color: var(--app-primary);
}
.sent {
  color: var(--app-primary);
}
.balance {
  @apply text-center text-[12px];
  color: var(--ui-text);
}
.age {
  @apply mx-auto w-fit rounded px-1.5 py-0.5;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
