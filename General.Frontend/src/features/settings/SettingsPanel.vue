<script setup lang="ts">
withDefaults(
  defineProps<{
    title: string;
    description: string;
    minHeight?: 'none' | 'tall' | 'empty';
  }>(),
  { minHeight: 'none' },
);
</script>

<template>
  <article :class="[$style.panel, $style[minHeight]]">
    <header :class="$style.header">
      <div :class="$style.heading">
        <h2 :class="$style.title">{{ title }}</h2>
        <slot name="description"
          ><p :class="$style.description">{{ description }}</p></slot
        >
      </div>
      <div
        v-if="$slots.actions"
        :class="$style.actions"
      >
        <slot name="actions" />
      </div>
    </header>
    <slot />
    <footer
      v-if="$slots.footer"
      :class="$style.footer"
    >
      <slot name="footer" />
    </footer>
  </article>
</template>

<style module>
@reference "../../styles/index.css";
.panel {
  @apply min-w-0 flex-1 border bg-white p-6;
  border-color: var(--ui-border);
  border-radius: var(--app-surface-radius);
}
.tall {
  @apply min-h-[690px];
}
.empty {
  @apply min-h-[590px];
}
.header {
  @apply flex items-start justify-between gap-4;
}
.heading {
  @apply min-w-0;
}
.title {
  @apply text-[18px] font-semibold;
  color: var(--ui-text);
}
.description {
  @apply mt-1 text-[12px];
  color: var(--ui-text-subtle);
}
.actions {
  @apply ml-auto flex shrink-0 items-center gap-2;
}
.footer {
  @apply mt-5 flex items-center;
}
</style>
