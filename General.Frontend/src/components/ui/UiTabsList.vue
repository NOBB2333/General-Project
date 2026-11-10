<script setup lang="ts">
import { TabsList } from 'reka-ui';

defineOptions({ inheritAttrs: false });
defineProps<{ label: string }>();
</script>

<template>
  <TabsList
    v-bind="$attrs"
    :aria-label="label"
    :class="$style.list"
  >
    <slot />
  </TabsList>
</template>

<style module>
@reference "../../styles/index.css";

.list {
  @apply inline-flex items-center gap-1;
}
</style>
