<script setup lang="ts">
import { CollapsibleContent, CollapsibleRoot, CollapsibleTrigger } from 'reka-ui';

defineProps<{
  open: boolean;
  disabled?: boolean;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
}>();

defineSlots<{
  trigger: (props: { open: boolean }) => unknown;
  default: (props: { open: boolean }) => unknown;
}>();
</script>

<template>
  <CollapsibleRoot
    :open="open"
    :disabled="disabled"
    :unmount-on-hide="false"
    @update:open="emit('update:open', $event)"
  >
    <CollapsibleTrigger as-child>
      <slot
        name="trigger"
        :open="open"
      />
    </CollapsibleTrigger>
    <CollapsibleContent
      force-mount
      :class="$style.content"
    >
      <div :class="$style.contentInner">
        <slot :open="open" />
      </div>
    </CollapsibleContent>
  </CollapsibleRoot>
</template>

<style module>
@reference "../../styles/index.css";

.content {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows 150ms var(--ease-out-ui);
}

.content[data-state='open'] {
  grid-template-rows: 1fr;
}

.contentInner {
  min-height: 0;
  overflow: hidden;
}

@media (prefers-reduced-motion: reduce) {
  .content {
    transition: none;
  }
}
</style>
