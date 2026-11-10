<script setup lang="ts">
import { AccordionContent } from 'reka-ui';
</script>

<template>
  <AccordionContent :class="$style.content">
    <div :class="$style.contentInner">
      <slot />
    </div>
  </AccordionContent>
</template>

<style module>
@reference "../../styles/index.css";

.content {
  @apply overflow-hidden;
}

.content[data-state='open'] {
  animation: accordionOpen 150ms var(--ease-out-ui);
}

.content[data-state='closed'] {
  animation: accordionClose 100ms ease-in;
}

.contentInner {
  @apply min-w-0;
}

@keyframes accordionOpen {
  from {
    height: 0;
    opacity: 0;
  }
  to {
    height: var(--reka-accordion-content-height);
    opacity: 1;
  }
}

@keyframes accordionClose {
  from {
    height: var(--reka-accordion-content-height);
    opacity: 1;
  }
  to {
    height: 0;
    opacity: 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  .content[data-state='open'],
  .content[data-state='closed'] {
    animation: none;
  }
}
</style>
