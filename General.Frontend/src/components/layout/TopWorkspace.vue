<script setup lang="ts">
import {
  Bell,
  ChevronDown,
  CircleHelp,
  Command,
  Database,
  Gift,
  GitFork,
  Grid3X3,
  LogOut,
  Megaphone,
  Menu,
  Network,
  Plus,
  Search,
  Settings,
  ShieldCheck,
  Sparkles,
  Users,
  X,
} from 'lucide-vue-next';
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { toast, UiCommand, UiTooltip, type UiCommandItem } from '@/components/ui';
import { exitTenantOperationApi } from '@/api/core/auth';
import { createPlatformNotificationsApi } from '@/api/core/platform-notification';
import { apiClient } from '@/api/request';
import PlatformNotificationCenter from './PlatformNotificationCenter.vue';
import { getAuthorizedHomePath, navigationState } from '@/navigation/menu-store';
import { logoutSession, replaceSessionFromOperation, sessionState } from '@/session/session-store';
import type { ResolvedMenuNode } from '@/navigation/page-registry';
import type { AppView } from '@/router/route.types';
import { routeNames } from '@/router/route.types';

defineProps<{ activeView: AppView }>();
const emit = defineEmits<{
  navigate: [view: AppView];
  toggleSidebar: [];
}>();

interface WorkspaceTab {
  affix: boolean;
  id: string;
  label: string;
  path: string;
  routeName: string;
  module: string;
}

const route = useRoute();
const router = useRouter();
const notificationsApi = createPlatformNotificationsApi(apiClient);
const workspaceTabs = ref<WorkspaceTab[]>([]);
const notificationUnreadCount = ref<number | null>(null);
const notificationUnreadBadge = computed(() => {
  if (notificationUnreadCount.value === null || notificationUnreadCount.value <= 0) return '';
  return notificationUnreadCount.value > 99 ? '99+' : String(notificationUnreadCount.value);
});
let notificationCountVersion = 0;
let notificationCountRequest: AbortController | undefined;
const authorizedPages = computed(() =>
  navigationState.pages.filter(
    (page): page is ResolvedMenuNode & { localPath: string; routeName: string } =>
      page.kind === 'page' && Boolean(page.localPath && page.routeName),
  ),
);
const searchCommands = computed<UiCommandItem[]>(() =>
  authorizedPages.value.map((page) => ({
    id: page.routeName,
    label: page.label,
    description: '已授权页面',
    group: '平台导航',
    keywords: [page.label, page.routeName],
  })),
);
const searchCommandTargets = computed(() => new Map(authorizedPages.value.map((page) => [page.routeName, page])));
const displayName = computed(() => sessionState.user?.realName?.trim() || sessionState.user?.username || '当前用户');
const avatarText = computed(() => Array.from(displayName.value)[0] ?? '当');
const operationTenantName = computed(
  () => sessionState.user?.operationTenantName?.trim() || sessionState.user?.operationTenantId || '目标租户',
);
const isTenantOperation = computed(() => Boolean(sessionState.user?.isHostTenantOperation));
const exitingTenantOperation = ref(false);
const activeWorkspaceId = ref('');
const preferredWorkspaceId = ref('');
const panel = ref('');
const searchQuery = ref('');
const workspaceScroller = ref<HTMLElement | null>(null);
const searchTrigger = ref<HTMLButtonElement | null>(null);
const authorizedNotificationPaths = computed(() => authorizedPages.value.map((page) => page.localPath));

async function refreshNotificationUnreadCount() {
  notificationCountRequest?.abort();
  const controller = new AbortController();
  notificationCountRequest = controller;
  const version = ++notificationCountVersion;
  try {
    const result = await notificationsApi.getUnreadCount(controller.signal);
    if (version === notificationCountVersion) {
      notificationUnreadCount.value = Number.isFinite(result.count) ? Math.max(0, result.count) : null;
    }
  } catch (error) {
    if (version === notificationCountVersion && !(error instanceof Error && error.name === 'AbortError')) {
      notificationUnreadCount.value = null;
    }
  }
}

function showPanel(name: string) {
  const opening = panel.value !== name;
  panel.value = opening ? name : '';
  if (name === '全局搜索' && opening) searchQuery.value = '';
  if (name === '通知中心' && opening) void refreshNotificationUnreadCount();
}

function notify(message: string) {
  toast.info(message);
}

function toWorkspaceTab(page: ResolvedMenuNode & { localPath: string; routeName: string }): WorkspaceTab {
  return {
    affix: page.affixTab,
    id: page.routeName,
    label: page.label,
    path: page.localPath,
    routeName: page.routeName,
    module: String(route.meta.module ?? '平台中心'),
  };
}

function syncAuthorizedTabs() {
  workspaceTabs.value = authorizedPages.value.filter((page) => page.affixTab).map(toWorkspaceTab);
  activeWorkspaceId.value = workspaceTabs.value[0]?.id ?? '';
  preferredWorkspaceId.value = activeWorkspaceId.value;
}

watch(() => navigationState.pages, syncAuthorizedTabs, { immediate: true });

function selectWorkspace(tab: WorkspaceTab) {
  preferredWorkspaceId.value = tab.id;
  activeWorkspaceId.value = tab.id;
  void router.push(tab.path);
}

function selectSearchCommand(item: UiCommandItem) {
  const page = searchCommandTargets.value.get(item.id);
  if (!page) return;
  panel.value = '';
  searchQuery.value = '';
  selectWorkspace(toWorkspaceTab(page));
}

function closePanel() {
  const returnToSearch = panel.value === '全局搜索';
  panel.value = '';
  if (returnToSearch) void nextTick(() => searchTrigger.value?.focus());
}

async function signOut() {
  try {
    await logoutSession();
  } catch {
    // logoutSession still clears local credentials when the API request fails.
  }
  panel.value = '';
  await router.replace({ name: routeNames.login });
}

async function exitTenantOperation() {
  if (!isTenantOperation.value || exitingTenantOperation.value) return;
  exitingTenantOperation.value = true;
  try {
    const result = await exitTenantOperationApi();
    const homePath = await replaceSessionFromOperation(result);
    if (!homePath) return;
    panel.value = '';
    await router.replace({ path: homePath });
    toast.success('已退出租户代操作。');
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '退出租户代操作失败，请重试。');
    if (!sessionState.user) await router.replace({ name: routeNames.login });
  } finally {
    exitingTenantOperation.value = false;
  }
}

function scrollActiveTabIntoView() {
  void nextTick(() => {
    const scroller = workspaceScroller.value;
    if (!scroller) return;
    const index = workspaceTabs.value.findIndex((tab) => tab.id === activeWorkspaceId.value);
    const tabElement = scroller.children.item(index);
    tabElement?.scrollIntoView({ block: 'nearest', inline: 'nearest' });
  });
}

function syncRouteTab() {
  const currentPage = authorizedPages.value.find((page) => page.routeName === String(route.name ?? ''));
  if (!currentPage) return;

  let existing = workspaceTabs.value.find((tab) => tab.routeName === currentPage.routeName);
  if (!existing) {
    existing = toWorkspaceTab(currentPage);
    workspaceTabs.value.push(existing);
  }
  activeWorkspaceId.value = existing.id;
  preferredWorkspaceId.value = activeWorkspaceId.value;
  scrollActiveTabIntoView();
}

function closeWorkspace(tab: WorkspaceTab) {
  if (tab.affix) return;
  const index = workspaceTabs.value.findIndex((item) => item.id === tab.id);
  if (index < 0) return;
  const wasActive = activeWorkspaceId.value === tab.id;
  workspaceTabs.value.splice(index, 1);
  if (!wasActive) return;

  const adjacent = workspaceTabs.value[index] ?? workspaceTabs.value[index - 1];
  if (!adjacent) {
    const homePath = getAuthorizedHomePath(sessionState.homePath);
    void router.replace(homePath ?? { name: routeNames.forbidden });
    return;
  }
  preferredWorkspaceId.value = adjacent.id;
  activeWorkspaceId.value = adjacent.id;
  void router.push(adjacent.path);
}

function workspaceTabSize(tab: string) {
  if (tab.length > 10) return 'tabWide';
  return 'tabDefault';
}

function openPanel(name: '全局搜索' | '快速新建' | '通知中心' | '项目中心') {
  if (name === '通知中心') {
    panel.value = name;
    void refreshNotificationUnreadCount();
    return;
  }
  if (name !== '全局搜索') return;
  panel.value = name;
  if (name === '全局搜索') searchQuery.value = '';
}

defineExpose({ openPanel, notify });

function refreshUnreadCountOnVisibility() {
  if (document.visibilityState === 'visible') void refreshNotificationUnreadCount();
}

onMounted(() => {
  void refreshNotificationUnreadCount();
  document.addEventListener('visibilitychange', refreshUnreadCountOnVisibility);
});

onBeforeUnmount(() => {
  notificationCountRequest?.abort();
  document.removeEventListener('visibilitychange', refreshUnreadCountOnVisibility);
});

watch(
  () => sessionState.contextVersion,
  () => {
    panel.value = '';
    void refreshNotificationUnreadCount();
  },
);

watch(() => [route.fullPath, route.meta.workspace, route.meta.pageLabel, route.meta.title] as const, syncRouteTab, {
  immediate: true,
});
</script>

<template>
  <div :class="$style.shell">
    <header :class="$style.toolbar">
      <button
        ref="searchTrigger"
        :class="$style.searchTrigger"
        @click="showPanel('全局搜索')"
      >
        <Search
          :size="15"
          :stroke-width="1.8"
        />
        <span>搜索...</span>
        <kbd
          ><Command
            :size="10"
            :stroke-width="1.8"
          />K</kbd
        >
      </button>

      <div :class="$style.toolbarActions">
        <button
          type="button"
          aria-label="通知中心"
          :aria-expanded="panel === '通知中心'"
          :class="[$style.iconButton, $style.notificationButton]"
          @click="showPanel('通知中心')"
        >
          <Bell
            :size="18"
            :stroke-width="1.8"
          />
          <span
            v-if="notificationUnreadBadge"
            :class="$style.notificationBadge"
          >
            {{ notificationUnreadBadge }}
          </span>
        </button>
        <button
          type="button"
          aria-label="账号菜单"
          :aria-expanded="panel === '账号菜单'"
          :class="$style.accountButton"
          @click="showPanel('账号菜单')"
        >
          <span :class="$style.accountAvatar">{{ avatarText }}</span>
          <span>{{ displayName }}</span>
          <ChevronDown
            :size="14"
            :stroke-width="1.8"
          />
        </button>
      </div>

      <div
        v-if="false"
        :class="$style.toolbarActions"
      >
        <button
          :class="$style.createButton"
          @click="showPanel('快速新建')"
        >
          <Plus
            :size="16"
            :stroke-width="1.8"
          />
          新建
          <ChevronDown
            :size="13"
            :stroke-width="1.8"
          />
        </button>
        <span :class="$style.divider" />
        <button
          :class="[$style.iconButton, $style.aiButton]"
          @click="showPanel('AI 助手')"
        >
          <Sparkles
            :size="18"
            :stroke-width="1.8"
          />
          <span :class="$style.newBadge">NEW</span>
        </button>
        <button
          :class="[$style.iconButton, $style.notificationButton]"
          @click="showPanel('通知中心')"
        >
          <Bell
            :size="18"
            :stroke-width="1.8"
          />
          <span
            v-if="notificationUnreadBadge"
            :class="$style.notificationBadge"
          >
            {{ notificationUnreadBadge }}
          </span>
        </button>
        <button
          :class="$style.communityButton"
          @click="showPanel('社区')"
        >
          <Users
            :size="16"
            :stroke-width="1.8"
          />
          社区
          <span :class="$style.communityBadge" />
        </button>
        <button
          :class="$style.subscribeButton"
          @click="showPanel('订阅方案')"
        >
          <Gift
            :size="15"
            :stroke-width="1.8"
          />
          立即订阅 <span :class="$style.trialDays">3 天</span>
          <span :class="$style.productUpdate">🎉 产品更新啦</span>
        </button>
        <button
          aria-label="产品公告"
          :class="[$style.squareButton, $style.announcementButton]"
          @click="showPanel('产品公告')"
        >
          <Megaphone
            :size="17"
            :stroke-width="1.8"
          />
        </button>
        <button
          aria-label="应用中心"
          :class="$style.squareButton"
          @click="showPanel('应用中心')"
        >
          <Grid3X3
            :size="17"
            :stroke-width="1.8"
          />
        </button>
        <button
          aria-label="流程中心"
          :class="[$style.iconButton, $style.workflowButton]"
          @click="showPanel('流程中心')"
        >
          <GitFork
            :size="18"
            :stroke-width="1.8"
          />
        </button>
        <button
          :class="$style.migrationButton"
          @click="showPanel('数据迁移')"
        >
          <Database
            :size="16"
            :stroke-width="1.8"
          />
          数据迁移
        </button>
        <button
          aria-label="组织架构"
          :class="$style.squareButton"
          @click="showPanel('组织架构')"
        >
          <Network
            :size="17"
            :stroke-width="1.8"
          />
        </button>
        <button
          aria-label="帮助中心"
          :class="$style.iconButton"
          @click="showPanel('帮助中心')"
        >
          <CircleHelp
            :size="18"
            :stroke-width="1.8"
          />
        </button>
        <UiTooltip content="系统设置">
          <button
            aria-label="系统设置"
            :class="$style.iconButton"
            @click="
              panel = '';
              emit('navigate', 'settings');
            "
          >
            <Settings
              :size="18"
              :stroke-width="1.8"
            />
          </button>
        </UiTooltip>
        <span :class="$style.divider" />
        <button
          :class="$style.accountButton"
          @click="showPanel('账号菜单')"
        >
          <span :class="$style.accountAvatar">张</span>
          <ChevronDown
            :size="14"
            :stroke-width="1.8"
          />
        </button>
      </div>

      <div :class="$style.mobileToolbar">
        <button
          aria-label="打开导航"
          :class="$style.mobileMenuButton"
          @click="emit('toggleSidebar')"
        >
          <Menu
            :size="25"
            :stroke-width="1.8"
          />
        </button>
        <div :class="$style.mobileBrand">
          <img
            src="/logo.png"
            alt="General Admin"
            :class="$style.mobileLogo"
          />
          <span>
            <strong>General Admin</strong>
            <small>项目管理平台</small>
          </span>
        </div>
        <button
          type="button"
          aria-label="通知中心"
          :aria-expanded="panel === '通知中心'"
          :class="[$style.mobileIconButton, $style.mobileNotificationButton]"
          @click="showPanel('通知中心')"
        >
          <Bell
            :size="21"
            :stroke-width="1.8"
          />
          <span
            v-if="notificationUnreadCount !== null && notificationUnreadCount > 0"
            :class="$style.notificationBadge"
          >
            {{ notificationUnreadCount > 99 ? '99+' : notificationUnreadCount }}
          </span>
        </button>
        <div
          v-if="false"
          :class="$style.mobileActions"
        >
          <button
            aria-label="全局搜索"
            :class="$style.mobileIconButton"
            @click="showPanel('全局搜索')"
          >
            <Search
              :size="25"
              :stroke-width="1.8"
            />
          </button>
          <button
            aria-label="快速新建"
            :class="$style.mobileCreateButton"
            @click="showPanel('快速新建')"
          >
            <Plus
              :size="27"
              :stroke-width="1.8"
            />
          </button>
          <button
            aria-label="AI 助手"
            :class="[$style.mobileIconButton, $style.mobileAiButton]"
            @click="showPanel('AI 助手')"
          >
            <Sparkles
              :size="23"
              :stroke-width="1.8"
            />
            <span :class="$style.mobileNewBadge">NEW</span>
          </button>
          <button
            aria-label="通知中心"
            :class="[$style.mobileIconButton, $style.mobileNotificationButton]"
            @click="showPanel('通知中心')"
          >
            <Bell
              :size="23"
              :stroke-width="1.8"
            />
            <span
              v-if="notificationUnreadBadge"
              :class="$style.notificationBadge"
            >
              {{ notificationUnreadBadge }}
            </span>
          </button>
          <button
            aria-label="账号菜单"
            :class="$style.mobileAccountButton"
            @click="showPanel('账号菜单')"
          >
            <span :class="$style.accountAvatar">{{ avatarText }}</span>
          </button>
        </div>
        <button
          type="button"
          aria-label="账号菜单"
          :aria-expanded="panel === '账号菜单'"
          :class="$style.mobileAccountButton"
          @click="showPanel('账号菜单')"
        >
          <span :class="$style.accountAvatar">{{ avatarText }}</span>
        </button>
      </div>
    </header>

    <div
      v-if="isTenantOperation"
      data-tenant-operation-context
      :class="$style.operationContext"
      role="status"
      aria-live="polite"
    >
      <div :class="$style.operationCopy">
        <ShieldCheck
          :size="16"
          :stroke-width="1.9"
        />
        <span>租户代操作中</span>
        <strong>{{ operationTenantName }}</strong>
      </div>
      <button
        type="button"
        :disabled="exitingTenantOperation"
        :aria-busy="exitingTenantOperation"
        @click="exitTenantOperation"
      >
        <LogOut
          :size="14"
          :stroke-width="1.9"
        />
        {{ exitingTenantOperation ? '正在退出…' : '退出代操作' }}
      </button>
    </div>

    <div
      ref="workspaceScroller"
      :class="$style.workspaceTabs"
    >
      <div
        v-for="tab in workspaceTabs"
        :key="tab.id"
        :class="[
          $style.workspaceTab,
          $style[workspaceTabSize(tab.label)],
          activeWorkspaceId === tab.id && $style.activeWorkspaceTab,
        ]"
      >
        <button
          type="button"
          :class="$style.workspaceTabTarget"
          :aria-current="activeWorkspaceId === tab.id ? 'page' : undefined"
          :title="tab.label"
          @click="selectWorkspace(tab)"
        >
          {{ tab.label }}
        </button>
        <button
          type="button"
          :aria-label="`关闭 ${tab.label}`"
          :tabindex="activeWorkspaceId === tab.id ? 0 : -1"
          v-if="!tab.affix"
          :class="$style.workspaceTabClose"
          @click="closeWorkspace(tab)"
        >
          <X
            :size="11"
            :stroke-width="1.8"
          />
        </button>
      </div>
    </div>

    <aside
      v-if="panel"
      data-workspace-panel
      :class="$style.floatingPanel"
    >
      <header :class="$style.panelHeader">
        <strong>{{ panel }}</strong
        ><button
          aria-label="关闭面板"
          @click="closePanel"
        >
          <X
            :size="14"
            :stroke-width="1.8"
          />
        </button>
      </header>
      <div v-if="panel === '全局搜索'">
        <UiCommand
          v-model="searchQuery"
          :items="searchCommands"
          label="全局搜索"
          input-label="搜索功能、单据、客户"
          placeholder="搜索功能、单据、客户..."
          empty-text="没有找到匹配的功能或工作区"
          hint="输入关键字开始搜索"
          @select="selectSearchCommand"
          @escape="closePanel"
        />
      </div>
      <PlatformNotificationCenter
        v-else-if="panel === '通知中心'"
        :authorized-paths="authorizedNotificationPaths"
        :unread-count="notificationUnreadCount"
        @changed="refreshNotificationUnreadCount"
        @close="closePanel"
      />
      <div
        v-else-if="panel === '账号菜单'"
        :class="$style.panelContent"
      >
        <p>{{ displayName }} · {{ sessionState.user?.username }}</p>
        <button
          type="button"
          @click="signOut"
        >
          退出登录
        </button>
      </div>
      <div
        v-else-if="panel === '快速新建'"
        :class="$style.createGrid"
      >
        <button
          v-for="item in ['商品', '询价单', '采购订单', '销售订单']"
          :key="item"
          @click="notify(`已选择新建${item}`)"
        >
          <Plus
            :size="13"
            :stroke-width="1.8"
          />{{ item }}
        </button>
      </div>
      <div
        v-else
        :class="$style.panelContent"
      >
        <p>「{{ panel }}」面板已打开。当前为商品管理复刻环境，保留壳层入口与交互状态。</p>
        <button
          @click="
            notify(`已进入${panel}`);
            panel = '';
          "
        >
          查看详情
        </button>
      </div>
    </aside>
  </div>
</template>

<style module>
@reference "../../styles/index.css";
.shell {
  @apply shrink-0;
}
.trialBanner {
  @apply flex h-[40px] items-center border-b pl-5 pr-4;
  color: var(--ui-warning);
  border-color: #ffe380;
  background: var(--ui-warning-soft);
}
.trialIcon {
  @apply mr-2;
}
.trialMessage {
  @apply font-semibold;
}
.expiryButton {
  @apply ml-auto rounded px-2.5 py-1 text-[11px] font-semibold text-white transition-colors;
  background: #ff7452;
}
.expiryButton:hover {
  background: #ff5630;
}
.planButton {
  @apply ml-2 flex w-[86.25px] items-center justify-center gap-1.5 rounded py-1 text-[12px] font-semibold text-white transition-colors;
  background: var(--ui-warning);
}
.planButton:hover {
  background: #ff991f;
}
.closeTrialButton {
  @apply ml-2 rounded p-0.5 transition-colors;
}
.closeTrialButton:hover {
  background: #fff0b3;
}
.toolbar {
  @apply flex h-[56px] items-center justify-between gap-2 border-b bg-white px-5;
  border-color: var(--ui-border);
}
.searchTrigger {
  @apply flex w-[228px] items-center gap-2 rounded px-3 py-2 transition-colors;
  color: #97a0af;
  background: var(--ui-surface-hover);
}
.searchTrigger:hover {
  background: var(--ui-border-subtle);
}
.searchTrigger > span {
  @apply flex-1 text-left text-[13px];
}
.searchTrigger kbd {
  @apply flex items-center gap-0.5 rounded border bg-white px-1.5 py-0.5 text-[10px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.toolbarActions {
  @apply flex items-center gap-1;
}
.operationContext {
  @apply flex min-h-10 items-center justify-between gap-3 border-b px-5 text-[12px];
  color: var(--ui-warning);
  border-color: var(--ui-warning-soft);
  background: var(--ui-warning-soft);
}
.operationCopy {
  @apply flex min-w-0 items-center gap-2;
}
.operationCopy strong {
  @apply truncate font-semibold;
  color: var(--ui-text);
}
.operationContext button {
  @apply flex shrink-0 items-center gap-1.5 rounded px-2.5 py-1.5 font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-60;
  color: var(--ui-text);
  background: white;
}
.operationContext button:hover:not(:disabled) {
  background: var(--ui-surface-hover);
}
.createButton {
  @apply flex h-[33.5px] w-[90.75px] items-center justify-center gap-2 rounded font-semibold text-white transition-colors active:scale-[.97];
  background: var(--app-primary);
}
.createButton:hover {
  background: color-mix(in srgb, var(--app-primary) 84%, black);
}
.divider {
  @apply mx-2 h-6 w-px;
  background: var(--ui-border);
}
.iconButton {
  @apply relative rounded p-2 transition-colors;
  color: var(--ui-text-muted);
}
.iconButton:hover,
.squareButton:hover {
  background: var(--ui-surface-hover);
}
.aiButton,
.workflowButton {
  color: var(--app-primary);
}
.newBadge {
  @apply absolute -top-2 left-0 rounded px-1 text-[8px] font-bold text-white;
  background: var(--ui-warning);
}
.notificationButton {
  color: var(--ui-text-subtle);
}
.notificationBadge {
  @apply absolute right-0 top-0 flex h-4 w-4 items-center justify-center rounded-full text-[9px] text-white;
  background: #ff5630;
}
.communityButton {
  @apply relative flex h-[28.5px] w-[62.75px] items-center justify-center gap-1.5 rounded font-semibold transition-colors;
  color: #5243aa;
  background: #eae6ff;
}
.communityButton:hover {
  background: #dfd8fd;
}
.communityBadge {
  @apply absolute -right-0.5 -top-0.5 h-2 w-2 rounded-full;
  background: #ff5630;
}
.subscribeButton {
  @apply relative flex h-[29px] w-[126.25px] items-center justify-center gap-1 rounded bg-gradient-to-r from-[#FF8B00] to-[#FF5630] font-semibold text-white shadow-md shadow-orange-200/70 active:scale-[.97];
}
.trialDays {
  @apply rounded bg-white/20 px-1.5 py-0.5 text-[10px];
}
.productUpdate {
  @apply absolute -top-5 left-0 rounded px-2 py-1 text-[10px] shadow;
  background: var(--ui-warning);
}
.squareButton {
  @apply flex h-[32px] w-[32px] items-center justify-center rounded transition-colors;
  color: var(--ui-text-muted);
}
.announcementButton {
  color: var(--ui-warning);
}
.announcementButton:hover {
  background: var(--ui-warning-soft);
}
.migrationButton {
  @apply flex h-[28.5px] w-[85.75px] items-center justify-center gap-1 rounded bg-gradient-to-r from-app-primary to-[#5243AA] font-semibold text-white;
}
.accountButton {
  @apply flex items-center gap-2 px-1 py-0.5;
  color: var(--ui-text-subtle);
}
.accountAvatar {
  @apply flex h-9 w-9 items-center justify-center rounded-full font-semibold text-white;
  background: var(--app-primary);
}
.workspaceTabs {
  @apply flex h-[36px] items-center gap-1 overflow-x-auto border-b bg-white px-1;
  border-color: var(--ui-border);
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.mobileToolbar {
  @apply hidden;
}
.workspaceTab {
  @apply flex h-[31px] shrink-0 translate-y-px items-center border text-[12px] font-medium transition-colors;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.workspaceTab:hover,
.activeWorkspaceTab {
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}
.workspaceTabTarget {
  @apply min-w-0 flex-1 truncate py-[6px] pl-2 text-left;
}
.workspaceTabClose {
  @apply mr-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded opacity-0 transition-colors pointer-events-none;
  color: var(--ui-text-subtle);
}
.workspaceTab:hover .workspaceTabClose,
.workspaceTab:focus-within .workspaceTabClose,
.activeWorkspaceTab .workspaceTabClose {
  @apply pointer-events-auto opacity-100;
}
.workspaceTabClose:hover {
  color: var(--ui-text);
  background: var(--ui-border-subtle);
}
.tabCalendar {
  @apply w-[145.9px];
}
.tabWide {
  @apply w-[117.8px];
}
.tabUser {
  @apply w-[102.3px];
}
.tabDefault {
  @apply w-[90.3px];
}
.floatingPanel {
  @apply fixed right-5 top-[92px] z-[150] w-[340px] overflow-hidden rounded-xl border bg-white shadow-2xl;
  border-color: var(--ui-border);
  animation: panelIn 150ms var(--ease-out-ui);
  transform-origin: top right;
}
.panelHeader {
  @apply flex items-center border-b px-4 py-3;
  border-color: var(--ui-border);
}
.panelHeader strong {
  @apply text-[14px];
}
.panelHeader button {
  @apply ml-auto rounded p-1;
  color: var(--ui-text-subtle);
}
.panelHeader button:hover {
  background: var(--ui-surface-hover);
}
.createGrid {
  @apply grid grid-cols-2 gap-2 p-4;
}
.createGrid button {
  @apply flex items-center rounded border px-3 py-3 text-left text-[12px];
  border-color: var(--ui-border);
}
.createGrid button:hover {
  background: var(--ui-surface-hover);
}
.createGrid svg {
  @apply mr-1;
  color: var(--app-primary);
}
.panelContent {
  @apply space-y-2 p-4;
}
.panelContent p {
  @apply text-[12px] leading-5;
  color: var(--ui-text-muted);
}
.panelContent button {
  @apply w-full rounded px-3 py-2 text-[12px] text-white;
  background: var(--app-primary);
}
@keyframes panelIn {
  from {
    opacity: 0;
    transform: scale(0.97) translateY(-2px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

@media (max-width: 480px) {
  .trialBanner {
    @apply h-[40px] px-3;
    color: white;
    background: linear-gradient(100deg, #ff5630, #ff8b00);
    border-color: transparent;
  }

  .trialIcon {
    @apply mr-2 shrink-0;
  }

  .trialMessage {
    @apply min-w-0 truncate text-[13px];
  }

  .expiryButton {
    @apply ml-2 shrink-0 px-2 py-1 text-[10px];
    background: rgb(255 255 255 / 0.22);
  }

  .planButton {
    @apply ml-1 shrink-0 w-auto px-2 py-1 text-[10px];
    background: rgb(255 255 255 / 0.22);
  }

  .closeTrialButton {
    @apply hidden;
  }

  .toolbar {
    @apply h-[51px] px-3;
  }

  .operationContext {
    @apply min-h-10 px-3;
  }

  .searchTrigger,
  .toolbarActions {
    @apply hidden;
  }

  .mobileToolbar {
    @apply flex min-w-0 w-full items-center justify-between gap-2;
  }

  .mobileMenuButton,
  .mobileIconButton,
  .mobileAccountButton {
    @apply relative flex shrink-0 items-center justify-center rounded;
    color: var(--ui-text-muted);
  }

  .mobileMenuButton {
    @apply h-9 w-9;
  }

  .mobileMenuButton:hover,
  .mobileIconButton:hover,
  .mobileAccountButton:hover {
    background: var(--ui-surface-hover);
  }

  .mobileBrand {
    @apply flex min-w-0 flex-1 items-center gap-2;
  }

  .mobileLogo {
    @apply h-10 w-10 shrink-0 rounded-lg object-contain;
  }

  .mobileBrand > span {
    @apply flex min-w-0 flex-col;
  }

  .mobileBrand strong {
    @apply text-[18px] leading-5;
    color: #101828;
  }

  .mobileBrand small {
    @apply text-[11px] leading-4;
    color: var(--ui-text-muted);
  }

  .mobileActions {
    @apply flex shrink-0 items-center gap-1;
  }

  .mobileIconButton {
    @apply h-9 w-9;
  }

  .mobileCreateButton {
    @apply flex h-9 w-9 shrink-0 items-center justify-center rounded text-white;
    background: var(--app-primary);
  }

  .mobileAiButton {
    color: var(--app-primary);
  }

  .mobileNotificationButton {
    color: var(--ui-text-subtle);
  }

  .mobileNewBadge {
    @apply absolute -right-1 -top-3 rounded bg-[#FF8B00] px-1 text-[8px] font-bold text-white;
  }

  .mobileAccountButton {
    @apply ml-1 h-9 w-9;
  }

  .mobileAccountButton .accountAvatar {
    @apply h-8 w-8 text-[13px];
  }

  .workspaceTabs {
    @apply h-[36px];
  }

  .floatingPanel {
    @apply left-3 right-3 top-[94px] w-auto;
  }
}
</style>
