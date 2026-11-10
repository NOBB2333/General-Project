<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import type { RecruitingPositionDraft } from '../../model/recruiting.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: RecruitingPositionDraft] }>();
const departmentOptions: readonly UiSelectOption[] = [
  '工程部',
  '销售部',
  '财务部',
  '人事行政部',
  '技术研发部',
  '市场部',
].map((value) => ({ value, label: value }));
const draft = reactive<RecruitingPositionDraft>(emptyDraft());

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): RecruitingPositionDraft {
  return {
    name: '',
    department: '工程部',
    level: 'P4',
    approved: '1',
    staffed: '0',
    salaryRange: '',
    reportsTo: '',
  };
}

function submit() {
  const approved = Number(draft.approved);
  const staffed = Number(draft.staffed);
  if (!draft.name.trim() || !draft.salaryRange.trim() || !draft.reportsTo.trim()) {
    toast.warning('请填写岗位名称、薪资范围和汇报对象');
    return;
  }
  if (approved < 1 || staffed < 0 || staffed > approved) {
    toast.warning('在岗人数必须小于或等于岗位编制');
    return;
  }
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新增岗位"
    description="岗位仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="岗位名称"
        required
        wide
        ><UiInput
          v-model="draft.name"
          aria-label="岗位名称" /></UiFormField
      ><UiFormField
        label="部门"
        required
        ><UiSelect
          v-model="draft.department"
          :options="departmentOptions"
          label="岗位部门" /></UiFormField
      ><UiFormField
        label="职级"
        required
        ><UiInput
          v-model="draft.level"
          aria-label="岗位职级" /></UiFormField
      ><UiFormField
        label="岗位编制"
        required
        ><UiInput
          v-model="draft.approved"
          type="number"
          min="1"
          aria-label="岗位编制" /></UiFormField
      ><UiFormField
        label="当前在岗"
        required
        ><UiInput
          v-model="draft.staffed"
          type="number"
          min="0"
          aria-label="当前在岗" /></UiFormField
      ><UiFormField
        label="薪资范围"
        required
        ><UiInput
          v-model="draft.salaryRange"
          placeholder="如 12K-18K"
          aria-label="薪资范围" /></UiFormField
      ><UiFormField
        label="汇报对象"
        required
        ><UiInput
          v-model="draft.reportsTo"
          aria-label="汇报对象"
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
        >保存岗位</UiButton
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
