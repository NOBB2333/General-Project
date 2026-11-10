<script setup lang="ts">
import { ref, watch } from 'vue';
import { UiButton, UiDialog, UiInput } from '@/components/ui';
import type { UserApi } from '@/api/core/user';

const props = defineProps<{
  error: string;
  open: boolean;
  saving: boolean;
  user: UserApi.ListItem | null;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  submit: [password: string];
}>();

const newPassword = ref('');
const confirmPassword = ref('');
const validationError = ref('');

function resetForm() {
  newPassword.value = '';
  confirmPassword.value = '';
  validationError.value = '';
}

function submitForm() {
  if (props.saving) return;
  validationError.value = '';
  if (!newPassword.value.trim() || newPassword.value.length > 128) {
    validationError.value = '新密码必填，且不能超过 128 个字符。';
    return;
  }
  if (newPassword.value !== confirmPassword.value) {
    validationError.value = '两次输入的新密码不一致。';
    return;
  }
  emit('submit', newPassword.value);
}

function updateOpen(value: boolean) {
  if (!value && props.saving) return;
  emit('update:open', value);
}

watch(
  () => props.open,
  (open) => {
    if (!open) resetForm();
  },
);
</script>

<template>
  <UiDialog
    :open="open"
    title="重置用户密码"
    description="新密码立即生效，用户下次登录时需要使用新密码。"
    size="md"
    @update:open="updateOpen"
  >
    <form
      id="platform-user-reset-password-form"
      :class="$style.form"
      @submit.prevent="submitForm"
    >
      <p :class="$style.target">
        账号：<strong>{{ user?.username ?? '—' }}</strong>
      </p>
      <label :class="$style.field">
        <span>新密码</span>
        <UiInput
          v-model="newPassword"
          type="password"
          autocomplete="new-password"
          maxlength="128"
          aria-label="新密码"
          :disabled="saving"
        />
      </label>
      <label :class="$style.field">
        <span>确认新密码</span>
        <UiInput
          v-model="confirmPassword"
          type="password"
          autocomplete="new-password"
          maxlength="128"
          aria-label="确认新密码"
          :disabled="saving"
        />
      </label>
      <p
        v-if="validationError"
        :class="$style.error"
        role="alert"
      >
        {{ validationError }}
      </p>
      <p
        v-if="error"
        :class="$style.error"
        role="alert"
      >
        {{ error }}
      </p>
    </form>
    <template #footer>
      <UiButton
        :disabled="saving"
        @click="emit('update:open', false)"
        >取消</UiButton
      >
      <UiButton
        type="submit"
        form="platform-user-reset-password-form"
        variant="primary"
        :loading="saving"
        >确认重置</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.form {
  @apply space-y-4 p-5;
}
.target {
  @apply m-0 border-b pb-3 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.target strong {
  color: var(--ui-text);
}
.field {
  @apply flex flex-col gap-2 text-[12px] font-medium;
  color: var(--ui-text-muted);
}
.error {
  @apply m-0 border px-3 py-2 text-[12px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
  border-color: color-mix(in srgb, var(--ui-danger) 25%, transparent);
  border-radius: var(--app-control-radius);
}
</style>
