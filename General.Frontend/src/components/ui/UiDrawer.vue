<script setup lang="ts">
import { X } from 'lucide-vue-next';
import { DrawerContent, DrawerDescription, DrawerOverlay, DrawerPortal, DrawerRoot, DrawerTitle } from 'reka-ui';
import { ref, watch } from 'vue';
import UiButton from './UiButton.vue';

type DrawerSize = 'sm' | 'md' | 'lg';

const props = withDefaults(
  defineProps<{
    open: boolean;
    title: string;
    description?: string;
    size?: DrawerSize;
    closeLabel?: string;
    layer?: 'standard' | 'top';
  }>(),
  {
    description: undefined,
    size: 'md',
    closeLabel: '关闭',
    layer: 'standard',
  },
);

const emit = defineEmits<{
  'update:open': [value: boolean];
  close: [];
}>();

defineSlots<{
  default?: (props: { close: () => void }) => unknown;
  icon?: () => unknown;
  description?: () => unknown;
  actions?: (props: { close: () => void }) => unknown;
  footer?: (props: { close: () => void }) => unknown;
}>();

const returnFocus = ref<HTMLElement | null>(null);

watch(
  () => props.open,
  (open) => {
    if (open && document.activeElement instanceof HTMLElement) {
      returnFocus.value = document.activeElement;
    }
  },
);

function updateOpen(value: boolean) {
  emit('update:open', value);
  if (!value) emit('close');
}

function restoreFocus(event: Event) {
  event.preventDefault();
  const target = returnFocus.value;
  window.setTimeout(() => {
    if (target?.isConnected) target.focus();
  });
}
</script>

<template>
  <DrawerRoot
    :open="open"
    :modal="true"
    swipe-direction="right"
    @update:open="updateOpen"
  >
    <DrawerPortal to="#ui-portals">
      <DrawerOverlay
        data-ui-drawer-overlay
        :class="[$style.overlay, layer === 'top' && $style.overlayTop]"
      />
      <DrawerContent
        data-ui-drawer
        :final-focus="returnFocus"
        :class="[$style.drawer, $style[size], layer === 'top' && $style.drawerTop]"
        @close-auto-focus="restoreFocus"
      >
        <header :class="$style.header">
          <slot name="icon" />
          <div :class="$style.heading">
            <DrawerTitle :class="$style.title">{{ title }}</DrawerTitle>
            <DrawerDescription
              v-if="description || $slots.description"
              :class="$style.description"
            >
              <slot name="description">{{ description }}</slot>
            </DrawerDescription>
          </div>
          <div
            v-if="$slots.actions"
            :class="$style.actions"
          >
            <slot
              name="actions"
              :close="() => updateOpen(false)"
            />
          </div>
          <UiButton
            :aria-label="closeLabel"
            :title="closeLabel"
            variant="ghost"
            size="icon-sm"
            @click="updateOpen(false)"
          >
            <X
              :size="16"
              :stroke-width="1.8"
            />
          </UiButton>
        </header>

        <div :class="$style.body">
          <slot :close="() => updateOpen(false)" />
        </div>

        <footer
          v-if="$slots.footer"
          :class="$style.footer"
        >
          <slot
            name="footer"
            :close="() => updateOpen(false)"
          />
        </footer>
      </DrawerContent>
    </DrawerPortal>
  </DrawerRoot>
</template>

<style module>
@reference "../../styles/index.css";

.overlay {
  @apply fixed inset-0;
  z-index: var(--ui-z-drawer-overlay);
  background: color-mix(in srgb, var(--app-primary-dark) 42%, transparent);
}

.overlayTop {
  z-index: var(--ui-z-elevated-overlay);
}

.overlay[data-state='open'] {
  animation: drawerOverlayIn 150ms var(--ease-out-ui);
}

.overlay[data-state='closed'] {
  animation: drawerOverlayOut 100ms ease-in;
}

.drawer {
  @apply fixed bottom-0 right-0 top-0 flex max-w-full flex-col border-l shadow-2xl outline-none;
  z-index: var(--ui-z-drawer-content);
  color: var(--ui-text);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  transform: translateX(var(--reka-drawer-swipe-movement-x, 0px));
  transform-origin: right center;
}

.drawerTop {
  z-index: var(--ui-z-elevated-content);
}

.drawer[data-state='open'] {
  animation: drawerIn 160ms var(--ease-out-ui);
}

.drawer[data-state='closed'] {
  animation: drawerOut 105ms ease-in;
}

.drawer[data-swiping] {
  animation: none;
  user-select: none;
}

.sm {
  @apply w-[480px];
}

.md {
  @apply w-[620px];
}

.lg {
  @apply w-[760px];
}

.header {
  @apply flex shrink-0 items-start gap-3 border-b px-5 py-3;
  border-color: var(--ui-border);
  background: var(--ui-surface);
}

.heading {
  @apply min-w-0 flex-1;
}

.title {
  @apply text-[15px] font-semibold;
  color: var(--ui-text);
}

.description {
  @apply mt-0.5 text-[12px] leading-5;
  color: var(--ui-text-subtle);
}

.actions {
  @apply flex shrink-0 items-center gap-2;
}

.body {
  @apply min-h-0 flex-1 overflow-y-auto;
  scrollbar-color: var(--ui-border) transparent;
  scrollbar-width: thin;
}

.body::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

.body::-webkit-scrollbar-thumb {
  background: var(--ui-border);
  border-radius: 999px;
}

.body::-webkit-scrollbar-track {
  background: transparent;
}

.footer {
  @apply flex shrink-0 items-center justify-end gap-2 border-t px-5 py-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

@keyframes drawerIn {
  from {
    opacity: 0;
    transform: translateX(100%);
  }
  to {
    opacity: 1;
    transform: translateX(var(--reka-drawer-swipe-movement-x, 0px));
  }
}

@keyframes drawerOut {
  from {
    opacity: 1;
    transform: translateX(var(--reka-drawer-swipe-movement-x, 0px));
  }
  to {
    opacity: 0;
    transform: translateX(100%);
  }
}

@keyframes drawerOverlayIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes drawerOverlayOut {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  .drawer[data-state='open'],
  .drawer[data-state='closed'],
  .overlay[data-state='open'],
  .overlay[data-state='closed'] {
    animation: none;
  }
}
</style>
