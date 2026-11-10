<script setup lang="ts">
import { FilePlus2 } from 'lucide-vue-next';
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import type { RevenueRecognitionDraft } from '../../model/revenue-recognition.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [value: RevenueRecognitionDraft];
}>();

const methodOptions: readonly UiSelectOption[] = ['按发货', '按里程碑', '按验收', '按周期', '手动', '混合', '服务'].map(
  (value) => ({ value, label: value }),
);
const draft = reactive<RevenueRecognitionDraft>(emptyDraft());

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): RevenueRecognitionDraft {
  return {
    contract: '',
    customer: '',
    project: '',
    method: '按发货',
    amount: '',
    confirmedAt: '2026-09-16',
    period: '2026-09',
  };
}

function submit() {
  if (!draft.contract.trim() || !draft.customer.trim() || Number(draft.amount) <= 0) {
    toast.warning('请填写来源合同、客户和有效确认金额');
    return;
  }
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建确认单"
    description="新建记录仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="submit"
    >
      <UiFormField
        label="来源合同"
        required
        ><UiInput
          v-model="draft.contract"
          placeholder="例如 HT-2026-0032"
          aria-label="收入确认来源合同"
      /></UiFormField>
      <UiFormField
        label="客户名称"
        required
        ><UiInput
          v-model="draft.customer"
          placeholder="请输入客户名称"
          aria-label="收入确认客户"
      /></UiFormField>
      <UiFormField
        label="关联项目"
        wide
        ><UiInput
          v-model="draft.project"
          placeholder="请输入项目名称"
          aria-label="收入确认项目"
      /></UiFormField>
      <UiFormField
        label="确认方式"
        required
        ><UiSelect
          v-model="draft.method"
          :options="methodOptions"
          label="收入确认方式"
      /></UiFormField>
      <UiFormField
        label="本次确认金额"
        required
        ><UiInput
          v-model="draft.amount"
          type="number"
          min="1"
          step="1"
          aria-label="收入确认金额"
      /></UiFormField>
      <UiFormField
        label="确认日期"
        required
        ><UiInput
          v-model="draft.confirmedAt"
          type="date"
          aria-label="收入确认日期"
      /></UiFormField>
      <UiFormField
        label="财务期间"
        required
        ><UiInput
          v-model="draft.period"
          placeholder="YYYY-MM"
          aria-label="收入确认财务期间"
      /></UiFormField>
    </form>
    <template #icon
      ><span :class="$style.dialogIcon"
        ><FilePlus2
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
        >保存草稿</UiButton
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
  color: var(--ui-success);
  background: #e3fcef;
}
</style>
