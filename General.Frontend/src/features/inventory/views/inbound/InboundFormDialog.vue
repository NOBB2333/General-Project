<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { InboundDraft, InboundType } from '../../model/inbound.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: InboundDraft];
}>();

const typeOptions: UiSelectOption[] = ['期初入库', '采购入库', '生产入库', '退货入库', '其他入库'].map((value) => ({
  value,
  label: value,
}));
const warehouseOptions: UiSelectOption[] = ['上海主仓', '备品备件仓', '中石化项目仓'].map((value) => ({
  value,
  label: value,
}));
const defaults: InboundDraft = {
  type: '采购入库',
  date: '2026-03-16',
  warehouse: '上海主仓',
  supplier: '',
  quantity: '1',
  amount: '0',
  remark: '',
};
const draft = reactive<InboundDraft>({ ...defaults });

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, { ...defaults, type: '采购入库' as InboundType });
  },
);

function submit() {
  if (!draft.supplier.trim()) return;
  emit('save', { ...draft, supplier: draft.supplier.trim(), remark: draft.remark.trim() });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建入库"
    description="录入入库来源、仓库和物料汇总信息"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="入库类型"
        required
      >
        <UiSelect
          v-model="draft.type"
          :options="typeOptions"
          label="入库类型"
        />
      </UiFormField>
      <UiFormField
        label="入库日期"
        required
      >
        <UiInput
          v-model="draft.date"
          type="date"
        />
      </UiFormField>
      <UiFormField
        label="仓库"
        required
      >
        <UiSelect
          v-model="draft.warehouse"
          :options="warehouseOptions"
          label="仓库"
        />
      </UiFormField>
      <UiFormField
        label="供应商/对方"
        required
      >
        <UiInput
          v-model="draft.supplier"
          placeholder="请输入供应商或对方名称"
        />
      </UiFormField>
      <UiFormField
        label="数量"
        required
      >
        <UiInput
          v-model="draft.quantity"
          type="number"
          min="1"
        />
      </UiFormField>
      <UiFormField label="金额">
        <UiInput
          v-model="draft.amount"
          type="number"
          min="0"
        />
      </UiFormField>
      <UiFormField
        label="备注"
        :class="$style.fullWidth"
      >
        <UiInput
          v-model="draft.remark"
          placeholder="补充本次入库说明"
        />
      </UiFormField>
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
        >保存入库单</UiButton
      >
    </template>
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
