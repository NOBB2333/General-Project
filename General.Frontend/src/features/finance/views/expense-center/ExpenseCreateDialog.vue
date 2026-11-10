<script setup lang="ts">
import { Building2, ReceiptText } from 'lucide-vue-next';
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import type { ExpenseDraft } from '../../model/expense-center.types';
const props = defineProps<{ open: boolean; kind: 'reimbursement' | 'enterprise' }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [value: ExpenseDraft] }>();
const ownershipOptions: readonly UiSelectOption[] = [
  { value: '项目成本', label: '项目成本' },
  { value: '期间费用', label: '期间费用' },
];
const draft = reactive<ExpenseDraft>(emptyDraft());
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);
function emptyDraft(): ExpenseDraft {
  return {
    name: '',
    ownership: props.kind === 'enterprise' ? '期间费用' : '项目成本',
    applicant: '',
    department: '',
    project: '',
    amount: '',
    date: '2026-09-16',
  };
}
function submit() {
  if (!draft.name.trim() || !draft.applicant.trim() || Number(draft.amount) <= 0) {
    toast.warning('请填写费用名称、申请人和有效金额');
    return;
  }
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="kind === 'reimbursement' ? '费用报销' : '企业支出登记'"
    description="记录仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="submit"
    >
      <UiFormField
        label="费用名称"
        required
        wide
        ><UiInput
          v-model="draft.name"
          placeholder="请输入费用用途"
          aria-label="费用名称"
      /></UiFormField>
      <UiFormField
        label="费用归属"
        required
        ><UiSelect
          v-model="draft.ownership"
          :options="ownershipOptions"
          label="费用归属"
      /></UiFormField>
      <UiFormField
        label="金额"
        required
        ><UiInput
          v-model="draft.amount"
          type="number"
          min="1"
          step="1"
          aria-label="费用金额"
      /></UiFormField>
      <UiFormField
        :label="kind === 'reimbursement' ? '申请人' : '经办人'"
        required
        ><UiInput
          v-model="draft.applicant"
          aria-label="费用申请人"
      /></UiFormField>
      <UiFormField
        label="部门"
        required
        ><UiInput
          v-model="draft.department"
          aria-label="费用部门"
      /></UiFormField>
      <UiFormField label="关联项目"
        ><UiInput
          v-model="draft.project"
          placeholder="可选"
          aria-label="费用项目"
      /></UiFormField>
      <UiFormField
        label="费用日期"
        required
        ><UiInput
          v-model="draft.date"
          type="date"
          aria-label="费用日期"
      /></UiFormField>
    </form>
    <template #icon
      ><span :class="$style.dialogIcon"
        ><ReceiptText
          v-if="kind === 'reimbursement'"
          :size="18"
          :stroke-width="1.8" /><Building2
          v-else
          :size="18"
          :stroke-width="1.8" /></span
    ></template>
    <template #footer
      ><UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:open', false)"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="submit"
        >保存并提交</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}
.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: #c2185b;
  background: #ffebf2;
}
</style>
