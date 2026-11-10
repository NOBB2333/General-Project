<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import type { UiSelectOption } from '@/components/ui';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiSwitch, UiTextarea } from '@/components/ui';
import type { AiApi } from '@/api/core/ai';
import { createProviderDraft, toProviderDraft, toProviderSaveInput, type ProviderDraft } from './providers.model';

const props = defineProps<{
  error: string;
  open: boolean;
  provider: AiApi.Provider | null;
  saving: boolean;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  save: [input: AiApi.ProviderSaveInput];
}>();

const draft = reactive<ProviderDraft>(createProviderDraft());
const localError = ref('');
const providerTypeOptions: UiSelectOption[] = [
  { value: 'OpenAIResponses', label: 'OpenAI Responses' },
  { value: 'AnthropicMessages', label: 'Anthropic Messages' },
];

watch(
  () => [props.open, props.provider] as const,
  ([open, provider]) => {
    localError.value = '';
    if (open) Object.assign(draft, toProviderDraft(provider));
    else Object.assign(draft, createProviderDraft());
  },
  { immediate: true },
);

function submit() {
  if (props.saving) return;
  if (!draft.name.trim()) {
    localError.value = '请输入供应商名称。';
    return;
  }
  localError.value = '';
  emit('save', toProviderSaveInput(draft));
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="provider ? `编辑供应商：${provider.name}` : '新增模型供应商'"
    description="保存供应商连接信息。密钥不会回显；编辑时留空表示沿用已保存密钥。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <form @submit.prevent="submit">
      <UiFormField label="供应商名称">
        <UiInput
          v-model="draft.name"
          maxlength="128"
          required
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField label="协议">
        <UiSelect
          v-model="draft.providerType"
          :options="providerTypeOptions"
          label="协议"
        />
      </UiFormField>
      <UiFormField label="Endpoint">
        <UiInput
          v-model="draft.endpoint"
          maxlength="512"
          autocomplete="url"
          placeholder="https://模型网关地址"
        />
      </UiFormField>
      <UiFormField
        label="API 密钥"
        :hint="
          provider?.hasApiKey
            ? '已配置。留空保持现值；输入新密钥会替换现值。'
            : '密钥由后端加密保存，列表和回读不会返回明文。'
        "
      >
        <UiInput
          v-model="draft.apiKey"
          type="password"
          autocomplete="new-password"
          maxlength="2048"
          :placeholder="provider?.hasApiKey ? '已配置，留空保持现值' : '请输入 API 密钥'"
        />
      </UiFormField>
      <UiFormField label="组织标识">
        <UiInput
          v-model="draft.organizationId"
          maxlength="128"
          autocomplete="off"
        />
      </UiFormField>
      <UiFormField label="供应商状态">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用供应商"
        />
      </UiFormField>
      <UiFormField label="默认供应商">
        <UiSwitch
          v-model="draft.isDefault"
          label="设为默认"
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
        >{{ saving ? '保存中…' : '保存供应商' }}</UiButton
      >
    </template>
  </UiDialog>
</template>
