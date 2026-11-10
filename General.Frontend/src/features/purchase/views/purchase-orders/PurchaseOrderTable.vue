<script setup lang="ts">
import { CreditCard, Eye, Star } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { UiButton, UiCheckbox } from '@/components/ui';
import type { PurchaseOrderRow } from '../../model/purchase-orders.types';

const props = defineProps<{ rows: readonly PurchaseOrderRow[] }>();
const emit = defineEmits<{
  details: [row: PurchaseOrderRow];
  payment: [row: PurchaseOrderRow];
  toggleFollow: [row: PurchaseOrderRow];
}>();

const selectedIds = ref<string[]>([]);
const allChecked = computed<boolean | 'indeterminate'>(() => {
  if (!selectedIds.value.length) return false;
  if (props.rows.length && props.rows.every((row) => selectedIds.value.includes(row.id))) return true;
  return 'indeterminate';
});

function toggleAll(value: boolean | 'indeterminate') {
  selectedIds.value = value === true ? props.rows.map((row) => row.id) : [];
}

function toggleRow(id: string, value: boolean | 'indeterminate') {
  selectedIds.value =
    value === true ? [...new Set([...selectedIds.value, id])] : selectedIds.value.filter((item) => item !== id);
}

function currency(value: number) {
  return `¥${value.toLocaleString('en-US')}`;
}

function initials(name: string) {
  return name.slice(0, 1);
}
</script>

<template>
  <div :class="$style.scroller">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>
            <UiCheckbox
              :model-value="allChecked"
              label="选择当前全部采购订单"
              @update:model-value="toggleAll"
            />
          </th>
          <th aria-label="关注状态" />
          <th>订单号</th>
          <th>供应商</th>
          <th>关联项目</th>
          <th>应付款确认方式</th>
          <th>采购员</th>
          <th>金额</th>
          <th>开票</th>
          <th>入库</th>
          <th>付款</th>
          <th>状态</th>
          <th>日期</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
        >
          <td>
            <UiCheckbox
              :model-value="selectedIds.includes(row.id)"
              :label="`选择${row.code}`"
              @update:model-value="toggleRow(row.id, $event)"
            />
          </td>
          <td>
            <button
              type="button"
              :class="[$style.follow, row.followed && $style.followed]"
              :aria-label="row.followed ? `取消关注${row.code}` : `关注${row.code}`"
              @click="emit('toggleFollow', row)"
            >
              <Star
                :size="15"
                :stroke-width="1.8"
              />
            </button>
          </td>
          <td>
            <button
              type="button"
              :class="$style.code"
              @click="emit('details', row)"
            >
              {{ row.code }}
            </button>
          </td>
          <td :class="$style.supplier">{{ row.supplier }}</td>
          <td :class="row.project === '库存采购' ? $style.muted : $style.project">{{ row.project }}</td>
          <td>
            <span :class="$style.rule">{{ row.payableRule }}</span>
          </td>
          <td>
            <span :class="$style.buyer"
              ><i>{{ initials(row.buyer) }}</i
              >{{ row.buyer }}</span
            >
          </td>
          <td :class="$style.amount">{{ currency(row.amount) }}</td>
          <td>
            <span
              :class="$style.progress"
              data-tone="invoice"
              :data-value="row.invoiceProgress"
              ><i /><b>{{ row.invoiceProgress }}%</b></span
            >
          </td>
          <td>
            <span
              :class="$style.progress"
              data-tone="stock"
              :data-value="row.stockProgress"
              ><i /><b>{{ row.stockProgress }}%</b></span
            >
          </td>
          <td>
            <span
              :class="$style.progress"
              data-tone="payment"
              :data-value="row.paymentProgress"
              ><i /><b>{{ row.paymentProgress }}%</b></span
            >
          </td>
          <td>
            <span :class="[$style.status, $style[`status_${row.status}`]]">{{ row.status }}</span>
          </td>
          <td :class="$style.date">{{ row.date }}</td>
          <td>
            <UiButton
              v-if="row.status === '已审核' || row.status === '部分到货'"
              variant="link"
              size="xs"
              @click="emit('payment', row)"
            >
              <template #leading><CreditCard :size="13" /></template>
              付款申请
            </UiButton>
            <UiButton
              v-else
              variant="ghost"
              size="icon-sm"
              :aria-label="`查看${row.code}`"
              title="查看订单"
              @click="emit('details', row)"
            >
              <Eye :size="14" />
            </UiButton>
          </td>
        </tr>
        <tr v-if="!rows.length">
          <td
            :class="$style.empty"
            colspan="14"
          >
            当前条件下没有采购订单
          </td>
        </tr>
      </tbody>
    </table>
  </div>
  <footer :class="$style.footer">
    <span>共 {{ rows.length }} 张订单</span>
    <span v-if="selectedIds.length">已选择 {{ selectedIds.length }} 张</span>
  </footer>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroller {
  @apply overflow-x-auto;
  scrollbar-width: thin;
}

.table {
  @apply w-full min-w-[1600px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-[40px] border-b px-3 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[70px] border-b px-3 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.table th:first-child,
.table td:first-child {
  @apply w-9 pl-4 pr-1;
}

.table th:nth-child(2),
.table td:nth-child(2) {
  @apply w-9 px-1 text-center;
}

.follow {
  @apply inline-flex h-7 w-7 items-center justify-center rounded;
  color: #c1c7d0;
}

.followed {
  color: #ffab00;
  fill: #ffab00;
}

.code {
  @apply whitespace-nowrap font-semibold hover:underline;
  color: var(--app-primary);
}

.supplier,
.amount {
  @apply whitespace-nowrap font-semibold;
  color: var(--ui-text);
}

.project {
  color: var(--app-primary);
}

.muted,
.date {
  color: var(--ui-text-subtle);
}

.rule {
  @apply inline-flex max-w-[95px] items-center rounded px-2 py-1 text-center font-semibold;
  color: #0065ff;
  background: #e6fcff;
}

.buyer {
  @apply inline-flex items-center gap-2 whitespace-nowrap;
}

.buyer i {
  @apply flex h-5 w-5 items-center justify-center rounded-full not-italic text-[9px] font-semibold text-white;
  background: var(--app-primary);
}

.progress {
  @apply flex items-center gap-2 whitespace-nowrap;
}

.progress > i {
  @apply relative block h-1.5 w-[62px] overflow-hidden rounded-full;
  background: #ebecf0;
}

.progress > i::after {
  @apply absolute inset-y-0 left-0 block rounded-full;
  content: '';
  width: 0;
  background: var(--app-primary);
}

.progress[data-tone='stock'] > i::after {
  background: var(--ui-warning);
}

.progress[data-tone='payment'] > i::after {
  background: var(--ui-success);
}

.progress[data-value='20'] > i::after {
  width: 20%;
}
.progress[data-value='30'] > i::after {
  width: 30%;
}
.progress[data-value='50'] > i::after {
  width: 50%;
}
.progress[data-value='60'] > i::after {
  width: 60%;
}
.progress[data-value='70'] > i::after {
  width: 70%;
}
.progress[data-value='100'] > i::after {
  width: 100%;
}

.progress b {
  @apply text-[10px] font-semibold;
  color: var(--ui-text-muted);
}

.status {
  @apply inline-flex whitespace-nowrap rounded px-2 py-1 text-[10px] font-semibold;
}

.status_草稿 {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.status_待审核 {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.status_已审核 {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.status_部分到货 {
  color: #008da6;
  background: #e6fcff;
}
.status_已完成 {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.status_已取消 {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.empty {
  @apply py-16 text-center;
  color: var(--ui-text-subtle);
}

.footer {
  @apply flex h-12 items-center gap-5 px-5 text-[11px];
  color: var(--ui-text-subtle);
}
</style>
