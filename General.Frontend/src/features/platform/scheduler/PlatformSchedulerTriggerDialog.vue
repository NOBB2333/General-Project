<script setup lang="ts">
import { reactive, watch } from 'vue';
import type { PlatformSchedulerApi } from '@/api/core/platform-scheduler';
import { toast, UiButton, UiDialog, UiFormField, UiInput, UiSwitch, UiTextarea } from '@/components/ui';

const props = defineProps<{
  loading: boolean;
  open: boolean;
  trigger: null | PlatformSchedulerApi.Trigger;
}>();

const emit = defineEmits<{
  cancel: [];
  save: [input: PlatformSchedulerApi.TriggerSaveInput];
}>();

const draft = reactive<PlatformSchedulerApi.TriggerSaveInput>({
  cronExpression: '0 0 2 * * ?',
  description: '',
  isEnabled: false,
  title: '',
  triggerKey: '',
});

function resetDraft() {
  const trigger = props.trigger;
  draft.cronExpression = trigger?.cronExpression ?? '0 0 2 * * ?';
  draft.description = trigger?.description ?? '';
  draft.isEnabled = trigger?.isEnabled ?? false;
  draft.title = trigger?.title ?? '';
  draft.triggerKey = trigger?.triggerKey ?? '';
}

watch(
  () => props.open,
  (open) => {
    if (open) resetDraft();
  },
);

function save() {
  const input = {
    cronExpression: draft.cronExpression.trim(),
    description: draft.description.trim(),
    isEnabled: draft.isEnabled,
    title: draft.title.trim(),
    triggerKey: draft.triggerKey.trim(),
  };
  if (!input.triggerKey || !input.title || !input.cronExpression) {
    toast.error('触发器键、名称和 Cron 表达式均为必填项。');
    return;
  }
  emit('save', input);
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="trigger ? '编辑触发器' : '新增触发器'"
    description="触发器启停与任务主开关分别维护；Cron 表达式按 Quartz 语法校验。"
    layer="top"
    @update:open="(value) => !value && emit('cancel')"
  >
    <form
      :class="$style.form"
      @submit.prevent="save"
    >
      <UiFormField
        label="触发器键"
        required
      >
        <UiInput
          v-model="draft.triggerKey"
          :disabled="Boolean(trigger)"
          maxlength="64"
          required
          autocomplete="off"
          placeholder="例如 morning-check"
        />
      </UiFormField>
      <UiFormField
        label="触发器名称"
        required
      >
        <UiInput
          v-model="draft.title"
          required
          maxlength="128"
          placeholder="请输入触发器名称"
        />
      </UiFormField>
      <UiFormField
        label="Cron 表达式"
        required
      >
        <UiInput
          v-model="draft.cronExpression"
          required
          autocomplete="off"
          placeholder="0 0 2 * * ?"
        />
      </UiFormField>
      <UiFormField label="说明">
        <UiTextarea
          v-model="draft.description"
          rows="3"
          placeholder="说明触发时间和用途"
        />
      </UiFormField>
      <UiFormField label="启用此触发器">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用此触发器"
          :disabled="loading"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton
        :disabled="loading"
        @click="emit('cancel')"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        :loading="loading"
        :disabled="loading"
        @click="save"
        >保存触发器</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply grid gap-4 p-5;
}
</style>
