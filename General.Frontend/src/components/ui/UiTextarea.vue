<script setup lang="ts">
defineOptions({ inheritAttrs: false });
defineProps<{ modelValue: string; invalid?: boolean }>();
const emit = defineEmits<{ 'update:modelValue': [value: string] }>();
</script>

<template>
  <textarea
    v-bind="$attrs"
    :value="modelValue"
    :aria-invalid="invalid || undefined"
    :class="[$style.textarea, invalid && $style.invalid]"
    @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
  />
</template>

<style module>
@reference "../../styles/index.css";
.textarea {
  @apply w-full resize-y border px-3 py-2 text-[12px] leading-5 outline-none transition-colors;
  color: var(--ui-text);
  background: var(--ui-surface);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.textarea:focus {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 1px var(--ui-focus);
}
.invalid {
  border-color: var(--ui-danger);
}
</style>
