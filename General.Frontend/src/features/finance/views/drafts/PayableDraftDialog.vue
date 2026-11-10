<script setup lang="ts">
import { Landmark } from 'lucide-vue-next';
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, toast } from '@/components/ui';
import type { PayableDraftRegistration } from '../../model/drafts.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [value: PayableDraftRegistration];
}>();
const payable = reactive<PayableDraftRegistration>({
  payee: '',
  amount: '',
  issueDate: '2026-09-16',
  dueDate: '2027-03-16',
});

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(payable, {
      payee: '',
      amount: '',
      issueDate: '2026-09-16',
      dueDate: '2027-03-16',
    });
  },
);

function submit() {
  if (!payable.payee.trim() || Number(payable.amount) <= 0) {
    toast.warning('请填写收款方和有效票面金额');
    return;
  }
  emit('save', { ...payable });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="开新银承付款"
    description="创建一张本地应付银行承兑汇票"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="submit"
    >
      <UiFormField
        label="收款方"
        required
        wide
      >
        <UiInput
          v-model="payable.payee"
          placeholder="请输入供应商或收款方"
          aria-label="银承收款方"
        />
      </UiFormField>
      <UiFormField
        label="票面金额"
        required
      >
        <UiInput
          v-model="payable.amount"
          type="number"
          min="0.01"
          step="0.01"
          aria-label="银承票面金额"
        />
      </UiFormField>
      <UiFormField
        label="出票日"
        required
      >
        <UiInput
          v-model="payable.issueDate"
          type="date"
          aria-label="银承出票日"
        />
      </UiFormField>
      <UiFormField
        label="到期日"
        required
        wide
      >
        <UiInput
          v-model="payable.dueDate"
          type="date"
          aria-label="银承到期日"
        />
      </UiFormField>
    </form>
    <template #icon>
      <span :class="$style.dialogIcon">
        <Landmark
          :size="18"
          :stroke-width="1.8"
        />
      </span>
    </template>
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
        >确认开票</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
