<script setup lang="ts">
import { CircleCheck, ClipboardList, Target, UsersRound } from 'lucide-vue-next';

defineProps<{
  total: number;
  active: number;
  approved: number;
  vacancies: number;
}>();
</script>

<template>
  <section :class="$style.grid">
    <article>
      <span
        ><ClipboardList
          :size="17"
          :stroke-width="1.8"
        />岗位总数</span
      ><strong>{{ total }}</strong>
    </article>
    <article data-tone="success">
      <span
        ><CircleCheck
          :size="17"
          :stroke-width="1.8"
        />使用中</span
      ><strong>{{ active }}</strong>
    </article>
    <article data-tone="purple">
      <span
        ><UsersRound
          :size="17"
          :stroke-width="1.8"
        />总编制</span
      ><strong>{{ approved }}</strong>
    </article>
    <article data-tone="danger">
      <span
        ><Target
          :size="17"
          :stroke-width="1.8"
        />缺编</span
      ><strong>{{ vacancies }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.grid article {
  @apply flex h-[84px] flex-col justify-center rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.grid span {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-muted);
}
.grid article > span svg,
.grid article strong {
  color: var(--app-primary);
}
.grid strong {
  @apply mt-2 text-[21px] leading-none;
}
.grid article[data-tone='success'] > span svg,
.grid article[data-tone='success'] strong {
  color: var(--ui-success);
}
.grid article[data-tone='purple'] > span svg,
.grid article[data-tone='purple'] strong {
  color: #6554c0;
}
.grid article[data-tone='danger'] > span svg,
.grid article[data-tone='danger'] strong {
  color: var(--ui-danger);
}
</style>
