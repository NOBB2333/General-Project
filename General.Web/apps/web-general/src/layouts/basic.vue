<script lang="ts" setup>
import type { NotificationItem } from '@vben/layouts';

import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';

import { AuthenticationLoginExpiredModal } from '@vben/common-ui';
import { VBEN_DOC_URL, VBEN_GITHUB_URL } from '@vben/constants';
import { useWatermark } from '@vben/hooks';
import { BookOpenText, CircleHelp, SvgGithubIcon } from '@vben/icons';
import {
  BasicLayout,
  LockScreen,
  Notification,
  UserDropdown,
} from '@vben/layouts';
import { preferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';
import { openWindow } from '@vben/utils';

import { Button } from 'ant-design-vue';

import {
  clearNotificationsApi,
  getNotificationListApi,
  getNotificationUnreadCountApi,
  markAllNotificationsReadApi,
  markNotificationReadApi,
  removeNotificationApi,
} from '#/api/core';
import { $t } from '#/locales';
import { useAuthStore } from '#/store';
import LoginForm from '#/views/_core/authentication/login.vue';

const notifications = ref<NotificationItem[]>([]);
const unreadCount = ref(0);
let unreadCountTimer: ReturnType<typeof setInterval> | undefined;

const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const accessStore = useAccessStore();
const { destroyWatermark, updateWatermark } = useWatermark();
const showDot = computed(() =>
  unreadCount.value > 0 || notifications.value.some((item) => !item.isRead),
);

const isHostTenantOperation = computed(
  () => userStore.userInfo?.isHostTenantOperation === true,
);

const menus = computed(() => {
  const items = [
    {
      handler: () => {
        router.push({ name: 'Profile' });
      },
      icon: 'lucide:user',
      text: $t('page.auth.profile'),
    },
    {
      handler: () => {
        openWindow(VBEN_DOC_URL, {
          target: '_blank',
        });
      },
      icon: BookOpenText,
      text: $t('ui.widgets.document'),
    },
    {
      handler: () => {
        openWindow(VBEN_GITHUB_URL, {
          target: '_blank',
        });
      },
      icon: SvgGithubIcon,
      text: 'GitHub',
    },
    {
      handler: () => {
        openWindow(`${VBEN_GITHUB_URL}/issues`, {
          target: '_blank',
        });
      },
      icon: CircleHelp,
      text: $t('ui.widgets.qa'),
    },
  ];

  if (isHostTenantOperation.value) {
    items.unshift({
      handler: () => {
        void handleExitTenantOperation();
      },
      icon: 'lucide:log-out',
      text: '退出租户运维',
    });
  }

  return items;
});

const avatar = computed(() => {
  return userStore.userInfo?.avatar ?? preferences.app.defaultAvatar;
});

async function handleLogout() {
  await authStore.logout(false);
}

async function handleExitTenantOperation() {
  await authStore.exitTenantOperation();
  await router.replace('/platform/tenants');
}

async function handleNoticeClear() {
  await clearNotificationsApi();
  notifications.value = [];
  unreadCount.value = 0;
}

async function markRead(id: number | string) {
  await markNotificationReadApi(String(id));
  const item = notifications.value.find((item) => item.id === id);
  if (item && !item.isRead) {
    item.isRead = true;
    unreadCount.value = Math.max(0, unreadCount.value - 1);
  }
}

async function remove(id: number | string) {
  await removeNotificationApi(String(id));
  const item = notifications.value.find((item) => item.id === id);
  if (item && !item.isRead) {
    unreadCount.value = Math.max(0, unreadCount.value - 1);
  }
  notifications.value = notifications.value.filter((item) => item.id !== id);
}

async function handleMakeAll() {
  await markAllNotificationsReadApi();
  notifications.value.forEach((item) => (item.isRead = true));
  unreadCount.value = 0;
}

async function loadNotifications() {
  const [items, unread] = await Promise.all([
    getNotificationListApi({ maxResultCount: 20 }),
    getNotificationUnreadCountApi(),
  ]);
  notifications.value = items;
  unreadCount.value = unread.count;
}

async function refreshUnreadCount() {
  if (document.visibilityState !== 'visible') {
    return;
  }

  const unread = await getNotificationUnreadCountApi();
  unreadCount.value = unread.count;
}

onMounted(() => {
  void loadNotifications();
  unreadCountTimer = setInterval(() => {
    void refreshUnreadCount();
  }, 60_000);
});

onBeforeUnmount(() => {
  if (unreadCountTimer) {
    clearInterval(unreadCountTimer);
  }
});
watch(
  () => ({
    enable: preferences.app.watermark,
    content: preferences.app.watermarkContent,
  }),
  async ({ enable, content }) => {
    if (enable) {
      await updateWatermark({
        content:
          content ||
          `${userStore.userInfo?.username} - ${userStore.userInfo?.realName}`,
      });
    } else {
      destroyWatermark();
    }
  },
  {
    immediate: true,
  },
);
</script>

<template>
  <BasicLayout @clear-preferences-and-logout="handleLogout">
    <template #header-right-120>
      <div v-if="isHostTenantOperation" class="tenant-operation-bar">
        <span class="tenant-operation-text">
          运维：{{ userStore.userInfo?.operationTenantName || '-' }}
        </span>
        <Button size="small" type="link" @click="handleExitTenantOperation">
          退出
        </Button>
      </div>
    </template>
    <template #user-dropdown>
      <UserDropdown
        :avatar
        :menus
        :text="userStore.userInfo?.realName"
        description="ann.vben@gmail.com"
        tag-text="Pro"
        @logout="handleLogout"
      />
    </template>
    <template #notification>
      <Notification
        :dot="showDot"
        :notifications="notifications"
        @clear="handleNoticeClear"
        @read="(item) => item.id && void markRead(item.id)"
        @remove="(item) => item.id && void remove(item.id)"
        @make-all="handleMakeAll"
      />
    </template>
    <template #extra>
      <AuthenticationLoginExpiredModal
        v-model:open="accessStore.loginExpired"
        :avatar
      >
        <LoginForm />
      </AuthenticationLoginExpiredModal>
    </template>
    <template #lock-screen>
      <LockScreen :avatar @to-login="handleLogout" />
    </template>
  </BasicLayout>
</template>

<style scoped>
.tenant-operation-bar {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  max-width: 240px;
  padding: 2px 10px;
  border: 1px solid hsl(var(--border));
  border-radius: 6px;
  color: hsl(var(--foreground));
  background: hsl(var(--muted));
  font-size: 13px;
}

.tenant-operation-text {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

@media (max-width: 640px) {
  .tenant-operation-bar {
    max-width: 132px;
    gap: 4px;
    padding-inline: 6px;
  }
}
</style>
