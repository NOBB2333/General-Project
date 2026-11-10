<script setup lang="ts">
import { Check, CheckCheck, ChevronRight, RotateCw, Trash2 } from 'lucide-vue-next';
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { createPlatformNotificationsApi, type PlatformNotificationApi } from '@/api/core/platform-notification';
import { apiClient } from '@/api/request';
import { toast, UiButton, UiDialog } from '@/components/ui';
import { formatNotificationDate, resolveAuthorizedNotificationPath } from './notification-center.model';

const props = defineProps<{ authorizedPaths: readonly string[]; unreadCount: number | null }>();
const emit = defineEmits<{ changed: []; close: [] }>();

const router = useRouter();
const notificationsApi = createPlatformNotificationsApi(apiClient);
const pageSize = 20;
const notifications = ref<PlatformNotificationApi.Item[]>([]);
const onlyUnread = ref(false);
const loading = ref(false);
const loadError = ref('');
const hasMore = ref(false);
const pendingIds = ref(new Set<string>());
const markingAllRead = ref(false);
const allReadConfirmed = ref(false);
const deleteTarget = ref<PlatformNotificationApi.Item | null>(null);
const deleteDialogOpen = ref(false);
const deleting = ref(false);
const safePaths = computed(() => notifications.value.map((item) => resolvePath(item.link)));

watch(
  () => props.unreadCount,
  (count) => {
    allReadConfirmed.value = count === 0;
  },
);

let requestVersion = 0;
let activeRequest: AbortController | undefined;

function resolvePath(link: string): string | null {
  return resolveAuthorizedNotificationPath(link, props.authorizedPaths);
}

function updatePending(id: string, isPending: boolean) {
  const updated = new Set(pendingIds.value);
  if (isPending) updated.add(id);
  else updated.delete(id);
  pendingIds.value = updated;
}

async function loadNotifications(reset = false) {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const version = ++requestVersion;
  const skipCount = reset ? 0 : notifications.value.length;
  loading.value = true;
  loadError.value = '';

  try {
    const result = await notificationsApi.getList(
      { maxResultCount: pageSize, onlyUnread: onlyUnread.value, skipCount },
      controller.signal,
    );
    if (version !== requestVersion) return;
    notifications.value = reset ? result : [...notifications.value, ...result];
    hasMore.value = result.length === pageSize;
  } catch (error) {
    if (version !== requestVersion || (error instanceof Error && error.name === 'AbortError')) return;
    loadError.value = error instanceof Error ? error.message : '通知加载失败，请重试。';
    toast.error(loadError.value);
  } finally {
    if (version === requestVersion) loading.value = false;
  }
}

function setFilter(value: boolean) {
  if (onlyUnread.value === value) return;
  onlyUnread.value = value;
  notifications.value = [];
  hasMore.value = false;
  void loadNotifications(true);
}

async function markRead(item: PlatformNotificationApi.Item): Promise<boolean> {
  if (item.isRead || pendingIds.value.has(item.id)) return false;
  updatePending(item.id, true);
  try {
    await notificationsApi.markRead(item.id);
    if (onlyUnread.value) {
      notifications.value = notifications.value.filter((current) => current.id !== item.id);
    } else {
      notifications.value = notifications.value.map((current) =>
        current.id === item.id ? { ...current, isRead: true } : current,
      );
    }
    if (props.unreadCount === 1) allReadConfirmed.value = true;
    emit('changed');
    return true;
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '标记已读失败，请重试。');
    return false;
  } finally {
    updatePending(item.id, false);
  }
}

async function markAllRead() {
  if (markingAllRead.value || allReadConfirmed.value || props.unreadCount === 0) return;
  markingAllRead.value = true;
  try {
    await notificationsApi.markAllRead();
    notifications.value = onlyUnread.value ? [] : notifications.value.map((item) => ({ ...item, isRead: true }));
    allReadConfirmed.value = true;
    emit('changed');
    toast.success('全部通知已标为已读。');
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '全部标为已读失败，请重试。');
  } finally {
    markingAllRead.value = false;
  }
}

async function openNotification(item: PlatformNotificationApi.Item) {
  if (pendingIds.value.has(item.id)) return;
  const path = resolvePath(item.link);
  if (!item.isRead) await markRead(item);
  if (!path) {
    if (item.link) toast.info('该通知关联的页面当前未授权或尚未接入。');
    return;
  }
  emit('close');
  await router.push(path);
}

function requestDelete(item: PlatformNotificationApi.Item) {
  deleteTarget.value = item;
  deleteDialogOpen.value = true;
}

async function confirmDelete() {
  const target = deleteTarget.value;
  if (!target || deleting.value) return;
  deleting.value = true;
  try {
    await notificationsApi.remove(target.id);
    notifications.value = notifications.value.filter((item) => item.id !== target.id);
    if (!target.isRead && props.unreadCount === 1) allReadConfirmed.value = true;
    deleteDialogOpen.value = false;
    deleteTarget.value = null;
    emit('changed');
    toast.success('通知已从本人列表移除。');
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '删除通知失败，请重试。');
  } finally {
    deleting.value = false;
  }
}

function retryLoad() {
  void loadNotifications(true);
}

onMounted(() => void loadNotifications(true));
onBeforeUnmount(() => activeRequest?.abort());
</script>

<template>
  <section :class="$style.center">
    <div :class="$style.toolbar">
      <button
        type="button"
        :class="[$style.filterButton, !onlyUnread && $style.activeFilter]"
        :aria-pressed="!onlyUnread"
        @click="setFilter(false)"
      >
        全部
      </button>
      <button
        type="button"
        :class="[$style.filterButton, onlyUnread && $style.activeFilter]"
        :aria-pressed="onlyUnread"
        @click="setFilter(true)"
      >
        未读
      </button>
      <UiButton
        :class="$style.readAllButton"
        variant="ghost"
        size="sm"
        :disabled="markingAllRead || allReadConfirmed || props.unreadCount === 0"
        @click="markAllRead"
      >
        <CheckCheck :size="14" />
        全部已读
      </UiButton>
      <UiButton
        :class="$style.iconButton"
        variant="ghost"
        size="icon-sm"
        aria-label="刷新通知"
        title="刷新通知"
        :disabled="loading"
        @click="retryLoad"
      >
        <RotateCw :size="14" />
      </UiButton>
    </div>

    <div :class="$style.listArea">
      <p
        v-if="loadError && notifications.length === 0"
        :class="$style.errorMessage"
        role="alert"
      >
        {{ loadError }}
      </p>
      <p
        v-else-if="loading && notifications.length === 0"
        :class="$style.stateMessage"
        role="status"
      >
        正在加载通知…
      </p>
      <p
        v-else-if="notifications.length === 0"
        :class="[$style.stateMessage, $style.emptyMessage]"
      >
        {{ onlyUnread ? '暂无未读通知。' : '暂无通知。' }}
      </p>

      <ul
        v-else
        :class="$style.notificationList"
      >
        <li
          v-for="(item, index) in notifications"
          :key="item.id"
          :class="[$style.notificationItem, !item.isRead && $style.unreadItem]"
        >
          <span :class="$style.avatar">
            {{ item.avatarText || '知' }}
          </span>
          <div :class="$style.notificationCopy">
            <button
              v-if="safePaths[index]"
              type="button"
              :class="$style.titleButton"
              :disabled="pendingIds.has(item.id)"
              @click="openNotification(item)"
            >
              {{ item.title || '系统通知' }}
            </button>
            <p
              v-else
              :class="$style.titleText"
            >
              {{ item.title || '系统通知' }}
            </p>
            <p :class="$style.message">
              {{ item.message }}
            </p>
            <div :class="$style.metadata">
              <time>{{ formatNotificationDate(item.creationTime, item.date) }}</time>
              <span
                v-if="item.link && !safePaths[index]"
                :class="$style.unavailable"
                >· 关联页面未授权或尚未接入</span
              >
            </div>
          </div>
          <div :class="$style.itemActions">
            <UiButton
              v-if="!item.isRead"
              :class="$style.iconButton"
              variant="ghost"
              size="icon-sm"
              :aria-label="`将${item.title || '通知'}标为已读`"
              title="标为已读"
              :disabled="pendingIds.has(item.id)"
              @click="markRead(item)"
            >
              <Check :size="14" />
            </UiButton>
            <UiButton
              :class="$style.iconButton"
              variant="ghost"
              size="icon-sm"
              :aria-label="`删除${item.title || '通知'}`"
              title="删除"
              :disabled="deleting"
              @click="requestDelete(item)"
            >
              <Trash2 :size="14" />
            </UiButton>
          </div>
        </li>
      </ul>

      <div
        v-if="loadError && notifications.length > 0"
        :class="$style.inlineError"
        role="alert"
      >
        {{ loadError }}
      </div>
      <div
        v-if="hasMore || loading"
        :class="$style.loadMoreArea"
      >
        <UiButton
          v-if="hasMore"
          :class="$style.loadMoreButton"
          variant="ghost"
          size="sm"
          :disabled="loading"
          @click="loadNotifications()"
        >
          {{ loading ? '加载中…' : '加载更多' }}
          <ChevronRight :size="13" />
        </UiButton>
        <span
          v-else
          :class="$style.loadingMore"
          role="status"
          >正在加载…</span
        >
      </div>
    </div>

    <UiDialog
      v-model:open="deleteDialogOpen"
      title="删除本人通知"
      description="删除后，这条通知将从你的通知列表中移除，不影响其他收件人。"
      size="sm"
      @close="deleteTarget = null"
    >
      <template #footer="{ close }">
        <UiButton
          :class="$style.dialogCancel"
          variant="ghost"
          :disabled="deleting"
          @click="close"
          >取消</UiButton
        >
        <UiButton
          :class="$style.dialogConfirm"
          :disabled="deleting"
          @click="confirmDelete"
        >
          {{ deleting ? '删除中…' : '确认删除' }}
        </UiButton>
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../styles/index.css";

.center {
  @apply w-full;
}

.toolbar {
  @apply flex items-center gap-1 border-b px-3 py-2;
  border-color: var(--ui-border);
}

.filterButton {
  @apply rounded px-2.5 py-1.5 text-[12px] font-medium transition-colors;
  color: var(--ui-text-muted);
}

.filterButton:hover:not(.activeFilter) {
  background: var(--ui-surface-hover);
}

.activeFilter {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.readAllButton {
  @apply ml-auto;
}

.iconButton {
  @apply shrink-0;
}

.listArea {
  max-height: min(68vh, 620px);
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.listArea::-webkit-scrollbar {
  width: 6px;
}

.listArea::-webkit-scrollbar-thumb {
  background: #c1c7d0;
  border-radius: 999px;
}

.stateMessage,
.errorMessage {
  @apply px-4 py-5 text-center text-[12px];
  color: var(--ui-text-muted);
}

.errorMessage,
.inlineError {
  color: var(--ui-danger);
}

.emptyMessage {
  @apply py-8;
}

.notificationList {
  @apply divide-y;
}

.notificationList > li {
  border-color: var(--ui-border-subtle);
}

.notificationItem {
  @apply flex gap-2.5 px-3 py-3 transition-colors;
}

.notificationItem:hover {
  background: var(--ui-surface-subtle);
}

.unreadItem {
  background: color-mix(in srgb, var(--app-primary-soft) 40%, white);
}

.avatar {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-[12px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.notificationCopy {
  @apply min-w-0 flex-1;
}

.titleButton,
.titleText {
  @apply block max-w-full truncate text-left text-[12px] font-semibold;
  color: var(--ui-text);
}

.titleButton:hover {
  color: var(--app-primary);
}

.message {
  @apply mt-1 line-clamp-2 whitespace-pre-wrap break-words text-[11px] leading-5;
  color: var(--ui-text-muted);
}

.metadata {
  @apply mt-1 flex flex-wrap items-center gap-1.5 text-[10px];
  color: var(--ui-text-subtle);
}

.unavailable {
  color: var(--ui-danger);
}

.itemActions {
  @apply flex shrink-0 items-start gap-0.5;
}

.inlineError,
.loadMoreArea {
  @apply border-t px-3 py-2;
  border-color: var(--ui-border);
}

.inlineError {
  @apply text-[11px];
}

.loadMoreArea {
  @apply text-center;
}

.loadMoreButton {
  @apply mx-auto;
}

.loadingMore {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}

.dialogCancel,
.dialogConfirm {
  @apply min-w-20;
}
</style>
