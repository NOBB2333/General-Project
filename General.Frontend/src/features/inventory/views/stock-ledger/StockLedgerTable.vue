<script setup lang="ts">
import type { StockLedgerRecord } from '../../model/stock-ledger.types';

defineProps<{ rows: readonly StockLedgerRecord[] }>();
const emit = defineEmits<{ view: [row: StockLedgerRecord] }>();
const currency = new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'CNY', maximumFractionDigits: 0 });
function money(value: number) {
  return `${value > 0 ? '+' : '-'}${currency.format(Math.abs(value))}`;
}
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <thead>
        <tr>
          <th>流水号 ↕</th>
          <th>类型</th>
          <th>业务类型</th>
          <th>日期 ↓</th>
          <th>仓库</th>
          <th>操作人</th>
          <th>数量 ↕</th>
          <th>金额 ↕</th>
          <th>状态</th>
          <th>关联单号</th>
          <th>对方</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <button
              :class="$style.codeButton"
              @click="emit('view', row)"
            >
              {{ row.flowCode }}
            </button>
          </td>
          <td>
            <span
              :class="$style.direction"
              :data-direction="row.direction"
              >{{ row.direction }}</span
            >
          </td>
          <td>
            <span
              :class="$style.business"
              :data-business="row.businessType"
              >{{ row.businessType }}</span
            >
          </td>
          <td>{{ row.date }}</td>
          <td>{{ row.warehouse }}</td>
          <td>{{ row.operator }}</td>
          <td>
            <strong>{{ row.quantity }}</strong>
          </td>
          <td>
            <strong
              :class="$style.amount"
              :data-direction="row.direction"
              >{{ money(row.amount) }}</strong
            >
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <button
              :class="$style.sourceButton"
              @click="emit('view', row)"
            >
              {{ row.sourceCode }}
            </button>
          </td>
          <td>{{ row.counterparty }}</td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的库存流水
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1420px] border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.scroller td {
  @apply h-[48px] whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.codeButton,
.sourceButton {
  @apply border-0 bg-transparent p-0 text-[11px] font-semibold;
  color: var(--app-primary);
}
.sourceButton {
  @apply font-medium;
}
.direction,
.business,
.status {
  @apply inline-flex rounded px-2 py-1 text-[9px] font-semibold;
}
.direction[data-direction='入库'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.direction[data-direction='出库'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.business {
  color: #6554c0;
  background: #eae6ff;
}
.business[data-business='采购入库'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.business[data-business='组装入库'] {
  color: #008da6;
  background: #e6fcff;
}
.business[data-business='组装退料'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.amount[data-direction='入库'] {
  color: var(--ui-success);
}
.amount[data-direction='出库'] {
  color: var(--ui-danger);
}
.status {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.status[data-status='待审批'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
