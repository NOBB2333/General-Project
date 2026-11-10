<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import type { OnboardingDraft } from '../../model/onboarding.types';
const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: OnboardingDraft] }>();
const typeOptions: readonly UiSelectOption[] = [
  { value: '入职', label: '入职' },
  { value: '离职', label: '离职' },
];
const departmentOptions: readonly UiSelectOption[] = [
  '工程部',
  '销售部',
  '财务部',
  '人事行政部',
  '技术研发部',
  '市场部',
].map((value) => ({ value, label: value }));
const draft = reactive<OnboardingDraft>(emptyDraft());
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);
function emptyDraft(): OnboardingDraft {
  return { employeeName: '', type: '入职', department: '工程部', position: '', date: '2026-09-16', owner: '孙丽萍' };
}
function submit() {
  if (!draft.employeeName.trim() || !draft.position.trim() || !draft.owner.trim()) {
    toast.warning('请填写员工、岗位和经办人');
    return;
  }
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新增记录"
    description="记录仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="员工姓名"
        required
        ><UiInput
          v-model="draft.employeeName"
          aria-label="员工姓名"
      /></UiFormField>
      <UiFormField
        label="类型"
        required
        ><UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="记录类型"
      /></UiFormField>
      <UiFormField
        label="部门"
        required
        ><UiSelect
          v-model="draft.department"
          :options="departmentOptions"
          label="员工部门"
      /></UiFormField>
      <UiFormField
        label="岗位"
        required
        ><UiInput
          v-model="draft.position"
          aria-label="员工岗位"
      /></UiFormField>
      <UiFormField
        label="日期"
        required
        ><UiInput
          v-model="draft.date"
          type="date"
          aria-label="办理日期"
      /></UiFormField>
      <UiFormField
        label="经办人"
        required
        ><UiInput
          v-model="draft.owner"
          aria-label="经办人"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        size="sm"
        @click="emit('update:open', false)"
        >取消</UiButton
      ><UiButton
        variant="primary"
        size="sm"
        @click="submit"
        >保存记录</UiButton
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
