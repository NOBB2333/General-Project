<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import type { PendingQcDraft } from '../../model/pending-qc.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: PendingQcDraft] }>();
const defaults: PendingQcDraft = { material: '', specification: '', quantity: '1 台', batchCode: '', supplier: '' };
const draft = reactive<PendingQcDraft>({ ...defaults });
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, defaults);
  },
);
function submit() {
  if (!draft.material.trim() || !draft.batchCode.trim() || !draft.supplier.trim()) return;
  emit('save', {
    ...draft,
    material: draft.material.trim(),
    specification: draft.specification.trim(),
    batchCode: draft.batchCode.trim(),
    supplier: draft.supplier.trim(),
  });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建待检单"
    description="为到货物料建立独立的来料检验记录"
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
        ><UiInput
          v-model="draft.material"
          placeholder="请输入物料名称"
      /></UiFormField>
      <UiFormField label="规格型号"
        ><UiInput
          v-model="draft.specification"
          placeholder="请输入规格型号"
      /></UiFormField>
      <UiFormField
        label="数量"
        required
        ><UiInput
          v-model="draft.quantity"
          placeholder="例如 10 台"
      /></UiFormField>
      <UiFormField
        label="批次号"
        required
        ><UiInput
          v-model="draft.batchCode"
          placeholder="B20260318-01"
      /></UiFormField>
      <UiFormField
        label="供应商"
        required
        ><UiInput
          v-model="draft.supplier"
          placeholder="请输入供应商"
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
        >保存待检单</UiButton
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
