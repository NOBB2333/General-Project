<script setup lang="ts">
import { Command } from 'lucide-vue-next';
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { RouterView, useRoute, useRouter } from 'vue-router';
import { getAuthorizedHomePath, getAuthorizedPageByPath, isRouteAuthorized } from '@/navigation/menu-store';
import type { AppView } from '@/router/route.types';
import { routeNames } from '@/router/route.types';
import type { HotkeyId } from './useHotkeyPreferences';
import { UiDialog, UiToaster } from '@/components/ui';
import { sessionState } from '@/session/session-store';
import AppSidebar from './AppSidebar.vue';
import TopWorkspace from './TopWorkspace.vue';
import { useAppearancePreferences } from './useAppearancePreferences';
import { formatHotkey, useHotkeyPreferences } from './useHotkeyPreferences';

const route = useRoute();
const router = useRouter();
const activeView = computed<AppView>(() => route.meta.appView ?? 'products');
const { appearance } = useAppearancePreferences();
const { hotkeyGroups, findHotkey } = useHotkeyPreferences();
const watermarkText = computed(() => {
  const displayName = sessionState.user?.realName?.trim() || sessionState.user?.username || 'General Admin';
  return `${displayName}  ${new Date().toLocaleDateString('zh-CN')}`;
});
const sidebar = ref<InstanceType<typeof AppSidebar> | null>(null);
const topWorkspace = ref<InstanceType<typeof TopWorkspace> | null>(null);
const hotkeyHelpOpen = ref(false);
const supportedHotkeyIds = computed(() => {
  const ids = new Set<HotkeyId>(['sidebar', 'notifications', 'search', 'help']);
  if (getAuthorizedHomePath(sessionState.homePath)) ids.add('home');
  if (getAuthorizedPageByPath('/projects-new/project-list')) ids.add('projects');
  if (isRouteAuthorized(routeNames.settings)) ids.add('settings');
  return ids;
});
const enabledHotkeys = computed(() =>
  hotkeyGroups.flatMap((group) => group.items).filter((item) => item.enabled && supportedHotkeyIds.value.has(item.id)),
);

function navigate(_view: AppView) {
  const homePath = getAuthorizedHomePath(sessionState.homePath);
  void router.replace(homePath ?? { name: routeNames.forbidden });
}

function navigateTo(target: { path: string; label: string; module: string }) {
  void router.push(target.path);
}

function runHotkey(event: KeyboardEvent) {
  if (event.key === 'Escape' && hotkeyHelpOpen.value) {
    hotkeyHelpOpen.value = false;
    return;
  }
  if (document.querySelector('[data-hotkey-recording="true"]')) return;
  const item = findHotkey(formatHotkey(event));
  if (!item || event.repeat) return;
  if (!supportedHotkeyIds.value.has(item.id)) return;
  const target = event.target;
  const isEditing =
    target instanceof HTMLInputElement ||
    target instanceof HTMLTextAreaElement ||
    target instanceof HTMLSelectElement ||
    (target instanceof HTMLElement && target.isContentEditable);
  if (isEditing && item.id !== 'search') return;
  event.preventDefault();
  if (item.id === 'home') navigate('feature');
  if (item.id === 'sidebar') sidebar.value?.toggleCollapsed();
  if (item.id === 'projects') {
    const projectList = getAuthorizedPageByPath('/projects-new/project-list');
    if (projectList?.localPath) void router.push(projectList.localPath);
  }
  if (item.id === 'settings') void router.push({ name: routeNames.settings });
  if (item.id === 'notifications') topWorkspace.value?.openPanel('通知中心');
  if (item.id === 'search') topWorkspace.value?.openPanel('全局搜索');
  if (item.id === 'help') hotkeyHelpOpen.value = !hotkeyHelpOpen.value;
}

onMounted(() => window.addEventListener('keydown', runHotkey));
onUnmounted(() => window.removeEventListener('keydown', runHotkey));
</script>

<template>
  <div
    :class="$style.shell"
    data-app-shell
  >
    <AppSidebar
      ref="sidebar"
      :active-view="activeView"
      @navigate="navigate"
      @navigate-to="navigateTo"
    />
    <div :class="$style.workspace">
      <TopWorkspace
        ref="topWorkspace"
        :active-view="activeView"
        @navigate="navigate"
        @toggle-sidebar="sidebar?.toggleCollapsed()"
      />
      <main :class="$style.mainContent">
        <RouterView :key="sessionState.contextVersion" />
      </main>
    </div>
    <div
      v-if="appearance.watermark"
      data-app-watermark
      :class="$style.watermark"
      aria-hidden="true"
    >
      <div :class="$style.watermarkGrid">
        <span
          v-for="index in 60"
          :key="index"
          >{{ watermarkText }}</span
        >
      </div>
    </div>
    <UiDialog
      v-model:open="hotkeyHelpOpen"
      title="快捷键帮助"
      description="以下组合均可在系统设置中修改或停用"
      size="md"
      layer="top"
    >
      <template #icon>
        <span :class="$style.hotkeyIcon"
          ><Command
            :size="17"
            :stroke-width="1.8"
        /></span>
      </template>
      <div :class="$style.hotkeyGrid">
        <div
          v-for="item in enabledHotkeys"
          :key="item.id"
          :class="$style.hotkeyItem"
        >
          <span :class="$style.hotkeyCopy">
            <strong :class="$style.hotkeyName">{{ item.name }}</strong>
            <small :class="$style.hotkeyDescription">{{ item.description }}</small>
          </span>
          <kbd :class="$style.hotkeyValue">{{ item.shortcut }}</kbd>
        </div>
      </div>
    </UiDialog>
    <UiToaster />
  </div>
</template>

<style module>
@reference "../../styles/index.css";
.shell {
  @apply relative flex h-full w-full;
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}
.workspace {
  @apply flex min-w-0 flex-1 flex-col;
}
.mainContent {
  @apply flex-1 overflow-auto px-2 py-2 sm:px-3 sm:py-3 md:px-4 md:py-4 lg:px-6 lg:py-5;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.mainContent::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
.mainContent::-webkit-scrollbar-thumb {
  background: #c1c7d0;
  border-radius: 999px;
}
.mainContent::-webkit-scrollbar-track {
  background: transparent;
}
.watermark {
  @apply pointer-events-none fixed inset-0 z-[300] overflow-hidden opacity-[.075];
}
.watermarkGrid {
  @apply absolute items-center justify-items-center whitespace-nowrap text-[13px] font-semibold;
  inset: -160px;
  display: grid;
  grid-template-columns: repeat(5, minmax(260px, 1fr));
  grid-auto-rows: 120px;
  color: var(--ui-text);
  transform: rotate(-22deg);
}
.hotkeyIcon {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.hotkeyGrid {
  @apply grid grid-cols-2 gap-x-5 gap-y-1 p-5;
}
.hotkeyItem {
  @apply flex min-h-[46px] items-center border-b py-2;
  border-color: var(--ui-border-subtle);
}
.hotkeyCopy {
  @apply min-w-0 flex-1;
}
.hotkeyName {
  @apply block text-[12px];
}
.hotkeyDescription {
  @apply mt-0.5 block truncate text-[10px];
  color: var(--ui-text-subtle);
}
.hotkeyValue {
  @apply ml-3 shrink-0 rounded border px-2 py-1 text-[10px] font-semibold;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

@media (min-width: 481px) and (max-width: 768px) {
  .workspace {
    flex: 0 0 calc(100% - 240px);
    width: calc(100% - 240px);
    margin-left: -191.5px;
  }

  .mainContent {
    padding: 14px;
  }
}

@media (max-width: 480px) {
  .workspace {
    flex: 1 1 auto;
    width: 100%;
  }
}
</style>
