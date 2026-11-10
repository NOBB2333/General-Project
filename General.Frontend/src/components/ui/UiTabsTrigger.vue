<script setup lang="ts">
import { TabsTrigger } from 'reka-ui';

defineOptions({ inheritAttrs: false });

withDefaults(
  defineProps<{
    value: string;
    disabled?: boolean;
    variant?: 'default' | 'inverted' | 'warning';
  }>(),
  {
    variant: 'default',
  },
);
</script>

<template>
  <TabsTrigger
    v-bind="$attrs"
    :value="value"
    :disabled="disabled"
    :class="[$style.trigger, $style[variant]]"
  >
    <slot />
  </TabsTrigger>
</template>

<style module>
@reference "../../styles/index.css";

.trigger {
  @apply relative inline-flex min-h-8 items-center justify-center gap-1.5 rounded px-3 py-1.5 text-[12px] font-medium outline-none transition-colors disabled:cursor-not-allowed disabled:opacity-50;
  color: var(--ui-text-muted);
}

.trigger:hover {
  background: var(--ui-surface-hover);
}

.trigger:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.default[data-state='active'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.warning[data-state='active'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.inverted {
  color: color-mix(in srgb, var(--ui-surface) 78%, transparent);
}

.inverted:hover {
  color: var(--ui-surface);
  background: color-mix(in srgb, var(--app-primary-dark) 18%, transparent);
}

.inverted[data-state='active'] {
  color: var(--ui-text);
  background: var(--ui-surface);
  box-shadow: 0 1px 3px color-mix(in srgb, var(--app-primary-dark) 20%, transparent);
}

@media (prefers-reduced-motion: reduce) {
  .trigger {
    transition: none;
  }
}
</style>
