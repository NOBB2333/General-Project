<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { ArrivalDraft, ArrivalType } from '../../model/arrival-register.types';
const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: ArrivalDraft] }>();
const typeOptions: UiSelectOption[] = ['采购到货', '生产到货', '退货到货', '其他到货'].map((value) => ({
  value,
  label: value,
}));
const warehouseOptions: UiSelectOption[] = ['上海主仓', '备品备件仓', '中石化项目仓'].map((value) => ({
  value,
  label: value,
}));
const draft = reactive<ArrivalDraft>({
  type: '采购到货',
  date: '2026-03-16',
  warehouse: '上海主仓',
  party: '',
  relatedCode: '',
  quantity: '1',
  amount: '0',
});
watch(
  () => props.open,
  (open) => {
    if (open)
      Object.assign(draft, {
        type: '采购到货' as ArrivalType,
        date: '2026-03-16',
        warehouse: '上海主仓',
        party: '',
        relatedCode: '',
        quantity: '1',
        amount: '0',
      });
  },
);
function submit() {
  if (!draft.party.trim() || !draft.relatedCode.trim()) return;
  emit('save', { ...draft, party: draft.party.trim(), relatedCode: draft.relatedCode.trim() });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建到货登记"
    description="登记后进入待检或免检流程"
    size="md"
    @update:open="emit('update:open', $event)"
    ><form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="到货类型"
        required
        ><UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="到货类型" /></UiFormField
      ><UiFormField
        label="到货日期"
        required
        ><UiInput
          v-model="draft.date"
          type="date" /></UiFormField
      ><UiFormField
        label="仓库"
        required
        ><UiSelect
          v-model="draft.warehouse"
          :options="warehouseOptions"
          label="仓库" /></UiFormField
      ><UiFormField
        label="对方"
        required
        ><UiInput
          v-model="draft.party"
          placeholder="供应商或客户" /></UiFormField
      ><UiFormField
        label="关联单号"
        required
        ><UiInput
          v-model="draft.relatedCode"
          placeholder="PO/SO/MO 单号" /></UiFormField
      ><UiFormField
        label="数量"
        required
        ><UiInput
          v-model="draft.quantity"
          type="number"
          min="1" /></UiFormField
      ><UiFormField label="金额"
        ><UiInput
          v-model="draft.amount"
          type="number"
          min="0"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton
        size="sm"
        @click="emit('update:open', false)"
        >取消</UiButton
      ><UiButton
        size="sm"
        variant="primary"
        @click="submit"
        >保存登记</UiButton
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
