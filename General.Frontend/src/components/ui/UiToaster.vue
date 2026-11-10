<script setup lang="ts">
import { CircleCheck, CircleX, Info, LoaderCircle, TriangleAlert, X } from 'lucide-vue-next';
import { Toaster } from 'vue-sonner';
import 'vue-sonner/style.css';

withDefaults(
  defineProps<{
    duration?: number;
    visibleToasts?: number;
  }>(),
  {
    duration: 2200,
    visibleToasts: 4,
  },
);
</script>

<template>
  <Teleport to="#ui-portals">
    <Toaster
      data-ui-toaster
      position="top-right"
      close-button-position="top-right"
      :duration="duration"
      :visible-toasts="visibleToasts"
      :gap="8"
      :offset="{ top: 64, right: 24 }"
      :swipe-directions="['right']"
      :toast-options="{
        unstyled: true,
        classes: {
          toast: $style.toast,
          title: $style.title,
          description: $style.description,
          content: $style.content,
          icon: $style.icon,
          closeButton: $style.closeButton,
          actionButton: $style.actionButton,
          cancelButton: $style.cancelButton,
          success: $style.success,
          info: $style.info,
          warning: $style.warning,
          error: $style.error,
          loading: $style.loading,
        },
      }"
      :class="$style.toaster"
      close-button
      container-aria-label="系统通知"
    >
      <template #success-icon>
        <CircleCheck
          :size="16"
          :stroke-width="1.8"
        />
      </template>
      <template #info-icon>
        <Info
          :size="16"
          :stroke-width="1.8"
        />
      </template>
      <template #warning-icon>
        <TriangleAlert
          :size="16"
          :stroke-width="1.8"
        />
      </template>
      <template #error-icon>
        <CircleX
          :size="16"
          :stroke-width="1.8"
        />
      </template>
      <template #loading-icon>
        <LoaderCircle
          :size="16"
          :stroke-width="1.8"
          :class="$style.spinner"
        />
      </template>
      <template #close-icon>
        <X
          :size="12"
          :stroke-width="1.8"
        />
      </template>
    </Toaster>
  </Teleport>
</template>

<style module>
@reference "../../styles/index.css";

.toaster {
  --width: 360px;
  z-index: var(--ui-z-toast);
  font-family: inherit;
}

.toast {
  @apply flex w-full items-start gap-2 border px-4 py-3 text-[12px] shadow-xl;
  color: var(--ui-text);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}

.title {
  @apply font-medium leading-5;
  color: var(--ui-text);
}

.description {
  @apply mt-0.5 text-[11px] leading-4;
  color: var(--ui-text-subtle);
}

.content {
  @apply min-w-0 flex-1;
}

.icon {
  @apply mt-0.5 inline-flex shrink-0;
}

.success .icon {
  color: var(--ui-success);
}

.info .icon {
  color: var(--app-primary);
}

.warning .icon {
  color: var(--ui-warning);
}

.error .icon {
  color: var(--ui-danger);
}

.loading .icon {
  color: var(--app-primary);
}

.closeButton {
  @apply absolute -right-1.5 -top-1.5 flex h-5 w-5 items-center justify-center border opacity-0 transition-opacity;
  color: var(--ui-text-subtle);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: 999px;
}

.toast:hover .closeButton,
.closeButton:focus-visible {
  opacity: 1;
}

.closeButton:focus-visible {
  outline: 2px solid var(--ui-focus);
  outline-offset: 1px;
}

.actionButton,
.cancelButton {
  @apply ml-auto shrink-0 border px-2 py-1 text-[11px] font-medium transition-colors;
  border-radius: var(--app-control-radius);
}

.actionButton {
  color: white;
  background: var(--app-primary);
  border-color: var(--app-primary);
}

.actionButton:hover {
  background: color-mix(in srgb, var(--app-primary) 84%, black);
}

.cancelButton {
  color: var(--ui-text-muted);
  background: var(--ui-surface);
  border-color: var(--ui-border);
}

.spinner {
  animation: toasterSpin 800ms linear infinite;
}

.toaster :global([data-sonner-toast]) {
  transition:
    transform 160ms var(--ease-out-ui),
    opacity 140ms ease-out,
    height 160ms var(--ease-out-ui);
}

@keyframes toasterSpin {
  to {
    transform: rotate(360deg);
  }
}

@media (prefers-reduced-motion: reduce) {
  .toaster :global([data-sonner-toast]) {
    transition-property: opacity, height;
  }

  .spinner {
    animation: none;
  }
}
</style>
