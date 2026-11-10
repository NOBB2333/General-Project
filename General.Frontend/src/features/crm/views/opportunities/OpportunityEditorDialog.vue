<script setup lang="ts">
import { reactive, watch } from 'vue';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type {
  OpportunityDraft,
  OpportunityPriority,
  OpportunitySource,
  OpportunityStage,
  OpportunityTraffic,
} from '@/features/crm/model/opportunities.types';

const props = defineProps<{
  open: boolean;
  customers: readonly string[];
  owners: readonly string[];
  stages: readonly OpportunityStage[];
  sources: readonly OpportunitySource[];
}>();
const emit = defineEmits<{
  'update:open': [value: boolean];
  save: [draft: OpportunityDraft];
}>();
const draft = reactive<OpportunityDraft>(emptyDraft());

const customerOptions = () => props.customers.map((value) => ({ value, label: value }));
const ownerOptions = () => props.owners.map((value) => ({ value, label: value }));
const stageOptions = () => props.stages.map((value) => ({ value, label: value }));
const sourceOptions = () => props.sources.map((value) => ({ value, label: value }));
const trafficOptions: UiSelectOption[] = [
  { value: 'green', label: '绿灯' },
  { value: 'orange', label: '橙灯' },
  { value: 'red', label: '红灯' },
];
const priorityOptions: UiSelectOption[] = ['高', '中', '低'].map((value) => ({ value, label: value }));

watch(
  () => props.open,
  (open) => {
    if (open) Object.assign(draft, emptyDraft());
  },
);

function emptyDraft(): OpportunityDraft {
  return {
    name: '',
    customer: '',
    contact: '',
    stage: '线索',
    traffic: 'green',
    priority: '中',
    amount: '',
    probability: '10',
    expectedDate: '2026-12-31',
    owner: props.owners[0] ?? '张明',
    source: '网络营销',
  };
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新建商机"
    description="建立销售机会，设置预计金额、成交概率和负责人。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form
      :class="$style.form"
      @submit.prevent="emit('save', { ...draft })"
    >
      <UiFormField
        label="商机名称"
        required
        :class="$style.full"
      >
        <UiInput
          v-model="draft.name"
          placeholder="请输入商机名称"
          autofocus
        />
      </UiFormField>
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
      <UiFormField label="联系人">
        <UiInput
          v-model="draft.contact"
          placeholder="请输入联系人"
        />
      </UiFormField>
      <UiFormField label="阶段">
        <UiSelect
          :model-value="draft.stage"
          :options="stageOptions()"
          label="阶段"
          @update:model-value="draft.stage = $event as OpportunityStage"
        />
      </UiFormField>
      <UiFormField label="来源">
        <UiSelect
          :model-value="draft.source"
          :options="sourceOptions()"
          label="来源"
          @update:model-value="draft.source = $event as OpportunitySource"
        />
      </UiFormField>
      <UiFormField label="红绿灯">
        <UiSelect
          :model-value="draft.traffic"
          :options="trafficOptions"
          label="红绿灯"
          @update:model-value="draft.traffic = $event as OpportunityTraffic"
        />
      </UiFormField>
      <UiFormField label="优先级">
        <UiSelect
          :model-value="draft.priority"
          :options="priorityOptions"
          label="优先级"
          @update:model-value="draft.priority = $event as OpportunityPriority"
        />
      </UiFormField>
      <UiFormField
        label="预计金额"
        required
      >
        <UiInput
          v-model="draft.amount"
          type="number"
          min="1"
          placeholder="请输入金额"
        />
      </UiFormField>
      <UiFormField label="成交概率 (%)">
        <UiInput
          v-model="draft.probability"
          type="number"
          min="0"
          max="100"
        />
      </UiFormField>
      <UiFormField label="预计成交日期">
        <UiInput
          v-model="draft.expectedDate"
          type="date"
        />
      </UiFormField>
      <UiFormField label="负责人">
        <UiSelect
          v-model="draft.owner"
          :options="ownerOptions()"
          label="负责人"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton @click="emit('update:open', false)">取消</UiButton>
      <UiButton
        variant="primary"
        @click="emit('save', { ...draft })"
      >
        创建商机
      </UiButton>
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../../styles/index.css";

.form {
  @apply grid grid-cols-2 gap-4 p-5;
}
.full {
  @apply col-span-2;
}
</style>
