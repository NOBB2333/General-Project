<script setup lang="ts">
import { ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, toast } from '@/components/ui';
import type { SupplierPricebookDraft } from '../../model/supplier-pricebook.types';
const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [v: boolean]; save: [v: SupplierPricebookDraft] }>();
const draft = ref<SupplierPricebookDraft>(blank());
watch(
  () => props.open,
  (o) => {
    if (o) draft.value = blank();
  },
);
function blank(): SupplierPricebookDraft {
  return { name: '', supplier: '', itemCount: '1', validFrom: '', validTo: '', discountRate: '' };
}
function save() {
  if (!draft.value.name.trim() || !draft.value.supplier.trim() || !draft.value.validFrom || !draft.value.validTo) {
    toast.warning('请填写价格本名称、供应商和有效期');
    return;
  }
  emit('save', { ...draft.value });
}
</script>
<template>
  <UiDialog
    :open="open"
    title="新建价格本"
    description="新价格本将以草稿状态保存在当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
    ><form
      :class="$style.form"
      @submit.prevent="save"
    >
      <UiFormField
        label="价格本名称"
        required
        wide
        ><UiInput v-model="draft.name" /></UiFormField
      ><UiFormField
        label="供应商"
        required
        wide
        ><UiInput v-model="draft.supplier" /></UiFormField
      ><UiFormField label="物料数"
        ><UiInput
          v-model="draft.itemCount"
          type="number"
          min="1" /></UiFormField
      ><UiFormField label="一级折扣"
        ><UiInput
          v-model="draft.discountRate"
          type="number"
          min="0"
          max="100"
          placeholder="可选" /></UiFormField
      ><UiFormField
        label="生效日期"
        required
        ><UiInput
          v-model="draft.validFrom"
          type="date" /></UiFormField
      ><UiFormField
        label="失效日期"
        required
        ><UiInput
          v-model="draft.validTo"
          type="date"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton @click="emit('update:open', false)">取消</UiButton
      ><UiButton
        variant="primary"
        @click="save"
        >保存草稿</UiButton
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
