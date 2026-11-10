<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import type { QcOrderDraft } from '../../model/qc-orders.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: QcOrderDraft] }>();
const defaults: QcOrderDraft = { material: '', specification: '', quantity: '1', supplier: '', sourceCode: '' };
const draft = reactive<QcOrderDraft>({ ...defaults });
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, defaults);
  },
);
function submit() {
  if (!draft.material.trim() || !draft.supplier.trim() || !draft.sourceCode.trim()) return;
  emit('save', {
    ...draft,
    material: draft.material.trim(),
    specification: draft.specification.trim(),
    supplier: draft.supplier.trim(),
    sourceCode: draft.sourceCode.trim(),
  });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建检验单"
    description="为一种来料物料建立检验任务"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="物料名称"
        required
        :class="$style.fullWidth"
        ><UiInput v-model="draft.material" /></UiFormField
      ><UiFormField label="规格型号"><UiInput v-model="draft.specification" /></UiFormField
      ><UiFormField
        label="数量"
        required
        ><UiInput
          v-model="draft.quantity"
          type="number"
          min="1" /></UiFormField
      ><UiFormField
        label="供应商"
        required
        ><UiInput v-model="draft.supplier" /></UiFormField
      ><UiFormField
        label="来源单号"
        required
        ><UiInput
          v-model="draft.sourceCode"
          placeholder="PO-2026-0000"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        size="sm"
        variant="secondary"
        @click="emit('update:open', false)"
        >取消</UiButton
      ><UiButton
        size="sm"
        variant="primary"
        @click="submit"
        >保存检验单</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.fullWidth {
  @apply col-span-2;
}
</style>
