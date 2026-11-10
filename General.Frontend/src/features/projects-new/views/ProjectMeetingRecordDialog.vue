<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { apiClient } from '@/api/request';
import { createProjectMeetingRecordApi, type ProjectApi } from '@/api/core/project';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTextarea } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { createPlatformFilesApi } from '@/features/platform/files/platform-files.api';

const props = defineProps<{
  detail: ProjectApi.Detail;
  open: boolean;
  projectId: string;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  saved: [];
}>();

const filesApi = createPlatformFilesApi(apiClient);
const form = reactive({
  decisions: '',
  facilitatorUserId: '',
  meetingTime: '',
  minutes: '',
  status: '草稿',
  topic: '',
});
const saving = ref(false);
const error = ref('');
const attachment = ref<File | null>(null);
const uploadedAttachmentId = ref<string | null>(null);

const facilitatorOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '请选择主持人' },
  ...props.detail.members
    .filter((member) => member.isActive)
    .map((member) => ({ value: member.userId, label: member.userName })),
]);
const statusOptions: UiSelectOption[] = [
  { value: '草稿', label: '草稿' },
  { value: '已确认', label: '已确认' },
  { value: '已归档', label: '已归档' },
];

watch(
  () => props.open,
  (open) => {
    if (!open) return;
    form.decisions = '';
    form.facilitatorUserId = '';
    form.meetingTime = '';
    form.minutes = '';
    form.status = '草稿';
    form.topic = '';
    attachment.value = null;
    uploadedAttachmentId.value = null;
    error.value = '';
  },
);

function updateOpen(value: boolean) {
  if (saving.value) return;
  emit('update:open', value);
}

function selectAttachment(event: Event) {
  const input = event.currentTarget;
  if (!(input instanceof HTMLInputElement)) return;
  attachment.value = input.files?.[0] ?? null;
  uploadedAttachmentId.value = null;
  error.value = '';
}

async function saveMeeting() {
  const topic = form.topic.trim();
  if (!topic) {
    error.value = '请填写会议主题。';
    return;
  }
  if (!form.facilitatorUserId) {
    error.value = facilitatorOptions.value.length === 1 ? '当前详情没有可选的在岗项目成员。' : '请选择主持人。';
    return;
  }

  saving.value = true;
  error.value = '';
  try {
    if (attachment.value && !uploadedAttachmentId.value) {
      const uploaded = await filesApi.upload({
        businessId: props.projectId,
        businessType: 'Project',
        category: 'Project/Meeting',
        file: attachment.value,
      });
      uploadedAttachmentId.value = uploaded.id;
    }

    const meetingTime = form.meetingTime.trim();
    await createProjectMeetingRecordApi(props.projectId, {
      actionItems: [],
      attachmentFileId: uploadedAttachmentId.value,
      decisions: form.decisions.trim(),
      facilitatorUserId: form.facilitatorUserId,
      meetingTime: meetingTime ? (meetingTime.length === 16 ? `${meetingTime}:00` : meetingTime) : null,
      minutes: form.minutes.trim(),
      participantUserIds: [],
      status: form.status,
      topic,
    });
    emit('update:open', false);
    emit('saved');
  } catch (cause) {
    error.value = cause instanceof Error ? cause.message : '会议记录创建失败，请重试。';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <UiDialog
    :open="open"
    title="新增会议记录"
    description="会议记录保存到当前项目；主持人和附件归属由服务端校验。"
    @update:open="updateOpen"
  >
    <form @submit.prevent="saveMeeting">
      <UiFormField
        label="会议主题"
        required
      >
        <UiInput
          v-model="form.topic"
          :disabled="saving"
          maxlength="128"
          required
        />
      </UiFormField>
      <UiFormField
        label="主持人"
        required
      >
        <UiSelect
          v-model="form.facilitatorUserId"
          label="主持人"
          :options="facilitatorOptions"
          :disabled="saving || facilitatorOptions.length === 1"
          required
        />
      </UiFormField>
      <UiFormField
        label="会议时间"
        hint="留空时由服务端记录当前时间。"
      >
        <UiInput
          v-model="form.meetingTime"
          type="datetime-local"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="记录状态">
        <UiSelect
          v-model="form.status"
          label="记录状态"
          :options="statusOptions"
          :disabled="saving"
        />
      </UiFormField>
      <UiFormField label="会议纪要">
        <UiTextarea
          v-model="form.minutes"
          :disabled="saving"
          maxlength="4000"
          rows="4"
        />
      </UiFormField>
      <UiFormField label="决议">
        <UiTextarea
          v-model="form.decisions"
          :disabled="saving"
          maxlength="2000"
          rows="3"
        />
      </UiFormField>
      <UiFormField label="附件">
        <input
          type="file"
          :disabled="saving"
          @change="selectAttachment"
        />
      </UiFormField>
      <p
        v-if="attachment"
        role="status"
      >
        已选择：{{ attachment.name }}
      </p>
      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p>
    </form>
    <template #footer>
      <UiButton
        type="button"
        variant="secondary"
        :disabled="saving"
        @click="updateOpen(false)"
      >
        取消
      </UiButton>
      <UiButton
        type="button"
        variant="primary"
        :loading="saving"
        @click="saveMeeting"
      >
        保存会议记录
      </UiButton>
    </template>
  </UiDialog>
</template>
