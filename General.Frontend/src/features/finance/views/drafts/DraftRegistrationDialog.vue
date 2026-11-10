<script setup lang="ts">
import { NotebookPen } from 'lucide-vue-next';
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import type { DraftRegistration } from '../../model/drafts.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [value: DraftRegistration];
}>();

const acceptanceOptions: UiSelectOption[] = [
  { value: '银行承兑', label: '银行承兑' },
  { value: '商业承兑', label: '商业承兑' },
];
const mediumOptions: UiSelectOption[] = [
  { value: '电票', label: '电票' },
  { value: '纸票', label: '纸票' },
];
const registration = reactive<DraftRegistration>({
  noteNumber: '',
  drawer: '',
  acceptor: '',
  amount: '',
  issueDate: '2026-09-16',
  dueDate: '2027-03-16',
  acceptanceKind: '银行承兑',
  medium: '电票',
});

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(registration, {
      noteNumber: '',
      drawer: '',
      acceptor: '',
      amount: '',
      issueDate: '2026-09-16',
      dueDate: '2027-03-16',
      acceptanceKind: '银行承兑',
      medium: '电票',
    });
  },
);

function submit() {
  if (!registration.noteNumber.trim() || !registration.drawer.trim() || Number(registration.amount) <= 0) {
    toast.warning('请填写票号、出票人和有效票面金额');
    return;
  }
  emit('save', { ...registration });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="登记应收票据"
    description="新增票据仅保存到当前本地会话"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="submit"
    >
      <UiFormField
        label="票号"
        required
        wide
      >
        <UiInput
          v-model="registration.noteNumber"
          placeholder="请输入票据号码"
          aria-label="应收票号"
        />
      </UiFormField>
      <UiFormField
        label="出票人"
        required
      >
        <UiInput
          v-model="registration.drawer"
          placeholder="请输入出票人"
          aria-label="出票人"
        />
      </UiFormField>
      <UiFormField label="承兑人">
        <UiInput
          v-model="registration.acceptor"
          placeholder="请输入银行或企业"
          aria-label="承兑人"
        />
      </UiFormField>
      <UiFormField
        label="承兑类型"
        required
      >
        <UiSelect
          v-model="registration.acceptanceKind"
          :options="acceptanceOptions"
          label="登记承兑类型"
        />
      </UiFormField>
      <UiFormField
        label="票据介质"
        required
      >
        <UiSelect
          v-model="registration.medium"
          :options="mediumOptions"
          label="票据介质"
        />
      </UiFormField>
      <UiFormField
        label="票面金额"
        required
      >
        <UiInput
          v-model="registration.amount"
          type="number"
          min="0.01"
          step="0.01"
          aria-label="应收票面金额"
        />
      </UiFormField>
      <UiFormField
        label="出票日"
        required
      >
        <UiInput
          v-model="registration.issueDate"
          type="date"
          aria-label="应收出票日"
        />
      </UiFormField>
      <UiFormField
        label="到期日"
        required
      >
        <UiInput
          v-model="registration.dueDate"
          type="date"
          aria-label="应收到期日"
        />
      </UiFormField>
    </form>
    <template #icon>
      <span :class="$style.dialogIcon">
        <NotebookPen
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
        >确认登记</UiButton
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
