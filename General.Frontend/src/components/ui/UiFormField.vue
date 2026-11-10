<script setup lang="ts">
defineProps<{
  label: string;
  required?: boolean;
  hint?: string;
  error?: string;
  wide?: boolean;
}>();
</script>

<template>
  <label :class="[$style.field, wide && $style.wide]">
    <span :class="$style.label">
      {{ label }}
      <span
        v-if="required"
        :class="$style.required"
        aria-hidden="true"
        >*</span
      >
    </span>
    <slot />
    <span
      v-if="error"
      :class="$style.error"
      >{{ error }}</span
    >
    <span
      v-else-if="hint"
      :class="$style.hint"
      >{{ hint }}</span
    >
  </label>
</template>

<style module>
@reference "../../styles/index.css";
.field {
  @apply block min-w-0;
}
.wide {
  @apply col-span-2;
}
.label {
  @apply mb-1.5 block text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}
.required {
  color: var(--ui-danger);
}
.hint {
  @apply mt-1 block text-[10px];
  color: var(--ui-text-subtle);
}
.error {
  @apply mt-1 block text-[10px];
  color: var(--ui-danger);
}
</style>
