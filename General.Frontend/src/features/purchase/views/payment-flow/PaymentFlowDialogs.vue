<script setup lang="ts">
import { Layers3, WalletCards } from 'lucide-vue-next';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { PaymentAllocationDraft, PaymentFlowDraft, PaymentFlowRow } from '../../model/payment-flow.types';

defineProps<{
  paymentOpen: boolean;
  allocationOpen: boolean;
  activeRow: PaymentFlowRow | null;
  paymentDraft: PaymentFlowDraft;
  allocationDraft: PaymentAllocationDraft;
  supplierOptions: readonly UiSelectOption[];
}>();

const emit = defineEmits<{
  'update:paymentOpen': [value: boolean];
  'update:allocationOpen': [value: boolean];
  'update:paymentDraft': [value: PaymentFlowDraft];
  'update:allocationDraft': [value: PaymentAllocationDraft];
  savePayment: [];
  saveAllocation: [];
}>();

const methodOptions: readonly UiSelectOption[] = [
  { value: '银行转账', label: '银行转账' },
  { value: '电汇', label: '电汇' },
  { value: '承兑汇票', label: '承兑汇票' },
];
const accountOptions: readonly UiSelectOption[] = [
  { value: '招商银行 (…0810 001)', label: '招商银行 (…0810 001)' },
  { value: '建设银行 (…9900)', label: '建设银行 (…9900)' },
  { value: '中国银行 (…5678)', label: '中国银行 (…5678)' },
];

function currency(value: number) {
  return `¥${value.toLocaleString('en-US', { minimumFractionDigits: 2 })}`;
}
</script>

<template>
  <UiDialog
    :open="paymentOpen"
    title="申请付款"
    description="登记一笔新的采购付款流水，仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:paymentOpen', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon">
        <WalletCards
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <form
      :class="$style.form"
      @submit.prevent="emit('savePayment')"
    >
      <UiFormField
        label="供应商"
        required
        wide
      >
        <UiSelect
          :model-value="paymentDraft.supplier"
          :options="supplierOptions.filter((option) => option.value !== '全部供应商')"
          label="付款供应商"
          placeholder="请选择供应商"
          @update:model-value="emit('update:paymentDraft', { ...paymentDraft, supplier: $event })"
        />
      </UiFormField>
      <UiFormField
        label="付款日期"
        required
      >
        <UiInput
          :model-value="paymentDraft.date"
          type="date"
          @update:model-value="emit('update:paymentDraft', { ...paymentDraft, date: $event })"
        />
      </UiFormField>
      <UiFormField
        label="付款金额"
        required
      >
        <UiInput
          :model-value="paymentDraft.amount"
          type="number"
          placeholder="0.00"
          @update:model-value="emit('update:paymentDraft', { ...paymentDraft, amount: $event })"
        />
      </UiFormField>
      <UiFormField label="付款方式">
        <UiSelect
          :model-value="paymentDraft.method"
          :options="methodOptions"
          label="付款方式"
          @update:model-value="emit('update:paymentDraft', { ...paymentDraft, method: $event })"
        />
      </UiFormField>
      <UiFormField label="付款账户">
        <UiSelect
          :model-value="paymentDraft.account"
          :options="accountOptions"
          label="付款账户"
          @update:model-value="emit('update:paymentDraft', { ...paymentDraft, account: $event })"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:paymentOpen', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('savePayment')"
        >确认申请</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    :open="allocationOpen"
    :title="activeRow?.status === 'allocated' ? '重新分配付款' : '手动分配付款'"
    :description="activeRow ? `${activeRow.code} · ${activeRow.supplier}` : undefined"
    size="lg"
    @update:open="emit('update:allocationOpen', $event)"
  >
    <template #icon>
      <span :class="[$style.dialogIcon, $style.allocationIcon]">
        <Layers3
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <div
      v-if="activeRow"
      :class="$style.allocationSummary"
    >
      <span
        ><small>付款金额</small><strong>{{ currency(activeRow.amount) }}</strong></span
      >
      <span
        ><small>已分配</small><strong>{{ currency(activeRow.allocatedAmount) }}</strong></span
      >
      <span
        ><small>待分配</small><strong>{{ currency(activeRow.amount - activeRow.allocatedAmount) }}</strong></span
      >
    </div>
    <form
      :class="$style.form"
      @submit.prevent="emit('saveAllocation')"
    >
      <UiFormField
        label="采购订单号"
        required
      >
        <UiInput
          :model-value="allocationDraft.orderCode"
          placeholder="例如 PO-2026-0219"
          @update:model-value="emit('update:allocationDraft', { ...allocationDraft, orderCode: $event })"
        />
      </UiFormField>
      <UiFormField
        label="本次分配金额"
        required
      >
        <UiInput
          :model-value="allocationDraft.amount"
          type="number"
          placeholder="0.00"
          @update:model-value="emit('update:allocationDraft', { ...allocationDraft, amount: $event })"
        />
      </UiFormField>
      <UiFormField
        label="分配备注"
        wide
      >
        <UiTextarea
          :model-value="allocationDraft.note"
          :rows="3"
          placeholder="可选，记录分配依据"
          @update:model-value="emit('update:allocationDraft', { ...allocationDraft, note: $event })"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:allocationOpen', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('saveAllocation')"
        >确认分配</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.allocationIcon {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}

.allocationSummary {
  @apply mx-5 mt-5 grid grid-cols-3 gap-3 rounded border p-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.allocationSummary span {
  @apply flex flex-col gap-1;
}

.allocationSummary small {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.allocationSummary strong {
  @apply text-[13px] font-semibold;
  color: var(--ui-text);
}
</style>
