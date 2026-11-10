<script setup lang="ts">
import { LoaderCircle } from 'lucide-vue-next';

type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'warning' | 'ghost' | 'subtle' | 'link' | 'inverted';
type ButtonSize = 'xs' | 'sm' | 'md' | 'lg' | 'icon-sm' | 'icon';

const props = withDefaults(
  defineProps<{
    variant?: ButtonVariant;
    size?: ButtonSize;
    type?: 'button' | 'submit' | 'reset';
    disabled?: boolean;
    loading?: boolean;
  }>(),
  {
    variant: 'secondary',
    size: 'md',
    type: 'button',
  },
);
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[$style.button, $style[variant], $style[size]]"
    :aria-busy="loading || undefined"
  >
    <LoaderCircle
      v-if="loading"
      :size="14"
      :class="$style.spinner"
    />
    <slot
      v-else
      name="leading"
    />
    <slot />
    <slot name="trailing" />
  </button>
</template>

<style module>
@reference "../../styles/index.css";
.button {
  @apply inline-flex shrink-0 items-center justify-center font-medium transition-colors active:scale-[.97] disabled:cursor-not-allowed disabled:opacity-50;
  border-radius: var(--app-control-radius);
}

.primary {
  color: white;
  background-color: var(--app-primary);
}
.primary:hover {
  background-color: color-mix(in srgb, var(--app-primary) 84%, black);
}
.secondary {
  @apply border;
  color: var(--ui-text-muted);
  background: var(--ui-surface);
  border-color: var(--ui-border);
}
.secondary:hover {
  background: var(--ui-surface-hover);
}
.danger {
  color: white;
  background: var(--ui-danger);
}
.danger:hover {
  background: var(--ui-danger-hover);
}
.warning {
  color: white;
  background: var(--ui-warning);
}
.warning:hover {
  background: color-mix(in srgb, var(--ui-warning) 88%, white);
}
.ghost {
  color: var(--ui-text-subtle);
}
.ghost:hover {
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}
.subtle {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.subtle:hover {
  background: var(--ui-border-subtle);
}
.link {
  color: var(--app-primary);
}
.link:hover {
  color: color-mix(in srgb, var(--app-primary) 84%, black);
  text-decoration: underline;
}

.xs {
  @apply min-h-6 gap-1 px-2 py-1 text-[11px];
}
.sm {
  @apply min-h-8 gap-1.5 px-3 py-1.5 text-[12px];
}
.md {
  @apply min-h-9 gap-1.5 px-4 py-2 text-[13px];
}
.lg {
  @apply min-h-10 gap-2 px-5 py-2.5 text-[13px];
}
.icon-sm {
  @apply h-7 w-7 p-1.5;
}
.icon {
  @apply h-8 w-8 p-2;
}
.spinner {
  @apply animate-spin;
}
</style>
