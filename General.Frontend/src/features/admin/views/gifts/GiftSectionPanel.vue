<script setup lang="ts">
import type { GiftSectionRecord } from '../../model/gifts.types';

defineProps<{ title: string; rows: readonly GiftSectionRecord[] }>();
</script>

<template>
  <section :class="$style.panel">
    <header>
      <h2>{{ title }}</h2>
      <span>{{ rows.length }} 条</span>
    </header>
    <div :class="$style.grid">
      <article
        v-for="row in rows"
        :key="row.id"
      >
        <span>{{ row.id }}</span>
        <h3>{{ row.title }}</h3>
        <p>{{ row.detail }}</p>
        <footer>
          <strong>{{ row.value }}</strong
          ><em>{{ row.status }}</em>
        </footer>
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply rounded border bg-white p-5;
  border-color: var(--ui-border);
}

.panel > header {
  @apply mb-4 flex items-center justify-between;
}

.panel h2 {
  @apply text-[14px] font-semibold;
}

.panel > header span {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.grid {
  @apply grid grid-cols-4 gap-3;
}

.grid article {
  @apply rounded border p-4;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.grid article > span {
  @apply text-[9px] font-semibold;
  color: var(--app-primary);
}

.grid h3 {
  @apply mt-2 text-[12px] font-semibold;
}

.grid p {
  @apply mt-1 min-h-8 text-[10px] leading-4;
  color: var(--ui-text-subtle);
}

.grid footer {
  @apply mt-3 flex items-center justify-between border-t pt-3;
  border-color: var(--ui-border-subtle);
}

.grid strong {
  @apply text-[12px];
}

.grid em {
  @apply rounded px-2 py-1 text-[9px] not-italic;
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
</style>
