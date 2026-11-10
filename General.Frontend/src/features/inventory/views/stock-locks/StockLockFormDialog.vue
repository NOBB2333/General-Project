<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { StockLockDraft, StockLockType } from '../../model/stock-locks.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: StockLockDraft] }>();
const typeOptions: UiSelectOption[] = ['订单锁库', '手动锁库'].map((value) => ({ value, label: value }));
const warehouseOptions: UiSelectOption[] = ['上海主仓', '备品备件仓'].map((value) => ({ value, label: value }));
const defaults: StockLockDraft = {
  product: '',
  specification: '',
  batchCode: '',
  type: '手动锁库',
  reason: '',
  quantity: '1',
  warehouse: '上海主仓',
};
const draft = reactive<StockLockDraft>({ ...defaults });
watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, { ...defaults, type: '手动锁库' as StockLockType });
  },
);
function submit() {
  if (!draft.product.trim() || !draft.reason.trim()) return;
  emit('save', {
    ...draft,
    product: draft.product.trim(),
    specification: draft.specification.trim(),
    batchCode: draft.batchCode.trim(),
    reason: draft.reason.trim(),
  });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建锁库"
    description="本地登记库存锁定，不会修改外部参考站数据"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="商品名称"
        required
        :class="$style.wide"
        ><UiInput
          v-model="draft.product"
          placeholder="输入商品名称" /></UiFormField
      ><UiFormField label="规格型号"><UiInput v-model="draft.specification" /></UiFormField
      ><UiFormField label="批次号"><UiInput v-model="draft.batchCode" /></UiFormField
      ><UiFormField label="锁定类型"
        ><UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="锁定类型" /></UiFormField
      ><UiFormField
        label="锁定数量"
        required
        ><UiInput
          v-model="draft.quantity"
          type="number"
          min="1" /></UiFormField
      ><UiFormField label="仓库"
        ><UiSelect
          v-model="draft.warehouse"
          :options="warehouseOptions"
          label="锁定仓库" /></UiFormField
      ><UiFormField
        label="关联订单/原因"
        required
        :class="$style.wide"
        ><UiInput
          v-model="draft.reason"
          placeholder="输入订单号或锁定原因"
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
        >保存锁库</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.wide {
  @apply col-span-2;
}
</style>
