<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import type { ConfigApi } from '@/api/core/config';
import { UiButton, UiDialog, UiFormField, UiInput, UiTextarea } from '@/components/ui';
import { buildConfigSaveInput, canEditConfig, configDescription, configValue, isKnownSafeConfig } from './config.model';

const props = defineProps<{
  open: boolean;
  item: ConfigApi.Item | null;
  tenantScoped: boolean;
  canManage: boolean;
  saving: boolean;
  saveError: string;
}>();
const emit = defineEmits<{ 'update:open': [open: boolean]; submit: [value: string] }>();
const draft = ref('');
const validationError = ref('');
const editable = computed(() => Boolean(props.item && props.canManage && canEditConfig(props.item)));

watch(
  () => [props.open, props.item] as const,
  ([open, item]) => {
    draft.value = open && item && isKnownSafeConfig(item) ? item.value : '';
    validationError.value = '';
  },
  { immediate: true },
);

function submit() {
  if (!props.item || !editable.value || props.saving) return;
  try {
    buildConfigSaveInput(props.item, draft.value, props.tenantScoped);
    validationError.value = '';
    emit('submit', draft.value);
  } catch (error) {
    validationError.value = error instanceof Error ? error.message : '请检查配置值。';
  }
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="item?.name ?? '配置详情'"
    description="保存后重新读取配置以确认结果。"
    @update:open="!saving && emit('update:open', $event)"
  >
    <form
      v-if="item"
      :aria-busy="saving"
      @submit.prevent="submit"
    >
      <UiFormField label="配置编码"
        ><span>{{ item.code }}</span></UiFormField
      >
      <UiFormField label="保存范围"
        ><span>{{ tenantScoped ? '当前租户' : '全局' }}</span></UiFormField
      >
      <UiFormField label="默认值"
        ><span>{{ configValue(item, 'defaultValue') }}</span></UiFormField
      >
      <UiFormField label="说明"
        ><span>{{ configDescription(item) }}</span></UiFormField
      >
      <UiFormField
        v-if="editable"
        label="配置值"
        :required="item.valueType === 'number'"
        :error="validationError || saveError"
        :hint="item.valueType === 'number' ? '0至2147483647之间的整数。' : '最多512字符；空值按该配置的默认值处理。'"
      >
        <UiInput
          v-if="item.valueType === 'number'"
          v-model="draft"
          type="number"
          aria-label="配置值"
          min="0"
          max="2147483647"
          step="1"
          :disabled="saving"
        />
        <UiTextarea
          v-else
          v-model="draft"
          aria-label="配置值"
          maxlength="512"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField
        v-else
        label="当前值"
        hint="该配置只读或尚未接入安全编辑。"
        ><span>{{ configValue(item) }}</span></UiFormField
      >
    </form>
    <template #footer>
      <UiButton
        v-if="editable"
        variant="secondary"
        :disabled="saving"
        @click="draft = item?.defaultValue ?? ''"
        >填入默认值</UiButton
      >
      <UiButton
        variant="secondary"
        :disabled="saving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        v-if="editable"
        :disabled="saving"
        @click="submit"
        >{{ saving ? '保存并确认中…' : '保存' }}</UiButton
      >
    </template>
  </UiDialog>
</template>
