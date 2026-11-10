<script setup lang="ts">
import { Building2, HandCoins } from 'lucide-vue-next';
import { UiButton, UiDialog } from '@/components/ui';
import type { BankLoanRow, EmployeeLoanRow } from '../../model/loans.types';

defineProps<{
  open: boolean;
  employee: EmployeeLoanRow | null;
  bank: BankLoanRow | null;
}>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  advance: [row: EmployeeLoanRow];
}>();

function money(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="employee ? `${employee.code} · 员工借款` : bank ? `${bank.code} · 银行贷款` : '借款详情'"
    :description="
      employee ? `${employee.borrower} · ${employee.status}` : bank ? `${bank.bank} · ${bank.status}` : undefined
    "
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <dl
      v-if="employee"
      :class="$style.details"
    >
      <div :class="$style.wide">
        <dt>借款用途</dt>
        <dd>{{ employee.title }}</dd>
      </div>
      <div>
        <dt>借款类型</dt>
        <dd>{{ employee.category }}</dd>
      </div>
      <div>
        <dt>借款金额</dt>
        <dd>{{ money(employee.amount) }}</dd>
      </div>
      <div>
        <dt>借款人 / 部门</dt>
        <dd>{{ employee.borrower }} · {{ employee.department }}</dd>
      </div>
      <div>
        <dt>申请日期</dt>
        <dd>{{ employee.requestedAt }}</dd>
      </div>
      <div>
        <dt>关联项目</dt>
        <dd>{{ employee.project }}</dd>
      </div>
      <div>
        <dt>未核销余额</dt>
        <dd :class="employee.balance ? $style.danger : undefined">{{ money(employee.balance) }}</dd>
      </div>
    </dl>
    <dl
      v-else-if="bank"
      :class="$style.details"
    >
      <div>
        <dt>贷款银行</dt>
        <dd>{{ bank.bank }}</dd>
      </div>
      <div>
        <dt>贷款状态</dt>
        <dd>{{ bank.status }}</dd>
      </div>
      <div>
        <dt>贷款金额</dt>
        <dd>{{ money(bank.amount) }}</dd>
      </div>
      <div>
        <dt>剩余本金</dt>
        <dd>{{ money(bank.balance) }}</dd>
      </div>
      <div>
        <dt>年利率 / 期限</dt>
        <dd>{{ bank.rate }} · {{ bank.termMonths }} 个月</dd>
      </div>
      <div>
        <dt>月供 / 起息日</dt>
        <dd>{{ money(bank.monthlyPayment) }} · {{ bank.startDate }}</dd>
      </div>
    </dl>
    <template #icon>
      <span :class="$style.dialogIcon">
        <HandCoins
          v-if="employee"
          :size="18"
          :stroke-width="1.8"
        />
        <Building2
          v-else
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:open', false)"
        >关闭</UiButton
      >
      <UiButton
        v-if="employee?.status === '待审核' || employee?.status === '审核中'"
        variant="primary"
        size="sm"
        @click="emit('advance', employee)"
      >
        {{ employee.status === '待审核' ? '开始审核' : '审核通过' }}
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.details {
  @apply m-0 grid grid-cols-2 gap-4 p-5;
}
.details > div {
  @apply rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.details dt,
.details dd {
  @apply m-0;
}
.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}
.wide {
  @apply col-span-2;
}
.danger {
  color: var(--ui-danger) !important;
}
.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
