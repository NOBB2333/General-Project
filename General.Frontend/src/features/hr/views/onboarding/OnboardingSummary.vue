<script setup lang="ts">
import { CalendarDays, TriangleAlert, UserRoundCheck, UserRoundX } from 'lucide-vue-next';

defineProps<{
  onboarding: number;
  waiting: number;
  offboarding: number;
  processing: number;
  month: number;
  pending: number;
}>();
</script>

<template>
  <section :class="$style.grid">
    <article data-tone="primary">
      <span :class="$style.icon"
        ><UserRoundCheck
          :size="21"
          :stroke-width="1.8"
      /></span>
      <p>
        <span>入职记录</span><strong>{{ onboarding }}</strong
        ><small>待入职 {{ waiting }}</small>
      </p>
    </article>
    <article data-tone="danger">
      <span :class="$style.icon"
        ><UserRoundX
          :size="21"
          :stroke-width="1.8"
      /></span>
      <p>
        <span>离职记录</span><strong>{{ offboarding }}</strong
        ><small>处理中 {{ processing }}</small>
      </p>
    </article>
    <article data-tone="success">
      <span :class="$style.icon"
        ><CalendarDays
          :size="21"
          :stroke-width="1.8"
      /></span>
      <p>
        <span>本月入职</span><strong>{{ month }}</strong
        ><small>3月</small>
      </p>
    </article>
    <article data-tone="warning">
      <span :class="$style.icon"
        ><TriangleAlert
          :size="21"
          :stroke-width="1.8"
      /></span>
      <p>
        <span>待处理</span><strong>{{ pending }}</strong
        ><small>需跟进</small>
      </p>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.grid article {
  @apply flex h-[96px] items-center gap-4 rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.icon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.grid p {
  @apply m-0 flex flex-col;
}
.grid p > span {
  @apply text-[11px];
  color: var(--ui-text-muted);
}
.grid strong {
  @apply mt-1 text-[21px] leading-none;
  color: var(--app-primary);
}
.grid small {
  @apply mt-1 text-[10px];
  color: var(--ui-text-subtle);
}
.grid article[data-tone='danger'] .icon,
.grid article[data-tone='danger'] strong {
  color: var(--ui-danger);
}
.grid article[data-tone='danger'] .icon {
  background: var(--ui-danger-soft);
}
.grid article[data-tone='success'] .icon,
.grid article[data-tone='success'] strong {
  color: var(--ui-success);
}
.grid article[data-tone='success'] .icon {
  background: var(--ui-success-soft);
}
.grid article[data-tone='warning'] .icon,
.grid article[data-tone='warning'] strong {
  color: var(--ui-warning);
}
.grid article[data-tone='warning'] .icon {
  background: var(--ui-warning-soft);
}
</style>
