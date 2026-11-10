<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, type UiSelectOption } from '@/components/ui';
import type { AttendanceCorrectionDraft } from '../../model/attendance-mgmt.types';

const props = defineProps<{ open: boolean; person?: string; date?: string }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: AttendanceCorrectionDraft] }>();
const people: UiSelectOption[] = ['陈大伟', '冯晓雨', '李小明', '刘芳芳', '钱思远', '孙丽丽', '王小红'].map(
  (value) => ({ value, label: value }),
);
const kindOptions: UiSelectOption[] = ['签到', '签退'].map((value) => ({ value, label: value }));
const submitted = ref(false);
const draft = reactive<AttendanceCorrectionDraft>({
  person: '李小明',
  date: '2026-03-10',
  time: '09:00',
  kind: '签到',
  reason: '',
});

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(draft, {
      person: props.person || '李小明',
      date: props.date || '2026-03-10',
      time: '09:00',
      kind: '签到',
      reason: '',
    });
    submitted.value = false;
  },
);
function submit() {
  submitted.value = true;
  if (!draft.person || !draft.date || !draft.time || !draft.reason.trim()) return;
  emit('save', { ...draft, reason: draft.reason.trim() });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建补签"
    description="补签申请提交后由考勤管理员审批"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="人员"
        required
        ><UiSelect
          v-model="draft.person"
          :options="people"
          label="选择人员"
      /></UiFormField>
      <UiFormField
        label="补签类型"
        required
        ><UiSelect
          v-model="draft.kind"
          :options="kindOptions"
          label="选择类型"
      /></UiFormField>
      <UiFormField
        label="日期"
        required
        ><UiInput
          v-model="draft.date"
          type="date"
      /></UiFormField>
      <UiFormField
        label="时间"
        required
        ><UiInput
          v-model="draft.time"
          type="text"
          placeholder="09:00"
      /></UiFormField>
      <UiFormField
        label="补签原因"
        required
        wide
        ><UiTextarea
          v-model="draft.reason"
          rows="4"
          placeholder="请输入补签原因"
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
        >提交补签</UiButton
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
