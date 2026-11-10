<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, type UiSelectOption } from '@/components/ui';
import type { OvertimeCompensation, OvertimeDraft } from '../../model/overtime.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: OvertimeDraft] }>();
const compensationOptions: UiSelectOption[] = ['调休', '加班费'].map((value) => ({ value, label: value }));
const submitted = ref(false);
const draft = reactive<OvertimeDraft>({
  date: '2026-03-25',
  startTime: '18:00',
  endTime: '21:00',
  reason: '',
  compensation: '调休',
  projectCount: '1',
});

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(draft, {
      date: '2026-03-25',
      startTime: '18:00',
      endTime: '21:00',
      reason: '',
      compensation: '调休' as OvertimeCompensation,
      projectCount: '1',
    });
    submitted.value = false;
  },
);
function submit() {
  submitted.value = true;
  if (!draft.date || !draft.startTime || !draft.endTime || draft.endTime <= draft.startTime || !draft.reason.trim())
    return;
  emit('save', { ...draft, reason: draft.reason.trim() });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建加班"
    description="提交后将进入直属主管审批流程"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="加班日期"
        required
        ><UiInput
          v-model="draft.date"
          type="date"
          :invalid="submitted && !draft.date"
      /></UiFormField>
      <UiFormField
        label="项目数量"
        required
        ><UiInput
          v-model="draft.projectCount"
          type="number"
          min="1"
      /></UiFormField>
      <UiFormField
        label="开始时间"
        required
        ><UiInput
          v-model="draft.startTime"
          type="text"
          placeholder="18:00"
          :invalid="submitted && !draft.startTime"
      /></UiFormField>
      <UiFormField
        label="结束时间"
        required
        :error="submitted && draft.endTime <= draft.startTime ? '结束时间必须晚于开始时间' : undefined"
        ><UiInput
          v-model="draft.endTime"
          type="text"
          placeholder="21:00"
          :invalid="submitted && draft.endTime <= draft.startTime"
      /></UiFormField>
      <UiFormField
        label="补偿方式"
        required
        ><UiSelect
          v-model="draft.compensation"
          :options="compensationOptions"
          label="选择补偿方式"
      /></UiFormField>
      <UiFormField
        label="加班事由"
        required
        wide
        ><UiTextarea
          v-model="draft.reason"
          rows="4"
          placeholder="请输入加班事由"
          :invalid="submitted && !draft.reason.trim()"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        size="sm"
        @click="emit('update:open', false)"
        >取消</UiButton
      ><UiButton
        size="sm"
        variant="primary"
        @click="submit"
        >提交申请</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
