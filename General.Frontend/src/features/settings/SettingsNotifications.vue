<script setup lang="ts">
import { Bell, FileText } from 'lucide-vue-next';
import { useRouter } from 'vue-router';
import { isRouteAuthorized } from '@/navigation/menu-store';
import { routeNames } from '@/router/route.types';
import SettingsPanel from './SettingsPanel.vue';

type NotificationTone = 'success' | 'warning' | 'error' | 'info';

const emit = defineEmits<{ notify: [message: string, tone?: NotificationTone] }>();
const router = useRouter();
const links = [
  {
    description: '维护外部集成使用的消息模板，并使用真实模板预览能力检查变量。',
    icon: FileText,
    key: 'templates',
    label: '消息模板',
    routeName: routeNames.integrationTemplates,
  },
  {
    description: '维护连接器通知规则；事件评估只检查规则，不会发送消息。',
    icon: Bell,
    key: 'rules',
    label: '通知规则',
    routeName: routeNames.integrationRules,
  },
];

function openRoute(routeName: string) {
  if (!isRouteAuthorized(routeName)) {
    emit('notify', '当前账号没有访问该通知页面的权限。', 'warning');
    return;
  }

  void router.push({ name: routeName });
}
</script>

<template>
  <SettingsPanel
    title="通知设置"
    description="消息模板和连接器通知规则使用外部集成模块的真实页面与权限。当前没有邮件、短信发送链路，不提供虚假渠道开关。"
  >
    <section :class="$style.section">
      <h3 :class="$style.sectionTitle">通知管理</h3>
      <div :class="$style.channelGrid">
        <button
          v-for="link in links"
          :key="link.key"
          type="button"
          :class="$style.channelCard"
          :data-notification-route="link.key"
          :disabled="!isRouteAuthorized(link.routeName)"
          :aria-label="`打开${link.label}`"
          @click="openRoute(link.routeName)"
        >
          <span :class="$style.channelIcon">
            <component
              :is="link.icon"
              :size="18"
              :stroke-width="1.8"
            />
          </span>
          <div :class="$style.channelContent">
            <strong :class="$style.channelName">{{ link.label }}</strong>
            <p :class="$style.channelDescription">{{ link.description }}</p>
          </div>
        </button>
      </div>
    </section>
  </SettingsPanel>
</template>

<style module>
@reference "../../styles/index.css";
.section {
  @apply mt-5;
}
.sectionTitle {
  @apply text-[13px] font-semibold;
}
.channelGrid {
  @apply mt-3 grid grid-cols-2 gap-3;
}
.channelCard {
  @apply flex h-[86px] items-center rounded border-2 px-4;
  border-color: var(--ui-border);
}
.channelCardActive {
  border-color: color-mix(in srgb, var(--app-primary) 35%, white);
  background: color-mix(in srgb, var(--app-primary-soft) 30%, transparent);
}
.channelIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.channelContent {
  @apply ml-3 min-w-0 flex-1;
}
.channelName {
  @apply text-[13px];
}
.channelDescription {
  @apply mt-2 text-[10px];
  color: var(--ui-text-subtle);
}
.ruleHeader {
  @apply flex items-center justify-between;
}
.tableFrame {
  @apply mt-3 overflow-hidden rounded border;
  border-color: var(--ui-border-subtle);
}
.ruleTable {
  @apply w-full table-fixed text-[11px];
}
.ruleTable thead {
  @apply h-10 text-left;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}
.ruleTable tbody tr {
  @apply h-10 border-t;
  border-color: var(--ui-border-subtle);
}
.moduleColumn {
  @apply w-[16%] px-3;
}
.eventColumn {
  @apply w-[27%] px-3;
}
.channelColumn {
  @apply w-[10%] text-center;
}
.smallChannelColumn {
  @apply w-[9%] text-center;
}
.timingColumn {
  @apply w-[19%] px-2;
}
.enabledColumn {
  @apply w-[10%] text-center;
}
.paddedCell {
  @apply px-3;
}
.moduleBadge {
  @apply rounded px-2 py-1 text-[10px] font-semibold;
  background: var(--ui-surface-hover);
}
.eventCell {
  @apply px-3 font-medium;
}
.controlCell {
  @apply text-center;
}
.controlCell > * {
  @apply mx-auto;
}
.timingCell {
  @apply px-2;
  color: var(--ui-text-subtle);
}
.saveButton {
  @apply ml-auto;
}
</style>
