<script setup lang="ts">
import { CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList, CommandRoot } from 'cmdk-vue';
import { CornerDownLeft, Search } from 'lucide-vue-next';
import { computed, nextTick, onMounted, ref } from 'vue';
import type { UiCommandItem } from './UiCommand.types';

const props = withDefaults(
  defineProps<{
    modelValue: string;
    items: readonly UiCommandItem[];
    label?: string;
    inputLabel?: string;
    placeholder?: string;
    emptyText?: string;
    hint?: string;
    autofocus?: boolean;
  }>(),
  {
    label: '命令菜单',
    inputLabel: '搜索命令',
    placeholder: '搜索...',
    emptyText: '没有找到匹配结果',
    hint: undefined,
    autofocus: true,
  },
);

const emit = defineEmits<{
  'update:modelValue': [value: string];
  select: [item: UiCommandItem];
  escape: [];
}>();

const container = ref<HTMLElement | null>(null);

const groups = computed(() => {
  const grouped = new Map<string, UiCommandItem[]>();
  for (const item of props.items) {
    const group = item.group || '命令';
    const current = grouped.get(group) ?? [];
    current.push(item);
    grouped.set(group, current);
  }
  return [...grouped.entries()].map(([label, items]) => ({ label, items }));
});

onMounted(() => {
  if (!props.autofocus) return;
  void nextTick(() => container.value?.querySelector<HTMLInputElement>('[cmdk-input]')?.focus());
});

function selectItem(item: UiCommandItem) {
  if (item.disabled) return;
  emit('select', item);
}
</script>

<template>
  <div
    ref="container"
    data-ui-command
    :class="$style.container"
    @keydown.esc="emit('escape')"
  >
    <CommandRoot
      :label="label"
      :class="$style.command"
      loop
    >
      <div :class="$style.inputRow">
        <Search
          :size="15"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <CommandInput
          :model-value="modelValue"
          :placeholder="placeholder"
          :aria-label="inputLabel"
          :class="$style.input"
          @update:model-value="emit('update:modelValue', $event)"
        />
      </div>

      <CommandList
        :label="`${label}结果`"
        :class="$style.list"
      >
        <CommandEmpty :class="$style.empty">{{ emptyText }}</CommandEmpty>

        <CommandGroup
          v-for="group in groups"
          :key="group.label"
          :heading="group.label"
          :class="$style.group"
        >
          <CommandItem
            v-for="item in group.items"
            :key="item.id"
            :value="`${item.label} ${item.id}`"
            :keywords="[item.label, item.description || '', ...(item.keywords || [])]"
            :disabled="item.disabled"
            :class="$style.item"
            @select="selectItem(item)"
          >
            <span :class="$style.itemCopy">
              <strong>{{ item.label }}</strong>
              <small v-if="item.description">{{ item.description }}</small>
            </span>
            <kbd v-if="item.shortcut">{{ item.shortcut }}</kbd>
          </CommandItem>
        </CommandGroup>
      </CommandList>

      <footer
        v-if="hint"
        :class="$style.footer"
      >
        <span>{{ hint }}</span>
        <span :class="$style.enterHint">
          <CornerDownLeft
            :size="12"
            :stroke-width="1.8"
          />
          选择
        </span>
      </footer>
    </CommandRoot>
  </div>
</template>

<style module>
@reference "../../styles/index.css";

.container {
  @apply p-2;
}

.command {
  @apply overflow-hidden;
  color: var(--ui-text);
  background: var(--ui-surface);
  border-radius: var(--app-control-radius);
}

.inputRow {
  @apply flex items-center gap-2 border px-3 py-2;
  color: var(--ui-text-subtle);
  border-color: var(--ui-focus);
  border-radius: var(--app-control-radius);
  box-shadow: 0 0 0 1px color-mix(in srgb, var(--ui-focus) 18%, transparent);
}

.input {
  @apply min-w-0 flex-1 bg-transparent text-[12px] outline-none;
  color: var(--ui-text);
}

.input::placeholder {
  color: var(--ui-text-subtle);
}

.list {
  @apply mt-2 max-h-[280px] overflow-y-auto;
  scroll-padding-block: 4px;
  scrollbar-color: var(--ui-border) transparent;
  scrollbar-width: thin;
}

.list::-webkit-scrollbar {
  width: 6px;
}

.list::-webkit-scrollbar-thumb {
  background: var(--ui-border);
  border-radius: 999px;
}

.group :global([cmdk-group-heading]) {
  @apply px-2 pb-1 pt-2 text-[10px] font-semibold uppercase tracking-wide;
  color: var(--ui-text-subtle);
}

.item {
  @apply flex min-h-10 cursor-default select-none items-center gap-2 px-2 py-1.5 text-[12px] outline-none transition-colors;
  border-radius: var(--app-control-radius);
}

.item[data-selected='true'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.item[data-disabled='true'] {
  @apply pointer-events-none opacity-45;
}

.itemCopy {
  @apply min-w-0 flex-1;
}

.itemCopy strong,
.itemCopy small {
  @apply block truncate;
}

.itemCopy strong {
  @apply font-medium;
}

.itemCopy small {
  @apply mt-0.5 text-[10px];
  color: var(--ui-text-subtle);
}

.item kbd {
  @apply shrink-0 border px-1.5 py-0.5 text-[9px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: var(--app-compact-control-radius);
}

.empty {
  @apply px-3 py-8 text-center text-[11px];
  color: var(--ui-text-subtle);
}

.footer {
  @apply mt-2 flex items-center justify-between border-t px-2 pt-2 text-[10px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}

.enterHint {
  @apply flex items-center gap-1;
}

@media (prefers-reduced-motion: reduce) {
  .item {
    transition: none;
  }
}
</style>
