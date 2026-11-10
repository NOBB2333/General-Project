<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { updateProjectRaidRiskApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiDialog, UiFormField, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  buildProjectRaidRiskUpdateInput,
  projectRaidRiskFormFromItem,
  type ProjectRaidRiskForm,
} from '../model/project-issues';

const props = defineProps<{
  item: null | ProjectApi.RaidItem;
  open: boolean;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  saved: [];
}>();

const form = reactive<ProjectRaidRiskForm>({
  contingencyPlan: '',
  impact: '0',
  probability: '0',
  responseStrategy: '',
  triggerCondition: '',
});
const saving = ref(false);
const error = ref('');
const riskValueOptions: UiSelectOption[] = Array.from({ length: 6 }, (_, value) => ({
  value: String(value),
  label: String(value),
}));

watch(
  () => [props.open, props.item?.id] as const,
  ([open]) => {
    if (!open || !props.item) return;
    Object.assign(form, projectRaidRiskFormFromItem(props.item));
    error.value = '';
  },
);

function updateDialogOpen(value: boolean) {
  if (saving.value) return;
  emit('update:open', value);
}

async function saveAssessment() {
  if (!props.item) return;
  const probability = Number(form.probability);
  const impact = Number(form.impact);
  if (
    !Number.isInteger(probability) ||
    probability < 0 ||
    probability > 5 ||
    !Number.isInteger(impact) ||
    impact < 0 ||
    impact > 5
  ) {
    error.value = '概率和影响请选择0至5的整数。';
    return;
  }

  saving.value = true;
  error.value = '';
  try {
    await updateProjectRaidRiskApi(props.item.id, buildProjectRaidRiskUpdateInput(form));
    emit('update:open', false);
    emit('saved');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '风险评估保存失败，请重试。';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <UiDialog
    :open="open"
    title="更新风险评估"
    description="风险得分由后端根据概率和影响计算；应对信息按当前记录保存。"
    @update:open="updateDialogOpen"
  >
    <form
      :class="$style.form"
      @submit.prevent="saveAssessment"
    >
      <UiFormField label="发生概率">
        <UiSelect
          v-model="form.probability"
          label="风险发生概率"
          :options="riskValueOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="影响程度">
        <UiSelect
          v-model="form.impact"
          label="风险影响程度"
          :options="riskValueOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField
        label="应对策略"
        wide
      >
        <UiTextarea
          v-model="form.responseStrategy"
          :disabled="saving"
          rows="3"
        />
      </UiFormField>
      <UiFormField
        label="触发条件"
        wide
      >
        <UiTextarea
          v-model="form.triggerCondition"
          :disabled="saving"
          rows="2"
        />
      </UiFormField>
      <UiFormField
        label="应急预案"
        wide
      >
        <UiTextarea
          v-model="form.contingencyPlan"
          :disabled="saving"
          rows="2"
        />
      </UiFormField>
      <p
        v-if="error"
        role="alert"
        :class="$style.error"
      >
        {{ error }}
      </p>
      <div :class="$style.actions">
        <UiButton
          type="submit"
          variant="primary"
          :loading="saving"
        >
          保存风险评估
        </UiButton>
        <UiButton
          type="button"
          variant="secondary"
          :disabled="saving"
          @click="updateDialogOpen(false)"
        >
          取消
        </UiButton>
      </div>
    </form>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply grid gap-3 sm:grid-cols-2;
}

.error {
  @apply rounded border px-3 py-2 text-[11px] sm:col-span-2;
  border-color: var(--ui-border);
  color: var(--ui-danger);
}

.actions {
  @apply flex justify-end gap-2 pt-2 sm:col-span-2;
}
</style>
