<script setup lang="ts">
import { AccordionItem } from 'reka-ui';

defineProps<{
  value: string;
  disabled?: boolean;
}>();
</script>

<template>
  <AccordionItem
    :value="value"
    :disabled="disabled"
    :class="$style.item"
  >
    <slot />
  </AccordionItem>
</template>

<style module>
@reference "../../styles/index.css";

.item {
  @apply min-w-0;
}
</style>
