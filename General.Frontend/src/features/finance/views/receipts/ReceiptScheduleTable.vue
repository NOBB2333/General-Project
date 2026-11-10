<script setup lang="ts">
import { Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { ReceiptScheduleRow } from '../../model/receipts.types';

defineProps<{
  title: string;
  rows: readonly ReceiptScheduleRow[];
  query: string;
  riskFilter: string;
  compact?: boolean;
}>();

const emit = defineEmits<{
  'update:query': [value: string];
  'update:riskFilter': [value: string];
  inspect: [row: ReceiptScheduleRow];
}>();

const riskOptions: UiSelectOption[] = [
  { value: '全部风险', label: '全部风险' },
  { value: '高风险', label: '高风险' },
  { value: '中风险', label: '中风险' },
  { value: '低风险', label: '低风险' },
];

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="[$style.tableSection, compact && $style.compact]">
    <header :class="$style.toolbar">
      <div>
        <h2>{{ title }}</h2>
        <p>{{ rows.length }} 个订单 · 逐期跟踪计划到账与逾期风险</p>
      </div>
      <div :class="$style.filters">
        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <UiInput
            :model-value="query"
            placeholder="搜索订单、客户..."
            aria-label="搜索订单、客户"
            @update:model-value="emit('update:query', $event)"
          />
        </label>
        <UiSelect
          :model-value="riskFilter"
          :options="riskOptions"
          label="风险等级"
          size="sm"
          @update:model-value="emit('update:riskFilter', $event)"
        />
      </div>
    </header>

    <div :class="$style.tableScroller">
      <table :class="$style.table">
        <thead>
          <tr>
            <th>风险</th>
            <th>订单 / 客户</th>
            <th>计划/已到</th>
            <th>逾期金额</th>
            <th>逾期 / 即将</th>
            <th>最长逾期</th>
            <th>下一期 · 倒计时</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <span :class="$style.riskBadge">{{ row.risk }}</span>
            </td>
            <td>
              <strong :class="$style.orderCode">{{ row.orderCode }}</strong>
              <span :class="$style.customer">{{ row.customer }}</span>
            </td>
            <td>
              <strong>{{ currency(row.planned) }}</strong>
              <small>已到 {{ currency(row.received) }}</small>
            </td>
            <td :class="$style.overdueAmount">{{ currency(row.overdueAmount) }}</td>
            <td>
              <strong>{{ row.overdueCount }}</strong>
              <small>{{ row.upcomingCount }} 期即将</small>
            </td>
            <td>{{ row.longestOverdueDays }} 天</td>
            <td>
              <strong>{{ row.nextDate }}</strong>
              <small :class="$style.overdueText">已逾期 {{ Math.abs(row.countdownDays) }} 天</small>
            </td>
            <td>
              <UiButton
                variant="ghost"
                size="sm"
                @click="emit('inspect', row)"
                >查看</UiButton
              >
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              :class="$style.emptyCell"
              colspan="8"
            >
              当前筛选下没有回款计划
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableSection {
  @apply min-w-0 bg-white;
}

.toolbar {
  @apply flex min-h-[66px] items-center justify-between gap-4 border-b px-5 py-3;
  border-color: var(--ui-border);
}

.toolbar h2 {
  @apply m-0 text-[14px] font-semibold;
  color: var(--ui-text);
}

.toolbar p {
  @apply m-0 mt-1 text-[11px];
  color: var(--ui-text-subtle);
}

.filters {
  @apply flex items-center gap-2;
}

.searchField {
  @apply flex h-8 w-[232px] items-center gap-2 rounded border px-2;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
  background: var(--ui-surface);
}

.searchField:focus-within {
  border-color: var(--app-primary-light);
  box-shadow: 0 0 0 1px var(--app-primary-light);
}

.searchField input {
  @apply h-full border-0 p-0 shadow-none;
}

.tableScroller {
  @apply overflow-x-auto;
}

.table {
  @apply w-full min-w-[1120px] border-collapse text-left text-[11px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-10 border-b px-4 font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[62px] border-b px-4 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.table td:nth-child(1) {
  @apply w-[90px];
}

.table td:nth-child(2) {
  @apply min-w-[210px];
}

.table td:nth-child(3) {
  @apply min-w-[150px];
}

.table td:nth-child(7) {
  @apply min-w-[170px];
}

.table strong,
.table small,
.customer {
  @apply block;
}

.table strong {
  @apply font-medium;
  color: var(--ui-text);
}

.table small,
.customer {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}

.orderCode {
  color: var(--app-primary) !important;
}

.riskBadge {
  @apply inline-flex rounded px-2 py-1 font-semibold;
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.overdueAmount,
.overdueText {
  color: var(--ui-danger) !important;
}

.emptyCell {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}

.compact .toolbar {
  @apply min-h-[58px];
}

@media (prefers-reduced-motion: reduce) {
  .table tbody tr {
    transition: none;
  }
}
</style>
