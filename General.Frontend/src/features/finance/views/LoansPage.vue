<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { bankLoanRows, loanMetrics, loansReferenceFixture } from '../model/loans.data';
import type {
  BankLoanRow,
  EmployeeLoanDraft,
  EmployeeLoanFilter,
  EmployeeLoanRow,
  EmployeeLoanStatus,
  LoanView,
} from '../model/loans.types';
import LoanCreateDialog from './loans/LoanCreateDialog.vue';
import LoanDetailDialog from './loans/LoanDetailDialog.vue';
import LoansHero from './loans/LoansHero.vue';
import LoansSummary from './loans/LoansSummary.vue';
import LoansViewToolbar from './loans/LoansViewToolbar.vue';
import LoansWorkspace from './loans/LoansWorkspace.vue';

const rows = ref<EmployeeLoanRow[]>(cloneReferenceRows(loansReferenceFixture.rows));
const localBankRows = ref<BankLoanRow[]>(cloneReferenceRows(bankLoanRows));
const activeView = ref<LoanView>(loansReferenceFixture.defaults?.activeView as LoanView);
const activeStatus = ref<EmployeeLoanFilter>(loansReferenceFixture.defaults?.activeStatus as EmployeeLoanFilter);
const query = ref('');
const createOpen = ref(false);
const detailOpen = ref(false);
const activeEmployee = ref<EmployeeLoanRow | null>(null);
const activeBank = ref<BankLoanRow | null>(null);

const statusCounts = computed<Record<EmployeeLoanStatus, number>>(() => ({
  待审核: rows.value.filter((row) => row.status === '待审核').length,
  审核中: rows.value.filter((row) => row.status === '审核中').length,
  已通过: rows.value.filter((row) => row.status === '已通过').length,
  已打款: rows.value.filter((row) => row.status === '已打款').length,
  部分核销: rows.value.filter((row) => row.status === '部分核销').length,
  已核销: rows.value.filter((row) => row.status === '已核销').length,
  已驳回: rows.value.filter((row) => row.status === '已驳回').length,
}));
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesQuery =
      !normalized ||
      `${row.code} ${row.title} ${row.borrower} ${row.department} ${row.project}`
        .toLocaleLowerCase('zh-CN')
        .includes(normalized);
    return matchesStatus && matchesQuery;
  });
});
const filteredBankRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return localBankRows.value.filter(
    (row) => !normalized || `${row.code} ${row.bank} ${row.status}`.toLocaleLowerCase('zh-CN').includes(normalized),
  );
});

function updateView(view: LoanView) {
  activeView.value = view;
  activeStatus.value = '全部';
  query.value = '';
}

function openEmployee(row: EmployeeLoanRow) {
  activeEmployee.value = row;
  activeBank.value = null;
  detailOpen.value = true;
}

function openBank(row: BankLoanRow) {
  activeBank.value = row;
  activeEmployee.value = null;
  detailOpen.value = true;
}

function createLoan(draft: EmployeeLoanDraft) {
  const created: EmployeeLoanRow = {
    id: `employee-loan-local-${Date.now()}`,
    code: `LOAN-LOCAL-${String(rows.value.length + 1).padStart(4, '0')}`,
    title: draft.title.trim(),
    category: draft.category,
    amount: Number(draft.amount),
    balance: 0,
    status: '待审核',
    borrower: draft.borrower.trim(),
    department: draft.department.trim(),
    requestedAt: '2026-09-16',
    project: draft.project.trim() || '未关联项目',
  };
  rows.value = [created, ...rows.value];
  activeView.value = 'employee';
  activeStatus.value = '全部';
  query.value = '';
  createOpen.value = false;
  toast.success(`${created.code} 已提交到本地待审核列表`);
}

function advanceLoan(row: EmployeeLoanRow) {
  const nextStatus: EmployeeLoanStatus = row.status === '待审核' ? '审核中' : '已通过';
  rows.value = rows.value.map((item) => (item.id === row.id ? { ...item, status: nextStatus } : item));
  activeEmployee.value = rows.value.find((item) => item.id === row.id) ?? null;
  toast.success(`${row.code} 已更新为${nextStatus}`);
}

function exportRows() {
  const employeeHeader = [
    '编号',
    '借款名称',
    '类型',
    '金额',
    '未核销余额',
    '状态',
    '借款人',
    '部门',
    '申请日期',
    '项目',
  ];
  const employeeBody = filteredRows.value.map((row) => [
    row.code,
    row.title,
    row.category,
    row.amount,
    row.balance,
    row.status,
    row.borrower,
    row.department,
    row.requestedAt,
    row.project,
  ]);
  const bankHeader = ['编号', '贷款银行', '贷款金额', '剩余本金', '年利率', '期限', '月供', '起息日', '状态'];
  const bankBody = filteredBankRows.value.map((row) => [
    row.code,
    row.bank,
    row.amount,
    row.balance,
    row.rate,
    row.termMonths,
    row.monthlyPayment,
    row.startDate,
    row.status,
  ]);
  const lines = activeView.value === 'employee' ? [employeeHeader, ...employeeBody] : [bankHeader, ...bankBody];
  const csv = lines.map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(',')).join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = activeView.value === 'employee' ? '员工借款.csv' : '银行贷款.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${lines.length - 1} 条${activeView.value === 'employee' ? '员工借款' : '银行贷款'}`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/loans"
    data-page-label="借款贷款管理"
    data-page-module="财务管理"
    data-loans-page
    :data-active-view="activeView"
    :data-active-status="activeStatus"
    :data-employee-count="rows.length"
    :data-bank-count="localBankRows.length"
    :data-result-count="activeView === 'employee' ? filteredRows.length : filteredBankRows.length"
  >
    <LoansHero />
    <LoansViewToolbar
      :view="activeView"
      :employee-count="rows.length"
      :bank-count="localBankRows.length"
      @update:view="updateView"
      @export="exportRows"
      @create="createOpen = true"
    />
    <LoansSummary :metrics="loanMetrics" />
    <LoansWorkspace
      :view="activeView"
      :rows="filteredRows"
      :bank-rows="filteredBankRows"
      :status="activeStatus"
      :status-counts="statusCounts"
      :query="query"
      @update:status="activeStatus = $event"
      @update:query="query = $event"
      @view-employee="openEmployee"
      @view-bank="openBank"
    />
    <LoanCreateDialog
      v-model:open="createOpen"
      @save="createLoan"
    />
    <LoanDetailDialog
      v-model:open="detailOpen"
      :employee="activeEmployee"
      :bank="activeBank"
      @advance="advanceLoan"
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
