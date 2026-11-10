<script setup lang="ts">
import { reactive, watch } from 'vue';
import type { PlatformSchedulerApi } from '@/api/core/platform-scheduler';
import { toast, UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiSwitch, UiTextarea } from '@/components/ui';

const props = defineProps<{
  handlers: PlatformSchedulerApi.Handler[];
  job: null | PlatformSchedulerApi.Job;
  loading: boolean;
  open: boolean;
}>();

const emit = defineEmits<{
  cancel: [];
  save: [input: PlatformSchedulerApi.JobSaveInput];
}>();

const draft = reactive<PlatformSchedulerApi.JobSaveInput>({
  cronExpression: '0 0 2 * * ?',
  description: '',
  handlerKey: '',
  isEnabled: false,
  jobKey: '',
  title: '',
});

function resetDraft() {
  const job = props.job;
  draft.cronExpression = job?.cronExpression ?? '0 0 2 * * ?';
  draft.description = job?.description ?? '';
  draft.handlerKey = job?.handlerKey ?? props.handlers[0]?.handlerKey ?? '';
  draft.isEnabled = job?.isEnabled ?? false;
  draft.jobKey = job?.jobKey ?? '';
  draft.title = job?.title ?? '';
}

watch(
  () => props.open,
  (open) => {
    if (open) resetDraft();
  },
);
watch(
  () => props.handlers,
  () => {
    if (!props.job && !draft.handlerKey) draft.handlerKey = props.handlers[0]?.handlerKey ?? '';
  },
  { deep: true },
);

function save() {
  const input = {
    cronExpression: draft.cronExpression.trim(),
    description: draft.description.trim(),
    handlerKey: draft.handlerKey.trim(),
    isEnabled: draft.isEnabled,
    jobKey: draft.jobKey.trim(),
    title: draft.title.trim(),
  };
  if (!input.jobKey || !input.title || !input.handlerKey || !input.cronExpression) {
    toast.error('任务键、名称、处理器和 Cron 表达式均为必填项。');
    return;
  }
  emit('save', input);
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="job ? '编辑定时任务' : '新建定时任务'"
    description="使用平台已注册的处理器；Cron 表达式按 Quartz 语法校验。新任务默认停用。"
    @update:open="(value) => !value && emit('cancel')"
  >
    <form
      :class="$style.form"
      @submit.prevent="save"
    >
      <UiFormField
        label="任务键"
        required
      >
        <UiInput
          v-model="draft.jobKey"
          :disabled="Boolean(job)"
          maxlength="64"
          required
          autocomplete="off"
          placeholder="例如 tenant-health-check"
        />
      </UiFormField>
      <UiFormField
        label="任务名称"
        required
      >
        <UiInput
          v-model="draft.title"
          required
          maxlength="128"
          placeholder="请输入任务名称"
        />
      </UiFormField>
      <UiFormField
        label="执行处理器"
        required
      >
        <UiSelect
          v-model="draft.handlerKey"
          label="执行处理器"
          :options="handlers.map((item) => ({ value: item.handlerKey, label: item.handlerKey }))"
          :disabled="handlers.length === 0"
          required
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
          placeholder="说明任务用途和影响范围"
        />
      </UiFormField>
      <UiFormField label="启用自动调度">
        <UiSwitch
          v-model="draft.isEnabled"
          label="启用自动调度"
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
        :disabled="loading || handlers.length === 0"
        @click="save"
        >保存任务</UiButton
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
