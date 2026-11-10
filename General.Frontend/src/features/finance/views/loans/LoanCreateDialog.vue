<script setup lang="ts">
import { HandCoins } from 'lucide-vue-next';
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import type { EmployeeLoanDraft } from '../../model/loans.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [value: EmployeeLoanDraft];
}>();

const categoryOptions: readonly UiSelectOption[] = [
  { value: '差旅预支', label: '差旅预支' },
  { value: '项目备用金', label: '项目备用金' },
  { value: '采购预支', label: '采购预支' },
  { value: '其他借款', label: '其他借款' },
];
const draft = reactive<EmployeeLoanDraft>(emptyDraft());

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): EmployeeLoanDraft {
  return { title: '', category: '差旅预支', borrower: '', department: '', amount: '', project: '' };
}

function submit() {
  if (!draft.title.trim() || !draft.borrower.trim() || !draft.department.trim() || Number(draft.amount) <= 0) {
    toast.warning('请填写借款名称、借款人、部门和有效金额');
    return;
  }
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="申请员工借款"
    description="申请仅保存到当前本地会话，提交后进入待审核状态"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="submit"
    >
      <UiFormField
        label="借款名称"
        required
        wide
      >
        <UiInput
          v-model="draft.title"
          placeholder="请输入借款用途"
          aria-label="借款名称"
        />
      </UiFormField>
      <UiFormField
        label="借款类型"
        required
      >
        <UiSelect
          v-model="draft.category"
          :options="categoryOptions"
          label="借款类型"
        />
      </UiFormField>
      <UiFormField
        label="借款金额"
        required
      >
        <UiInput
          v-model="draft.amount"
          type="number"
          min="1"
          step="1"
          aria-label="借款金额"
        />
      </UiFormField>
      <UiFormField
        label="借款人"
        required
      >
        <UiInput
          v-model="draft.borrower"
          placeholder="姓名"
          aria-label="借款人"
        />
      </UiFormField>
      <UiFormField
        label="所属部门"
        required
      >
        <UiInput
          v-model="draft.department"
          placeholder="部门"
          aria-label="所属部门"
        />
      </UiFormField>
      <UiFormField
        label="关联项目"
        wide
      >
        <UiInput
          v-model="draft.project"
          placeholder="可选，输入项目名称"
          aria-label="关联项目"
        />
      </UiFormField>
    </form>
    <template #icon>
      <span :class="$style.dialogIcon"
        ><HandCoins
          :size="18"
          :stroke-width="1.8"
      /></span>
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
        >提交申请</UiButton
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
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
