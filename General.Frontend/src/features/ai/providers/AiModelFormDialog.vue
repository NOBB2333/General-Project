<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import type { AiApi } from '@/api/core/ai';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiSwitch, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  createModelDraft,
  toModelDraft,
  toModelSaveInput,
  validateModelDraft,
  type ModelDraft,
} from './providers.model';

const props = defineProps<{
  error: string;
  model: AiApi.Model | null;
  models: AiApi.Model[];
  open: boolean;
  providerId: string;
  saving: boolean;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  save: [input: AiApi.ModelSaveInput];
}>();

const draft = reactive<ModelDraft>(createModelDraft());
const localError = ref('');
const fallbackOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '不使用回退模型' },
  ...props.models
    .filter((item) => item.id !== props.model?.id && item.isEnabled)
    .map((item) => ({ value: item.id, label: `${item.displayName}（${item.modelCode}）` })),
]);

watch(
  () => [props.open, props.model] as const,
  ([open, model]) => {
    localError.value = '';
    if (open) Object.assign(draft, toModelDraft(model));
    else Object.assign(draft, createModelDraft());
  },
  { immediate: true },
);

function submit() {
  if (props.saving) return;
  localError.value = validateModelDraft(draft) ?? '';
  if (localError.value) return;
  emit('save', toModelSaveInput(draft, props.providerId));
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="model ? `编辑模型：${model.displayName}` : '新增模型'"
    description="模型启用状态、默认模型和参数以服务端保存结果为准。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form @submit.prevent="submit">
      <UiFormField label="模型编码">
        <UiInput
          v-model="draft.modelCode"
          maxlength="128"
          required
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField label="显示名称">
        <UiInput
          v-model="draft.displayName"
          maxlength="128"
          required
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField label="模型类型">
        <UiInput
          v-model="draft.modelType"
          maxlength="32"
          required
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField label="回退模型">
        <UiSelect
          v-model="draft.fallbackModelId"
          :options="fallbackOptions"
          label="回退模型"
        />
      </UiFormField>
      <UiFormField label="上下文窗口">
        <UiInput
          v-model="draft.contextWindow"
          type="number"
          min="0"
          step="1"
          inputmode="numeric"
        />
      </UiFormField>
      <UiFormField label="最大输出 Token 数">
        <UiInput
          v-model="draft.maxOutputTokens"
          type="number"
          min="0"
          step="1"
          inputmode="numeric"
        />
      </UiFormField>
      <UiFormField label="输入 Token 单价">
        <UiInput
          v-model="draft.inputTokenPrice"
          type="number"
          min="0"
          step="any"
          inputmode="decimal"
        />
      </UiFormField>
      <UiFormField label="输出 Token 单价">
        <UiInput
          v-model="draft.outputTokenPrice"
          type="number"
          min="0"
          step="any"
          inputmode="decimal"
        />
      </UiFormField>
      <UiFormField label="价格币种">
        <UiInput
          v-model="draft.currency"
          maxlength="16"
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField label="排序值">
        <UiInput
          v-model="draft.sort"
          type="number"
          step="1"
          inputmode="numeric"
        />
      </UiFormField>
      <UiFormField
        label="能力"
        hint="每行填写一个能力，也可用逗号分隔。"
        wide
      >
        <UiTextarea
          v-model="draft.capabilitiesText"
          rows="4"
          spellcheck="false"
        />
      </UiFormField>
      <UiFormField label="模型状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用模型"
        />
      </UiFormField>
      <UiFormField label="默认模型">
        <UiSwitch
          v-model="draft.isDefault"
          label="设为此供应商的默认模型"
        />
      </UiFormField>
      <UiFormField
        label="备注"
        wide
      >
        <UiTextarea
          v-model="draft.remark"
          rows="3"
          maxlength="512"
        />
      </UiFormField>
      <p
        v-if="localError || error"
        role="alert"
      >
        {{ localError || error }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="saving"
        @click="submit"
        >{{ saving ? '保存中…' : '保存模型' }}</UiButton
      >
    </template>
  </UiDialog>
</template>
