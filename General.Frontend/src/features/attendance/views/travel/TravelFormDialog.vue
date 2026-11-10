<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea, type UiSelectOption } from '@/components/ui';
import type { TravelDraft, TravelTransport } from '../../model/travel.types';

const props = defineProps<{ open: boolean }>();
const emit = defineEmits<{ 'update:open': [value: boolean]; save: [draft: TravelDraft] }>();
const transportOptions: UiSelectOption[] = ['高铁', '飞机', '汽车', '其他'].map((value) => ({ value, label: value }));
const submitted = ref(false);
const draft = reactive<TravelDraft>({
  destination: '',
  transport: '高铁',
  reason: '',
  departureDate: '2026-03-25',
  returnDate: '2026-03-25',
  budget: '',
});

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    Object.assign(draft, {
      destination: '',
      transport: '高铁' as TravelTransport,
      reason: '',
      departureDate: '2026-03-25',
      returnDate: '2026-03-25',
      budget: '',
    });
    submitted.value = false;
  },
);

function submit() {
  submitted.value = true;
  if (
    !draft.destination.trim() ||
    !draft.reason.trim() ||
    !draft.departureDate ||
    !draft.returnDate ||
    draft.returnDate < draft.departureDate ||
    Number(draft.budget) <= 0
  )
    return;
  emit('save', { ...draft, destination: draft.destination.trim(), reason: draft.reason.trim() });
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建出差申请"
    description="填写行程与预算后提交审批"
    size="md"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="submit"
    >
      <UiFormField
        label="目的地"
        required
        ><UiInput
          v-model="draft.destination"
          placeholder="请输入城市"
          :invalid="submitted && !draft.destination.trim()"
      /></UiFormField>
      <UiFormField
        label="交通方式"
        required
        ><UiSelect
          v-model="draft.transport"
          :options="transportOptions"
          label="选择交通方式"
      /></UiFormField>
      <UiFormField
        label="出发日期"
        required
        ><UiInput
          v-model="draft.departureDate"
          type="date"
          :invalid="submitted && !draft.departureDate"
      /></UiFormField>
      <UiFormField
        label="返回日期"
        required
        :error="submitted && draft.returnDate < draft.departureDate ? '返回日期不能早于出发日期' : undefined"
        ><UiInput
          v-model="draft.returnDate"
          type="date"
          :invalid="submitted && (!draft.returnDate || draft.returnDate < draft.departureDate)"
      /></UiFormField>
      <UiFormField
        label="预算（元）"
        required
        ><UiInput
          v-model="draft.budget"
          type="number"
          min="1"
          placeholder="0.00"
          :invalid="submitted && Number(draft.budget) <= 0"
      /></UiFormField>
      <UiFormField
        label="出差事由"
        required
        wide
        ><UiTextarea
          v-model="draft.reason"
          rows="4"
          placeholder="请输入出差事由"
          :invalid="submitted && !draft.reason.trim()"
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
        >提交申请</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";
.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
