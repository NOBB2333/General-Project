<script setup lang="ts">
import { PopoverContent, PopoverPortal, PopoverRoot, PopoverTrigger } from 'reka-ui';

const props = withDefaults(
  defineProps<{
    open?: boolean;
    side?: 'top' | 'right' | 'bottom' | 'left';
    align?: 'start' | 'center' | 'end';
    size?: 'xs' | 'sm' | 'md' | 'lg';
    sideOffset?: number;
  }>(),
  { side: 'bottom', align: 'end', size: 'md', sideOffset: 6 },
);

const emit = defineEmits<{ 'update:open': [value: boolean] }>();
</script>

<template>
  <PopoverRoot
    :open="open"
    @update:open="emit('update:open', $event)"
  >
    <PopoverTrigger as-child><slot name="trigger" /></PopoverTrigger>
    <PopoverPortal to="#ui-portals">
      <PopoverContent
        data-ui-popover
        :class="[$style.popover, $style[size]]"
        :side="side"
        :align="align"
        :side-offset="sideOffset"
      >
        <slot />
      </PopoverContent>
    </PopoverPortal>
  </PopoverRoot>
</template>

<style module>
@reference "../../styles/index.css";
.popover {
  @apply overflow-hidden border bg-white shadow-xl outline-none;
  z-index: var(--ui-z-floating);
  color: var(--ui-text);
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
  animation: popoverIn 150ms var(--ease-out-ui);
}

.popover[data-side='top'] {
  transform-origin: bottom;
}
.popover[data-side='right'] {
  transform-origin: left;
}
.popover[data-side='bottom'] {
  transform-origin: top;
}
.popover[data-side='left'] {
  transform-origin: right;
}
.xs {
  @apply w-[210px];
}
.sm {
  @apply w-[240px];
}
.md {
  @apply w-[340px];
}
.lg {
  @apply w-[420px];
}

@keyframes popoverIn {
  from {
    opacity: 0;
    transform: scale(0.97) translateY(-2px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .popover {
    animation: popoverFadeIn 120ms ease-out;
  }
}

@keyframes popoverFadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
</style>
