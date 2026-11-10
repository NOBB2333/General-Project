<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { OutsourceFeedDraft } from '../../model/outsource-transit.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: OutsourceFeedDraft] }>();
const supplierOptions: UiSelectOption[] = ['东莞顺达注塑科技', '苏州精机外协加工厂'].map((value) => ({
  value,
  label: value,
}));
const defaults: OutsourceFeedDraft = {
  purchaseOrder: 'OS-2026-0518-004',
  supplier: '东莞顺达注塑科技',
  productCode: '',
  productName: '',
  specification: '',
  quantity: '',
  unit: '件',
};
const draft = reactive<OutsourceFeedDraft>({ ...defaults });
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, defaults);
  },
);
function submit() {
  if (!draft.productCode.trim() || !draft.productName.trim() || Number(draft.quantity) <= 0) return;
  emit('save', { ...draft });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="委外发料"
    description="登记发往外协厂的物料，本次操作只影响本地复刻数据"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="采购订单"
        required
      >
        <UiInput v-model="draft.purchaseOrder" />
      </UiFormField>
      <UiFormField
        label="外协供应商"
        required
      >
        <UiSelect
          v-model="draft.supplier"
          :options="supplierOptions"
          label="外协供应商"
        />
      </UiFormField>
      <UiFormField
        label="产品编号"
        required
      >
        <UiInput
          v-model="draft.productCode"
          placeholder="例如 RM-AL-6061"
        />
      </UiFormField>
      <UiFormField
        label="品名"
        required
      >
        <UiInput v-model="draft.productName" />
      </UiFormField>
      <UiFormField label="规格">
        <UiInput v-model="draft.specification" />
      </UiFormField>
      <div :class="$style.quantityFields">
        <UiFormField
          label="发料数量"
          required
        >
          <UiInput
            v-model="draft.quantity"
            type="number"
            min="1"
          />
        </UiFormField>
        <UiFormField label="单位">
          <UiInput v-model="draft.unit" />
        </UiFormField>
      </div>
    </form>
    <template #footer>
      <UiButton
        size="sm"
        variant="secondary"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        size="sm"
        variant="primary"
        @click="submit"
        >确认发料</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.quantityFields {
  @apply grid grid-cols-[1fr_90px] gap-3;
}
</style>
