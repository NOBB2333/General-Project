<script setup lang="ts">
import { ChevronDown, ChevronRight, Folder, LayoutDashboard, Settings2 } from 'lucide-vue-next';
import { computed, reactive, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import type { Component } from 'vue';
import { changeCurrentPasswordApi } from '@/api/core/auth';
import { UiButton, UiCollapsible, UiDialog, UiFormField, UiInput, UiPopover, toast } from '@/components/ui';
import { navigationState } from '@/navigation/menu-store';
import type { ResolvedMenuNode } from '@/navigation/page-registry';
import type { AppView } from '@/router/route.types';
import { routeNames } from '@/router/route.types';
import { logoutSession, sessionState } from '@/session/session-store';
import { useAppearancePreferences } from './useAppearancePreferences';
import { useNavigationPreferences } from './useNavigationPreferences';

const props = defineProps<{ activeView: AppView }>();
const emit = defineEmits<{
  navigate: [view: AppView];
  navigateTo: [target: { path: string; label: string; module: string }];
}>();

const route = useRoute();
const router = useRouter();
const { customNames, isChildEnabled, isModuleEnabled } = useNavigationPreferences();
const { appearance } = useAppearancePreferences();
const collapsed = ref(false);
// Route selection highlights the active module; expansion is an explicit user action.
const expandedMain = ref<string | null>(null);
const activeMain = ref('');
const activeChild = ref('');
const profileOpen = ref(false);
const passwordDialogOpen = ref(false);
const passwordSaving = ref(false);
const passwordError = ref('');
const passwordForm = reactive({ currentPassword: '', newPassword: '', confirmPassword: '' });
const currentDisplayName = computed(
  () => sessionState.user?.realName?.trim() || sessionState.user?.username || '未登录',
);
const currentUsername = computed(() => sessionState.user?.username || '当前没有登录账号');
const avatarCharacter = computed(() => Array.from(currentDisplayName.value)[0] ?? '用');

const visibleItems = computed(() =>
  navigationState.items.flatMap((item) => {
    if (!isModuleEnabled(item.label)) return [];
    const children = item.children.filter((child) => isChildEnabled(item.label, child.label));
    if (item.kind === 'group' && children.length === 0) return [];
    return [{ ...item, children }];
  }),
);

const iconByKey: Record<string, Component> = {
  'carbon:workspace': LayoutDashboard,
  'lucide:layout-dashboard': LayoutDashboard,
  'lucide:settings-2': Settings2,
};

function resetPasswordForm() {
  passwordForm.currentPassword = '';
  passwordForm.newPassword = '';
  passwordForm.confirmPassword = '';
  passwordError.value = '';
}

watch(passwordDialogOpen, (isOpen) => {
  if (!isOpen) resetPasswordForm();
});

function openPasswordDialog() {
  profileOpen.value = false;
  resetPasswordForm();
  passwordDialogOpen.value = true;
}

async function submitPasswordChange() {
  passwordError.value = '';
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    passwordError.value = '两次输入的新密码不一致。';
    return;
  }

  passwordSaving.value = true;
  try {
    await changeCurrentPasswordApi({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
    });
    passwordDialogOpen.value = false;
    toast.info('密码已更新。');
  } catch (error) {
    passwordError.value = error instanceof Error ? error.message : '密码修改失败，请稍后重试。';
  } finally {
    passwordSaving.value = false;
  }
}

async function performLogout() {
  profileOpen.value = false;
  try {
    await logoutSession();
  } catch {
    toast.warning('本机登录状态已清除，但服务端注销请求未成功。');
  }
  await router.replace({ name: routeNames.login });
}

function displayName(label: string) {
  return customNames[label] || label;
}

function menuIcon(icon: null | string): Component {
  return icon ? (iconByKey[icon] ?? Folder) : Folder;
}

function visibleChildren(item: ResolvedMenuNode) {
  return item.children;
}

function openDestination(destination: ResolvedMenuNode) {
  if (!destination.localPath) return;
  emit('navigateTo', {
    path: destination.localPath,
    label: destination.label,
    module: activeMain.value || destination.label,
  });
}

function selectMain(item: ResolvedMenuNode) {
  activeMain.value = item.label;
  if (item.children.length > 0) return;
  activeChild.value = item.label;
  openDestination(item);
}

function setMainExpanded(item: ResolvedMenuNode, open: boolean) {
  activeMain.value = item.label;
  expandedMain.value = open ? item.label : null;
}

function selectChild(item: ResolvedMenuNode, child: ResolvedMenuNode) {
  activeMain.value = item.label;
  activeChild.value = child.label;
  openDestination(child);
}

watch(
  () => [props.activeView, route.meta.module, route.meta.pageLabel, route.meta.workspace, route.path] as const,
  () => {
    const module = String(route.meta.module ?? '');
    const label = String(route.meta.pageLabel ?? route.meta.workspace ?? '');
    if (module) {
      activeMain.value = module;
      activeChild.value = label || module;
    }
  },
  { immediate: true },
);

function toggleCollapsed() {
  collapsed.value = !collapsed.value;
}

defineExpose({ toggleCollapsed });
</script>

<template>
  <aside :class="[$style.sidebar, collapsed ? $style.sidebarCollapsed : $style.sidebarExpanded]">
    <div :class="$style.brandHeader">
      <img
        v-if="!collapsed"
        src="/logo.png"
        alt="General Admin"
        :class="$style.logo"
      />
      <button
        :aria-label="collapsed ? '展开侧边栏' : '收起侧边栏'"
        :class="$style.collapseButton"
        @click="toggleCollapsed"
      >
        <ChevronRight
          :size="15"
          :stroke-width="1.8"
          :class="!collapsed && $style.collapseIconExpanded"
        />
      </button>
    </div>

    <nav
      :class="[
        $style.navigation,
        appearance.density === 'compact' ? $style.navigationCompact : $style.navigationComfortable,
      ]"
    >
      <div
        v-for="item in visibleItems"
        :key="item.label"
      >
        <UiCollapsible
          v-if="item.children?.length"
          :open="expandedMain === item.label && !collapsed"
          @update:open="setMainExpanded(item, $event)"
        >
          <template #trigger>
            <button
              :title="collapsed ? displayName(item.label) : undefined"
              :class="[
                $style.mainItem,
                appearance.density === 'compact' ? $style.mainItemCompact : $style.mainItemComfortable,
                activeMain === item.label && $style.activeItem,
              ]"
              @click="selectMain(item)"
            >
              <component
                :is="menuIcon(item.icon)"
                :size="18"
                :stroke-width="1.8"
                :class="$style.itemIcon"
              />
              <span
                v-if="!collapsed"
                :class="$style.itemLabel"
                >{{ displayName(item.label) }}</span
              >
              <ChevronDown
                v-if="!collapsed"
                :size="14"
                :stroke-width="1.8"
                :class="expandedMain !== item.label && $style.chevronClosed"
              />
            </button>
          </template>

          <div :class="$style.childList">
            <button
              v-for="child in visibleChildren(item)"
              :key="child.label"
              :class="[
                $style.childItem,
                appearance.density === 'compact' ? $style.childItemCompact : $style.childItemComfortable,
                activeMain === item.label && activeChild === child.label && $style.activeChild,
              ]"
              @click="selectChild(item, child)"
            >
              <span :class="$style.itemLabel">{{ child.label }}</span>
            </button>
          </div>
        </UiCollapsible>

        <button
          v-else
          :title="collapsed ? displayName(item.label) : undefined"
          :class="[
            $style.mainItem,
            appearance.density === 'compact' ? $style.mainItemCompact : $style.mainItemComfortable,
            activeMain === item.label && $style.activeItem,
          ]"
          @click="selectMain(item)"
        >
          <component
            :is="menuIcon(item.icon)"
            :size="18"
            :stroke-width="1.8"
            :class="$style.itemIcon"
          />
          <span
            v-if="!collapsed"
            :class="$style.itemLabel"
            >{{ displayName(item.label) }}</span
          >
        </button>
      </div>
    </nav>

    <UiPopover
      v-model:open="profileOpen"
      side="top"
      align="start"
      size="xs"
    >
      <template #trigger>
        <button :class="$style.profileButton">
          <span :class="$style.avatar">{{ avatarCharacter }}</span>
          <span
            v-if="!collapsed"
            :class="$style.profileIdentity"
            ><span>{{ currentDisplayName }}</span
            ><small>{{ currentUsername }}</small></span
          >
          <ChevronDown
            v-if="!collapsed"
            :size="14"
            :stroke-width="1.8"
            :class="$style.profileChevron"
          />
        </button>
      </template>
      <div :class="$style.profileMenu">
        <div :class="$style.profileSummary">
          <strong>{{ currentDisplayName }}</strong
          ><small>{{ currentUsername }}</small>
        </div>
        <button @click="openPasswordDialog">修改密码</button>
        <button
          :class="$style.logoutButton"
          @click="performLogout"
        >
          退出登录
        </button>
      </div>
    </UiPopover>

    <UiDialog
      v-model:open="passwordDialogOpen"
      title="修改密码"
      description="请输入当前密码，并设置新密码。"
      size="sm"
    >
      <form
        id="password-change-form"
        :class="$style.passwordForm"
        @submit.prevent="submitPasswordChange"
      >
        <UiFormField
          label="当前密码"
          required
        >
          <UiInput
            v-model="passwordForm.currentPassword"
            autocomplete="current-password"
            maxlength="128"
            required
            type="password"
          />
        </UiFormField>
        <UiFormField
          label="新密码"
          required
        >
          <UiInput
            v-model="passwordForm.newPassword"
            autocomplete="new-password"
            maxlength="128"
            required
            type="password"
          />
        </UiFormField>
        <UiFormField
          label="确认新密码"
          required
        >
          <UiInput
            v-model="passwordForm.confirmPassword"
            autocomplete="new-password"
            maxlength="128"
            required
            type="password"
          />
        </UiFormField>
        <p
          v-if="passwordError"
          :class="$style.passwordError"
          role="alert"
        >
          {{ passwordError }}
        </p>
      </form>
      <template #footer>
        <UiButton @click="passwordDialogOpen = false">取消</UiButton>
        <UiButton
          form="password-change-form"
          :loading="passwordSaving"
          type="submit"
          variant="primary"
        >
          更新密码
        </UiButton>
      </template>
    </UiDialog>
  </aside>
</template>

<style module>
@reference "../../styles/index.css";
.sidebar {
  @apply relative flex h-full shrink-0 flex-col border-r bg-white transition-[width] duration-150;
  border-color: var(--ui-border);
}
.sidebarCollapsed {
  @apply w-[56px];
}
.sidebarExpanded {
  @apply w-[240px];
}
.brandHeader {
  @apply flex h-[56px] shrink-0 items-center border-b px-[10.5px];
  border-color: var(--ui-border);
}
.logo {
  @apply h-[56px] w-[128.7px] object-contain;
}
.collapseButton {
  @apply ml-auto rounded p-1 transition-colors;
  color: var(--ui-text-subtle);
}
.collapseButton:hover {
  background: var(--ui-surface-hover);
}
.collapseIconExpanded {
  @apply rotate-180;
}
.navigation {
  @apply flex-1 overflow-y-auto px-2;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.navigation::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
.navigation::-webkit-scrollbar-thumb {
  background: #c1c7d0;
  border-radius: 999px;
}
.navigationCompact {
  @apply py-1;
}
.navigationComfortable {
  @apply py-2;
}
.mainItem {
  @apply flex w-full items-center gap-2.5 rounded px-2 text-left font-medium transition-colors;
  color: var(--ui-text-muted);
}
.mainItem:hover,
.childItem:hover {
  background: var(--ui-surface-hover);
}
.mainItemCompact {
  @apply h-[29px] text-[13px];
}
.mainItemComfortable {
  @apply h-[33.5px] text-[14px];
}
.activeItem,
.activeChild {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.itemIcon {
  @apply shrink-0;
}
.itemLabel {
  @apply min-w-0 flex-1 truncate;
}
.mainBadge {
  @apply rounded-full px-1.5 py-0.5 text-[10px] font-bold;
}
.progressBadge {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.countBadge {
  color: white;
  background: var(--ui-danger);
}
.chevronClosed {
  @apply -rotate-90;
}
.childList {
  @apply space-y-0.5 pl-[30px] pt-[1.5px];
}
.childItem {
  @apply flex w-full items-center rounded px-2.5 text-left transition-colors;
  color: var(--ui-text-muted);
}
.childItemCompact {
  @apply h-[27px] text-[12px];
}
.childItemComfortable {
  @apply h-[31.5px] text-[13px];
}
.activeChild {
  @apply font-medium;
}
.childBadge {
  @apply ml-2 rounded-full px-1.5 py-0.5 text-[9px] font-bold;
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.industrySection {
  @apply mt-2 border-t px-0 pt-3;
  border-color: var(--ui-border-subtle);
}
.industryHeader {
  @apply mb-1 flex items-center justify-between px-2 text-[10px] font-semibold tracking-[.08em];
  color: #97a0af;
}
.industryHeader button {
  color: var(--ui-text-subtle);
  transition: color 150ms;
}
.industryHeader button:hover {
  color: var(--app-primary);
}
.profileButton {
  @apply flex h-[68px] w-full shrink-0 items-center gap-3 border-t px-5 text-left transition-colors;
  border-color: var(--ui-border);
}
.profileButton:hover {
  background: var(--ui-surface-subtle);
}
.avatar {
  @apply flex h-9 w-9 items-center justify-center rounded-full text-sm font-semibold text-white;
  background: var(--app-primary);
}
.profileIdentity {
  @apply min-w-0 flex-1;
}
.profileIdentity > span {
  @apply block text-[13px] font-semibold;
  color: var(--ui-text);
}
.profileIdentity > small {
  @apply block truncate text-[11px];
  color: var(--ui-text-subtle);
}
.profileChevron {
  color: var(--ui-text-subtle);
}
.profileMenu {
  @apply p-2;
}
.profileSummary {
  @apply border-b px-2 py-2;
  border-color: var(--ui-border-subtle);
}
.profileSummary strong {
  @apply block text-[12px];
}
.profileSummary small {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.profileMenu > button {
  @apply w-full rounded px-2 py-2 text-left text-[12px];
}
.profileMenu > button:first-of-type {
  @apply mt-1;
}
.profileMenu > button:hover {
  background: var(--ui-surface-hover);
}
.profileMenu > .logoutButton {
  color: var(--ui-danger);
}
.profileMenu > .logoutButton:hover {
  background: var(--ui-danger-soft);
}
.passwordForm {
  @apply grid gap-4 p-5;
}
.passwordError {
  @apply m-0 text-xs;
  color: var(--ui-danger);
}

@media (min-width: 481px) and (max-width: 768px) {
  .sidebar {
    transform: translateX(-191.5px);
  }
}

@media (max-width: 480px) {
  .sidebar {
    display: none;
  }
}
</style>
