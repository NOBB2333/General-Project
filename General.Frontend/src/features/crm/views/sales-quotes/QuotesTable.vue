<script setup lang="ts">
import type { SalesQuoteRow } from '@/features/crm/model/sales-quotes.types';
import QuoteTableRow from './QuoteTableRow.vue';

defineProps<{ rows: readonly SalesQuoteRow[] }>();
</script>

<template>
  <div :class="$style.scroll">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>报价单号</th>
          <th>客户</th>
          <th>关联项目</th>
          <th>负责人</th>
          <th>商品数</th>
          <th>报价金额</th>
          <th>折扣</th>
          <th>状态</th>
          <th>有效期至</th>
          <th>日期</th>
          <th aria-label="操作" />
        </tr>
      </thead>
      <tbody>
        <QuoteTableRow
          v-for="row in rows"
          :key="row.id"
          :row="row"
        />
        <tr v-if="!rows.length">
          <td
            colspan="11"
            :class="$style.empty"
          >
            当前条件下没有报价单
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroll {
  @apply overflow-x-auto;
}
.table {
  @apply w-full min-w-[1240px] border-collapse text-left text-[12px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-[49px] whitespace-nowrap px-4 font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
  border-bottom: 1px solid var(--ui-border);
}
.table th:nth-child(1) {
  width: 150px;
}
.table th:nth-child(2) {
  width: 170px;
}
.table th:nth-child(3) {
  width: 180px;
}
.table th:nth-child(4) {
  width: 130px;
}
.table th:nth-child(5) {
  width: 90px;
}
.table th:nth-child(6) {
  width: 130px;
}
.table th:nth-child(7) {
  width: 80px;
}
.table th:nth-child(8) {
  width: 100px;
}
.table th:nth-child(9) {
  width: 130px;
}
.table th:nth-child(10) {
  width: 120px;
}
.table th:nth-child(11) {
  width: 54px;
}
.empty {
  @apply h-36 text-center;
  color: var(--ui-text-subtle);
}
</style>
