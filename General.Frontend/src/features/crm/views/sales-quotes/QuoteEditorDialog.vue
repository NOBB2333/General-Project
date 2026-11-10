<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { QuoteStatus, SalesQuoteDraft } from '@/features/crm/model/sales-quotes.types';

const props = defineProps<{ open: boolean; customers: readonly string[]; owners: readonly string[] }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: SalesQuoteDraft] }>();
const draft = reactive<SalesQuoteDraft>(emptyDraft());
const customerOptions = () => props.customers.map((value) => ({ value, label: value }));
const ownerOptions = () => props.owners.map((value) => ({ value, label: value }));
const statusOptions: UiSelectOption[] = ['草稿', '待审核', '已发送'].map((value) => ({ value, label: value }));

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): SalesQuoteDraft {
  return {
    customer: '',
    project: '',
    owner: props.owners[0] ?? '张明',
    productCount: '1',
    amount: '',
    discount: '',
    validUntil: '2026-03-31',
    status: '草稿',
  };
}

function updateStatus(value: string) {
  draft.status = value as QuoteStatus;
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建报价"
    description="创建客户报价单并进入审批与跟进流程。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="客户"
        required
      >
        <UiSelect
          v-model="draft.customer"
          :options="customerOptions()"
          label="客户"
          placeholder="请选择客户"
        />
      </UiFormField>
      <UiFormField label="关联项目">
        <UiInput
          v-model="draft.project"
          placeholder="请输入项目名称"
        />
      </UiFormField>
      <UiFormField label="负责人">
        <UiSelect
          v-model="draft.owner"
          :options="ownerOptions()"
          label="负责人"
        />
      </UiFormField>
      <UiFormField label="报价状态">
        <UiSelect
          :model-value="draft.status"
          :options="statusOptions"
          label="报价状态"
          @update:model-value="updateStatus"
        />
      </UiFormField>
      <UiFormField
        label="商品数"
        required
      >
        <UiInput
          v-model="draft.productCount"
          type="number"
          min="1"
        />
      </UiFormField>
      <UiFormField
        label="报价金额"
        required
      >
        <UiInput
          v-model="draft.amount"
          type="number"
          min="0"
          placeholder="请输入含税金额"
        />
      </UiFormField>
      <UiFormField label="折扣 (%)">
        <UiInput
          v-model="draft.discount"
          type="number"
          min="0"
          max="100"
        />
      </UiFormField>
      <UiFormField label="有效期至">
        <UiInput
          v-model="draft.validUntil"
          type="date"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
      >
        创建报价
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
