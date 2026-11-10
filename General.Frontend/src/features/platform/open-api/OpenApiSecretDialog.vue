<script setup lang="ts">
import { UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';

defineProps<{
  appId: string;
  appSecret: string;
  open: boolean;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  copy: [];
}>();
</script>

<template>
  <UiDialog
    :open="open"
    title="开放接口密钥（仅展示一次）"
    description="请立即将密钥保存到受控密码库；关闭后无法再次查看。"
    size="lg"
    @update:open="emit('update:open', $event)"
  >
    <div :class="$style.secretContent">
      <p role="alert">密钥仅在本次创建或轮换响应中显示。不要将其粘贴到工单、截图或普通聊天中。</p>
      <UiFormField label="AppId">
        <UiInput
          :model-value="appId"
          readonly
          aria-label="开放接口 AppId"
        />
      </UiFormField>
      <UiFormField label="AppSecret">
        <UiInput
          :model-value="appSecret"
          readonly
          aria-label="仅展示一次的开放接口密钥"
          autocomplete="off"
        />
      </UiFormField>
    </div>
    <template #footer>
      <UiButton @click="emit('copy')">复制密钥</UiButton>
      <UiButton
        variant="primary"
        @click="emit('update:open', false)"
        >我已安全保存，关闭</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.secretContent {
  @apply space-y-4 p-5;
}
</style>
