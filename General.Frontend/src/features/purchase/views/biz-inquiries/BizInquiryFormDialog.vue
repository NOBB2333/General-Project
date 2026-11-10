<script setup lang="ts">
import { ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption, toast } from '@/components/ui';
import type { BizInquiryDraft, BizInquiryPriority } from '../../model/biz-inquiries.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: BizInquiryDraft] }>();
const draft = ref<BizInquiryDraft>(blank());
const priorities: UiSelectOption[] = ['紧急', '普通', '低'].map((value) => ({ value, label: value }));
const buyers: UiSelectOption[] = ['', '王建华', '陈伟杰', '林国栋', '周天宇'].map((value) => ({
  value,
  label: value || '暂不分配',
}));
watch(
  () => props.open,
  (open) => {
    if (open) draft.value = blank();
  },
);
function blank(): BizInquiryDraft {
  return {
    title: '',
    customerName: '',
    salesPerson: '张明远',
    priority: '普通',
    assignedBuyer: '',
    materialCount: '1',
    estimatedAmount: '',
    deadline: '',
    note: '',
  };
}
function save() {
  if (!draft.value.title.trim() || !draft.value.customerName.trim() || !draft.value.deadline) {
    toast.warning('请填写标题、客户和截止日期');
    return;
  }
  emit('save', { ...draft.value });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建业务询价"
    description="录入销售询价需求，数据仅保存在当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
    ><form
      :class="$style.form"
      @submit.prevent="save"
    >
      <UiFormField
        label="询价标题"
        required
        wide
        ><UiInput
          v-model="draft.title"
          placeholder="请输入业务询价标题"
      /></UiFormField>
      <UiFormField
        label="客户名称"
        required
        ><UiInput
          v-model="draft.customerName"
          placeholder="请输入客户名称" /></UiFormField
      ><UiFormField label="负责销售"><UiInput v-model="draft.salesPerson" /></UiFormField>
      <UiFormField label="优先级"
        ><UiSelect
          :model-value="draft.priority"
          :options="priorities"
          label="优先级"
          @update:model-value="draft.priority = $event as BizInquiryPriority" /></UiFormField
      ><UiFormField label="分配采购"
        ><UiSelect
          v-model="draft.assignedBuyer"
          :options="buyers"
          label="分配采购"
      /></UiFormField>
      <UiFormField label="物料数"
        ><UiInput
          v-model="draft.materialCount"
          type="number"
          min="1" /></UiFormField
      ><UiFormField label="预估金额"
        ><UiInput
          v-model="draft.estimatedAmount"
          type="number"
          min="0"
          placeholder="0"
      /></UiFormField>
      <UiFormField
        label="截止日期"
        required
        ><UiInput
          v-model="draft.deadline"
          type="date" /></UiFormField
      ><UiFormField label="备注"
        ><UiInput
          v-model="draft.note"
          placeholder="可选"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton @click="emit('update:open', false)">取消</UiButton
      ><UiButton
        variant="primary"
        @click="save"
        >创建询价</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
