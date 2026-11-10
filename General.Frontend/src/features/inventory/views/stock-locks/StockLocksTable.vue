<script setup lang="ts">
import type { StockLockRecord } from '../../model/stock-locks.types';

defineProps<{ rows: readonly StockLockRecord[] }>();
const emit = defineEmits<{ view: [row: StockLockRecord] }>();
</script>

<template>
  <div :class="$style.scroller">
    <table>
      <colgroup>
        <col :class="$style.lockColumn" />
        <col :class="$style.productColumn" />
        <col :class="$style.batchColumn" />
        <col :class="$style.typeColumn" />
        <col :class="$style.reasonColumn" />
        <col :class="$style.quantityColumn" />
        <col :class="$style.releasedColumn" />
        <col :class="$style.statusColumn" />
        <col :class="$style.orderStatusColumn" />
        <col :class="$style.dateColumn" />
        <col :class="$style.dateColumn" />
        <col :class="$style.countdownColumn" />
      </colgroup>
      <thead>
        <tr>
          <th>锁定单号</th>
          <th>商品信息</th>
          <th>批次号</th>
          <th>锁定类型</th>
          <th>关联订单/原因</th>
          <th>锁定数量</th>
          <th>已释放</th>
          <th>状态</th>
          <th>订单状态</th>
          <th>锁定日期</th>
          <th>到期释放</th>
          <th>剩余倒计时</th>
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
              {{ row.lockCode }}</button
            ><small v-if="row.note">{{ row.note }}</small>
          </td>
          <td>
            <strong>{{ row.product }}</strong
            ><small>{{ row.specification }}</small>
          </td>
          <td>
            <code>{{ row.batchCode }}</code>
          </td>
          <td>
            <span
              :class="$style.type"
              :data-type="row.type"
              >{{ row.type }}</span
            >
          </td>
          <td>
            <button
              :class="$style.reasonButton"
              @click="emit('view', row)"
            >
              {{ row.reasonCode }}</button
            ><small>{{ row.reasonName }}</small>
          </td>
          <td>
            <strong>{{ row.quantity }}</strong
            ><small :class="$style.unit">{{ row.unit }}</small>
          </td>
          <td>
            <strong v-if="row.released">{{ row.released }}</strong
            ><span v-else>-</span>
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <span
              :class="$style.orderStatus"
              :data-status="row.orderStatus"
              >{{ row.orderStatus }}</span
            >
          </td>
          <td>{{ row.lockDate }}</td>
          <td :class="$style.releaseDate">{{ row.releaseDate }}</td>
          <td>
            <strong
              :class="$style.countdown"
              :data-state="row.countdown"
              >{{ row.countdown }}</strong
            >
          </td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      暂无匹配的库存锁定记录
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.scroller {
  @apply overflow-x-auto;
}
.scroller table {
  @apply w-full min-w-[1280px] table-fixed border-collapse text-left text-[10px];
}
.scroller th {
  @apply h-10 whitespace-nowrap border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.scroller td {
  @apply h-[54px] whitespace-nowrap border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.lockColumn {
  width: 118px;
}
.productColumn {
  width: 220px;
}
.batchColumn {
  width: 106px;
}
.typeColumn {
  width: 100px;
}
.reasonColumn {
  width: 160px;
}
.quantityColumn {
  width: 76px;
}
.releasedColumn {
  width: 70px;
}
.statusColumn {
  width: 84px;
}
.orderStatusColumn {
  width: 92px;
}
.dateColumn {
  width: 96px;
}
.countdownColumn {
  width: 90px;
}
.scroller td:nth-child(2),
.scroller td:nth-child(5) {
  @apply overflow-hidden;
}
.scroller td:nth-child(2) strong,
.scroller td:nth-child(2) small,
.scroller td:nth-child(5) small {
  @apply truncate;
}
.scroller small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}
.scroller code {
  @apply text-[10px];
  color: var(--ui-text-muted);
}
.codeButton,
.reasonButton {
  @apply border-0 bg-transparent p-0 text-[11px] font-bold;
  color: var(--app-primary);
}
.reasonButton {
  @apply font-semibold;
}
.unit {
  @apply ml-1 inline;
}
.type,
.status,
.orderStatus {
  @apply inline-flex rounded px-2 py-1 text-[9px] font-semibold;
}
.type {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.type[data-type='手动锁库'] {
  color: #6554c0;
  background: #eae6ff;
}
.status {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.status[data-status='部分释放'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='已释放'] {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.orderStatus {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.orderStatus[data-status='审批中'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.orderStatus[data-status='无需审批'] {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.releaseDate,
.countdown[data-state='已过期'] {
  @apply font-semibold;
  color: var(--ui-danger);
}
.countdown[data-state='已释放'] {
  color: var(--ui-success);
}
.empty {
  @apply py-16 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
