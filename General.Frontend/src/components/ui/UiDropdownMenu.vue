<script setup lang="ts">
import {
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuPortal,
  DropdownMenuRoot,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from 'reka-ui';
import type { UiDropdownMenuItem as MenuItem } from './UiDropdownMenu.types';

withDefaults(
  defineProps<{
    items: readonly MenuItem[];
    label: string;
    align?: 'start' | 'center' | 'end';
    disabled?: boolean;
  }>(),
  {
    align: 'end',
  },
);

const emit = defineEmits<{
  select: [id: string];
  'update:open': [value: boolean];
}>();

defineSlots<{
  trigger: () => unknown;
}>();
</script>

<template>
  <DropdownMenuRoot @update:open="emit('update:open', $event)">
    <DropdownMenuTrigger
      as-child
      :disabled="disabled"
      :aria-label="label"
    >
      <slot name="trigger" />
    </DropdownMenuTrigger>

    <DropdownMenuPortal to="#ui-portals">
      <DropdownMenuContent
        data-ui-dropdown-menu
        :align="align"
        :side-offset="4"
        :class="$style.content"
      >
        <template
          v-for="item in items"
          :key="item.id"
        >
          <DropdownMenuSeparator
            v-if="item.separatorBefore"
            :class="$style.separator"
          />
          <DropdownMenuItem
            :disabled="item.disabled"
            :class="[$style.item, item.tone === 'danger' && $style.dangerItem]"
            @select="emit('select', item.id)"
          >
            <span>{{ item.label }}</span>
            <kbd
              v-if="item.shortcut"
              :class="$style.shortcut"
              >{{ item.shortcut }}</kbd
            >
          </DropdownMenuItem>
        </template>
      </DropdownMenuContent>
    </DropdownMenuPortal>
  </DropdownMenuRoot>
</template>

<style module>
@reference "../../styles/index.css";

.content {
  @apply min-w-40 overflow-hidden border p-1 shadow-lg outline-none;
  z-index: var(--ui-z-floating);
  color: var(--ui-text);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
  transform-origin: var(--reka-dropdown-menu-content-transform-origin, top right);
}

.content[data-state='open'] {
  animation: dropdownIn 150ms var(--ease-out-ui);
}

.content[data-state='closed'] {
  animation: dropdownOut 100ms ease-in;
}

.item {
  @apply flex min-h-8 cursor-default select-none items-center justify-between gap-4 rounded px-2.5 py-1.5 text-[12px] outline-none transition-colors;
}

.item[data-highlighted] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.item[data-disabled] {
  @apply pointer-events-none opacity-45;
}

.dangerItem {
  color: var(--ui-danger);
}

.dangerItem[data-highlighted] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.shortcut {
  @apply font-sans text-[10px];
  color: var(--ui-text-subtle);
}

.separator {
  @apply my-1 h-px;
  background: var(--ui-border-subtle);
}

@keyframes dropdownIn {
  from {
    opacity: 0;
    transform: scale(0.97);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes dropdownOut {
  from {
    opacity: 1;
    transform: scale(1);
  }
  to {
    opacity: 0;
    transform: scale(0.98);
  }
}

@media (prefers-reduced-motion: reduce) {
  .content[data-state='open'],
  .content[data-state='closed'] {
    animation: none;
  }
}
</style>
