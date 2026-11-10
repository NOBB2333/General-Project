<script setup lang="ts">
import { BarChart3, Building2, CircleHelp, Download, FileCheck2, Plus, ReceiptText, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type {
  ExpenseOwnershipFilter,
  ExpenseRow,
  ExpenseStatus,
  ExpenseStatusFilter,
  ExpenseView,
} from '../../model/expense-center.types';

defineProps<{
  view: ExpenseView;
  rows: readonly ExpenseRow[];
  ownership: ExpenseOwnershipFilter;
  status: ExpenseStatusFilter;
  query: string;
  reimbursementCount: number;
  enterpriseCount: number;
  paymentCount: number;
  statusCounts: Readonly<Record<ExpenseStatus, number>>;
  ownershipCounts: Readonly<Record<ExpenseOwnershipFilter, number>>;
}>();
const emit = defineEmits<{
  'update:view': [value: ExpenseView];
  'update:ownership': [value: ExpenseOwnershipFilter];
  'update:status': [value: ExpenseStatusFilter];
  'update:query': [value: string];
  export: [];
  createReimbursement: [];
  createEnterprise: [];
  viewRow: [row: ExpenseRow];
}>();
const statuses: readonly ExpenseStatusFilter[] = ['全部', '待审核', '审核中', '已通过', '已驳回', '已打款'];
function statusLabel(item: ExpenseStatusFilter, counts: Readonly<Record<ExpenseStatus, number>>) {
  return item === '全部' ? item : `${item} (${counts[item]})`;
}
function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <section :class="$style.workspace">
    <header :class="$style.businessToolbar">
      <UiTabs
        :model-value="view"
        @update:model-value="emit('update:view', $event as ExpenseView)"
      >
        <UiTabsList
          label="费用业务视图"
          :class="$style.businessTabs"
        >
          <UiTabsTrigger
            value="reimbursements"
            :class="$style.businessTab"
            ><ReceiptText
              :size="14"
              :stroke-width="1.8"
            />报销管理 <span>{{ reimbursementCount }}</span></UiTabsTrigger
          >
          <UiTabsTrigger
            value="enterprise"
            :class="$style.businessTab"
            ><Building2
              :size="14"
              :stroke-width="1.8"
            />企业支出 <span>{{ enterpriseCount }}</span></UiTabsTrigger
          >
          <UiTabsTrigger
            value="payments"
            :class="$style.businessTab"
            ><FileCheck2
              :size="14"
              :stroke-width="1.8"
            />付款申请 <span>{{ paymentCount }}</span></UiTabsTrigger
          >
          <UiTabsTrigger
            value="statistics"
            :class="$style.businessTab"
            ><BarChart3
              :size="14"
              :stroke-width="1.8"
            />费用统计</UiTabsTrigger
          >
          <UiTabsTrigger
            value="guide"
            :class="$style.businessTab"
            ><CircleHelp
              :size="14"
              :stroke-width="1.8"
            />支出说明</UiTabsTrigger
          >
        </UiTabsList>
      </UiTabs>
      <div :class="$style.actions">
        <UiButton
          variant="secondary"
          size="sm"
          @click="emit('export')"
          ><template #leading
            ><Download
              :size="14"
              :stroke-width="1.8" /></template
          >导出</UiButton
        ><UiButton
          variant="primary"
          size="sm"
          @click="emit('createReimbursement')"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >费用报销</UiButton
        ><UiButton
          variant="subtle"
          size="sm"
          @click="emit('createEnterprise')"
          ><template #leading
            ><Building2
              :size="14"
              :stroke-width="1.8" /></template
          >企业支出登记</UiButton
        >
      </div>
    </header>

    <template v-if="view === 'statistics'">
      <div :class="$style.analytics">
        <article><strong>项目成本</strong><b>¥7.29万</b><span>占总费用 28%</span></article>
        <article><strong>期间费用</strong><b>¥18.50万</b><span>占总费用 72%</span></article>
        <article><strong>审批通过率</strong><b>80%</b><span>16 / 20 笔</span></article>
      </div>
    </template>
    <template v-else-if="view === 'guide'">
      <div :class="$style.guide">
        <strong>支出归属说明</strong>
        <p>
          项目成本直接归集到合同或项目；期间费用用于日常经营，不计入项目成本。企业支出登记完成后，可在付款申请视图跟踪付款流程。
        </p>
      </div>
    </template>
    <template v-else>
      <div :class="$style.ownershipToolbar">
        <span>费用归属</span
        ><button
          v-for="item in ['全部', '项目成本', '期间费用'] as const"
          :key="item"
          type="button"
          :class="[$style.ownershipButton, ownership === item && $style.ownershipActive]"
          @click="emit('update:ownership', item)"
        >
          {{ item }}<small v-if="item !== '全部'">({{ ownershipCounts[item] }})</small>
        </button>
      </div>
      <div :class="$style.statusToolbar">
        <UiTabs
          :model-value="status"
          @update:model-value="emit('update:status', $event as ExpenseStatusFilter)"
          ><UiTabsList
            label="费用状态"
            :class="$style.statusTabs"
            ><UiTabsTrigger
              v-for="item in statuses"
              :key="item"
              :value="item"
              :class="$style.statusTab"
              >{{ statusLabel(item, statusCounts) }}</UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        >
        <label :class="$style.searchField"
          ><Search
            :size="14"
            :stroke-width="1.8"
            aria-hidden="true" /><UiInput
            :model-value="query"
            placeholder="搜索..."
            aria-label="搜索费用"
            @update:model-value="emit('update:query', $event)"
        /></label>
      </div>
      <table :class="$style.table">
        <thead>
          <tr>
            <th>单号</th>
            <th>费用名称</th>
            <th>归属</th>
            <th>申请人</th>
            <th>日期</th>
            <th>项目</th>
            <th>金额</th>
            <th>状态</th>
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
                @click="emit('viewRow', row)"
              >
                {{ row.code }}
              </button>
            </td>
            <td>
              <strong>{{ row.name }}</strong
              ><small
                v-if="row.ownership === '期间费用'"
                :class="$style.periodBadge"
                >期间</small
              >
            </td>
            <td>{{ row.ownership }}</td>
            <td>
              {{ row.applicant }} <small>{{ row.department }}</small>
            </td>
            <td>{{ row.date }}</td>
            <td>{{ row.project }}</td>
            <td>
              <strong>{{ money(row.amount) }}</strong>
            </td>
            <td>
              <span
                :class="$style.status"
                :data-status="row.status"
                >{{ row.status }}</span
              >
            </td>
          </tr>
          <tr v-if="!rows.length">
            <td
              :class="$style.empty"
              colspan="8"
            >
              当前筛选下没有费用记录
            </td>
          </tr>
        </tbody>
      </table>
    </template>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.businessToolbar {
  @apply flex min-h-[50px] items-center border-b px-4;
  border-color: var(--ui-border);
}
.businessTabs {
  @apply gap-1;
}
.businessTab {
  @apply min-h-[48px] rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-subtle);
}
.businessTab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}
.businessTab span {
  @apply ml-1 inline-flex h-[18px] min-w-[18px] items-center justify-center rounded-full px-1 text-[9px];
  background: var(--ui-surface-hover);
}
.actions {
  @apply ml-auto flex items-center gap-2;
}
.ownershipToolbar {
  @apply flex min-h-[48px] items-center gap-1 border-b px-4 text-[11px];
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.ownershipButton {
  @apply ml-1 rounded px-3 py-2 font-semibold;
}
.ownershipActive {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.ownershipButton small {
  @apply ml-1 font-normal;
}
.statusToolbar {
  @apply flex min-h-[50px] items-center border-b px-4;
  border-color: var(--ui-border);
}
.statusTabs {
  @apply gap-1;
}
.statusTab {
  @apply min-h-[48px] rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-subtle);
}
.statusTab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}
.searchField {
  @apply ml-auto flex h-8 w-[185px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: transparent;
  background: var(--ui-surface-hover);
}
.searchField input {
  @apply h-full border-0 bg-transparent p-0 shadow-none;
}
.table {
  @apply w-full border-collapse text-left text-[10px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-9 border-b px-4 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.table td {
  @apply h-[49px] border-b px-4;
  border-color: var(--ui-border-subtle);
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.table td small {
  color: var(--ui-text-subtle);
}
.code {
  @apply font-semibold;
  color: var(--app-primary);
}
.periodBadge {
  @apply ml-2 rounded px-1.5 py-0.5 text-[8px];
  color: var(--ui-warning) !important;
  background: var(--ui-warning-soft);
}
.status {
  @apply inline-flex rounded px-2 py-0.5 text-[9px] font-semibold;
}
.status[data-status='待审核'],
.status[data-status='审核中'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='已通过'] {
  color: var(--ui-success);
  background: #e3fcef;
}
.status[data-status='已打款'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.status[data-status='已驳回'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.empty {
  @apply h-28 text-center;
  color: var(--ui-text-subtle);
}
.analytics {
  @apply grid grid-cols-3 gap-4 p-5;
}
.analytics article {
  @apply flex min-h-28 flex-col rounded border p-4;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.analytics b {
  @apply mt-3 text-[24px];
  color: var(--app-primary);
}
.analytics span {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.guide {
  @apply p-6;
}
.guide strong {
  @apply text-[15px];
}
.guide p {
  @apply mt-3 max-w-3xl text-[12px] leading-6;
  color: var(--ui-text-muted);
}
</style>
