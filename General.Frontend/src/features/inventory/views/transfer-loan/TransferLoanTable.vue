<script setup lang="ts">
import { ArrowDownLeft, ArrowRight, ArrowUpRight, Search } from 'lucide-vue-next';
import { UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { TransferLoanRow, TransferLoanStatus } from '../../model/transfer-loan.types';

defineProps<{
  rows: readonly TransferLoanRow[];
  activeStatus: '全部' | TransferLoanStatus;
  query: string;
}>();

const emit = defineEmits<{
  'update:activeStatus': [value: '全部' | TransferLoanStatus];
  'update:query': [value: string];
  details: [row: TransferLoanRow];
}>();

const statuses: readonly { value: '全部' | TransferLoanStatus; label: string }[] = [
  { value: '全部', label: '全部' },
  { value: '待审核', label: '待审核(1)' },
  { value: '已审核', label: '已审核(1)' },
  { value: '调拨中', label: '调拨中(3)' },
  { value: '已完成', label: '已完成(4)' },
  { value: '已取消', label: '已取消(1)' },
];

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.tableCard">
    <header :class="$style.toolbar">
      <UiTabs
        :model-value="activeStatus"
        @update:model-value="emit('update:activeStatus', $event as '全部' | TransferLoanStatus)"
      >
        <UiTabsList
          label="单据状态"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            v-for="status in statuses"
            :key="status.value"
            :value="status.value"
            :class="$style.tab"
          >
            {{ status.label }}
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          :model-value="query"
          aria-label="搜索调拨与借出单据"
          placeholder="搜索单号、仓库、单位..."
          @update:model-value="emit('update:query', $event)"
        />
      </label>
    </header>

    <div :class="$style.scroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th>单号</th>
            <th>业务类型</th>
            <th>发出方</th>
            <th>接收方</th>
            <th>原因</th>
            <th>商品/数量</th>
            <th>金额</th>
            <th>状态</th>
            <th>关联</th>
            <th>日期/申请人</th>
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
                @click="emit('details', row)"
              >
                {{ row.code }}
              </button>
            </td>
            <td>
              <span :class="[$style.typeBadge, $style[`kind_${row.kind}`]]">
                <ArrowUpRight
                  v-if="row.direction === 'out'"
                  :size="12"
                />
                <ArrowDownLeft
                  v-else-if="row.direction === 'return'"
                  :size="12"
                />
                <ArrowRight
                  v-else
                  :size="12"
                />
                {{ row.type }}
              </span>
            </td>
            <td>{{ row.sender }}</td>
            <td>{{ row.receiver }}</td>
            <td>{{ row.reason }}</td>
            <td>
              <strong>{{ row.productCount }}种 / {{ row.quantity }}件</strong>
            </td>
            <td>
              <strong>{{ currency(row.amount) }}</strong>
              <small
                v-if="row.adjustment"
                :class="$style.adjustment"
                >{{ row.adjustment }}</small
              >
            </td>
            <td>
              <span :class="[$style.statusBadge, $style[`status_${row.status}`]]">{{ row.status }}</span>
              <small
                v-if="row.overdueDays"
                :class="$style.overdue"
                >逾期</small
              >
            </td>
            <td>{{ row.relatedCode ?? '-' }}</td>
            <td>
              <span>{{ row.date }}</span>
              <small>{{ row.applicant }}</small>
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              colspan="10"
              :class="$style.empty"
            >
              当前筛选下没有调拨与借出单据
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <footer :class="$style.footer">共 {{ rows.length }} 条记录</footer>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableCard {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.toolbar {
  @apply flex min-h-[48px] items-center justify-between gap-4 border-b px-4;
  border-color: var(--ui-border);
}

.tabs {
  @apply gap-1;
}

.tab {
  @apply min-h-10 rounded-none border-b-2 border-transparent px-3 text-[11px];
}

.tab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}

.searchField {
  @apply flex h-8 w-[280px] items-center gap-2 rounded border px-2;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.searchField:focus-within {
  border-color: var(--app-primary-light);
  box-shadow: 0 0 0 1px var(--app-primary-light);
}

.searchField input {
  @apply h-full border-0 p-0 shadow-none;
}

.scroller {
  @apply overflow-x-auto;
}

.table {
  @apply w-full min-w-[1420px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-10 border-b px-4 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[58px] border-b px-4 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.table th:nth-child(1),
.table td:nth-child(1) {
  @apply min-w-[150px];
}
.table th:nth-child(2),
.table td:nth-child(2) {
  @apply min-w-[150px];
}
.table th:nth-child(3),
.table td:nth-child(3) {
  @apply min-w-[135px];
}
.table th:nth-child(4),
.table td:nth-child(4) {
  @apply min-w-[165px];
}
.table th:nth-child(5),
.table td:nth-child(5) {
  @apply min-w-[175px];
}

.code {
  @apply font-semibold;
  color: var(--app-primary);
}

.code:hover {
  text-decoration: underline;
}

.typeBadge {
  @apply inline-flex items-center gap-1 rounded px-2 py-1 font-medium;
}

.kind_transfer {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.kind_loan {
  color: #6554c0;
  background: #eae6ff;
}

.kind_repricing {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.statusBadge {
  @apply inline-flex rounded px-2 py-1 font-medium;
}

.status_待审核 {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.status_已审核,
.status_调拨中 {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.status_已完成 {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.status_已取消 {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}

.table small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}

.adjustment,
.overdue {
  color: var(--ui-danger) !important;
}

.empty {
  @apply h-24 text-center;
  color: var(--ui-text-subtle);
}

.footer {
  @apply flex h-10 items-center justify-end px-4 text-[10px];
  color: var(--ui-text-subtle);
}
</style>
