<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import type { OnlineUserApi } from '@/api/core/online-user';
import { UiButton, UiDialog, UiFormField, UiInput } from '@/components/ui';
import { deviceLabel, formatMonitoringDate } from './monitoring.model';

const props = defineProps<{
  open: boolean;
  item: OnlineUserApi.Item | null;
  canManage: boolean;
  ownAccount: boolean;
  busy: boolean;
  error: string;
}>();
const emit = defineEmits<{ 'update:open': [open: boolean]; revoke: [] }>();
const confirmation = ref('');
const canRevoke = computed(() => Boolean(props.item?.canForceLogout && props.canManage));
watch(
  () => [props.open, props.item?.userId],
  () => {
    confirmation.value = '';
  },
);
function updateOpen(open: boolean) {
  if (!props.busy) emit('update:open', open);
}
</script>

<template>
  <UiDialog
    :open="open"
    title="在线账号详情"
    description="强制退出作用于该账号所有已签发凭证，账号仍可重新登录。"
    @update:open="updateOpen"
  >
    <template v-if="item">
      <dl>
        <dt>账号</dt>
        <dd>{{ item.userName }}</dd>
        <dt>租户</dt>
        <dd>{{ item.currentTenantId ? item.tenantName || '租户名称未提供' : '宿主' }}</dd>
        <dt>设备</dt>
        <dd>{{ deviceLabel(item.device) }}</dd>
        <dt>客户端 IP</dt>
        <dd>{{ item.ipAddress || '未提供' }}</dd>
        <dt>浏览器信息</dt>
        <dd>{{ item.browser || '未提供' }}</dd>
        <dt>最近登录</dt>
        <dd>{{ formatMonitoringDate(item.signedInAt) }}</dd>
        <dt>最近访问</dt>
        <dd>{{ formatMonitoringDate(item.lastAccessedAt) }}</dd>
      </dl>
      <p v-if="ownAccount">这是当前账号。确认后当前页面也将退出登录。</p>
      <UiFormField
        v-if="canRevoke"
        label="输入账号以确认强制退出"
      >
        <UiInput
          v-model="confirmation"
          :disabled="busy"
          :maxlength="64"
          autocomplete="off"
          aria-label="输入账号以确认强制退出"
        />
      </UiFormField>
      <p v-else>该记录不允许强制退出。</p>
      <p
        v-if="error"
        role="alert"
      >
        {{ error }}
      </p>
    </template>
    <template #footer>
      <UiButton
        :disabled="busy"
        @click="updateOpen(false)"
        >取消</UiButton
      >
      <UiButton
        v-if="canRevoke"
        variant="danger"
        :loading="busy"
        :disabled="busy || confirmation.trim() !== item?.userName"
        @click="emit('revoke')"
        >确认强制退出</UiButton
      >
    </template>
  </UiDialog>
</template>
