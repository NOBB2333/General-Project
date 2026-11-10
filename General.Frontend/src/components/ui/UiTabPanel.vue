<script setup lang="ts">
import { TabsContent } from 'reka-ui';

defineOptions({ inheritAttrs: false });
defineProps<{ value: string }>();
</script>

<template>
  <TabsContent
    v-bind="$attrs"
    :value="value"
    :class="$style.panel"
  >
    <slot />
  </TabsContent>
</template>

<style module>
@reference "../../styles/index.css";

.panel {
  @apply outline-none;
}

.panel:focus-visible {
  box-shadow: inset 0 0 0 2px var(--ui-focus);
}
</style>
