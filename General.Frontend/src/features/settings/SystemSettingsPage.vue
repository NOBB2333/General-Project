<script setup lang="ts">
import {
  AlertTriangle,
  Bell,
  Building2,
  Check,
  ChevronRight,
  Database,
  FileText,
  Grid3X3,
  Keyboard,
  MonitorCog,
  Palette,
  Pencil,
  ShieldCheck,
  Settings2,
  ToggleLeft,
  ToggleRight,
  UsersRound,
  X,
} from 'lucide-vue-next';
import { computed, ref } from 'vue';
import type { Component } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { routeNames } from '@/router/route.types';
import { isRouteAuthorized, navigationState } from '@/navigation/menu-store';
import {
  toast,
  UiAccordion,
  UiAccordionContent,
  UiAccordionItem,
  UiAccordionTrigger,
  UiButton,
  UiInput,
  UiSwitch,
} from '@/components/ui';
import { useNavigationPreferences } from '@/components/layout/useNavigationPreferences';
import type { HotkeyId } from '@/components/layout/useHotkeyPreferences';
import { getAuthorizedHomePath } from '@/navigation/menu-store';
import SettingsAppearance from './SettingsAppearance.vue';
import SettingsCompany from './SettingsCompany.vue';
import SettingsData from './SettingsData.vue';
import SettingsHeaders from './SettingsHeaders.vue';
import SettingsHotkeys from './SettingsHotkeys.vue';
import SettingsNotifications from './SettingsNotifications.vue';
import SettingsPanel from './SettingsPanel.vue';

type SettingsSection =
  | 'company'
  | 'headers'
  | 'notifications'
  | 'data'
  | 'appearance'
  | 'modules'
  | 'hotkeys'
  | 'platformConfigs'
  | 'platformUsers'
  | 'platformOrganization'
  | 'platformRoles'
  | 'platformSystemMonitor'
  | 'integrationTemplates'
  | 'integrationRules';
type NotificationTone = 'success' | 'warning' | 'error' | 'info';

interface SettingsSectionItem {
  key: SettingsSection;
  label: string;
  description: string;
  icon: Component;
  routeName?: string;
}

interface SettingsModule {
  label: string;
  core: boolean;
  settingsSummary: string;
  children: string[];
  protectedChildren: string[];
}

const platformRoutes: Partial<Record<SettingsSection, string>> = {
  platformConfigs: routeNames.platformConfigs,
  platformUsers: routeNames.platformUsers,
  platformOrganization: routeNames.platformOrganization,
  platformRoles: routeNames.platformRoles,
  platformSystemMonitor: routeNames.platformSystemMonitor,
  integrationTemplates: routeNames.integrationTemplates,
  integrationRules: routeNames.integrationRules,
};

const baseSections: SettingsSectionItem[] = [
  { key: 'company' as const, label: '企业信息', description: '管理企业基本信息、Logo等', icon: Building2 },
  { key: 'headers' as const, label: '多抬头管理', description: '管理合同签署主体的公司抬头', icon: FileText },
  { key: 'notifications' as const, label: '通知设置', description: '管理外部集成消息模板与通知规则', icon: Bell },
  { key: 'data' as const, label: '数据管理', description: '查看主机备份说明与文件管理入口', icon: Database },
  { key: 'appearance' as const, label: '外观定制', description: '自定义系统主题和界面', icon: Palette },
  { key: 'modules' as const, label: '功能模块', description: '启用/隐藏侧边栏模块，自定义菜单名称', icon: Grid3X3 },
  { key: 'hotkeys' as const, label: '热键管理', description: '自定义全局快捷键，提升操作效率', icon: Keyboard },
  {
    key: 'platformConfigs',
    label: '平台参数',
    description: '打开平台配置参数',
    icon: Settings2,
    routeName: platformRoutes.platformConfigs,
  },
  {
    key: 'platformUsers',
    label: '用户管理',
    description: '打开平台用户管理',
    icon: UsersRound,
    routeName: platformRoutes.platformUsers,
  },
  {
    key: 'platformOrganization',
    label: '组织机构',
    description: '打开平台组织管理',
    icon: Building2,
    routeName: platformRoutes.platformOrganization,
  },
  {
    key: 'platformRoles',
    label: '角色权限',
    description: '打开平台角色与授权管理',
    icon: ShieldCheck,
    routeName: platformRoutes.platformRoles,
  },
  {
    key: 'platformSystemMonitor',
    label: '在线用户与监控',
    description: '打开会话和系统监控',
    icon: MonitorCog,
    routeName: platformRoutes.platformSystemMonitor,
  },
  {
    key: 'integrationTemplates',
    label: '消息模板',
    description: '打开外部集成消息模板',
    icon: FileText,
    routeName: platformRoutes.integrationTemplates,
  },
  {
    key: 'integrationRules',
    label: '通知规则',
    description: '打开外部集成通知规则',
    icon: Bell,
    routeName: platformRoutes.integrationRules,
  },
];

const sections = computed(() =>
  baseSections.filter((section) => !section.routeName || isRouteAuthorized(section.routeName)),
);

const {
  childEnabled,
  customNames,
  enableAll,
  isModuleEnabled,
  keepCoreOnly,
  resetNames,
  setChildEnabled,
  setCustomName,
  setModuleEnabled,
} = useNavigationPreferences();

const route = useRoute();
const router = useRouter();
const sectionKeys = new Set<SettingsSection>([
  'company',
  'headers',
  'notifications',
  'data',
  'appearance',
  'modules',
  'hotkeys',
]);
const availableHotkeyIds = computed<HotkeyId[]>(() => {
  const ids: HotkeyId[] = ['sidebar', 'notifications', 'search', 'help'];
  if (getAuthorizedHomePath('')) ids.unshift('home');
  if (navigationState.pages.some((page) => page.localPath === '/projects-new/project-list')) ids.push('projects');
  if (isRouteAuthorized(routeNames.settings)) ids.push('settings');
  return ids;
});
const activeSection = computed<SettingsSection>(() => {
  const section = String(route.params.section ?? 'company') as SettingsSection;
  return sectionKeys.has(section) ? section : 'company';
});
const expandedModule = ref('');
const editingName = ref('');
const nameDraft = ref('');

const settingsModules = computed<SettingsModule[]>(() =>
  navigationState.items.map((item) => {
    const protectedChildren = item.children
      .filter((child) => child.defaultHome || child.routeName === routeNames.settings)
      .map((child) => child.label);
    return {
      label: item.label,
      core: item.defaultHome || item.routeName === routeNames.settings || protectedChildren.length > 0,
      settingsSummary: item.kind === 'group' ? `${item.children.length} 个当前已授权页面` : '当前已授权页面',
      children: item.children.map((child) => child.label),
      protectedChildren,
    };
  }),
);
const enabledCount = computed(() => settingsModules.value.filter((item) => isModuleEnabled(item.label)).length);

function notify(message: string, tone: NotificationTone = 'success') {
  toast[tone](message);
}

function selectSection(section: SettingsSection) {
  const routeName = platformRoutes[section];
  if (routeName) {
    if (!isRouteAuthorized(routeName)) {
      notify('当前账号没有访问该页面的权限。', 'warning');
      return;
    }
    void router.push({ name: routeName });
    return;
  }
  void router.push({ name: routeNames.settings, params: { section } });
}

function startRename(item: SettingsModule) {
  editingName.value = item.label;
  nameDraft.value = customNames[item.label] || item.label;
}

function saveRename(item: SettingsModule) {
  setCustomName(item.label, nameDraft.value);
  editingName.value = '';
  notify('菜单名称已更新');
}

function configurableChildren(item: SettingsModule) {
  return item.children.filter((child) => !item.protectedChildren.includes(child));
}

function enabledChildren(item: SettingsModule) {
  return configurableChildren(item).filter((child) => childEnabled[item.label]?.[child] ?? true).length;
}

function displayChildCount(item: SettingsModule) {
  const total = configurableChildren(item).length;
  if (!total) return '';
  return `${enabledChildren(item)}/${total} 子菜单`;
}

function setAllChildren(item: SettingsModule, enabled: boolean) {
  configurableChildren(item).forEach((child) => setChildEnabled(item.label, child, enabled));
}

function runAction(action: 'reset' | 'enable' | 'core') {
  if (action === 'reset') {
    resetNames();
    notify('已恢复默认菜单名称');
  }
  if (action === 'enable') {
    enableAll();
    notify('已启用全部功能模块');
  }
  if (action === 'core') {
    keepCoreOnly();
    notify('已仅保留核心模块');
  }
}
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="settings"
    data-page-route="/settings"
    :data-settings-active-section="activeSection"
  >
    <p :class="$style.eyebrow">系统设置</p>
    <h1 :class="$style.pageTitle">系统设置</h1>

    <div :class="$style.settingsLayout">
      <nav
        :class="$style.sectionNavigation"
        aria-label="系统设置分类"
      >
        <button
          v-for="section in sections"
          :key="section.key"
          :class="[$style.sectionButton, activeSection === section.key && $style.activeSection]"
          :data-settings-section="section.key"
          :aria-current="activeSection === section.key ? 'page' : undefined"
          @click="selectSection(section.key)"
        >
          <span :class="[$style.sectionIcon, activeSection === section.key && $style.activeSectionIcon]">
            <component
              :is="section.icon"
              :size="19"
              :stroke-width="1.8"
            />
          </span>
          <span :class="$style.sectionCopy">
            <strong :class="$style.sectionName">{{ section.label }}</strong>
            <small
              :class="[$style.sectionDescription, activeSection === section.key && $style.activeSectionDescription]"
            >
              {{ section.description }}
            </small>
          </span>
        </button>
      </nav>

      <SettingsCompany
        v-if="activeSection === 'company'"
        @notify="notify"
      />
      <SettingsHeaders
        v-else-if="activeSection === 'headers'"
        @notify="notify"
      />
      <SettingsNotifications
        v-else-if="activeSection === 'notifications'"
        @notify="notify"
      />
      <SettingsData v-else-if="activeSection === 'data'" />
      <SettingsAppearance
        v-else-if="activeSection === 'appearance'"
        @notify="notify"
      />

      <SettingsPanel
        v-else-if="activeSection === 'modules'"
        title="功能模块管理"
        description=""
      >
        <template #description>
          <p :class="$style.moduleDescription">
            只调整当前账号已获授权菜单的侧边栏显示，不会改变服务端权限。可按模块或菜单项调整，也可自定义模块名称。
            当前显示 <strong>{{ enabledCount }}</strong
            >/{{ settingsModules.length }} 个模块。
          </p>
        </template>
        <template #actions>
          <UiButton
            :class="$style.resetAction"
            size="sm"
            @click="runAction('reset')"
            >恢复默认名称</UiButton
          >
          <UiButton
            :class="$style.enableAction"
            size="sm"
            @click="runAction('enable')"
            >全部启用</UiButton
          >
          <UiButton
            :class="$style.coreAction"
            size="sm"
            @click="runAction('core')"
            >仅保留核心</UiButton
          >
        </template>

        <aside :class="$style.moduleNotice">
          <AlertTriangle
            :size="14"
            :stroke-width="1.8"
            :class="$style.noticeIcon"
          />
          <p>
            含系统首页或系统设置入口的模块无法隐藏，避免隐藏后找不到基础入口。二级菜单只从当前已授权菜单生成；隐藏菜单不会改变服务端权限。
          </p>
        </aside>

        <div :class="$style.moduleGroupTitle">
          <strong>已授权模块</strong><span>{{ enabledCount }}/{{ settingsModules.length }} 已显示</span>
        </div>
        <UiAccordion
          v-model="expandedModule"
          data-settings-modules
          :class="$style.moduleList"
        >
          <template
            v-for="item in settingsModules"
            :key="item.label"
          >
            <UiAccordionItem
              :value="item.label"
              :class="[$style.moduleCard, !isModuleEnabled(item.label) && $style.moduleDisabled]"
            >
              <div :class="$style.moduleRow">
                <UiAccordionTrigger
                  v-if="configurableChildren(item).length"
                  :class="$style.expandButton"
                  :aria-label="expandedModule === item.label ? `收起${item.label}` : `展开${item.label}`"
                >
                  <ChevronRight
                    :size="14"
                    :stroke-width="1.8"
                    :class="[$style.expandIcon, expandedModule === item.label && $style.expandIconOpen]"
                  />
                </UiAccordionTrigger>
                <span
                  v-else
                  :class="$style.expandPlaceholder"
                />
                <span :class="$style.moduleSwitch">
                  <UiSwitch
                    :class="$style.moduleToggle"
                    :model-value="isModuleEnabled(item.label)"
                    size="sm"
                    :disabled="item.core"
                    :label="`${item.label}启用状态`"
                    @update:model-value="setModuleEnabled(item.label, $event)"
                  />
                  <ToggleRight
                    v-if="isModuleEnabled(item.label)"
                    :size="28"
                    :stroke-width="2"
                    :class="$style.moduleToggleIcon"
                  />
                  <ToggleLeft
                    v-else
                    :size="28"
                    :stroke-width="2"
                    :class="[$style.moduleToggleIcon, $style.moduleToggleIconDisabled]"
                  />
                </span>
                <div :class="$style.moduleIdentity">
                  <div :class="$style.moduleHeading">
                    <template v-if="editingName === item.label">
                      <UiInput
                        v-model="nameDraft"
                        autofocus
                        :class="$style.nameInput"
                        @keydown.enter="saveRename(item)"
                        @keydown.escape="editingName = ''"
                      />
                      <UiButton
                        aria-label="保存名称"
                        title="保存名称"
                        variant="ghost"
                        size="icon-sm"
                        @click="saveRename(item)"
                      >
                        <Check
                          :size="13"
                          :stroke-width="1.8"
                          :class="$style.saveNameIcon"
                        />
                      </UiButton>
                      <UiButton
                        aria-label="取消重命名"
                        title="取消"
                        variant="ghost"
                        size="icon-sm"
                        @click="editingName = ''"
                      >
                        <X
                          :size="13"
                          :stroke-width="1.8"
                          :class="$style.cancelNameIcon"
                        />
                      </UiButton>
                    </template>
                    <template v-else>
                      <strong :class="$style.moduleName">{{ customNames[item.label] || item.label }}</strong>
                      <UiButton
                        :class="$style.renameButton"
                        aria-label="自定义菜单名称"
                        title="自定义菜单名称"
                        variant="ghost"
                        size="icon-sm"
                        @click="startRename(item)"
                      >
                        <Pencil
                          :size="12"
                          :stroke-width="2"
                        />
                      </UiButton>
                      <span
                        v-if="item.core"
                        :class="$style.coreBadge"
                        >核心</span
                      >
                      <span
                        v-if="displayChildCount(item)"
                        :class="$style.childCount"
                        >{{ displayChildCount(item) }}</span
                      >
                    </template>
                  </div>
                  <p :class="$style.moduleSummary">{{ item.settingsSummary }}</p>
                </div>
              </div>

              <UiAccordionContent v-if="configurableChildren(item).length">
                <div :class="$style.childPanel">
                  <header :class="$style.childPanelHeader">
                    <strong>二级菜单项</strong>
                    <span :class="$style.childBulkActions">
                      <button @click="setAllChildren(item, true)">全部显示</button>
                      <i>|</i>
                      <button @click="setAllChildren(item, false)">全部隐藏</button>
                    </span>
                  </header>
                  <div :class="$style.childGrid">
                    <button
                      v-for="child in configurableChildren(item)"
                      :key="child"
                      role="switch"
                      :aria-checked="childEnabled[item.label]?.[child] ?? true"
                      :class="$style.childButton"
                      @click="setChildEnabled(item.label, child, !(childEnabled[item.label]?.[child] ?? true))"
                    >
                      <ToggleRight
                        v-if="childEnabled[item.label]?.[child] ?? true"
                        :size="20"
                        :stroke-width="2"
                        :class="$style.childEnabledIcon"
                      />
                      <ToggleLeft
                        v-else
                        :size="20"
                        :stroke-width="2"
                        :class="$style.childDisabledIcon"
                      />
                      <span>{{ child }}</span>
                    </button>
                  </div>
                </div>
              </UiAccordionContent>
            </UiAccordionItem>
          </template>
        </UiAccordion>
      </SettingsPanel>

      <SettingsHotkeys
        v-else-if="activeSection === 'hotkeys'"
        :available-hotkey-ids="availableHotkeyIds"
        @notify="notify"
      />
    </div>
  </section>
</template>

<style module>
@reference "../../styles/index.css";
.page {
  @apply min-w-[1040px] pt-[2px];
}
.eyebrow {
  @apply text-[12px] font-semibold;
  color: var(--ui-text);
}
.pageTitle {
  @apply mt-1 text-[22px] font-bold;
  color: var(--ui-text);
}
.settingsLayout {
  @apply mt-5 flex items-start gap-4;
}
.sectionNavigation {
  @apply w-[323.5px] shrink-0 space-y-1.5;
}
.sectionButton {
  @apply flex h-[62.5px] w-full items-center gap-3 rounded-lg border bg-white p-3.5 text-left transition-colors;
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.sectionButton:hover {
  border-color: color-mix(in srgb, var(--app-primary) 20%, transparent);
  background: var(--ui-surface-hover);
}
.activeSection {
  @apply h-[60.5px] border-transparent;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.activeSection:hover {
  background: var(--app-primary-soft);
}
.sectionIcon {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.activeSectionIcon {
  color: white;
  background: var(--app-primary);
}
.sectionCopy {
  @apply min-w-0;
}
.sectionName {
  @apply block text-[13px];
}
.sectionDescription {
  @apply mt-0.5 block truncate text-[11px] font-normal;
  color: #97a0af;
}
.activeSectionDescription {
  color: var(--ui-focus);
}
.moduleDescription {
  @apply mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.moduleDescription strong {
  color: var(--app-primary);
}
.resetAction {
  @apply h-[48.5px] w-[93.1px];
}
.enableAction {
  @apply h-[48.5px] w-[69.7px];
}
.coreAction {
  @apply h-[48.5px] w-[81.4px];
}
.moduleNotice {
  @apply mb-5 mt-5 flex items-start gap-2 rounded-lg border p-3;
  color: var(--ui-text);
  border-color: #ffe380;
  background: var(--ui-warning-soft);
}
.noticeIcon {
  @apply mt-0.5 shrink-0;
  color: var(--ui-warning);
}
.moduleNotice p {
  @apply text-[12px];
}
.moduleGroupTitle {
  @apply mb-3 flex items-center;
}
.moduleGroupTitle strong {
  @apply text-[14px] font-semibold;
}
.moduleGroupTitle span {
  @apply ml-2 text-[11px];
  color: #97a0af;
}
.moduleList {
  @apply space-y-2;
}
.moduleList > .moduleGroupTitle {
  @apply pb-1 pt-3;
}
.moduleCard {
  @apply overflow-hidden rounded-lg border bg-white transition-colors;
  border-color: var(--ui-border);
}
.moduleDisabled {
  @apply opacity-55;
}
.moduleRow {
  @apply flex h-[57px] items-center gap-3 p-3;
}
.expandButton {
  @apply flex h-5 w-5 items-center justify-center;
  color: var(--ui-text-subtle);
}
.expandIcon {
  @apply transition-transform;
}
.expandIconOpen {
  @apply rotate-90;
}
.expandPlaceholder {
  @apply h-5 w-5;
}
.moduleSwitch {
  @apply relative flex h-8 w-8 items-center justify-center;
}
.moduleToggle {
  @apply h-8 w-8 bg-transparent;
}
.moduleToggle[data-state='checked'],
.moduleToggle[data-state='unchecked'] {
  background: transparent;
}
.moduleToggle > span {
  @apply opacity-0;
}
.moduleToggle:focus-visible {
  outline: 2px solid var(--ui-focus);
  outline-offset: 1px;
}
.moduleToggleIcon {
  @apply pointer-events-none absolute inset-0;
  color: var(--app-primary);
}
.moduleToggleIconDisabled {
  color: var(--ui-text-subtle);
}
.moduleIdentity {
  @apply min-w-0 flex-1;
}
.moduleHeading {
  @apply flex items-center gap-1.5;
}
.nameInput {
  @apply h-7 w-[180px] px-2;
}
.saveNameIcon {
  color: var(--ui-success);
}
.cancelNameIcon {
  color: var(--ui-danger);
}
.moduleName {
  @apply text-[13px] font-medium;
}
.renameButton {
  @apply h-5 w-5 p-0.5 opacity-0;
}
.moduleCard:hover .renameButton,
.moduleCard:focus-within .renameButton {
  @apply opacity-100;
}
.coreBadge {
  @apply rounded px-1.5 py-0.5 text-[9px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.childCount {
  @apply text-[10px];
  color: #97a0af;
}
.moduleSummary {
  @apply truncate text-[11px];
  color: var(--ui-text-subtle);
}
.childPanel {
  @apply border-t px-3 py-2;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.childPanelHeader {
  @apply flex h-6 items-center justify-between px-1;
}
.childPanelHeader > strong {
  @apply text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
.childBulkActions {
  @apply flex h-6 w-[105.5px] items-center gap-2 text-[11px];
}
.childBulkActions button:first-child {
  @apply font-semibold;
  color: var(--app-primary);
}
.childBulkActions button:last-child {
  @apply font-semibold;
  color: var(--ui-text-subtle);
}
.childBulkActions i {
  @apply not-italic;
  color: var(--ui-border);
}
.childGrid {
  @apply mt-2 grid grid-cols-3 gap-1.5;
}
.childButton {
  @apply flex h-[36px] items-center gap-2 border bg-white px-2.5 py-2 text-left text-[12px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.childButton:hover {
  background: var(--ui-surface-hover);
}
.childButton span {
  @apply truncate;
}
.childEnabledIcon,
.childDisabledIcon {
  @apply shrink-0;
}
.childEnabledIcon {
  color: var(--app-primary);
}
.childDisabledIcon {
  color: #97a0af;
}
</style>
