<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import type { EmployeeDraft } from '../../model/employee-files.types';
const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: EmployeeDraft] }>();
const genderOptions: readonly UiSelectOption[] = [
  { value: '女', label: '女' },
  { value: '男', label: '男' },
];
const departmentOptions: readonly UiSelectOption[] = [
  '工程部',
  '销售部',
  '财务部',
  '人事行政部',
  '技术研发部',
  '市场部',
  '生产部',
].map((value) => ({ value, label: value }));
const draft = reactive<EmployeeDraft>(emptyDraft());
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);
function emptyDraft(): EmployeeDraft {
  return {
    name: '',
    gender: '女',
    age: '25',
    department: '工程部',
    position: '',
    level: 'P3',
    salary: '',
    joinedAt: '2026-09-16',
    phone: '',
  };
}
function submit() {
  if (!draft.name.trim() || !draft.position.trim() || Number(draft.salary) <= 0) {
    toast.warning('请填写姓名、岗位和有效月薪');
    return;
  }
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新增员工"
    description="新员工档案仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="姓名"
        required
        ><UiInput
          v-model="draft.name"
          aria-label="员工姓名" /></UiFormField
      ><UiFormField
        label="性别"
        required
        ><UiSelect
          v-model="draft.gender"
          :options="genderOptions"
          label="员工性别" /></UiFormField
      ><UiFormField
        label="年龄"
        required
        ><UiInput
          v-model="draft.age"
          type="number"
          min="18"
          aria-label="员工年龄" /></UiFormField
      ><UiFormField
        label="部门"
        required
        ><UiSelect
          v-model="draft.department"
          :options="departmentOptions"
          label="员工部门" /></UiFormField
      ><UiFormField
        label="岗位"
        required
        ><UiInput
          v-model="draft.position"
          aria-label="员工岗位" /></UiFormField
      ><UiFormField
        label="职级"
        required
        ><UiInput
          v-model="draft.level"
          aria-label="员工职级" /></UiFormField
      ><UiFormField
        label="月薪"
        required
        ><UiInput
          v-model="draft.salary"
          type="number"
          min="1"
          aria-label="员工月薪" /></UiFormField
      ><UiFormField
        label="入职日期"
        required
        ><UiInput
          v-model="draft.joinedAt"
          type="date"
          aria-label="员工入职日期" /></UiFormField
      ><UiFormField
        label="联系方式"
        wide
        ><UiInput
          v-model="draft.phone"
          aria-label="员工联系方式"
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
        >保存员工</UiButton
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
