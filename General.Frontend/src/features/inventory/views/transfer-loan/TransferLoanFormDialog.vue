<script setup lang="ts">
import { ArrowRightLeft } from 'lucide-vue-next';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { TransferLoanDraft } from '../../model/transfer-loan.types';

const props = defineProps<{
  open: boolean;
  draft: TransferLoanDraft;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  'update:draft': [value: TransferLoanDraft];
  save: [];
}>();

const kindOptions: UiSelectOption[] = [
  { value: 'transfer', label: '普通调拨' },
  { value: 'loan', label: '借出管理' },
  { value: 'repricing', label: '变价调拨' },
];

const warehouseOptions: UiSelectOption[] = [
  { value: '上海主仓', label: '上海主仓' },
  { value: '备品备件仓', label: '备品备件仓' },
  { value: '华能DCS项目仓', label: '华能DCS项目仓' },
];

function updateDraft<K extends keyof TransferLoanDraft>(key: K, value: TransferLoanDraft[K]) {
  emit('update:draft', { ...props.draft, [key]: value });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建调拨与借出单据"
    description="本地演示数据仅保存在当前会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <template #icon>
      <span :class="$style.dialogIcon"><ArrowRightLeft :size="18" /></span>
    </template>
    <form
      :class="$style.form"
      @submit.prevent="emit('save')"
    >
      <UiFormField
        label="业务类型"
        required
      >
        <UiSelect
          :model-value="draft.kind"
          :options="kindOptions"
          label="业务类型"
          @update:model-value="updateDraft('kind', $event as TransferLoanDraft['kind'])"
        />
      </UiFormField>
      <UiFormField
        label="发出仓库"
        required
      >
        <UiSelect
          :model-value="draft.sender"
          :options="warehouseOptions"
          label="发出仓库"
          @update:model-value="updateDraft('sender', $event)"
        />
      </UiFormField>
      <UiFormField
        label="接收方"
        required
        wide
      >
        <UiInput
          :model-value="draft.receiver"
          placeholder="仓库、客户或合作单位"
          @update:model-value="updateDraft('receiver', $event)"
        />
      </UiFormField>
      <UiFormField
        label="业务原因"
        required
        wide
      >
        <UiInput
          :model-value="draft.reason"
          placeholder="填写调拨、借出或价格调整原因"
          @update:model-value="updateDraft('reason', $event)"
        />
      </UiFormField>
      <UiFormField label="商品种类">
        <UiInput
          :model-value="draft.productCount"
          type="number"
          min="1"
          @update:model-value="updateDraft('productCount', $event)"
        />
      </UiFormField>
      <UiFormField label="商品数量">
        <UiInput
          :model-value="draft.quantity"
          type="number"
          min="1"
          @update:model-value="updateDraft('quantity', $event)"
        />
      </UiFormField>
      <UiFormField
        label="金额"
        wide
      >
        <UiInput
          :model-value="draft.amount"
          type="number"
          min="0"
          placeholder="0"
          @update:model-value="updateDraft('amount', $event)"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        size="sm"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('save')"
        >保存单据</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
