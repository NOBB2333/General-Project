<script setup lang="ts">
import { BriefcaseBusiness, CalendarDays, Search, UserRound } from 'lucide-vue-next';
import { UiButton, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type {
  BankLoanRow,
  EmployeeLoanFilter,
  EmployeeLoanRow,
  EmployeeLoanStatus,
  LoanView,
} from '../../model/loans.types';

defineProps<{
  view: LoanView;
  rows: readonly EmployeeLoanRow[];
  bankRows: readonly BankLoanRow[];
  status: EmployeeLoanFilter;
  statusCounts: Readonly<Record<EmployeeLoanStatus, number>>;
  query: string;
}>();

const emit = defineEmits<{
  'update:status': [value: EmployeeLoanFilter];
  'update:query': [value: string];
  viewEmployee: [row: EmployeeLoanRow];
  viewBank: [row: BankLoanRow];
}>();

const statuses: readonly EmployeeLoanFilter[] = ['全部', '待审核', '审核中', '已打款', '部分核销', '已核销', '已驳回'];

function statusLabel(status: EmployeeLoanFilter, counts: Readonly<Record<EmployeeLoanStatus, number>>) {
  return status === '全部' ? status : `${status}(${counts[status]})`;
}

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.workspace">
    <header :class="$style.workspaceHeader">
      <UiTabs
        v-if="view === 'employee'"
        :model-value="status"
        @update:model-value="emit('update:status', $event as EmployeeLoanFilter)"
      >
        <UiTabsList
          label="员工借款状态"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            v-for="item in statuses"
            :key="item"
            :value="item"
            :class="$style.tab"
          >
            {{ statusLabel(item, statusCounts) }}
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>
      <strong
        v-else
        :class="$style.bankTitle"
        >银行贷款合同</strong
      >
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          :model-value="query"
          placeholder="搜索..."
          aria-label="搜索借款贷款"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
    </header>

    <div
      v-if="view === 'employee'"
      :class="$style.loanList"
    >
      <article
        v-for="row in rows"
        :key="row.id"
        :class="$style.loanRow"
      >
        <div :class="$style.loanMain">
          <div :class="$style.loanHeading">
            <strong :class="$style.code">{{ row.code }}</strong>
            <b>{{ row.title }}</b>
            <span :class="$style.category">{{ row.category }}</span>
          </div>
          <div :class="$style.meta">
            <span
              ><UserRound
                :size="12"
                :stroke-width="1.8"
              />{{ row.borrower }} · {{ row.department }}</span
            >
            <span
              ><CalendarDays
                :size="12"
                :stroke-width="1.8"
              />{{ row.requestedAt }}</span
            >
            <span
              ><BriefcaseBusiness
                :size="12"
                :stroke-width="1.8"
              />{{ row.project }}</span
            >
          </div>
          <div
            :class="$style.progress"
            aria-hidden="true"
          >
            <i /><i /><i />
          </div>
          <span
            v-if="row.balance"
            :class="$style.balance"
            >余额 {{ money(row.balance) }}</span
          >
        </div>
        <div :class="$style.loanAside">
          <strong>{{ money(row.amount) }}</strong>
          <span
            :class="$style.status"
            :data-status="row.status"
            >{{ row.status }}</span
          >
          <UiButton
            variant="link"
            size="xs"
            @click="emit('viewEmployee', row)"
            >查看</UiButton
          >
        </div>
      </article>
      <div
        v-if="!rows.length"
        :class="$style.empty"
      >
        当前筛选下没有员工借款
      </div>
    </div>

    <div
      v-else
      :class="$style.bankGrid"
    >
      <article
        v-for="row in bankRows"
        :key="row.id"
        :class="$style.bankCard"
      >
        <header>
          <span :class="$style.bankIcon"
            ><BriefcaseBusiness
              :size="18"
              :stroke-width="1.8"
          /></span>
          <div>
            <strong>{{ row.bank }}</strong
            ><small>{{ row.code }}</small>
          </div>
          <span
            :class="$style.status"
            :data-status="row.status"
            >{{ row.status }}</span
          >
        </header>
        <dl>
          <div>
            <dt>贷款金额</dt>
            <dd>{{ money(row.amount) }}</dd>
          </div>
          <div>
            <dt>剩余本金</dt>
            <dd>{{ money(row.balance) }}</dd>
          </div>
          <div>
            <dt>年利率</dt>
            <dd>{{ row.rate }}</dd>
          </div>
          <div>
            <dt>期限 / 月供</dt>
            <dd>{{ row.termMonths }} 个月 · {{ money(row.monthlyPayment) }}</dd>
          </div>
        </dl>
        <footer>
          <small>起息日 {{ row.startDate }}</small
          ><UiButton
            variant="link"
            size="xs"
            @click="emit('viewBank', row)"
            >查看贷款</UiButton
          >
        </footer>
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.workspaceHeader {
  @apply flex min-h-[50px] items-center border-b px-4;
  border-color: var(--ui-border);
}
.tabs {
  @apply gap-1;
}
.tab {
  @apply min-h-[48px] rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-subtle);
}
.tab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}
.bankTitle {
  @apply text-[13px];
  color: var(--ui-text);
}
.searchField {
  @apply ml-auto flex h-8 w-[185px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: transparent;
  background: var(--ui-surface-hover);
}
.searchField:focus-within {
  border-color: var(--app-primary-light);
  background: var(--ui-surface);
}
.searchField input {
  @apply h-full border-0 bg-transparent p-0 shadow-none;
}
.loanRow {
  @apply flex min-h-[88px] items-start border-b px-4 py-3;
  border-color: var(--ui-border-subtle);
}
.loanRow:hover {
  background: var(--ui-surface-hover);
}
.loanMain {
  @apply min-w-0 flex-1;
}
.loanHeading {
  @apply flex items-center gap-2;
}
.loanHeading b {
  @apply truncate text-[13px] font-semibold;
  color: var(--ui-text);
}
.code {
  @apply text-[12px] font-semibold;
  color: var(--app-primary);
}
.category {
  @apply rounded px-2 py-0.5 text-[9px] font-semibold;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.meta {
  @apply mt-2 flex items-center gap-4 text-[10px];
  color: var(--ui-text-subtle);
}
.meta span {
  @apply flex items-center gap-1;
}
.progress {
  @apply mt-2 flex items-center gap-1;
}
.progress i {
  @apply relative h-[13px] w-[13px] rounded-full border-2;
  border-color: var(--ui-success);
}
.progress i:not(:last-child)::after {
  @apply absolute left-[12px] top-[4px] h-0.5 w-4;
  content: '';
  background: var(--ui-success);
}
.progress i + i {
  @apply ml-3;
}
.balance {
  @apply ml-3 inline-flex rounded px-2 py-0.5 text-[9px] font-semibold;
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.loanAside {
  @apply ml-4 flex items-center gap-3;
}
.loanAside > strong {
  @apply text-[14px];
  color: var(--ui-text);
}
.status {
  @apply inline-flex whitespace-nowrap rounded-full px-2 py-0.5 text-[9px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.status[data-status='已核销'],
.status[data-status='已通过'],
.status[data-status='已结清'] {
  color: var(--ui-success);
  background: #e3fcef;
}
.status[data-status='待审核'],
.status[data-status='审核中'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='已打款'],
.status[data-status='还款中'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.status[data-status='部分核销'] {
  color: #6554c0;
  background: #eae6ff;
}
.status[data-status='已驳回'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.empty {
  @apply flex h-36 items-center justify-center text-[12px];
  color: var(--ui-text-subtle);
}
.bankGrid {
  @apply grid grid-cols-2 gap-4 p-4;
}
.bankCard {
  @apply rounded border p-4;
  border-color: var(--ui-border);
}
.bankCard header {
  @apply flex items-center gap-3;
}
.bankIcon {
  @apply flex h-9 w-9 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.bankCard header div {
  @apply min-w-0 flex-1;
}
.bankCard header strong,
.bankCard header small {
  @apply block;
}
.bankCard header small {
  @apply mt-0.5 text-[9px];
  color: var(--ui-text-subtle);
}
.bankCard dl {
  @apply mt-4 grid grid-cols-2 gap-3;
}
.bankCard dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.bankCard dd {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}
.bankCard footer {
  @apply mt-4 flex items-center justify-between border-t pt-3;
  border-color: var(--ui-border-subtle);
  color: var(--ui-text-subtle);
}
</style>
