<script setup lang="ts">
import { ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption, toast } from '@/components/ui';
import type { PurchaseContractDraft, PurchaseContractType } from '../../model/purchase-contracts.types';
const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [v: boolean]; save: [v: PurchaseContractDraft] }>();
const draft = ref<PurchaseContractDraft>(blank());
const types: UiSelectOption[] = ['框架合同', '采购合同', '服务合同'].map((value) => ({ value, label: value }));
watch(
  () => props.open,
  (o) => {
    if (o) draft.value = blank();
  },
);
function blank(): PurchaseContractDraft {
  return {
    name: '',
    project: '',
    type: '采购合同',
    supplier: '',
    amount: '',
    retentionRate: '',
    dueDate: '',
    owner: '张经理',
  };
}
function save() {
  if (!draft.value.name.trim() || !draft.value.supplier.trim() || !draft.value.dueDate) {
    toast.warning('请填写合同名称、供应商和到期日');
    return;
  }
  emit('save', { ...draft.value });
}
</script>
<template>
  <UiDialog
    :open="open"
    title="新建采购合同"
    description="创建待审批合同，数据仅保存在当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
    ><form
      :class="$style.form"
      @submit.prevent="save"
    >
      <UiFormField
        label="合同名称"
        required
        wide
        ><UiInput
          v-model="draft.name"
          placeholder="请输入合同名称" /></UiFormField
      ><UiFormField label="关联项目"
        ><UiInput
          v-model="draft.project"
          placeholder="可选" /></UiFormField
      ><UiFormField label="合同类型"
        ><UiSelect
          :model-value="draft.type"
          :options="types"
          label="合同类型"
          @update:model-value="draft.type = $event as PurchaseContractType" /></UiFormField
      ><UiFormField
        label="供应商"
        required
        ><UiInput v-model="draft.supplier" /></UiFormField
      ><UiFormField label="合同总额"
        ><UiInput
          v-model="draft.amount"
          type="number"
          min="0" /></UiFormField
      ><UiFormField label="质保金比例"
        ><UiInput
          v-model="draft.retentionRate"
          type="number"
          min="0"
          max="100"
          placeholder="可选" /></UiFormField
      ><UiFormField
        label="到期日"
        required
        ><UiInput
          v-model="draft.dueDate"
          type="date" /></UiFormField
      ><UiFormField label="负责人"><UiInput v-model="draft.owner" /></UiFormField>
    </form>
    <template #footer
      ><UiButton @click="emit('update:open', false)">取消</UiButton
      ><UiButton
        variant="primary"
        @click="save"
        >保存合同</UiButton
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
