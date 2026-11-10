<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { NcrDraft, NcrResponsibility, NcrUrgency } from '../../model/ncr.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: NcrDraft] }>();
const urgencyOptions: UiSelectOption[] = ['普通', '紧急', '重大'].map((value) => ({ value, label: value }));
const responsibilityOptions: UiSelectOption[] = ['供应商', '内部', '运输', '待判定'].map((value) => ({
  value,
  label: value,
}));
const defaults: NcrDraft = {
  inspectionCode: '',
  supplier: '',
  failedQuantity: '1',
  urgency: '普通',
  responsibility: '待判定',
  resolution: '复检',
};
const draft = reactive<NcrDraft>({ ...defaults });
watch(
  () => props.open,
  (open) => {
    if (open)
      Object.assign(draft, {
        ...defaults,
        urgency: '普通' as NcrUrgency,
        responsibility: '待判定' as NcrResponsibility,
      });
  },
);
function submit() {
  if (!draft.inspectionCode.trim() || !draft.supplier.trim()) return;
  emit('save', {
    ...draft,
    inspectionCode: draft.inspectionCode.trim(),
    supplier: draft.supplier.trim(),
    resolution: draft.resolution.trim(),
  });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建处理单"
    description="登记不合格物料并确定责任方与处理方式"
    size="md"
    @update:open="emit('update:open', $event)"
    ><form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="来源检验单"
        required
        ><UiInput
          v-model="draft.inspectionCode"
          placeholder="IQC-2026-0000" /></UiFormField
      ><UiFormField
        label="供应商"
        required
        ><UiInput v-model="draft.supplier" /></UiFormField
      ><UiFormField
        label="不合格数"
        required
        ><UiInput
          v-model="draft.failedQuantity"
          type="number"
          min="1" /></UiFormField
      ><UiFormField label="紧急度"
        ><UiSelect
          v-model="draft.urgency"
          :options="urgencyOptions"
          label="紧急度" /></UiFormField
      ><UiFormField label="责任方"
        ><UiSelect
          v-model="draft.responsibility"
          :options="responsibilityOptions"
          label="责任方" /></UiFormField
      ><UiFormField label="处理方式"><UiInput v-model="draft.resolution" /></UiFormField>
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
        >保存处理单</UiButton
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
