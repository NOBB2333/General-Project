<script setup lang="ts">
import type { ProductTraceRow } from '../../model/product-trace.types';
defineProps<{ rows: readonly ProductTraceRow[] }>();
const emit = defineEmits<{ open: [row: ProductTraceRow] }>();
function statusTone(value: ProductTraceRow['status']) {
  if (value === '在库') return 'statusStock';
  if (value === '已出库') return 'statusOutbound';
  if (value === '已报修' || value === '维修中') return 'statusRepair';
  if (value === '已换货') return 'statusSwap';
  return 'statusNeutral';
}
function warrantyTone(value: ProductTraceRow['warrantyStatus']) {
  if (value === '在保') return 'warrantyActive';
  if (value === '即将到期') return 'warrantyWarning';
  if (value === '已过保') return 'warrantyExpired';
  return 'warrantyPending';
}
</script>

<template>
  <div :class="$style.frame">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>追溯码</th>
          <th>外部SN</th>
          <th>商品名称</th>
          <th>型号</th>
          <th>来源</th>
          <th>状态</th>
          <th>质保状态</th>
          <th>客户</th>
          <th>出库日期</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <button
              type="button"
              :class="$style.code"
              @click="emit('open', row)"
            >
              {{ row.traceCode }}
            </button>
          </td>
          <td :class="$style.serial">{{ row.externalSn }}</td>
          <td :class="$style.name">{{ row.productName }}</td>
          <td>{{ row.model }}</td>
          <td>
            <span :class="[$style.badge, row.source === '自制' ? $style.sourceSelf : $style.sourceExternal]">{{
              row.source
            }}</span>
          </td>
          <td>
            <span :class="[$style.badge, $style[statusTone(row.status)]]">{{ row.status }}</span>
          </td>
          <td>
            <span :class="[$style.badge, $style[warrantyTone(row.warrantyStatus)]]">{{ row.warrantyStatus }}</span>
          </td>
          <td>{{ row.customer }}</td>
          <td>{{ row.outboundDate }}</td>
        </tr>
      </tbody>
    </table>
    <div
      v-if="!rows.length"
      :class="$style.empty"
    >
      当前筛选下没有产品实例
    </div>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";
.frame {
  @apply min-h-[330px] overflow-auto rounded border bg-white;
  border-color: var(--ui-border);
}
.table {
  @apply w-full min-w-[1180px] table-fixed border-collapse text-left text-[11.5px];
}
.table th {
  @apply h-10 border-b bg-[#FAFBFC] px-3 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.table th:nth-child(1) {
  @apply w-[180px];
}
.table th:nth-child(2) {
  @apply w-[220px];
}
.table th:nth-child(3) {
  @apply w-[170px];
}
.table th:nth-child(4) {
  @apply w-[130px];
}
.table th:nth-child(5),
.table th:nth-child(6),
.table th:nth-child(7) {
  @apply w-[90px];
}
.table th:nth-child(8) {
  @apply w-[140px];
}
.table th:nth-child(9) {
  @apply w-[110px];
}
.table td {
  @apply h-10 border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.code {
  @apply font-semibold;
  color: #0065ff;
}
.serial {
  @apply font-mono text-[10.5px];
  color: #5e6c84;
}
.name {
  @apply font-medium;
  color: var(--ui-text);
}
.badge {
  @apply inline-flex rounded px-2 py-0.5 text-[10.5px] font-semibold;
}
.sourceExternal {
  @apply bg-[#DEEBFF];
  color: #0052cc;
}
.sourceSelf {
  @apply bg-[#E3FCEF];
  color: #006644;
}
.statusStock {
  @apply bg-[#E3FCEF];
  color: #00875a;
}
.statusOutbound {
  @apply bg-[#DEEBFF];
  color: #0052cc;
}
.statusRepair {
  @apply bg-[#FFEBE6];
  color: #de350b;
}
.statusSwap {
  @apply bg-[#EAE6FF];
  color: #5243aa;
}
.statusNeutral {
  @apply bg-[#F4F5F7];
  color: #6b778c;
}
.warrantyActive {
  @apply bg-[#E3FCEF];
  color: #00875a;
}
.warrantyWarning {
  @apply bg-[#FFFAE6];
  color: #ff8b00;
}
.warrantyExpired {
  @apply bg-[#FFEBE6];
  color: #de350b;
}
.warrantyPending {
  @apply bg-[#F4F5F7];
  color: #97a0af;
}
.empty {
  @apply py-20 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
