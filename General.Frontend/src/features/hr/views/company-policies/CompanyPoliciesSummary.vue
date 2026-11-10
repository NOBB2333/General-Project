<script setup lang="ts">
import { BookOpen, Eye, FileSignature, Megaphone } from 'lucide-vue-next';
const items = [
  { label: '制度数量', value: '5 部', note: '涵盖 8 个分类', tone: 'primary', icon: BookOpen },
  { label: '全员阅读率', value: '89%', note: '4 部存在逾期未读', tone: 'warning', icon: Eye },
  { label: '电签覆盖率', value: '88%', note: '重要条款须电签', tone: 'success', icon: FileSignature },
  { label: '违规通报', value: '5', note: '4 起进行中', tone: 'danger', icon: Megaphone },
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
        :size="17"
        :stroke-width="1.8"
      />
      <div>
        <span>{{ item.label }}</span
        ><strong>{{ item.value }}</strong
        ><small>{{ item.note }}</small>
      </div>
    </article>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-4 gap-3;
}
.grid article {
  @apply flex h-[86px] items-center gap-3 rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.grid svg {
  @apply box-content rounded p-2;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.grid div {
  @apply flex flex-col;
}
.grid span,
.grid small {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.grid strong {
  @apply mt-1 text-[18px];
}
.grid article[data-tone='warning'] svg {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.grid article[data-tone='success'] svg {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.grid article[data-tone='danger'] svg {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
</style>
