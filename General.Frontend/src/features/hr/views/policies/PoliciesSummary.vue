<script setup lang="ts">
import { CheckCircle2, FileEdit, Files, Tags } from 'lucide-vue-next';
const items = [
  { label: '制度总数', value: '7', tone: 'primary', icon: Files },
  { label: '生效中', value: '6', tone: 'success', icon: CheckCircle2 },
  { label: '修订中', value: '1', tone: 'warning', icon: FileEdit },
  { label: '制度类别', value: '6', tone: 'purple', icon: Tags },
] as const;
</script>
<template>
  <section :class="$style.grid">
    <article
      v-for="item in items"
      :key="item.label"
      :data-tone="item.tone"
    >
      <component
        :is="item.icon"
        :size="16"
        :stroke-width="1.8"
      /><span>{{ item.label }}</span
      ><strong>{{ item.value }}</strong>
    </article>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-4 gap-4;
}
.grid article {
  @apply grid h-[84px] grid-cols-[24px_1fr] items-center rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.grid svg {
  color: var(--app-primary);
}
.grid span {
  @apply text-[10px];
  color: var(--ui-text-muted);
}
.grid strong {
  @apply col-span-2 text-[20px];
  color: var(--app-primary);
}
.grid article[data-tone='success'] svg,
.grid article[data-tone='success'] strong {
  color: var(--ui-success);
}
.grid article[data-tone='warning'] svg,
.grid article[data-tone='warning'] strong {
  color: var(--ui-warning);
}
.grid article[data-tone='purple'] svg,
.grid article[data-tone='purple'] strong {
  color: #6554c0;
}
</style>
