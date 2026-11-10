<script setup lang="ts">
import { ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption, toast } from '@/components/ui';
import type { InquiryDraft, InquiryRow } from '../../model/inquiries.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: InquiryDraft] }>();
const draft = ref<InquiryDraft>(blankDraft());
const sourceOptions: UiSelectOption[] = ['手动创建', '采购申请', 'PTO合同', '销售合同'].map((value) => ({
  value,
  label: value,
}));
const buyerOptions: UiSelectOption[] = ['', '王建华', '陈伟杰', '林国栋', '周天宇'].map((value) => ({
  value,
  label: value || '暂不分配',
}));

watch(
  () => props.open,
  (open) => {
    if (open) draft.value = blankDraft();
  },
);
function blankDraft(): InquiryDraft {
  return {
    title: '',
    projectName: '',
    sourceType: '手动创建',
    sourceId: '',
    supplierCount: '1',
    materialCount: '1',
    totalBudget: '',
    deadline: '',
    assignedBuyer: '',
  };
}
function save() {
  if (!draft.value.title.trim() || !draft.value.deadline) {
    toast.warning('请填写询价标题和截止日期');
    return;
  }
  emit('save', { ...draft.value });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建询价单"
    description="询价单将以待发布草稿保存在当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="save"
    >
      <UiFormField
        label="询价标题"
        required
        wide
        ><UiInput
          v-model="draft.title"
          placeholder="请输入询价标题"
      /></UiFormField>
      <UiFormField label="关联项目"
        ><UiInput
          v-model="draft.projectName"
          placeholder="留空表示无关联项目"
      /></UiFormField>
      <UiFormField label="负责采购"
        ><UiSelect
          v-model="draft.assignedBuyer"
          :options="buyerOptions"
          label="负责采购"
      /></UiFormField>
      <UiFormField label="来源类型"
        ><UiSelect
          :model-value="draft.sourceType"
          :options="sourceOptions"
          label="来源类型"
          @update:model-value="draft.sourceType = $event as InquiryRow['sourceType']"
      /></UiFormField>
      <UiFormField label="来源单号"
        ><UiInput
          v-model="draft.sourceId"
          placeholder="可选"
      /></UiFormField>
      <UiFormField label="供应商数"
        ><UiInput
          v-model="draft.supplierCount"
          type="number"
          min="1"
      /></UiFormField>
      <UiFormField label="物料数"
        ><UiInput
          v-model="draft.materialCount"
          type="number"
          min="1"
      /></UiFormField>
      <UiFormField label="预算金额"
        ><UiInput
          v-model="draft.totalBudget"
          type="number"
          min="0"
          placeholder="0"
      /></UiFormField>
      <UiFormField
        label="截止日期"
        required
        ><UiInput
          v-model="draft.deadline"
          type="date"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton @click="emit('update:open', false)">取消</UiButton
      ><UiButton
        variant="primary"
        @click="save"
        >保存询价草稿</UiButton
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
