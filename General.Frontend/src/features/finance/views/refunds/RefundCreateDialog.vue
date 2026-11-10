<script setup lang="ts">
import { FilePlus2 } from 'lucide-vue-next';
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { RefundDraft, RefundKind } from '../../model/refunds.types';

const props = defineProps<{
  open: boolean;
  kind: RefundKind;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [value: RefundDraft];
}>();

const currencyOptions: readonly UiSelectOption[] = [
  { value: 'CNY', label: 'CNY · 人民币' },
  { value: 'USD', label: 'USD · 美元' },
  { value: 'EUR', label: 'EUR · 欧元' },
];
const methodOptions: readonly UiSelectOption[] = [
  { value: '银行转账', label: '银行转账' },
  { value: '原路退回', label: '原路退回' },
  { value: '电汇', label: '电汇' },
  { value: '承兑汇票', label: '承兑汇票' },
];

const draft = reactive<RefundDraft>(emptyDraft());

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): RefundDraft {
  return {
    party: '',
    contract: '',
    description: '',
    currency: 'CNY',
    amount: '',
    method: '银行转账',
  };
}

function submit() {
  if (!draft.party.trim() || !draft.contract.trim() || Number(draft.amount) <= 0) {
    toast.warning(`请填写${props.kind === 'customer' ? '客户' : '供应商'}、关联单据和有效退款金额`);
    return;
  }
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="`新建${kind === 'customer' ? '客户' : '供应商'}退款`"
    description="新建记录仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="submit"
    >
      <UiFormField
        :label="kind === 'customer' ? '客户' : '供应商'"
        required
      >
        <UiInput
          v-model="draft.party"
          :placeholder="kind === 'customer' ? '请输入客户名称' : '请输入供应商名称'"
          aria-label="退款往来单位"
        />
      </UiFormField>
      <UiFormField
        :label="kind === 'customer' ? '关联合同' : '关联采购单'"
        required
      >
        <UiInput
          v-model="draft.contract"
          placeholder="请输入关联单据号"
          aria-label="退款关联单据"
        />
      </UiFormField>
      <UiFormField
        label="业务说明"
        wide
      >
        <UiInput
          v-model="draft.description"
          placeholder="请输入退款原因或合同说明"
          aria-label="退款业务说明"
        />
      </UiFormField>
      <UiFormField
        label="币种"
        required
      >
        <UiSelect
          v-model="draft.currency"
          :options="currencyOptions"
          label="退款币种"
        />
      </UiFormField>
      <UiFormField
        label="实退金额"
        required
      >
        <UiInput
          v-model="draft.amount"
          type="number"
          min="0.01"
          step="0.01"
          aria-label="实退金额"
        />
      </UiFormField>
      <UiFormField
        label="退款方式"
        required
        wide
      >
        <UiSelect
          v-model="draft.method"
          :options="methodOptions"
          label="退款方式"
        />
      </UiFormField>
    </form>

    <template #icon>
      <span :class="$style.dialogIcon">
        <FilePlus2
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
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="submit"
        >保存草稿</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: #6554c0;
  background: #eae6ff;
}
</style>
