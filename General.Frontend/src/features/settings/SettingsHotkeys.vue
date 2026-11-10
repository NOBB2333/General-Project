<script setup lang="ts">
import { RotateCcw } from 'lucide-vue-next';
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { formatHotkey, useHotkeyPreferences } from '@/components/layout/useHotkeyPreferences';
import type { HotkeyId } from '@/components/layout/useHotkeyPreferences';
import { UiButton, UiSwitch } from '@/components/ui';
import SettingsPanel from './SettingsPanel.vue';

const emit = defineEmits<{ notify: [message: string] }>();
const props = defineProps<{ availableHotkeyIds: readonly HotkeyId[] }>();
const { hotkeyGroups: groups, resetHotkeys, saveHotkeys } = useHotkeyPreferences();
const recordingId = ref('');
const visibleGroups = computed(() =>
  groups
    .map((group) => ({ ...group, items: group.items.filter((item) => props.availableHotkeyIds.includes(item.id)) }))
    .filter((group) => group.items.length > 0),
);

function restore() {
  resetHotkeys();
  recordingId.value = '';
  emit('notify', '快捷键已恢复默认');
}

function toggleItem(id: string) {
  if (!props.availableHotkeyIds.includes(id as HotkeyId)) return;
  const item = groups.flatMap((group) => group.items).find((entry) => entry.id === id);
  if (!item) return;
  item.enabled = !item.enabled;
  saveHotkeys();
  emit('notify', `${item.name}已${item.enabled ? '启用' : '停用'}`);
}

function record(event: KeyboardEvent) {
  if (!recordingId.value) return;
  event.preventDefault();
  event.stopPropagation();
  const shortcut = formatHotkey(event);
  if (!shortcut || ['⌘', 'Ctrl', '⌥', '⇧'].includes(shortcut)) return;
  const item = groups.flatMap((group) => group.items).find((entry) => entry.id === recordingId.value);
  const duplicate = groups
    .flatMap((group) => group.items)
    .find((entry) => entry.id !== recordingId.value && entry.shortcut === shortcut);
  if (duplicate) {
    emit('notify', `该组合已用于“${duplicate.name}”，请重新录入`);
    return;
  }
  if (item) {
    item.shortcut = shortcut;
    saveHotkeys();
  }
  recordingId.value = '';
  emit('notify', `快捷键已更新为 ${shortcut}`);
}

onMounted(() => window.addEventListener('keydown', record, true));
onUnmounted(() => window.removeEventListener('keydown', record, true));
</script>

<template>
  <SettingsPanel
    title="热键管理"
    description="自定义全局快捷键，点击按键组合可重新录入"
  >
    <template #actions>
      <UiButton
        size="sm"
        @click="restore"
      >
        <template #leading
          ><RotateCcw
            :size="14"
            :stroke-width="1.8"
        /></template>
        恢复默认
      </UiButton>
    </template>

    <section
      v-for="group in visibleGroups"
      :key="group.title"
      :class="$style.hotkeyGroup"
    >
      <header :class="$style.groupHeader">
        <strong :class="$style.groupTitle">{{ group.title }}</strong>
        <p :class="$style.groupDescription">{{ group.description }}</p>
      </header>
      <div
        v-for="item in group.items"
        :key="item.id"
        :class="$style.hotkeyRow"
        :data-hotkey-id="item.id"
      >
        <div :class="$style.hotkeyContent">
          <strong :class="$style.hotkeyName">{{ item.name }}</strong>
          <p :class="$style.hotkeyDescription">{{ item.description }}</p>
        </div>
        <button
          :class="[$style.shortcutButton, recordingId === item.id && $style.shortcutRecording]"
          :data-hotkey-recording="recordingId === item.id ? 'true' : undefined"
          @click="recordingId = item.id"
        >
          {{ recordingId === item.id ? '请按键…' : item.shortcut }}
        </button>
        <UiSwitch
          :model-value="item.enabled"
          size="sm"
          :label="`${item.name}启用状态`"
          @update:model-value="toggleItem(item.id)"
        />
      </div>
    </section>

    <aside :class="$style.hintPanel">
      <strong :class="$style.hintTitle">提示</strong>
      按下 ⌘ / 可随时查看所有快捷键。快捷键在输入框中输入时不会触发（搜索快捷键除外）。
    </aside>
  </SettingsPanel>
</template>

<style module>
@reference "../../styles/index.css";
.hotkeyGroup {
  @apply mt-5 overflow-hidden rounded-lg border;
  border-color: var(--ui-border);
}
.groupHeader {
  @apply px-4 py-3;
  background: var(--ui-surface-subtle);
}
.groupTitle {
  @apply text-[14px];
}
.groupDescription {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.hotkeyRow {
  @apply flex h-[59.5px] items-center gap-3 border-t px-4;
  border-color: var(--ui-border);
}
.hotkeyContent {
  @apply min-w-0 flex-1;
}
.hotkeyName {
  @apply text-[13px];
}
.hotkeyDescription {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.shortcutButton {
  @apply min-w-[45px] rounded border px-3 py-2 text-[11px];
  color: var(--ui-text);
  border-color: var(--ui-border);
}
.shortcutButton:hover {
  background: var(--ui-surface-hover);
}
.shortcutRecording {
  color: var(--app-primary);
  border-color: var(--ui-focus);
  background: var(--app-primary-soft);
}
.hintPanel {
  @apply mt-5 rounded border p-4 text-[11px] leading-5;
  color: var(--ui-text-muted);
  border-color: color-mix(in srgb, var(--app-primary) 35%, white);
  background: color-mix(in srgb, var(--app-primary-soft) 30%, transparent);
}
.hintTitle {
  @apply block text-[12px];
}
</style>
