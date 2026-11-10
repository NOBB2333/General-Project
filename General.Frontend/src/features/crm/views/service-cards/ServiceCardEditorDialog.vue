<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { ServiceCardDraft, ServiceCardTone } from '@/features/crm/model/service-cards.types';

const props = defineProps<{
  open: boolean;
  customers: readonly string[];
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: ServiceCardDraft];
}>();

const draft = reactive<ServiceCardDraft>(emptyDraft());
const tierOptions: UiSelectOption[] = [
  { value: '钻石卡', label: '钻石卡' },
  { value: '铂金卡', label: '铂金卡' },
  { value: '黄金卡', label: '黄金卡' },
  { value: '白银卡', label: '白银卡' },
];
const toneOptions: UiSelectOption[] = [
  { value: 'diamond', label: '钻石紫' },
  { value: 'platinum', label: '铂金黑' },
  { value: 'gold', label: '黄金橙' },
  { value: 'silver', label: '白银灰' },
];
const customerOptions = (): UiSelectOption[] =>
  props.customers.map((customer) => ({ value: customer, label: customer }));

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): ServiceCardDraft {
  return {
    serviceName: '',
    customer: props.customers[0] ?? '',
    tierLabel: '钻石卡',
    cardNumber: '6280 8801 0007 2026',
    validUntil: '2027-08-16',
    validityLabel: '有效期 1 年',
    amount: '',
    tone: 'diamond',
  };
}

function updateTone(value: string) {
  draft.tone = value as ServiceCardTone;
}
</script>

<template>
  <UiDialog
    :open="open"
    title="录入服务权益"
    description="为客户创建服务权益卡，并记录有效期与服务金额。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="持卡客户"
        required
      >
        <UiSelect
          v-model="draft.customer"
          :options="customerOptions()"
          label="持卡客户"
        />
      </UiFormField>
      <UiFormField
        label="服务名称"
        required
      >
        <UiInput
          v-model="draft.serviceName"
          placeholder="例如 年度运维服务"
        />
      </UiFormField>
      <UiFormField label="权益等级">
        <UiSelect
          v-model="draft.tierLabel"
          :options="tierOptions"
          label="权益等级"
        />
      </UiFormField>
      <UiFormField label="卡面风格">
        <UiSelect
          :model-value="draft.tone"
          :options="toneOptions"
          label="卡面风格"
          @update:model-value="updateTone"
        />
      </UiFormField>
      <UiFormField
        label="卡号"
        required
        wide
      >
        <UiInput
          v-model="draft.cardNumber"
          placeholder="请输入 16 位服务卡号"
        />
      </UiFormField>
      <UiFormField label="有效期描述">
        <UiInput
          v-model="draft.validityLabel"
          placeholder="例如 有效期 1 年"
        />
      </UiFormField>
      <UiFormField
        label="有效期至"
        required
      >
        <UiInput
          v-model="draft.validUntil"
          type="date"
        />
      </UiFormField>
      <UiFormField
        label="服务金额"
        required
        wide
      >
        <UiInput
          v-model="draft.amount"
          type="number"
          min="0"
          placeholder="请输入服务金额"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
      >
        生成权益卡
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
