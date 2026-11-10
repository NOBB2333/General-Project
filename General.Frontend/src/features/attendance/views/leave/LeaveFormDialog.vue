<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, type UiSelectOption } from '@/components/ui';
import type { LeaveDraft, LeaveType } from '../../model/leave.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: LeaveDraft] }>();
const typeOptions: UiSelectOption[] = ['年假', '病假', '事假', '婚假', '产假', '陪产假', '丧假', '调休', '哺乳假'].map(
  (value) => ({ value, label: value }),
);
const draft = reactive<LeaveDraft>({ type: '年假', startDate: '2026-03-25', endDate: '2026-03-25', reason: '' });
const submitted = ref(false);

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(draft, { type: '年假', startDate: '2026-03-25', endDate: '2026-03-25', reason: '' });
    submitted.value = false;
  },
);

function submit() {
  submitted.value = true;
  if (!draft.startDate || !draft.endDate || !draft.reason.trim() || draft.endDate < draft.startDate) return;
  emit('save', { ...draft, reason: draft.reason.trim() });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建请假"
    description="提交后将进入直属主管审批流程"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="假别"
        required
        ><UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="选择假别"
      /></UiFormField>
      <UiFormField
        label="开始日期"
        required
        ><UiInput
          v-model="draft.startDate"
          type="date"
          :invalid="submitted && !draft.startDate"
      /></UiFormField>
      <UiFormField
        label="结束日期"
        required
        :error="submitted && draft.endDate < draft.startDate ? '结束日期不能早于开始日期' : undefined"
        ><UiInput
          v-model="draft.endDate"
          type="date"
          :invalid="submitted && (!draft.endDate || draft.endDate < draft.startDate)"
      /></UiFormField>
      <UiFormField
        label="请假事由"
        required
        wide
        :error="submitted && !draft.reason.trim() ? '请输入请假事由' : undefined"
        ><UiTextarea
          v-model="draft.reason"
          rows="4"
          placeholder="请输入请假事由"
          :invalid="submitted && !draft.reason.trim()"
      /></UiFormField>
    </form>
    <template #footer>
      <UiButton
        size="sm"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        size="sm"
        variant="primary"
        @click="submit"
        >提交申请</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
