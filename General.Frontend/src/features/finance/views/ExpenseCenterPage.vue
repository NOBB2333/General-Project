<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { expenseCenterFixture, expenseMetrics } from '../model/expense-center.data';
import type {
  ExpenseDraft,
  ExpenseOwnershipFilter,
  ExpenseRow,
  ExpenseStatus,
  ExpenseStatusFilter,
  ExpenseView,
} from '../model/expense-center.types';
import ExpenseCenterHero from './expense-center/ExpenseCenterHero.vue';
import ExpenseCreateDialog from './expense-center/ExpenseCreateDialog.vue';
import ExpenseDetailDialog from './expense-center/ExpenseDetailDialog.vue';
import ExpensePeriodToolbar from './expense-center/ExpensePeriodToolbar.vue';
import ExpenseSummary from './expense-center/ExpenseSummary.vue';
import ExpenseWorkspace from './expense-center/ExpenseWorkspace.vue';

const rows = ref<ExpenseRow[]>(cloneReferenceRows(expenseCenterFixture.rows));
const activeView = ref<ExpenseView>(expenseCenterFixture.defaults?.view as ExpenseView);
const ownership = ref<ExpenseOwnershipFilter>(expenseCenterFixture.defaults?.ownership as ExpenseOwnershipFilter);
const activeStatus = ref<ExpenseStatusFilter>(expenseCenterFixture.defaults?.status as ExpenseStatusFilter);
const period = ref(String(expenseCenterFixture.defaults?.period));
const query = ref('');
const createOpen = ref(false);
const createKind = ref<'reimbursement' | 'enterprise'>('reimbursement');
const detailOpen = ref(false);
const activeRow = ref<ExpenseRow | null>(null);

const reimbursementCount = computed(() => rows.value.filter((row) => row.kind === 'reimbursement').length);
const enterpriseCount = computed(() => rows.value.filter((row) => row.kind === 'enterprise').length);
const paymentCount = computed(() => rows.value.filter((row) => row.hasPaymentApplication).length);
const viewRows = computed(() => {
  if (activeView.value === 'reimbursements') return rows.value.filter((row) => row.kind === 'reimbursement');
  if (activeView.value === 'enterprise') return rows.value.filter((row) => row.kind === 'enterprise');
  if (activeView.value === 'payments') return rows.value.filter((row) => row.hasPaymentApplication);
  return [];
});
const statusCounts = computed<Record<ExpenseStatus, number>>(() => ({
  待审核: viewRows.value.filter((row) => row.status === '待审核').length,
  审核中: viewRows.value.filter((row) => row.status === '审核中').length,
  已通过: viewRows.value.filter((row) => row.status === '已通过').length,
  已驳回: viewRows.value.filter((row) => row.status === '已驳回').length,
  已打款: viewRows.value.filter((row) => row.status === '已打款').length,
}));
const ownershipCounts = computed<Record<ExpenseOwnershipFilter, number>>(() => ({
  全部: viewRows.value.length,
  项目成本: viewRows.value.filter((row) => row.ownership === '项目成本').length,
  期间费用: viewRows.value.filter((row) => row.ownership === '期间费用').length,
}));
function matchesPeriod(row: ExpenseRow) {
  if (period.value === '全部') return true;
  const [year, month] = row.date.split('-').map(Number);
  if (period.value.startsWith('Q')) return year === 2026 && Math.ceil(month / 3) === Number(period.value.slice(1));
  if (/^\d{2}\/\d{2}$/.test(period.value)) {
    const [shortYear, shortMonth] = period.value.split('/');
    return row.date.startsWith(`20${shortYear}-${shortMonth}`);
  }
  return true;
}
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return viewRows.value.filter((row) => {
    const matchesOwnership = ownership.value === '全部' || row.ownership === ownership.value;
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesQuery =
      !normalized ||
      `${row.code} ${row.name} ${row.applicant} ${row.project}`.toLocaleLowerCase('zh-CN').includes(normalized);
    return matchesOwnership && matchesStatus && matchesQuery && matchesPeriod(row);
  });
});

function updateView(view: ExpenseView) {
  activeView.value = view;
  ownership.value = '全部';
  activeStatus.value = '全部';
  query.value = '';
}
function openCreate(kind: 'reimbursement' | 'enterprise') {
  createKind.value = kind;
  createOpen.value = true;
}
function saveExpense(draft: ExpenseDraft) {
  const isReimbursement = createKind.value === 'reimbursement';
  const created: ExpenseRow = {
    id: `expense-local-${Date.now()}`,
    kind: createKind.value,
    code: `${isReimbursement ? 'EXP' : 'ENT'}-LOCAL-${String(rows.value.length + 1).padStart(4, '0')}`,
    name: draft.name.trim(),
    ownership: draft.ownership,
    applicant: draft.applicant.trim(),
    department: draft.department.trim(),
    date: draft.date,
    project: draft.project.trim() || '-',
    amount: Number(draft.amount),
    status: '待审核',
    hasPaymentApplication: !isReimbursement,
  };
  rows.value = [created, ...rows.value];
  updateView(isReimbursement ? 'reimbursements' : 'enterprise');
  createOpen.value = false;
  toast.success(`${created.code} 已登记到本地费用列表`);
}
function openDetail(row: ExpenseRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function advance(row: ExpenseRow) {
  const next: ExpenseStatus = row.status === '待审核' ? '审核中' : row.status === '审核中' ? '已通过' : '已打款';
  rows.value = rows.value.map((item) => (item.id === row.id ? { ...item, status: next } : item));
  activeRow.value = rows.value.find((item) => item.id === row.id) ?? null;
  toast.success(`${row.code} 已更新为${next}`);
}
function exportRows() {
  const header = ['单号', '费用名称', '归属', '申请人', '部门', '日期', '项目', '金额', '状态'];
  const body = filteredRows.value.map((row) => [
    row.code,
    row.name,
    row.ownership,
    row.applicant,
    row.department,
    row.date,
    row.project,
    row.amount,
    row.status,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '费用中心.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${body.length} 条费用记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/expense-center"
    data-page-label="费用中心"
    data-page-module="财务管理"
    data-expense-center-page
    :data-active-view="activeView"
    :data-active-status="activeStatus"
    :data-result-count="filteredRows.length"
    :data-reimbursement-count="reimbursementCount"
    :data-enterprise-count="enterpriseCount"
  >
    <ExpenseCenterHero /><ExpensePeriodToolbar
      :period="period"
      @update:period="period = $event"
    /><ExpenseSummary :metrics="expenseMetrics" />
    <ExpenseWorkspace
      :view="activeView"
      :rows="filteredRows"
      :ownership="ownership"
      :status="activeStatus"
      :query="query"
      :reimbursement-count="reimbursementCount"
      :enterprise-count="enterpriseCount"
      :payment-count="paymentCount"
      :status-counts="statusCounts"
      :ownership-counts="ownershipCounts"
      @update:view="updateView"
      @update:ownership="ownership = $event"
      @update:status="activeStatus = $event"
      @update:query="query = $event"
      @export="exportRows"
      @create-reimbursement="openCreate('reimbursement')"
      @create-enterprise="openCreate('enterprise')"
      @view-row="openDetail"
    />
    <ExpenseCreateDialog
      v-model:open="createOpen"
      :kind="createKind"
      @save="saveExpense"
    />
    <ExpenseDetailDialog
      v-model:open="detailOpen"
      :row="activeRow"
      @advance="advance"
    />
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
</style>
