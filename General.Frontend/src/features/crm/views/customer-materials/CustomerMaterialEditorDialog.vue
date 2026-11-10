<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { CustomerMaterialDraft, CustomerMaterialRow } from '@/features/crm/model/customer-materials.types';

const props = defineProps<{
  open: boolean;
  row: CustomerMaterialRow | null;
  customers: readonly string[];
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: CustomerMaterialDraft];
}>();

const draft = reactive<CustomerMaterialDraft>(emptyDraft());
const customerOptions = (): UiSelectOption[] =>
  props.customers.map((customer) => ({ value: customer, label: customer }));

watch(
  () => [props.open, props.row] as const,
  ([open, row]) => {
    if (!open) return;
    Object.assign(draft, row ? fromRow(row) : emptyDraft());
  },
  { immediate: true },
);

function emptyDraft(): CustomerMaterialDraft {
  return {
    customer: props.customers[0] ?? '',
    internalCode: '',
    internalName: '',
    internalSpec: '',
    unit: '台',
    customerCode: '',
    customerName: '',
    remark: '',
  };
}

function fromRow(row: CustomerMaterialRow): CustomerMaterialDraft {
  return {
    customer: row.customer,
    internalCode: row.internalCode,
    internalName: row.internalName,
    internalSpec: row.internalSpec,
    unit: row.unit,
    customerCode: row.customerCode,
    customerName: row.customerName,
    remark: row.remark === '-' ? '' : row.remark,
  };
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="row ? '编辑物料对照' : '新增物料对照'"
    description="维护我方物料与客户物料的双向编码关系。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="客户名称"
        required
        wide
      >
        <UiSelect
          v-model="draft.customer"
          :options="customerOptions()"
          label="客户名称"
        />
      </UiFormField>
      <UiFormField
        label="我方物料编码"
        required
      >
        <UiInput
          v-model="draft.internalCode"
          placeholder="例如 SP-2025-00001"
        />
      </UiFormField>
      <UiFormField
        label="我方物料名称"
        required
      >
        <UiInput
          v-model="draft.internalName"
          placeholder="请输入我方物料名称"
        />
      </UiFormField>
      <UiFormField label="规格型号">
        <UiInput
          v-model="draft.internalSpec"
          placeholder="请输入规格型号"
        />
      </UiFormField>
      <UiFormField label="单位">
        <UiInput
          v-model="draft.unit"
          placeholder="台"
        />
      </UiFormField>
      <UiFormField
        label="客户物料编码"
        required
      >
        <UiInput
          v-model="draft.customerCode"
          placeholder="请输入客户物料编码"
        />
      </UiFormField>
      <UiFormField
        label="客户物料名称"
        required
      >
        <UiInput
          v-model="draft.customerName"
          placeholder="请输入客户物料名称"
        />
      </UiFormField>
      <UiFormField
        label="备注"
        wide
      >
        <UiTextarea
          v-model="draft.remark"
          :class="$style.remarkInput"
          placeholder="补充客户图纸代号、使用场景等信息"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
      >
        保存对照
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}

.remarkInput {
  min-height: 80px;
}
</style>
