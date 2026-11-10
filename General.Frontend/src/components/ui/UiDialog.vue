<script setup lang="ts">
import { X } from 'lucide-vue-next';
import { DialogContent, DialogDescription, DialogOverlay, DialogPortal, DialogRoot, DialogTitle } from 'reka-ui';
import UiButton from './UiButton.vue';

type DialogSize = 'sm' | 'md' | 'lg' | 'xl';
const props = withDefaults(
  defineProps<{
    open: boolean;
    title?: string;
    description?: string;
    size?: DialogSize;
    bare?: boolean;
    closeLabel?: string;
    layer?: 'standard' | 'top';
    overlayClass?: string;
    contentClass?: string;
  }>(),
  { size: 'md', closeLabel: '关闭', layer: 'standard' },
);

const emit = defineEmits<{ 'update:open': [value: boolean]; close: [] }>();
defineSlots<{
  default?: (props: { close: () => void }) => unknown;
  icon?: () => unknown;
  actions?: (props: { close: () => void }) => unknown;
  footer?: (props: { close: () => void }) => unknown;
}>();

function updateOpen(value: boolean) {
  emit('update:open', value);
  if (!value) emit('close');
}
</script>

<template>
  <DialogRoot
    :open="open"
    @update:open="updateOpen"
  >
    <DialogPortal to="#ui-portals">
      <DialogOverlay :class="[$style.overlay, layer === 'top' && $style.overlayTop, overlayClass]" />
      <DialogContent
        data-ui-dialog
        aria-modal="true"
        :class="[$style.dialog, $style[size], layer === 'top' && $style.dialogTop, contentClass]"
        @open-auto-focus="$emit('update:open', true)"
      >
        <template v-if="bare">
          <DialogTitle :class="$style.visuallyHidden">{{ title }}</DialogTitle>
          <DialogDescription
            v-if="description"
            :class="$style.visuallyHidden"
            >{{ description }}</DialogDescription
          >
          <slot :close="() => updateOpen(false)" />
        </template>
        <template v-else>
          <header :class="$style.header">
            <slot name="icon" />
            <div :class="$style.heading">
              <DialogTitle :class="$style.title">{{ title }}</DialogTitle>
              <DialogDescription
                v-if="description"
                :class="$style.description"
                >{{ description }}</DialogDescription
              >
            </div>
            <slot
              name="actions"
              :close="() => updateOpen(false)"
            />
            <UiButton
              :aria-label="closeLabel"
              :title="closeLabel"
              variant="ghost"
              size="icon-sm"
              @click="updateOpen(false)"
            >
              <X :size="16" />
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
        </template>
      </DialogContent>
    </DialogPortal>
  </DialogRoot>
</template>

<style module>
@reference "../../styles/index.css";
.overlay {
  @apply fixed inset-0;
  z-index: var(--ui-z-modal-overlay);
  background: color-mix(in srgb, var(--app-primary-dark) 45%, transparent);
}

.overlayTop {
  z-index: var(--ui-z-elevated-overlay);
}

.dialog {
  @apply fixed left-1/2 top-1/2 max-h-[85vh] -translate-x-1/2 -translate-y-1/2 overflow-hidden border bg-white shadow-2xl outline-none;
  z-index: var(--ui-z-modal-content);
  color: var(--ui-text);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
  animation: dialogIn 150ms var(--ease-out-ui);
  transform-origin: center;
}

.dialogTop {
  z-index: var(--ui-z-elevated-content);
}
.sm {
  @apply w-[480px];
}
.md {
  @apply w-[620px];
}
.lg {
  @apply w-[720px];
}
.xl {
  @apply w-[900px];
}
.visuallyHidden {
  @apply sr-only;
}
.header {
  @apply flex items-start gap-3 border-b px-5 py-4;
  border-color: var(--ui-border);
}
.heading {
  @apply min-w-0 flex-1;
}
.title {
  @apply text-[16px] font-semibold;
  color: var(--ui-text);
}
.description {
  @apply mt-1 text-[12px] leading-5;
  color: var(--ui-text-subtle);
}
.body {
  @apply max-h-[calc(85vh-130px)] overflow-y-auto;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}
.body::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
.body::-webkit-scrollbar-thumb {
  background: #c1c7d0;
  border-radius: 999px;
}
.body::-webkit-scrollbar-track {
  background: transparent;
}
.footer {
  @apply flex items-center justify-end gap-2 border-t px-5 py-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

@keyframes dialogIn {
  from {
    opacity: 0;
    transform: translate(-50%, -50%) scale(0.97);
  }
  to {
    opacity: 1;
    transform: translate(-50%, -50%) scale(1);
  }
}

@media (prefers-reduced-motion: reduce) {
  .dialog {
    animation: dialogFadeIn 120ms ease-out;
  }
}

@keyframes dialogFadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
</style>
