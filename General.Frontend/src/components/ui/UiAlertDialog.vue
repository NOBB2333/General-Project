<script setup lang="ts">
import { AlertTriangle } from 'lucide-vue-next';
import {
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogOverlay,
  AlertDialogPortal,
  AlertDialogRoot,
  AlertDialogTitle,
} from 'reka-ui';
import { computed } from 'vue';
import UiButton from './UiButton.vue';

type AlertTone = 'default' | 'warning' | 'danger';

const props = withDefaults(
  defineProps<{
    open: boolean;
    title: string;
    description?: string;
    confirmLabel?: string;
    cancelLabel?: string;
    tone?: AlertTone;
  }>(),
  {
    confirmLabel: '确认',
    cancelLabel: '取消',
    tone: 'danger',
  },
);

const emit = defineEmits<{
  'update:open': [value: boolean];
  confirm: [];
  cancel: [];
}>();

defineSlots<{
  default?: () => unknown;
}>();

const confirmVariant = computed(() => {
  if (props.tone === 'warning') return 'warning';
  if (props.tone === 'danger') return 'danger';
  return 'primary';
});
</script>

<template>
  <AlertDialogRoot
    :open="open"
    @update:open="emit('update:open', $event)"
  >
    <AlertDialogPortal to="#ui-portals">
      <AlertDialogOverlay :class="$style.overlay" />
      <AlertDialogContent
        data-ui-alert-dialog
        aria-modal="true"
        :class="$style.dialog"
      >
        <div :class="[$style.icon, $style[tone]]">
          <AlertTriangle
            :size="20"
            :stroke-width="1.8"
          />
        </div>

        <div :class="$style.content">
          <AlertDialogTitle :class="$style.title">{{ title }}</AlertDialogTitle>
          <AlertDialogDescription
            v-if="description"
            :class="$style.description"
            >{{ description }}</AlertDialogDescription
          >
          <div
            v-if="$slots.default"
            :class="$style.body"
          >
            <slot />
          </div>
        </div>

        <footer :class="$style.footer">
          <AlertDialogCancel as-child>
            <UiButton
              variant="secondary"
              @click="emit('cancel')"
              >{{ cancelLabel }}</UiButton
            >
          </AlertDialogCancel>
          <AlertDialogAction as-child>
            <UiButton
              :variant="confirmVariant"
              @click="emit('confirm')"
              >{{ confirmLabel }}</UiButton
            >
          </AlertDialogAction>
        </footer>
      </AlertDialogContent>
    </AlertDialogPortal>
  </AlertDialogRoot>
</template>

<style module>
@reference "../../styles/index.css";

.overlay {
  @apply fixed inset-0;
  z-index: var(--ui-z-elevated-overlay);
  background: color-mix(in srgb, var(--app-primary-dark) 45%, transparent);
}

.dialog {
  @apply fixed left-1/2 top-1/2 grid w-[480px] max-w-[calc(100vw-32px)] -translate-x-1/2 -translate-y-1/2 grid-cols-[40px_1fr] gap-4 overflow-hidden border p-5 shadow-2xl outline-none;
  z-index: var(--ui-z-elevated-content);
  color: var(--ui-text);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
  animation: alertDialogIn 150ms var(--ease-out-ui);
}

.icon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-full;
}

.default {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.warning {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.danger {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.content {
  @apply min-w-0;
}

.title {
  @apply text-[16px] font-semibold;
  color: var(--ui-text);
}

.description {
  @apply mt-1 text-[13px] leading-5;
  color: var(--ui-text-muted);
}

.body {
  @apply mt-3;
}

.footer {
  @apply col-span-2 mt-1 flex items-center justify-end gap-2 border-t pt-4;
  border-color: var(--ui-border-subtle);
}

@keyframes alertDialogIn {
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
    animation: alertDialogFadeIn 120ms ease-out;
  }
}

@keyframes alertDialogFadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
</style>
