<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { customerOwners } from '@/features/crm/model/customers.model';
import type { CustomerDraft, CustomerLevel, CustomerRow, CustomerType } from '@/features/crm/model/customers.types';

const props = defineProps<{ open: boolean; customer: CustomerRow | null }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: CustomerDraft];
}>();

const typeOptions: UiSelectOption[] = [
  { value: '终端客户', label: '终端客户' },
  { value: '集成商', label: '集成商' },
  { value: 'EPC', label: 'EPC' },
];
const levelOptions: UiSelectOption[] = [
  { value: 'S级（战略客户）', label: 'S级（战略客户）' },
  { value: 'A级（重要客户）', label: 'A级（重要客户）' },
  { value: 'B级（一般客户）', label: 'B级（一般客户）' },
  { value: 'C级（小客户）', label: 'C级（小客户）' },
];
const ownerOptions: UiSelectOption[] = customerOwners.map((owner) => ({ value: owner, label: owner }));

const draft = reactive<CustomerDraft>(emptyDraft());

watch(
  () => [props.open, props.customer] as const,
  ([open, customer]) => {
    if (!open) return;
    Object.assign(draft, customer ? fromCustomer(customer) : emptyDraft());
  },
  { immediate: true },
);

function emptyDraft(): CustomerDraft {
  return {
    name: '',
    type: '终端客户',
    level: 'B级（一般客户）',
    owner: '张伟',
    contact: '',
    phone: '',
    email: '',
    address: '',
    creditLimit: '300000',
    paymentDays: '30',
  };
}

function fromCustomer(customer: CustomerRow): CustomerDraft {
  return {
    name: customer.name,
    type: customer.type,
    level: customer.level,
    owner: customer.owner,
    contact: customer.contact,
    phone: customer.phone,
    email: customer.email,
    address: customer.address,
    creditLimit: String(customer.creditLimit),
    paymentDays: String(customer.paymentDays),
  };
}

function updateType(value: string) {
  draft.type = value as CustomerType;
}

function updateLevel(value: string) {
  draft.level = value as CustomerLevel;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="customer ? '编辑客户' : '新增客户'"
    description="客户档案只保存在当前本地复刻会话。"
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
        <UiInput
          v-model="draft.name"
          placeholder="请输入客户全称"
        />
      </UiFormField>
      <UiFormField label="客户类型">
        <UiSelect
          :model-value="draft.type"
          :options="typeOptions"
          label="客户类型"
          @update:model-value="updateType"
        />
      </UiFormField>
      <UiFormField label="客户级别">
        <UiSelect
          :model-value="draft.level"
          :options="levelOptions"
          label="客户级别"
          @update:model-value="updateLevel"
        />
      </UiFormField>
      <UiFormField label="负责人">
        <UiSelect
          v-model="draft.owner"
          :options="ownerOptions"
          label="负责人"
        />
      </UiFormField>
      <UiFormField label="主要联系人">
        <UiInput
          v-model="draft.contact"
          placeholder="请输入联系人"
        />
      </UiFormField>
      <UiFormField label="联系电话">
        <UiInput
          v-model="draft.phone"
          type="tel"
          placeholder="请输入手机号"
        />
      </UiFormField>
      <UiFormField label="邮箱">
        <UiInput
          v-model="draft.email"
          type="email"
          placeholder="name@company.com"
        />
      </UiFormField>
      <UiFormField label="信用额度">
        <UiInput
          v-model="draft.creditLimit"
          type="number"
        />
      </UiFormField>
      <UiFormField label="账期（天）">
        <UiInput
          v-model="draft.paymentDays"
          type="number"
        />
      </UiFormField>
      <UiFormField
        label="详细地址"
        wide
      >
        <UiInput
          v-model="draft.address"
          placeholder="请输入客户地址"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
      >
        保存客户
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
