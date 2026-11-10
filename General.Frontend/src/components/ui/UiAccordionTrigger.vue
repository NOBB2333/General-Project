<script setup lang="ts">
import { AccordionHeader, AccordionTrigger } from 'reka-ui';

defineOptions({ inheritAttrs: false });
</script>

<template>
  <AccordionHeader
    as="h3"
    :class="$style.header"
  >
    <AccordionTrigger
      v-bind="$attrs"
      :class="$style.trigger"
    >
      <slot />
    </AccordionTrigger>
  </AccordionHeader>
</template>

<style module>
@reference "../../styles/index.css";

.header {
  @apply m-0 flex;
}

.trigger {
  @apply inline-flex h-5 w-5 shrink-0 items-center justify-center outline-none;
  color: var(--ui-text-subtle);
  border-radius: var(--app-compact-control-radius);
}

.trigger:hover {
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}

.trigger:focus-visible {
  outline: 2px solid var(--ui-focus);
  outline-offset: 1px;
}
</style>
