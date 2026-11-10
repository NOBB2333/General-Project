<script setup lang="ts">
import { TooltipArrow, TooltipContent, TooltipPortal, TooltipProvider, TooltipRoot, TooltipTrigger } from 'reka-ui';

withDefaults(
  defineProps<{
    content: string;
    side?: 'top' | 'right' | 'bottom' | 'left';
    align?: 'start' | 'center' | 'end';
    delayDuration?: number;
    disabled?: boolean;
  }>(),
  {
    side: 'top',
    align: 'center',
    delayDuration: 350,
  },
);

defineSlots<{
  default: () => unknown;
}>();
</script>

<template>
  <TooltipProvider :delay-duration="delayDuration">
    <TooltipRoot :disabled="disabled">
      <TooltipTrigger as-child>
        <slot />
      </TooltipTrigger>
      <TooltipPortal to="#ui-portals">
        <TooltipContent
          data-ui-tooltip
          :side="side"
          :align="align"
          :side-offset="6"
          :class="$style.content"
        >
          {{ content }}
          <TooltipArrow :class="$style.arrow" />
        </TooltipContent>
      </TooltipPortal>
    </TooltipRoot>
  </TooltipProvider>
</template>

<style module>
@reference "../../styles/index.css";

.content {
  @apply max-w-64 px-2 py-1 text-[11px] leading-4 shadow-md;
  z-index: var(--ui-z-tooltip);
  color: var(--ui-surface);
  background: var(--app-primary-dark);
  border-radius: var(--app-control-radius);
  transform-origin: var(--reka-tooltip-content-transform-origin, center);
}

.content[data-state='delayed-open'] {
  animation: tooltipIn 140ms var(--ease-out-ui);
}

.content[data-state='closed'] {
  animation: tooltipOut 90ms ease-in;
}

.arrow {
  fill: var(--app-primary-dark);
}

@keyframes tooltipIn {
  from {
    opacity: 0;
    transform: scale(0.97);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes tooltipOut {
  from {
    opacity: 1;
  }
  to {
    opacity: 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  .content[data-state='delayed-open'],
  .content[data-state='closed'] {
    animation: none;
  }
}
</style>
